namespace Core.Import;

using System.Text.Json;
using Core.Dto;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            List<ProductDto> rawItems = JsonSerializer.Deserialize<List<ProductDto>>(json, options) ?? [];

            for (int i = 0; i < rawItems.Count; i++)
            {
                int number = i + 1;
                ProductDto item = rawItems[i];

                switch (item)
                {
                    case { Quantity: < 0 }:
                        errors.Add($"елемент {number}: кількість '{item.Quantity}' не є невід'ємним числом");
                        break;

                    case { Sku: "" or null } or { Name: "" or null }:
                        errors.Add($"елемент {number}: SKU або назва порожні");
                        break;

                    case { Id: "" or null }:
                        errors.Add($"елемент {number}: ідентифікатор Id порожній");
                        break;

                    default:
                        items.Add(item);
                        break;
                }
            }

            return new ImportResult<ProductDto>(items, errors);
        }
        catch (JsonException ex)
        {
            errors.Add($"Помилка структури JSON: {ex.Message}");
            return new ImportResult<ProductDto>([], errors);
        }
    }
}