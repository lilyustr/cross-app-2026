namespace Core.Domain;
using Core.Dto;

public sealed class Product
{
    // Приватне поле: стан залишку сховано всередині об'єкта
    private int _quantity;

    // Властивості лише для читання: після створення їх неможливо змінити ззовні
    public string Id { get; }
    public string Sku { get; }
    public string Name { get; }
    public string Unit { get; }

    // Властивість-вираз: дає прочитати залишок, але не дозволяє пряме присвоєння
    public int Quantity => _quantity;

    // Приватний конструктор: забороняє прямий виклик new Product(...) з інших класів
    private Product(string id, string sku, string name, string unit, int quantity)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Unit = unit;
        _quantity = quantity;
    }
public static Product Create(string id, string sku, string name, string unit, int quantity)
    {
        // Перевірка 1: ідентифікатор не може бути порожнім
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));

        // Перевірка 2: SKU не може бути порожнім
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU не може бути порожнім", nameof(sku));

        // Перевірка 3: назва товару не може бути порожньою
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва не може бути порожньою", nameof(name));

        // Перевірка 4: одиниця вимірювання обов'язкова
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Одиниця вимірювання обов'язкова", nameof(unit));

        // Перевірка 5: початковий залишок не може бути від'ємним
        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Початковий залишок не може бути від'ємним");

        // Якщо всі умови виконані — викликаємо приватний конструктор із нормалізованими даними
        return new Product(
            id.Trim(),
            sku.Trim().ToUpperInvariant(),
            name.Trim(),
            unit.Trim(),
            quantity);
    }

// Операція 1: Прихід товару на склад
    public void RegisterArrival(int amount)
    {
        // Кількість має бути строго більшою за нуль
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість приходу має бути більшою за нуль");

        _quantity += amount;
    }

    // Операція 2: Видача товару зі складу
    public void Issue(int amount)
    {
        // Перевірка 1: некоректний вхідний аргумент
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість видачі має бути більшою за нуль");

        // Перевірка 2: операція неможлива через поточний стан (перевитрата)
        if (amount > _quantity)
            throw new InvalidOperationException($"Не можна видати {amount}: залишок {Sku} = {_quantity}");

        _quantity -= amount;
    }

// Перетворення сутності у DTO для збереження (використає сховище)
    public ProductDto ToDto() => new(Id, Sku, Name, Unit, Quantity);

    // Відновлення сутності з DTO: обов'язково через фабрику Create з перевірками!
    public static Product FromDto(ProductDto dto) =>
        Create(dto.Id, dto.Sku, dto.Name, dto.Unit, dto.Quantity);

    // Читабельний вивід інформації про товар у консоль
    public override string ToString() => $"{Id} [{Sku}] {Name} — {Quantity} {Unit}";
}
