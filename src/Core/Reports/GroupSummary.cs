namespace Core.Reports;

/// <summary>
/// DTO для підсумкових даних групи товарів.
/// </summary>
public sealed record GroupSummary(string Key, int Count, int TotalQuantity);