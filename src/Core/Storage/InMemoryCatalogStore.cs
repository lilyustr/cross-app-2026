using Core;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryCatalogStore : ICatalogStore
{
    private readonly Dictionary<string, Product> _items;
    private readonly object _sync = new();

    public InMemoryCatalogStore(IEnumerable<Product>? seed = null)
    {
        _items = (seed ?? Enumerable.Empty<Product>())
            .ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<Product> List()
    {
        lock (_sync)
            return _items.Values.ToList();
    }

    public Product? GetById(string id)
    {
        lock (_sync)
            return _items.GetValueOrDefault(id);
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_sync)
        {
            if (_items.Values.Any(p =>
                    !string.Equals(p.Id, item.Id, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(p.Sku, item.Sku, StringComparison.OrdinalIgnoreCase)))
            {
                throw new DomainException($"Товар з артикулом '{item.Sku}' вже існує в каталозі.");
            }

            _items[item.Id] = item;
        }
    }

    public bool Remove(string id)
    {
        lock (_sync)
            return _items.Remove(id);
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (_sync)
        {
            if (_items.ContainsKey(item.Id))
                throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");

            if (_items.Values.Any(p => string.Equals(p.Sku, item.Sku, StringComparison.OrdinalIgnoreCase)))
                throw new DomainException($"Товар з артикулом '{item.Sku}' вже існує в каталозі.");

            _items.Add(item.Id, item);
        }
    }
}