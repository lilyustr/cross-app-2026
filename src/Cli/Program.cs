using System;
using System.IO;
using System.Threading;
using Core;
using Core.Services;
using Core.Storage;
using Cli;

// 1. Користувацький токен (Ctrl+C)
using var userCts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    userCts.Cancel();
};

// 2. Таймаут-токен (5 секунд)
using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

// 3. Зв'язаний токен (CreateLinkedTokenSource)
using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

var store = StoreFactory.Create(args);
var service = new CatalogService(store);
var dataPath = args.Length > 0 ? args[0] : "data/sample.json";

// 4. Підписник IProgress для виводу прогресу
var progress = new Progress<int>(count =>
{
    Console.Write($"\rОброблено записів: {count}");
});

try
{
    var added = await service.ImportStreamAsync(new JsonCatalogImporter(), dataPath, progress, linkedCts.Token);
    Console.WriteLine($"\nУспішно імпортовано нових записів: {added}");
}
catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
{
    Console.WriteLine("\nІмпорт скасовано: перевищено таймаут виконання.");
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nІмпорт скасовано користувачем (Ctrl+C).");
}
catch (DomainException ex)
{
    Console.WriteLine($"\nДані відхилено: {ex.Message}");
}
catch (FileNotFoundException)
{
    Console.WriteLine($"\nФайл не знайдено: {dataPath}");
}