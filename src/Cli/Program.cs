using Core.Domain;
using Core.Dto;
using Core.Import;

Console.WriteLine("=== Сценарій 1: успіх ===");
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine(product);
product.RegisterArrival(50);
product.Issue(30);
Console.WriteLine(product);
Console.WriteLine();

Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
TryDo("видача більша за залишок", () => product.Issue(1000));
TryDo("порожній SKU", () => Product.Create("P-002", "", "Пісок", "т", 10));
TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));
Console.WriteLine();

Console.WriteLine("=== Додаткове завдання 1: Перевірка зв'язку з тижнем 3 (ImportResult -> Сутності) ===");

// Симулюємо результат імпорту: 2 валідні записи і 2 записи з порушенням доменних правил
var testImport = new ImportResult<ProductDto>(
    Items: [
        new ProductDto("P-10", "SKU-010", "Ґрунтовка глибокого проникнення", "каністра", 25),
        new ProductDto("P-11", "", "Шпаклівка фінішна", "мішок", 10), // Помилка: порожній SKU
        new ProductDto("P-12", "SKU-012", "Клей для плитки", "мішок", -3), // Помилка: від'ємна кількість
        new ProductDto("P-13", "SKU-013", "Фарба фасадна біла", "відро", 15)
    ],
    Errors: ["Рядок 99: помилка парсингу дати (попередження з файлу)"]
);

var (entities, allErrors) = ProductMapper.ToDomainEntities(testImport);

Console.WriteLine($"Успішно створено доменних сутностей: {entities.Count}");
foreach (var item in entities)
{
    Console.WriteLine($"  + {item}");
}

Console.WriteLine($"\nВсього зафіксовано помилок: {allErrors.Count}");
foreach (var err in allErrors)
{
    Console.WriteLine($"  ! {err}");
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [FAIL] {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [OK] {title}: {ex.GetType().Name} — {ex.Message}");
    }
}

Console.WriteLine("=== Додаткове завдання 2: Інваріант двох сутностей (Warehouse + Product) ===");

var centralWarehouse = Warehouse.Create("W-01", "Центральний склад", 500);
var cement = Product.Create("P-001", "SKU-001", "Цемент М400", "мішок", 100);

Console.WriteLine($"Склад: {centralWarehouse}");
Console.WriteLine($"Товар: {cement}");

// 1. Успішний випадок: зайнято 350 + додаємо 100 = 450 (вкладаємося в ліміт 500)
WarehouseTransferRule.EnsureCanStore(centralWarehouse, currentTotalStock: 350, cement, incomingAmount: 100);
cement.RegisterArrival(100);
Console.WriteLine($"  [OK] Успішно додано 100 од. Новий залишок товару: {cement.Quantity}");

// 2. Порушення інваріанта: зайнято 450 + спроба додати 100 = 550 (> 500)
TryDo("переповнення місткості складу", () =>
{
    WarehouseTransferRule.EnsureCanStore(centralWarehouse, currentTotalStock: 450, cement, incomingAmount: 100);
});

Console.WriteLine();
Console.WriteLine("=== Додаткове завдання 3: Переходи статусів через switch expression ===");

var receipt = new StockReceipt("REC-001");
Console.WriteLine($"1. Створено: {receipt}");

// Успішний перехід Draft -> Confirmed
receipt.ChangeStatus(StockReceiptStatus.Confirmed);
Console.WriteLine($"2. Після підтвердження: {receipt}");

// Помилка: повторне встановлення Confirmed
TryDo("повторне підтвердження", () => receipt.ChangeStatus(StockReceiptStatus.Confirmed));

// Успішний перехід Confirmed -> Cancelled
receipt.ChangeStatus(StockReceiptStatus.Cancelled);
Console.WriteLine($"3. Після скасування: {receipt}");

// Помилка: повернення з Cancelled назад у Draft
TryDo("перехід із Cancelled у Draft", () => receipt.ChangeStatus(StockReceiptStatus.Draft));

// Помилка: повернення з Cancelled у Confirmed
TryDo("перехід із Cancelled у Confirmed", () => receipt.ChangeStatus(StockReceiptStatus.Confirmed));