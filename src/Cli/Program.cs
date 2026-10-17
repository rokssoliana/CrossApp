using Core;
using Core.Abstractions;
using Core.Events;
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

// === 2. НАЛАШТУВАННЯ ЛОГУВАННЯ ТА ПІДПИСНИКІВ ===
var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
var logPath = Path.Combine(logDir, "app.log");
Directory.CreateDirectory(logDir);

// Підписник 1: Локальний метод (виведення в консоль і запис у logs/app.log)
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var sign = e.Delta >= 0 ? $"+{e.Delta}" : $"{e.Delta}";
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {sign,4} залишок {e.QuantityAfter}";
    Console.WriteLine($"  [Event] {line}");
    File.AppendAllText(logPath, line + Environment.NewLine);
}

// Підписник 2: Лямбда-вираз із замиканням над локальною змінною
int changes = 0;
EventHandler<CatalogChangedEventArgs> counterHandler = (_, _) => changes++;

// Підписка обох обробників
service.Changed += OnCatalogChanged;
service.Changed += counterHandler;

// === 3. ОСНОВНИЙ СЦЕНАРІЙ (ДОДАВАННЯ ТА ЗМІНА СТАНУ) ===
Console.WriteLine("\n--- 1. Виконання операцій з активною підпискою ---");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);
Console.WriteLine($"Подій отримано лічильником: {changes}");

// === 4. ВІДПИСКА (ДЕМОНСТРАЦІЯ ОПЕРАТОРА -=) ===
Console.WriteLine("\n--- 2. Відписка локального методу (-= OnCatalogChanged) ---");
service.Changed -= OnCatalogChanged;

Console.WriteLine("Виконуємо операцію Receive (+5) після відписки:");
service.Receive(created.Id, 5); // Ця операція тиха для консолі й файлу, але враховується лічильником
Console.WriteLine($"Подій отримано лічильником після тихої операції: {changes}");

// === 5. СЦЕНАРІЙ ВІДМОВИ (ВИНЯТОК НЕ ПОВИНЕН ГЕНЕРУВАТИ ПОДІЮ) ===
Console.WriteLine("\n--- 3. Сценарій відмови (інваріант або неіснуючий Id) ---");
try
{
    Console.WriteLine("Спроба виконати Receive для неіснуючого запису:");
    service.Receive("UNKNOWN-ID", 10);
}
catch (Exception ex)
{
    Console.WriteLine($"  [Перехоплено виняток] {ex.GetType().Name}: {ex.Message}");
    Console.WriteLine($"  Лічильник подій не змінився і дорівнює: {changes}");
}

// === 6. ДЕЛЕГАТ ЗАМІСТЬ ПОДІЇ: ПОШУК ЧЕРЕЗ Func<Product, bool> ===
Console.WriteLine("\n--- 4. Виклик FindAll із лямбда-предикатом (делегат Func) ---");
Console.WriteLine("Товари з кількістю < 30:");
var lowStock = service.FindAll(p => p.Quantity < 30);
foreach (var p in lowStock)
{
    Console.WriteLine($"  * [{p.Sku}] {p.Name,-25} : {p.Quantity} {p.Unit}");
}