using System;
using System.IO;
using Core.Events;

namespace Cli.Sinks;

public class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs e)
    {
        Console.WriteLine($"[ConsoleSink] {e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}");
    }
}

public class FileSink : INotificationSink
{
    private readonly string _filePath;

    public FileSink(string filePath)
    {
        _filePath = filePath;
    }

    public void Notify(CatalogChangedEventArgs e)
    {
        var line = $"[FileSink] {e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";
        File.AppendAllText(_filePath, line + Environment.NewLine);
    }
}