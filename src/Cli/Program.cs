using System;
using System.IO;
using Cli;
using Core.Services;
using Core.Events;
using Cli.Sinks;
using Core.Abstractions;

// 1. Створення сховища через StoreFactory
var store = StoreFactory.Create(args);
var service = new CatalogService(store);

// 2. Налаштування шляху до файлу логування 
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

// 3. Локальний метод обробки події 
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    // Форматування рядка:
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";
    
    // Вивід у консоль
    Console.WriteLine(line);
    
    // Дописування рядка у файл logs/app.log
    File.AppendAllText(logPath, line + Environment.NewLine);
}

// 4. Підписка 1 (консоль + файл)
service.Changed += OnCatalogChanged;

// 5. Підписка 2 (лямбда-лічильник із замиканням)
int changes = 0;
service.Changed += (_, _) => changes++;

Console.WriteLine("=== 1. Початок сценарію (підписані обидва обробники) ===");

// Створення та перші операції
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);

// Побачимо значення лічильника
Console.WriteLine($"Подій отримано: {changes}"); 

Console.WriteLine("\n=== 2. Відписка OnCatalogChanged та тиха операція ===");

// Відписуємося від першого обробника
service.Changed -= OnCatalogChanged;

// Робимо операцію: запис у лог/консоль більше НЕ йде, але лямбда-лічильник працює!
service.Receive(created.Id, 5);

// Побачимо, що changes збільшився ще на 1
Console.WriteLine($"Подій отримано: {changes}");

// 6. Сценарій відмови
Console.WriteLine("\n=== 3. Сценарій відмови (try/catch) ===");
try
{
    Console.WriteLine("Спроба виконати Receive з від'ємною кількістю (-10)...");
    service.Receive(created.Id, -10);
}
catch (Exception ex)
{
    Console.WriteLine($"[OK] Перехоплено виняток: {ex.Message}");
}

// 7. Пошук через Func
Console.WriteLine("\n=== 4. Пошук товарів за допомогою Func (залишок < 100) ===");
var lowStock = service.FindAll(p => p.Quantity < 100);
foreach (var p in lowStock)
{
    Console.WriteLine($"  [{p.Sku}] {p.Name} — {p.Quantity} {p.Unit}");
}

Console.WriteLine("\n=== 5. Додаткове завдання: INotificationSink (Ручний DI) ===");

// Створюємо ConsoleSink та FileSink
INotificationSink consoleSink = new ConsoleSink();
INotificationSink fileSink = new FileSink(logPath);

// Створюємо окремий сервіс, передаючи Sink через конструктор (Ручний DI)
var serviceWithSink = new CatalogServiceWithSink(store, consoleSink);

// Викликаємо операцію — сповіщення піде прямо в ConsoleSink
Console.WriteLine("Виклики через CatalogServiceWithSink:");
serviceWithSink.Receive(created.Id, 10);

// Також можемо використати FileSink
var serviceWithFileSink = new CatalogServiceWithSink(store, fileSink);
serviceWithFileSink.Receive(created.Id, 5);

Console.WriteLine("\n=== 6. Додаткове завдання 2: Action<string> для пропущених рядків ===");

// Створюємо тестовий CSV-файл із сумішшю коректних та битих рядків
string sampleFilePath = Path.Combine(AppContext.BaseDirectory, "sample_import.csv");
File.WriteAllLines(sampleFilePath, new[]
{
    "SKU-01;Кабель UTP;м;100",        // Коректний
    "SKU-02;Розетка",                  // Недостатньо колонок
    "SKU-03;Коробка;шт;-5",           // Від'ємна кількість
    "",                                // Порожній рядок
    "SKU-04;Патчкорд 1м;шт;50"        // Коректний
});

// Передаємо лямбда-вираз як Action<string>
int importedCount = CatalogImporter.ImportCsv(sampleFilePath, reason =>
{
    // Отримуємо звіт про пропущені рядки прямо у викликаючому коді
    Console.WriteLine($"  [ПРОПУЩЕНО] {reason}");
});

Console.WriteLine($"Успішно імпортовано рядків: {importedCount}");