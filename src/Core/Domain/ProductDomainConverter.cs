using Core.Dto;

namespace Core.Domain;

public record DomainImportResult<T>(
    IReadOnlyList<T> Entities,
    IReadOnlyList<string> InvariantErrors);

public static class ProductDomainConverter
{
    public static DomainImportResult<Product> Convert(ImportResult<ProductDto> importResult)
    {
        var entities = new List<Product>();
        var errors = new List<string>(importResult.Errors);

        foreach (var dto in importResult.Items)
        {
            try
            {
                var product = Product.FromDto(dto);
                entities.Add(product);
            }
            catch (Exception ex)
            {
                errors.Add($"Товар {dto.Id}: порушення інваріанту ({ex.GetType().Name} - {ex.Message})");
            }
        }

        return new DomainImportResult<Product>(entities, errors);
    }
}