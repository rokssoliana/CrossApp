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

        // 1. GET списку всіх товарів
        group.MapGet("/", (ICatalogStore store) =>
            TypedResults.Ok(store.List().Select(MapToApiDto).ToList()));

        // Додатковий маршрут варіанта "Склад": дефіцитні товари
        group.MapGet("/low-stock", (ICatalogStore store, int threshold = 5) =>
            TypedResults.Ok(store.Find(p => p.Quantity < threshold).Select(MapToApiDto).ToList()));

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