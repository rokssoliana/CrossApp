using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// === 1. COMPOSITION ROOT (Ручний DI) ===
bool useFile = args.Contains("--file");
string dataDir = Path.Combine(AppContext.BaseDirectory, "data");
string dataPath = Path.Combine(dataDir, "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine("==================================================");
Console.WriteLine($"  Сховище: {store.GetType().Name}");
Console.WriteLine("==================================================");

// === 2. СЦЕНАРІЙ РОБОТИ ===
Console.WriteLine("\n--- 1. Додавання нового товару та операція приходу ---");
var created = service.Add("SKU-999", "Кабель UTP cat6", "м", 50);
Console.WriteLine($"Створено: [{created.Id}] {created.Name} - {created.Quantity} {created.Unit}");

service.Receive(created.Id, 25);
var updated = service.Find(created.Id);
Console.WriteLine($"Після приходу (+25): [{updated!.Id}] {updated.Name} - {updated.Quantity} {updated.Unit}");

Console.WriteLine("\n--- 2. Поточний список товарів у каталозі ---");
foreach (var p in service.All().Take(6))
{
    Console.WriteLine($"  {p.Id,-8} {p.Sku,-10} {p.Name,-28} {p.Quantity,5} {p.Unit}");
}
Console.WriteLine($"  ... Всього записів у каталозі: {service.All().Count}");

// === 3. СЦЕНАРІЇ ВІДМОВИ (try/catch) ===
Console.WriteLine("\n--- 3. Сценарії відмови (обробка помилок) ---");

// Спроба додати запис із дублікатом id
TryDo("Спроба додати товар з існуючим Id", () =>
{
    store.Add(Product.Create(created.Id, "SKU-DUP", "Дублікат", "шт", 10));
});

// Спроба викликати прихід для неіснуючого id
TryDo("Операція Receive для неіснуючого Id", () =>
{
    service.Receive("NON-EXISTENT-ID", 100);
});

// === 4. ДОДАТКОВЕ ЗАВДАННЯ: ПОШУК ЗА ПРЕДИКАТОМ ===
Console.WriteLine("\n--- 4. Додаткове завдання: пошук за предикатом (Func<Product, bool>) ---");
Console.WriteLine("Пошук товарів з одиницею виміру 'шт' і залишком > 50:");
var filtered = service.Search(p => p.Unit == "шт" && p.Quantity > 50);
foreach (var p in filtered)
{
    Console.WriteLine($"  * [{p.Sku}] {p.Name,-25} : {p.Quantity} {p.Unit}");
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [Провал] {title}: очікуваний виняток не відбувся!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [Успішно перехоплено] {title}: {ex.GetType().Name} -> {ex.Message}");
    }
}