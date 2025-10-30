using Luley_Integracion_Net.Models;
using Microsoft.EntityFrameworkCore;

namespace Luley_Integracion_Net.Data;

public class LuleyDbContext(DbContextOptions<LuleyDbContext> options) : DbContext(options)
{
    public DbSet<DeliveryNoteDataModel> DeliveryNotesDataModel { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<DeliveryNoteDataModel>()
            .HasKey(dn => new { dn.nroRemito, dn.codArticulo });
    }
}
