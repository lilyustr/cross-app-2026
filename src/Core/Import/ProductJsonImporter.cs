namespace Core.Import;

using System.Text.Json;
using Core.Dto;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            List<ProductDto> items = JsonSerializer.Deserialize<List<ProductDto>>(json, options) ?? [];
            return new ImportResult<ProductDto>(items, errors);
        }
        catch (JsonException ex)
        {
            errors.Add($"Помилка структури JSON: {ex.Message}");
            return new ImportResult<ProductDto>([], errors);
        }
    }
}