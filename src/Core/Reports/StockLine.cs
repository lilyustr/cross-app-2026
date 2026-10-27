namespace Core.Reports;

/// <summary>
/// DTO рядка звіту для відображення конкретної товарної позиції.
/// </summary>
public sealed record StockLine(
    string Sku,
    string Name,
    int Quantity,
    string Unit
);