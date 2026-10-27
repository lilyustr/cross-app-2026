using Core.Domain; 
using System.Diagnostics;

namespace Core.Reports;

/// <summary>
/// Сервіс формування аналітичних звітів на основі LINQ to Objects.
/// Приймає IEnumerable<Product> і повертає чисті DTO.
/// </summary>
public static class ReportService
{
    // 1) Фільтрація + Сортування + Проєкція в DTO (TopLowStock з використанням TopBy)
    public static IReadOnlyList<StockLine> TopLowStock(IEnumerable<Product> source, int threshold, int n)
    {
        return source
            .Where(p => p.Quantity < threshold)
            .TopBy(p => p.Quantity, n) // Друге використання TopBy
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();
    }

    // 1б) Звичайний LowStock для стандартного звіту report-low
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold)
    {
        return source
            .Where(p => p.Quantity < threshold)
            .OrderBy(p => p.Quantity)
            .ThenBy(p => p.Sku, StringComparer.Ordinal)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
            .ToList();
    }

    // 2) Топ-N за кількістю
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n)
        {
            return source
                .TopBy(p => p.Quantity, n) // Перше використання розширювального методу TopBy
                .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
                .ToList();
        }
 
    // 3a) Групування за одиницею виміру (Method Syntax)
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source)
    {
        return source
            .GroupBy(p => p.Unit)
            .Select(g => new GroupSummary(
                g.Key,
                g.Count(),
                g.Sum(p => p.Quantity)
            ))
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();
    }

    // 3б) Групування у синтаксисі запитів Query Syntax (Додаткове завдання 3)
    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Product> source)
    {
        var query = from p in source
                    group p by p.Unit into g
                    orderby g.Key
                    select new GroupSummary(
                        g.Key,
                        g.Count(),
                        g.Sum(x => x.Quantity)
                    );

        return query.ToList();
    }

    // 4) Агрегати з безпечною обробкою порожньої колекції через кортеж
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = source.ToList();

        if (list.Count == 0)
        {
            return (0, 0, 0d, 0); // бо Average() та Max() викидають виняток на порожній колекції.
        }

        return (
            list.Count,
            list.Sum(p => p.Quantity),
            list.Average(p => p.Quantity),
            list.Max(p => p.Quantity)
        );
    }


    // Додаткове завдання 1: GroupJoin двох колекцій (товари x переміщення)
    //З'єднує товари Product із їхніми переміщеннями StockMovement
    public static IReadOnlyList<MovementSummary> ProductMovementsSummary(
        IEnumerable<Product> products, 
        IEnumerable<StockMovement> movements)
    {
        return products
            .GroupJoin(
                movements,
                p => p.Id,
                m => m.ProductId,
                (p, productMovements) => new MovementSummary(
                    p.Sku,
                    p.Name,
                    productMovements.Sum(m => m.Quantity),
                    productMovements.Count()
                )
            )
            .OrderByDescending(ms => ms.TotalReceived)
            .ToList();
    }

    // Додаткове завдання 4: Замір часу на 100 000 записів (ToList усередині vs без нього)
    public static (long WithToListMs, long WithoutToListMs) BenchmarkToListImpact()
    {
        // Генеруємо 100 000 тестових записів
        var largeData = Enumerable.Range(1, 100_000)
            .Select(i => Product.Create($"id_{i}", $"SKU-{i}", $"Product {i}", i % 2 == 0 ? "шт" : "кг", i % 500))
            .ToList();

        // 1. БЕЗ ToList() усередині (лінива обробка)
        var sw1 = Stopwatch.StartNew();
        var res1 = largeData
            .Where(p => p.Quantity < 100)
            .Select(p => p.Quantity * 2)
            .OrderByDescending(x => x)
            .Take(10000)
            .ToList();
        sw1.Stop();

        // 2. З ToList() у середині ланцюжка (передчасна матеріалізація масиву)
        var sw2 = Stopwatch.StartNew();
        var res2 = largeData
            .Where(p => p.Quantity < 100)
            .ToList() // Проміжна матеріалізація в пам'ять
            .Select(p => p.Quantity * 2)
            .OrderByDescending(x => x)
            .Take(10000)
            .ToList();
        sw2.Stop();

        return (sw2.ElapsedMilliseconds, sw1.ElapsedMilliseconds);
    }
}