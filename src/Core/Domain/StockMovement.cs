namespace Core.Domain;

public sealed class StockMovement
{
    public string ProductId { get; }
    public int Quantity { get; }
    public DateTime At { get; }

    public StockMovement(string productId, int quantity, DateTime at)
    {
        ProductId = productId;
        Quantity = quantity;
        At = at;
    }
}