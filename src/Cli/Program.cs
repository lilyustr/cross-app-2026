using Cli;
using Core.Services;

// 1. Створення сховища через StoreFactory (Індивідуальне завдання 3)
var store = StoreFactory.Create(args);
var service = new CatalogService(store);

Console.WriteLine($"=== Активне сховище: {store.GetType().Name} ===");

// 2. Успішний базовий сценарій: додавання та надходження
Console.WriteLine("\n--- 1. Базовий сценарій: Add та Receive ---");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
Console.WriteLine($"Додано: {created.Id} [{created.Sku}] {created.Name} — {created.Quantity} {created.Unit}");

service.Receive(created.Id, 25);
Console.WriteLine($"Прийнято +25 од. Актуальний залишок: {service.Find(created.Id)?.Quantity} {created.Unit}");

// 3. Вивід каталогу товарів
Console.WriteLine("\n--- 2. Каталог товарів ---");
foreach (var p in service.All())
{
    Console.WriteLine($"  {p.Id,-10} {p.Sku,-10} {p.Name,-28} {p.Quantity,5} {p.Unit}");
}

// 4. Сценарій відмови з перехопленням у try/catch
Console.WriteLine("\n--- 3. Сценарій відмови (try/catch) ---");
try
{
    Console.WriteLine("Спроба виконати прихід для неіснуючого товару 'NON-EXISTENT-ID'...");
    service.Receive("NON-EXISTENT-ID", 10);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"[OK] Очікувано перехоплено виняток: {ex.Message}");
}

// 5. Пошук за параметром Func (Індивідуальне завдання 2)
Console.WriteLine("\n--- 4. Пошук через Func (товари із залишком >= 100) ---");
var abundant = service.Find(p => p.Quantity >= 100);
foreach (var p in abundant)
{
    Console.WriteLine($"  [{p.Sku}] {p.Name,-25} — {p.Quantity,5} {p.Unit}");
}

// 6. Перевірка кешування Decorator (Індивідуальне завдання 1)
Console.WriteLine("\n--- 5. Робота CachingCatalogStore (GetById) ---");
Console.WriteLine("Перший запит (пошук P-001):");
service.Find("P-001");

Console.WriteLine("Другий запит (повторний пошук P-001):");
service.Find("P-001");