using System.Security.Cryptography;
using Api.Contracts;
using Api.Validation;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        
// 1. GET списку товарів з пагінацією та пошуком
        group.MapGet("/", Results<Ok<PagedResponse<ProductDto>>, ValidationProblem> (
            ICatalogStore store,
            int page = 1,
            int pageSize = 20,
            string? search = null) =>
        {
            var errors = new Dictionary<string, string[]>();
            if (page < 1)
                errors[nameof(page)] = ["Номер сторінки має бути не меншим за 1."];
            if (pageSize is < 1 or > 100)
                errors[nameof(pageSize)] = ["Розмір сторінки має бути в межах від 1 до 100."];

            if (errors.Count > 0)
                return TypedResults.ValidationProblem(errors);

            IEnumerable<Product> query = store.List();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    p.Sku.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query.ToList();
            var total = filtered.Count;

            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ProductDto.FromDomain)
                .ToList();

            return TypedResults.Ok(new PagedResponse<ProductDto>(items, total, page, pageSize));
        });

        // Додатковий маршрут за варіантом "Склад": товари з низьким залишком
        group.MapGet("/low-stock", (ICatalogStore store, int threshold = 5) =>
        {
            var items = store.List()
                .Where(p => p.Quantity <= threshold)
                .Select(ProductDto.FromDomain)
                .ToList();

            return TypedResults.Ok(items);
        });

        // 2. GET одного товару за id
        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound> (string id, ICatalogStore store) =>
        {
            return store.GetById(id) is { } found
                ? TypedResults.Ok(ProductDto.FromDomain(found))
                : TypedResults.NotFound();
        });

        // 3. POST створення нового товару
        group.MapPost("/", Results<Created<ProductDto>, ValidationProblem> (
            SaveProductRequest request,
            ICatalogStore store) =>
        {

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Product.Create(
                id,
                request.Sku!.Trim(),
                request.Name!.Trim(),
                request.Unit!.Trim(),
                request.Quantity);

            store.Add(created);

            return TypedResults.Created($"/api/products/{created.Id}", ProductDto.FromDomain(created));
        })
        .AddEndpointFilter<ValidationFilter<SaveProductRequest>>();

        // 4. PUT повне оновлення товару за id
        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound, ValidationProblem> (
            string id,
            SaveProductRequest request,
            ICatalogStore store) =>
        {

            if (store.GetById(id) is null)
                return TypedResults.NotFound();

            var replacement = Product.Create(
                id,
                request.Sku!.Trim(),
                request.Name!.Trim(),
                request.Unit!.Trim(),
                request.Quantity);

            store.Update(replacement);

            return TypedResults.Ok(ProductDto.FromDomain(replacement));
        })
        .AddEndpointFilter<ValidationFilter<SaveProductRequest>>();

        // 5. DELETE видалення товару за id
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, ICatalogStore store) =>
        {
            return store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}