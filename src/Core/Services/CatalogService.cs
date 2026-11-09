using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Storage;

namespace Core.Services;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;

    public CatalogService(ICatalogStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    // 1. Оголошення події за стандартом .NET (використовує EventHandler<TEventArgs>)
    public event EventHandler<CatalogChangedEventArgs>? Changed;

    // 2. Приватний метод підняття події (OnChanged)
    private void OnChanged(Product p, CatalogChangeKind kind, int delta)
    {
        Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity));
    }

    // 3. Метод додавання товару
    public Product Add(string sku, string name, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        
        _store.Add(product);

        // Подію викликаємо ЛИШЕ ПІСЛЯ успішного збереження в сховище
        OnChanged(product, CatalogChangeKind.Added, quantity);
        
        return product;
    }

    // 4. Метод приходу товару
    public void Receive(string id, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Кількість для приходу повинна бути більше 0.", nameof(quantity));

        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);

        // Подію викликаємо ЛИШЕ ПІСЛЯ оновлення товару
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    // 5. Метод FindAll із використанням Func<Product, bool> 
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate)
    {
        return _store.List().Where(predicate).ToList();
    }

    public Product? Find(string id) => _store.GetById(id);
    public IReadOnlyList<Product> All() => _store.List();

    // 6. Асинхронний імпорт всього списку одразу
    public async Task<int> ImportAsync(
        ICatalogImporter importer, 
        string path, 
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(importer);

        var items = await importer.LoadAsync(path, ct);
        var added = 0;

        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();

            if (_store.GetById(item.Id) is not null) 
                continue;

            _store.Add(item);
            OnChanged(item, CatalogChangeKind.Added, item.Quantity);
            added++;
        }

        return added;
    }

    // 7. Додаткове завдання: Потоковий асинхронний імпорт із підтримкою IProgress<int>
    public async Task<int> ImportStreamAsync(
        ICatalogImporter importer, 
        string path, 
        IProgress<int>? progress = null, 
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(importer);

        var added = 0;
        var processed = 0;

        await foreach (var item in importer.StreamAsync(path, ct).WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            processed++;

            if (_store.GetById(item.Id) is null)
            {
                _store.Add(item);
                OnChanged(item, CatalogChangeKind.Added, item.Quantity);
                added++;
            }

            // Передаємо кількість оброблених об'єктів підписнику
            progress?.Report(processed);
        }

        return added;
    }
}