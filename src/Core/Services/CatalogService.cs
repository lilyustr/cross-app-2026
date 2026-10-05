namespace Core.Services;

using Core.Abstractions;
using Core.Domain;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;

    public CatalogService(ICatalogStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var product = Product.Create(id, sku, name, unit, quantity);
        _store.Add(product);
        return product;
    }

    public void Receive(string id, int quantity) //Реєстрація приходу товару
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);
    }

    public void Issue(string id, int quantity) //Реєстрація видачі товару
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.Issue(quantity);
        _store.Update(product);
    }

    public IReadOnlyList<Product> All() => _store.List();

    public Product? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Product> Find(Func<Product, bool> predicate)//приймає будь-яке правило фільтрації
    {
        ArgumentNullException.ThrowIfNull(predicate); //захищає сервіс, якщо замість правила передадуть null
        return _store.List().Where(predicate).ToList();//бере всі товари зі сховища, пропускає їх через передане правило і повертає список тих, що пройшли фільтр
    }
    

}