using System;
using System.IO;

namespace Core.Services;

public static class CatalogImporter
{
    /// <summary>
    /// Метод імпорту даних з CSV-файлу.
    /// </summary>
    /// <param name="filePath">Шлях до файлу імпорту.</param>
    /// <param name="onSkippedLine">Callback (Action) для сповіщення про пропущені/некоректні рядки.</param>
    /// <returns>Кількість успішно імпортованих рядків.</returns>
    public static int ImportCsv(string filePath, Action<string>? onSkippedLine = null)
    {
        if (!File.Exists(filePath))
        {
            onSkippedLine?.Invoke($"[Файл не знайдено] За вказаним шляхом файл відсутній: {filePath}");
            return 0;
        }

        int importedCount = 0;
        int lineNumber = 0;

        foreach (var line in File.ReadLines(filePath))
        {
            lineNumber++;

            // Пропускаємо порожні рядки
            if (string.IsNullOrWhiteSpace(line))
            {
                onSkippedLine?.Invoke($"Рядок {lineNumber}: Порожній рядок.");
                continue;
            }

            var parts = line.Split(';');

            // Перевірка кількості колонок (наприклад, SKU;Name;Unit;Quantity)
            if (parts.Length < 4)
            {
                onSkippedLine?.Invoke($"Рядок {lineNumber}: Недостатньо колонок (очікувалося 4, отримано {parts.Length}) -> '{line}'");
                continue;
            }

            // Перевірка коректності кількості
            if (!int.TryParse(parts[3], out int quantity) || quantity < 0)
            {
                onSkippedLine?.Invoke($"Рядок {lineNumber}: Некоректне значення кількості '{parts[3]}' -> '{line}'");
                continue;
            }

            // Якщо всі перевірки пройшли успішно — збільшуємо лічильник успішних рядків
            importedCount++;
        }

        return importedCount;
    }
}