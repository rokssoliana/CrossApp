using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("==================================================");
Console.WriteLine("  Сценарій 1: Успішна робота з доменною сутністю");
Console.WriteLine("==================================================");

Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);
Console.WriteLine($"Створено об'єкт   : {product}");

product.RegisterArrival(50);
Console.WriteLine($"Після приходу (+50): {product}");

product.Issue(30);
Console.WriteLine($"Після видачі (-30) : {product}");

Console.WriteLine("\n==================================================");
Console.WriteLine("  Сценарій 2: Захист інваріантів (try/catch)");
Console.WriteLine("==================================================");

TryDo("Видача більша за залишок", () => product.Issue(1000));
TryDo("Порожній SKU", () => Product.Create("P-002", "   ", "Пісок", "т", 10));
TryDo("Від'ємний початковий залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -15));
TryDo("Нульовий прихід", () => product.RegisterArrival(0));

Console.WriteLine($"\nСтан об'єкта після спроб порушення інваріантів: {product}");

Console.WriteLine("\n==================================================");
Console.WriteLine("  Додаткове завдання: DTO -> Domain конвертація");
Console.WriteLine("==================================================");

string csvPath = Path.Combine("data", "sample.csv");
if (File.Exists(csvPath))
{
    ImportResult<ProductDto> csvResult = ProductCsvImporter.Load(csvPath);
    var domainResult = ProductDomainConverter.Convert(csvResult);

    Console.WriteLine($"Успішно валідовано та створено сутностей: {domainResult.Entities.Count}");
    Console.WriteLine($"Всього помилок (синтаксис CSV + інваріанти): {domainResult.InvariantErrors.Count}");
    foreach (var err in domainResult.InvariantErrors.Take(3))
    {
        Console.WriteLine($"  ! {err}");
    }
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"[Провал] {title}: інваріант не спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Захищено] {title}: {ex.GetType().Name} -> {ex.Message}");
    }
}