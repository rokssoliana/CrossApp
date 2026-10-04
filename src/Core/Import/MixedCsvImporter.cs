using Core.Dto;

namespace Core.Import;

public record MixedImportResult(
    IReadOnlyList<ProductDto> Products,
    IReadOnlyList<WarehouseDto> Warehouses,
    IReadOnlyList<string> Errors);

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var warehouses = new List<WarehouseDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

            switch (parts)
            {
                // Розпізнаємо склад за префіксом W: W;id;name;location
                case ["W", var id, var name, var loc]:
                    warehouses.Add(new WarehouseDto(id, name, loc));
                    break;

                // Розпізнаємо товар за префіксом P: P;id;sku;name;unit;qty
                case ["P", var id, var sku, var name, var unit, var qty] when int.TryParse(qty, out int q) && q >= 0:
                    products.Add(new ProductDto(id, sku, name, unit, q));
                    break;

                // Помилки розбору товарів і складів
                case ["P", .. var _]:
                    errors.Add($"рядок {number}: помилка в полях товару або кількості");
                    break;

                case ["W", .. var _]:
                    errors.Add($"рядок {number}: склад очікує 3 параметри (id, name, location)");
                    break;

                case [var prefix, .. var _]:
                    errors.Add($"рядок {number}: невідомий тип запису '{prefix}'");
                    break;

                default:
                    errors.Add($"рядок {number}: порожній або некоректний рядок");
                    break;
            }
        }

        return new MixedImportResult(products, warehouses, errors);
    }
}