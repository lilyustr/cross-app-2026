namespace Core.Events;

public interface INotificationSink
{
    void Notify(CatalogChangedEventArgs e);
}