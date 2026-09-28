namespace Core.Domain;

public static class WarehouseTransferRule
{
    // Правило контролює узгоджений стан ДВОХ сутностей одночасно
    public static void EnsureCanStore(Warehouse warehouse, int currentTotalStock, Product product, int incomingAmount)
    {
        if (incomingAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(incomingAmount), incomingAmount, "Кількість приходу має бути більшою за нуль");

        // Інваріант двох сутностей: перевірка сумарної місткості складу
        if (currentTotalStock + incomingAmount > warehouse.Capacity)
        {
            throw new InvalidOperationException(
                $"Склад '{warehouse.Name}' (ID: {warehouse.Id}) переповнений! " +
                $"Місткість: {warehouse.Capacity}, вже зайнято: {currentTotalStock}. " +
                $"Неможливо розмістити ще {incomingAmount} од. товару '{product.Sku}'.");
        }
    }
}