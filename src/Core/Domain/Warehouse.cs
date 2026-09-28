namespace Core.Domain;

public sealed class Warehouse
{
    public string Id { get; }
    public string Name { get; }
    public int Capacity { get; } // Максимальна місткість складу

    private Warehouse(string id, string name, int capacity)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
    }

    public static Warehouse Create(string id, string name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор складу обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва складу обов'язкова", nameof(name));

        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Місткість складу має бути більшою за нуль");

        return new Warehouse(id.Trim(), name.Trim(), capacity);
    }

    public override string ToString() => $"{Id}: {Name} (Місткість: {Capacity})";
}