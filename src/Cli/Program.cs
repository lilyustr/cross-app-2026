using Core.Dto;
using Core.Import;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

// Якщо передано змішаний CSV-файл (Додаткове завдання 2)
if (Path.GetFileName(path).Equals("mixed.csv", StringComparison.OrdinalIgnoreCase))
{
    ImportResult<IStorageRecord> mixedResult = MixedCsvImporter.Load(path);

    Console.WriteLine($"Завантажено записів: {mixedResult.Items.Count}");
    foreach (IStorageRecord record in mixedResult.Items)
    {
        switch (record)
        {
            case ProductDto p:
                Console.WriteLine($"  [ТОВАР] {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");
                break;
            case WarehouseDto w:
                Console.WriteLine($"  [СКЛАД] {w.Id,-6} {w.Code,-10} {w.Address,-26} {w.Capacity,5} місткість");
                break;
        }
    }

    if (mixedResult.Errors.Count > 0)
    {
        Console.WriteLine($"Пропущено рядків: {mixedResult.Errors.Count}");
        foreach (string e in mixedResult.Errors)
        {
            Console.WriteLine($"  ! {e}");
        }
    }

    int acc = mixedResult.Items.Count;
    int skp = mixedResult.Errors.Count;
    int tot = acc + skp;
    double rate = tot > 0 ? (double)skp / tot * 100 : 0;
    Console.WriteLine($"Статистика: усього {tot} | прийнято {acc} | пропущено {skp} | помилок {rate:F1}%");

    return 0;
}

// Стандартна обробка для sample.csv та sample.json
string extension = Path.GetExtension(path).ToLowerInvariant();

ImportResult<ProductDto> result = extension switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _ => new ImportResult<ProductDto>([], [$"Формат '{extension}' не підтримується. Очікується .csv або .json"])
};

if (result.Errors.Count > 0 && result.Items.Count == 0 && extension != ".csv" && extension != ".json")
{
    Console.WriteLine($"Помилка: {result.Errors[0]}");
    return 1;
}

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($"  {p.Id,-6} {p.Sku,-10} {p.Name,-26} {p.Quantity,5} {p.Unit}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($"  ! {e}");
    }
}

int accepted = result.Items.Count;
int skipped = result.Errors.Count;
int total = accepted + skipped;
double errorRate = total > 0 ? (double)skipped / total * 100 : 0;

Console.WriteLine($"Статистика: усього {total} | прийнято {accepted} | пропущено {skipped} | помилок {errorRate:F1}%");

return 0;