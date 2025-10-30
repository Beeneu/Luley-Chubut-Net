namespace Luley_Integracion_Net.models;

public class DeliveryNote()
{
    public string nroRemito { get; set; } = string.Empty;
    public int cantidadRemitida { get; set; }
    public int estadoRemito { get; set; }

    public DeliveryNote(string nroRemito, int cantidadRemitida, int estadoRemito)
        : this()
    {
        this.nroRemito = nroRemito;
        this.cantidadRemitida = cantidadRemitida;
        this.estadoRemito = estadoRemito;
    }

    public static DeliveryNote Create(string nroRemito, int cantidadRemitida, int estadoRemito)
    {
        return new DeliveryNote(nroRemito, cantidadRemitida, estadoRemito);
    }
}
