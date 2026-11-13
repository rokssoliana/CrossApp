using System;
using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<string, Product> _cache = new();
    private bool _loaded;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public FileCatalogStore(string path)
    {
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

        var dtos = JsonSerializer.Deserialize<List<ProductDto>>(File.ReadAllText(_path), Options) ?? [];
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
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllText(tmp, JsonSerializer.Serialize(dtos, Options));
        File.Move(tmp, _path, overwrite: true);
    }

    public void Add(Product item)
    {
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public Product? GetById(string id)
    {
        EnsureLoaded();
        _cache.TryGetValue(id, out var item);
        return item;
    }

    public void Update(Product item)
    {
        EnsureLoaded();
        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (_cache.TryRemove(id, out _))
        {
            Flush();
            return true;
        }
        return false;
    }

    public IReadOnlyList<Product> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public IReadOnlyList<Product> Find(Func<Product, bool> predicate)
    {
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }

    public IReadOnlyList<Product> FindAll(Predicate<Product> match)
    {
        EnsureLoaded();
        return _cache.Values.Where(p => match(p)).ToList();
    }
}