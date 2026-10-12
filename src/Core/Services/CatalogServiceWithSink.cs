using System;
using Core.Domain;
using Core.Events;
using Core.Abstractions;

namespace Core.Services;

public sealed class CatalogServiceWithSink
{
    private readonly ICatalogStore _store;
    private readonly INotificationSink _sink; // Передається через конструктор (DI)

    public CatalogServiceWithSink(ICatalogStore store, INotificationSink sink)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
    }

    public void Receive(string id, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Кількість повинна бути більше 0.", nameof(quantity));

        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);

        // Викликаємо метод інтерфейсу
        _sink.Notify(new CatalogChangedEventArgs(product.Id, CatalogChangeKind.Received, quantity, product.Quantity));
    }
}