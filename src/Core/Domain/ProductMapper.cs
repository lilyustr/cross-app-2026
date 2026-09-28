namespace Core.Domain;

using Core.Dto;

public static class ProductMapper
{
    public static (List<Product> Items, List<string> Errors) ToDomainEntities(ImportResult<ProductDto> importResult)
    {
        var domainEntities = new List<Product>();
        var errors = new List<string>(importResult.Errors);

        foreach (var dto in importResult.Items)
        {
            try
            {
                // Відновлення сутності через FromDto проходить через Product.Create()
                Product entity = Product.FromDto(dto);
                domainEntities.Add(entity);
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Помилка створення товару '{dto.Sku}': {ex.Message}");
            }
        }

        return (domainEntities, errors);
    }
}