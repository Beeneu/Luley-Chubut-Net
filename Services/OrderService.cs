using Luley_Integracion_Net.models;
using Luley_Integracion_Net.Repositories;

namespace Luley_Integracion_Net.Services;

public class OrderService(OrderRepository repository, HttpService httpService)
{
    private readonly OrderRepository _repository = repository;
    private readonly HttpService _httpService = httpService;
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
            if (
                !raw_order.TryGetValue(NUM_SUB_PEDIDO, out var numSubPedido)
                || numSubPedido == null
            )
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
                    numeroRemitoStr!,
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

        await _httpService.EnsureAuthenticatedAsync();

        // make requests in parallel with a maximun of 5 concurrent requests
        var semaphore = new SemaphoreSlim(3);
        var tasks = ordersToUpdate
            .Select(async otu =>
            {
                await semaphore.WaitAsync();

                // orders to cancel
                if (
                    otu.articulos.Any(a =>
                        a.remitos is not null && a.remitos.Any(r => r.estadoRemito == 7)
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
            $"\n✓ Successfully updated {ordersToUpdate.Count} orders (max 5 concurrent)"
        );
        Console.ResetColor();
    }
}
