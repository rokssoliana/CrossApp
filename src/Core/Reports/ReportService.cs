using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;

namespace Core.Reports;

/// <summary>
/// Сервіс аналітичних звітів на основі LINQ to Objects.
/// </summary>
public static class ReportService
{
    // 1) Фільтр + сортування + проєкція у DTO
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source
            .Where(p => p.Quantity < threshold)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();

    // 2) Top-N за кількістю через ВЛАСНИЙ метод-розширення TopBy (Додаткове завдання №2)
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source
            .TopBy(p => p.Quantity, p => p.Sku, n)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();

    // 2.1) Повторне використання власного методу TopBy (Додаткове завдання №2)
    public static IReadOnlyList<StockLine> TopLowStock(IEnumerable<Product> source, int n) =>
        source
            .TopBy(p => -p.Quantity, p => p.Sku, n)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();

    // 3) Звіт у СИНТАКСИСІ ЗАПИТІВ / Query Syntax (Додаткове завдання №3)
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        (from p in source
         group p by p.Unit.ToUpperInvariant() into g
         let total = g.Sum(x => x.Quantity)
         orderby total descending
         select new GroupSummary(g.Key, g.Count(), total))
        .ToList();

    // 4) Агрегати з безпечною обробкою порожньої послідовності
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity), list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }
}