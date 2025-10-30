namespace Luley_Integracion_Net.models;

public class UpdateOrderRequested()
{
    public int numPedido { get; set; }
    public string numSubPedido { get; set; } = string.Empty;
    public List<Article> articulos { get; set; } = [];

    public UpdateOrderRequested(int NumPedido, string NumSubPedido, List<Article> Articulos)
        : this()
    {
        this.numPedido = NumPedido;
        this.numSubPedido = NumSubPedido;
        this.articulos = Articulos;
    }

    public static UpdateOrderRequested Create(
        int numPedido,
        string numSubPedido,
        List<Article> articulos
    )
    {
        return new UpdateOrderRequested(numPedido, numSubPedido, articulos);
    }
}
