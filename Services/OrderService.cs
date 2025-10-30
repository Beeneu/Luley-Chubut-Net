using Luley_Integracion_Net.Models;
using Luley_Integracion_Net.Repositories;

namespace Luley_Integracion_Net.Services;

public class OrderService(
    HanaDbRepository repository,
    HttpService httpService,
    DeliveryNoteDbRepository dnrepository
)
{
    private readonly HanaDbRepository _repository = repository;
    private readonly HttpService _httpService = httpService;
    private readonly DeliveryNoteDbRepository _dnrepository = dnrepository;
    private readonly string NRO_REMITO = "nroRemito";
    private readonly string NUM_PEDIDO = "numPedido";
    private readonly string NUM_SUB_PEDIDO = "numSubPedido";
    private readonly string COD_ARTICULO = "codArticulo";
    private readonly string ESTADO_REMITO = "estadoRemito";
    private readonly string CANTIDAD_REMITIDA = "cantidadRemitida";

    public async Task ProcessUpdatedOrdersAsync()
    {
        List<Dictionary<string, object>> results = await _repository.GetOrdersToUpdateAsync();

        List<object> remitos =
        [
            .. results
                .Where(rlc =>
                    rlc.TryGetValue(NRO_REMITO, out var remito)
                    && !string.IsNullOrEmpty(remito.ToString())
                )
                .Select(rlc => rlc["nroRemito"]),
        ];

        List<UpdateOrderRequested> ordersToUpdate = [];

        foreach (var raw_order in results)
        {
            if (!raw_order.TryGetValue(NUM_PEDIDO, out var numPedido) || numPedido == null)
                continue;
            if (!raw_order.TryGetValue(NUM_SUB_PEDIDO, out var numSubPedido) || numSubPedido == null)
                continue;

            raw_order.TryGetValue(NRO_REMITO, out var numRemito);
            raw_order.TryGetValue(COD_ARTICULO, out var codArticulo);
            raw_order.TryGetValue(ESTADO_REMITO, out var estadoRemito);
            raw_order.TryGetValue(CANTIDAD_REMITIDA, out var cantidadRemitida);

            string? numeroRemitoStr = null;
            int? cantidadRemitidaInt = null;

            if (numRemito is not DBNull && cantidadRemitida is not DBNull)
            {
                numeroRemitoStr = Convert.ToString(numRemito);
                cantidadRemitidaInt = Convert.ToInt32(cantidadRemitida);
            }

            int numPedidoInt = Convert.ToInt32(numPedido);
            int estadoRemitoInt = Convert.ToInt32(estadoRemito);
            string numeroSubPedidoStr = Convert.ToString(numSubPedido)!;
            string codArticuloStr = Convert.ToString(codArticulo)!;

            

            bool hasDeliveryNote = numeroRemitoStr != null && cantidadRemitidaInt != null;
            DeliveryNote? deliveryNote = null;

            Article article = Article.Create(codArticuloStr, []);

            // Create DeliveryNote
            if (hasDeliveryNote)
            {
                deliveryNote = DeliveryNote.Create(
                    numeroRemitoStr ?? "",
                    cantidadRemitidaInt ?? -1, // It will never reach this ??, but ill put it here just so the compiles stfu
                    estadoRemitoInt
                );

                article.remitos?.Add(deliveryNote);
            }

            // Find the Order
            var existingOrder = ordersToUpdate.Find(o =>
                o.numPedido == numPedidoInt && o.numSubPedido == numeroSubPedidoStr
            );

            if (existingOrder == null)
            {
                // Create new order with empty list of remitos
                var newOrder = UpdateOrderRequested.Create(numPedidoInt, numeroSubPedidoStr, []);
                newOrder.articulos?.Add(article);

                ordersToUpdate.Add(newOrder);
                existingOrder = newOrder;
            }
            else
            {
                existingOrder.articulos.Add(article);
            }
        }

        var ordersWithRemitos = ordersToUpdate
            .Where(o => o.articulos.Any(a => a.remitos != null && a.remitos.Count > 0))
            .ToList();

        var ordersWithoutRemitos = ordersToUpdate
            .Where(o => o.articulos.All(a => a.remitos == null || a.remitos.Count == 0))
            .ToList();

        var allDeliveryNotesDataModel = ordersWithRemitos
            .SelectMany(o => o.articulos)
            .SelectMany(a =>
                a.remitos?.Select(r => new DeliveryNoteDataModel
                {
                    nroRemito = r.nroRemito,
                    codArticulo = a.codArticulo,
                    cantidadRemitida = r.cantidadRemitida,
                    estadoRemito = r.estadoRemito,
                })
                    ?? []
            )
            .ToList();

        var deliveryNotesToSend =
            await _dnrepository.GetModifiedDeliveryNotesAsync(allDeliveryNotesDataModel);

        var validKeys = deliveryNotesToSend
            .Select(dn => (dn.nroRemito, dn.codArticulo))
            .ToHashSet();

        var filteredOrdersWithRemitos = ordersWithRemitos
            .Select(order => new UpdateOrderRequested
            {
                numPedido = order.numPedido,
                numSubPedido = order.numSubPedido,
                articulos =
                [
                    .. order
                        .articulos.Select(article => new Article
                        {
                            codArticulo = article.codArticulo,
                            remitos = article
                                .remitos?.Where(r =>
                                    validKeys.Contains((r.nroRemito, article.codArticulo))
                                )
                                .ToList(),
                        })
                        .Where(a => a.remitos != null && a.remitos.Count > 0),
                ],
            })
            .Where(o => o.articulos.Count > 0)
            .ToList();

        var finalOrders = filteredOrdersWithRemitos.Concat(ordersWithoutRemitos).ToList();

        await _httpService.EnsureAuthenticatedAsync();

        var semaphore = new SemaphoreSlim(3);
        var tasks = finalOrders
            .Select(async otu =>
            {
                await semaphore.WaitAsync();

                // orders to cancel
                if (
                    otu.articulos.Any(a =>
                        a.remitos is not null && a.remitos.Any(r => r.estadoRemito == 8)
                    )
                )
                    try
                    {
                        await _httpService.PatchAsync("/order/cancel/delivery/", otu);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                // orders to update
                else
                {
                    try
                    {
                        await _httpService.PatchAsync("/order/", otu);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }
            })
            .ToList();

        await Task.WhenAll(tasks);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(
            $"\n✓ Successfully updated {finalOrders.Count} orders (max 3 concurrent)"
        );
        Console.ResetColor();
    }
}
