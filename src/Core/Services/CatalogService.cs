using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class CatalogService
{
    private readonly ICatalogStore _store;

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    public CatalogService(ICatalogStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    private void OnChanged(Product p, CatalogChangeKind kind, int delta)
    {
        Changed?.Invoke(this, new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity));
    }

    public Product Add(string sku, string name, string unit, int quantity)
    {
        string id = Guid.NewGuid().ToString("N")[..8];
        var product = Product.Create(id, sku, name, unit, quantity);
        _store.Add(product);
        OnChanged(product, CatalogChangeKind.Added, quantity);
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.Issue(quantity);
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Issued, -quantity);
    }

    public bool Remove(string id)
    {
        var product = _store.GetById(id);
        if (product is null) return false;

        bool removed = _store.Remove(id);
        if (removed)
        {
            OnChanged(product, CatalogChangeKind.Removed, -product.Quantity);
        }
        return removed;
    }

    public IReadOnlyList<Product> All() => _store.List();

    public Product? Find(string id) => _store.GetById(id);

    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate) =>
        _store.List().Where(predicate).ToList();

   public async Task<int> ImportAsync(
        ICatalogImporter importer, 
        string path, 
        CancellationToken ct = default, 
        IProgress<int>? progress = null)
    {
        var items = await importer.LoadAsync(path, ct, progress);
        var added = 0;
        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            if (_store.GetById(item.Id) is not null) continue; // Ідемпотентність імпорту

            _store.Add(item);
            OnChanged(item, CatalogChangeKind.Added, item.Quantity);
            added++;
        }
        return added;
    }
}