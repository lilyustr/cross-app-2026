using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class FileCatalogStore : ICatalogStore
{
    private readonly string _path;
    private readonly Dictionary<string, Product> _cache = new();
    private bool _loaded;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public FileCatalogStore(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Шлях до файлу не може бути порожнім.", nameof(path));

        _path = path;
    }

    private void EnsureLoaded()
    {
        if (_loaded) return;
        if (!File.Exists(_path))
        {
            _loaded = true;
            return;
        }

        var json = File.ReadAllText(_path);
        var dtos = JsonSerializer.Deserialize<List<ProductDto>>(json, Options) ?? [];

        _cache.Clear();
        foreach (var dto in dtos)
        {
            var product = Product.FromDto(dto);
            _cache[product.Id] = product;
        }

        _loaded = true;
    }

    private void Flush()
    {
        var dtos = _cache.Values.Select(product => product.ToDto()).ToList();
        var tmp = _path + ".tmp";
        
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Options));
        File.Move(tmp, _path, overwrite: true);
    }

    public void Add(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        EnsureLoaded();

        if (_cache.ContainsKey(product.Id))
            throw new DomainException($"Товар з ID {product.Id} вже існує.");

        _cache[product.Id] = product;
        Flush();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();
        return _cache.TryGetValue(id, out var product) ? product : null;
    }

    public IReadOnlyList<Product> GetAll()
    {
        EnsureLoaded();
        return _cache.Values.ToList().AsReadOnly();
    }

    public IReadOnlyList<Product> List()
    {
        return GetAll();
    }

    public void Update(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        EnsureLoaded();

        if (!_cache.ContainsKey(product.Id))
            throw new DomainException($"Товар з ID {product.Id} не знайдено.");

        _cache[product.Id] = product;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id))
            return false;

        Flush();
        return true;
    }
}