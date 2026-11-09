using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    // 👈 Налаштування для ігнорування регістру полів (читає "Id", "id", "ID")
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.Read, 
            bufferSize: 4096, 
            useAsync: true);

        // Передаємо JsonOptions другим параметром
        var dtos = await JsonSerializer.DeserializeAsync<List<ProductDto>>(stream, JsonOptions, cancellationToken: ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} не містить списку.");

        var result = new List<Product>(dtos.Count);

        for (var i = 0; i < dtos.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                result.Add(Product.FromDto(dtos[i]));
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {i + 1}: {ex.Message}", ex);
            }
        }

        return result;
    }

    // Додаткове завдання 1: Потоковий імпорт через IAsyncEnumerable та [EnumeratorCancellation]
    public async IAsyncEnumerable<Product> StreamAsync(
        string path, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path, 
            FileMode.Open, 
            FileAccess.Read, 
            FileShare.Read, 
            bufferSize: 4096, 
            useAsync: true);

        // Передаємо JsonOptions другим параметром
        var dtos = JsonSerializer.DeserializeAsyncEnumerable<ProductDto>(stream, JsonOptions, cancellationToken: ct);

        var index = 0;
        await foreach (var dto in dtos.WithCancellation(ct))
        {
            ct.ThrowIfCancellationRequested();
            if (dto is null) continue;

            index++;
            Product product;
            try
            {
                product = Product.FromDto(dto);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {index}: {ex.Message}", ex);
            }

            yield return product;
        }
    }
}