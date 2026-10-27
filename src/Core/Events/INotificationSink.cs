namespace Core.Events;

public interface INotificationSink
{
    // Будь-який Sink повинен вміти приймати об'єкт події CatalogChangedEventArgs
    void Notify(CatalogChangedEventArgs e);
}