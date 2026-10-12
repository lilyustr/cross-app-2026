using System;

namespace Core.Events;

public sealed class CatalogChangedEventArgs : EventArgs
{
    public string Id { get; }
    public CatalogChangeKind Kind { get; }
    public int Delta { get; }
    public int QuantityAfter { get; }
    public DateTimeOffset At { get; } = DateTimeOffset.Now;

    public CatalogChangedEventArgs(string id, CatalogChangeKind kind, int delta, int quantityAfter)
    {
        Id = id;
        Kind = kind;
        Delta = delta;
        QuantityAfter = quantityAfter;
    }
}