using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // 1. GET списку товарів із пагінацією та пошуком (Додаткове завдання №1)
        group.MapGet("/", (ICatalogStore store, int page = 1, int pageSize = 20, string? search = null) =>
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = store.List().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Sku.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query.ToList();
            var total = filtered.Count;
            var items = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToApiDto)
                .ToList();

            return TypedResults.Ok(new PagedResult<ProductDto>(items, total));
        });
        // 2. GET товару за id
        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound> (string id, ICatalogStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(MapToApiDto(found))
                : TypedResults.NotFound());

        // 3. POST створення нового товару
        group.MapPost("/", Results<Created<ProductDto>, ValidationProblem> (SaveProductRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Product.Create(id, request.Sku!, request.Name!, request.Unit!, request.Quantity);

            store.Add(created);
            return TypedResults.Created($"/api/products/{created.Id}", MapToApiDto(created));
        });

        // 4. PUT повне оновлення товару за id
        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound, ValidationProblem> (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            if (store.GetById(id) is null)
                return TypedResults.NotFound();

            var replacement = Product.Create(id, request.Sku!, request.Name!, request.Unit!, request.Quantity);
            store.Update(replacement);
            return TypedResults.Ok(MapToApiDto(replacement));
        });

        // 5. DELETE видалення товару за id
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, ICatalogStore store) =>
            store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.NotFound());
    }

    private static ProductDto MapToApiDto(Product p) =>
        new(p.Id, p.Sku, p.Name, p.Unit, p.Quantity);
}