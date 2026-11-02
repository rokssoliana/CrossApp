namespace Core.Reports;

/// <summary>
/// DTO для представлення окремого рядка товару у звітах.
/// </summary>
public sealed record StockLine(string Sku, string Name, int Quantity, string Unit);