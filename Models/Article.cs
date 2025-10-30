using Microsoft.AspNetCore.StaticAssets;

namespace Luley_Integracion_Net.models;

public class Article()
{
    public string codArticulo { get; set; } = string.Empty;
    public List<DeliveryNote>? remitos { get; set; }

    public Article(string CodArticulo, List<DeliveryNote>? Remitos)
        : this()
    {
        this.codArticulo = CodArticulo;
        this.remitos = Remitos;
    }

    public static Article Create(string codArticulo, List<DeliveryNote>? remitos)
    {
        return new Article(codArticulo, remitos);
    }
}
