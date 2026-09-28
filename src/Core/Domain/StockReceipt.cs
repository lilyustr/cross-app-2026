namespace Core.Domain;

// Перелічування допустимих станів накладної
public enum StockReceiptStatus
{
    Draft,
    Confirmed,
    Cancelled
}

public sealed class StockReceipt
{
    public string Number { get; }
    public StockReceiptStatus Status { get; private set; }

    public StockReceipt(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Номер накладної обов'язковий", nameof(number));

        Number = number.Trim();
        Status = StockReceiptStatus.Draft; // Початковий статус завжди Draft
    }

    // Перевірка допустимості переходів через switch expression
    public void ChangeStatus(StockReceiptStatus targetStatus)
    {
        Status = (Status, targetStatus) switch
        {
            // Дозволені переходи з Draft:
            (StockReceiptStatus.Draft, StockReceiptStatus.Confirmed) => StockReceiptStatus.Confirmed,
            (StockReceiptStatus.Draft, StockReceiptStatus.Cancelled) => StockReceiptStatus.Cancelled,

            // Дозволений перехід з Confirmed:
            (StockReceiptStatus.Confirmed, StockReceiptStatus.Cancelled) => StockReceiptStatus.Cancelled,

            // Спроба повторно встановити поточний статус:
            var (current, target) when current == target =>
                throw new InvalidOperationException($"Накладна '{Number}' уже перебуває в статусі '{current}'"),

            // Будь-який інший заборонений перехід (наприклад, Cancelled -> Draft або Cancelled -> Confirmed):
            (var current, var target) =>
                throw new InvalidOperationException($"Неприпустимий перехід для накладної '{Number}': з '{current}' у '{target}'")
        };
    }

    public override string ToString() => $"Накладна {Number} [Статус: {Status}]";
}