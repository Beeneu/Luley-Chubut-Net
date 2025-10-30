using System;

namespace Luley_Integracion_Net.models;

public class CancelOrder()
{
    public required Guid purchaseOrderUUID { get; set; }
}
