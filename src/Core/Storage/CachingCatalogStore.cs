namespace Core.Storage;

using Core.Abstractions;
using Core.Domain;

public sealed class CachingCatalogStore : ICatalogStore
{
    private readonly ICatalogStore _inner;
    private readonly Dictionary<string, Product> _lookupCache = new(StringComparer.OrdinalIgnoreCase);

    public CachingCatalogStore(ICatalogStore inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public Product? GetById(string id)
    {
        if (_lookupCache.TryGetValue(id, out var cached))
        {
            Console.WriteLine($"[Кеш] Знайдено в пам'яті декоратора: {id}");
            return cached;
        }

        var item = _inner.GetById(id);
        if (item is not null)
        {
            _lookupCache[id] = item;
            Console.WriteLine($"[Кеш] Отримано зі сховища та збережено в кеш: {id}");
        }

        return item;
    }

    public IReadOnlyList<Product> List() => _inner.List();

    public void Add(Product item)
    {
        _inner.Add(item);
        _lookupCache[item.Id] = item;
    }

    public void Update(Product item) //Оновлює змінений товар у справжньому сховищі та змінений товар у справжньому сховищі
    {
        _inner.Update(item);
        _lookupCache[item.Id] = item;
    }

    public bool Remove(string id) //щоб випадково не віддати видалений товар
    {
        _lookupCache.Remove(id);
        return _inner.Remove(id);
    }
}