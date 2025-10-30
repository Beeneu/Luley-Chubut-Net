using Luley_Integracion_Net.Data;
using Luley_Integracion_Net.Models;
using Microsoft.EntityFrameworkCore;

namespace Luley_Integracion_Net.Repositories;

public class DeliveryNoteDbRepository(LuleyDbContext context)
    : GenericRepository<DeliveryNoteDataModel>(context)
{
    public async Task<List<DeliveryNoteDataModel>> GetModifiedDeliveryNotesAsync(List<DeliveryNoteDataModel> deliveryNotes)
    {
        var compositeKeys = deliveryNotes
            .Select(dn => new { dn.nroRemito, dn.codArticulo })
            .ToList();

        var nroRemitos = deliveryNotes.Select(dn => dn.nroRemito).Distinct().ToList();
        var codArticulos = deliveryNotes.Select(dn => dn.codArticulo).Distinct().ToList();

        var existingDeliveryNotes = await _dbSet.Where(r => nroRemitos.Contains(r.nroRemito) 
                                                        && codArticulos.Contains(r.codArticulo))
                                                .ToListAsync();

        var existingDict = existingDeliveryNotes.ToDictionary(r => (r.nroRemito, r.codArticulo));

        var toSendToApi = new List<DeliveryNoteDataModel>();

        foreach (var newDeliveryNote in deliveryNotes)
        {
            var key = (newDeliveryNote.nroRemito, newDeliveryNote.codArticulo);

            if (existingDict.TryGetValue(key, out var existing))
            {
                if (existing.estadoRemito != newDeliveryNote.estadoRemito)
                {
                    existing.estadoRemito = newDeliveryNote.estadoRemito;
                    existing.cantidadRemitida = newDeliveryNote.cantidadRemitida;
                    _dbSet.Update(existing);
                    toSendToApi.Add(newDeliveryNote);
                }
            }
            else
            {
                await _dbSet.AddAsync(newDeliveryNote);
                toSendToApi.Add(newDeliveryNote);
            }
        }

        await _context.SaveChangesAsync();

        return toSendToApi;
    }
}
