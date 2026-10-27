using System;
using System.IO;
using System.Linq;
using Core;
using Core.Reports;
using Core.Domain;
using Core.Services;
using Cli;


// 1. Ініціалізація сховища та сервісу
var store = StoreFactory.Create(args);
var service = new CatalogService(store);

// 2. ДЕМОНСТРАЦІЯ ВІДКТАДЕННОГО ВИКОНАННЯ (Deferred Execution)
Console.WriteLine("=== Демонстрація відкладеного виконання LINQ ===");

// Беремо локальний список, бо service.All() повертає новий знімок
var source = service.All().ToList();

// Створюємо відкладений запит (побудований, але не виконаний)
var query = source.Where(p => p.Quantity < 10);
Console.WriteLine($"Дефіцитних до змін: {query.Count()}"); // 1-ше виконання

// Динамічно змінюємо джерело (додаємо новий дефіцитний товар)
source.Add(Product.Create("tmp00001", "SKU-999", "Скоба канцелярська", "шт", 3));

Console.WriteLine($"Дефіцитних після змін: {query.Count()}"); // 2-ге виконання (число змінилося!)

// Матеріалізуємо через ToList() у фіксований знімок
var snapshot = query.ToList();
source.Clear();

// Перевіряємо зафіксований знімок
Console.WriteLine($"У знімку лишилось: {snapshot.Count}"); // Не змінюється!
Console.WriteLine("===================================================\n");


// 3. ОБРОБКА ПІДКОМАНД ЗВІТІВ В CLI
var items = service.All();

// Отримуємо назву звіту (відкидаючи прапорці на кшталт --file з 5-го тижня)
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(items, 5));
        break;

    case "report-low":
        Print(ReportService.LowStock(items, 10));
        break;

    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
        {
            Console.WriteLine($"  {g.Key,-6} позицій: {g.Count,3}  сумарно: {g.TotalQuantity,6}");
        }
        break;

    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($"  позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;

    default:
        Console.WriteLine("Доступні підкоманди: report-top | report-low | report-groups | report-sum");
        break;
    case "report-join":
    // Генеруємо фейкові переміщення для демо
    var movements = items.Select(i => new Core.Domain.StockMovement(i.Id, i.Quantity + 10, DateTime.Now)).ToList();
    var joined = ReportService.ProductMovementsSummary(items, movements);
    foreach (var j in joined)
    {
        Console.WriteLine($"  {j.Sku,-10} {j.Name,-22} Отримано всього: {j.TotalReceived,5} (Операцій: {j.TotalMovements})");
    }
    break;

case "report-bench":
    Console.WriteLine("Виконується замір часу на 100 000 записів...");
    var (withList, withoutList) = ReportService.BenchmarkToListImpact();
    Console.WriteLine($"  Без ToList() усередині: {withoutList} ms");
    Console.WriteLine($"  З ToList() усередині:    {withList} ms");
    break;
}

// Форматований вивід для StockLine
static void Print(System.Collections.Generic.IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
    {
        Console.WriteLine($"  {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
    }
}


