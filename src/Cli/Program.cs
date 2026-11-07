using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Abstractions;
using Core.Events;
using Core.Reports;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

bool useFile = args.Contains("--file");

// Завжди беремо корінну папку data відносно поточного робочого каталогу
string dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
string dataPath = Path.Combine(dataDir, "catalog.json");
string importPath = Path.Combine(dataDir, "import.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

try
{
    var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "all";
    var items = service.FindAll(_ => true);

    switch (command)
    {
        case "import":
            if (!File.Exists(importPath))
            {
                Console.WriteLine($"Файл для імпорту не знайдено: {importPath}");
                return;
            }

            // Додаткове завдання: відстеження прогресу імпорту у реальному часі
            var progress = new Progress<int>(count =>
            {
                Console.WriteLine($"  [Прогрес] Оброблено записів: {count}");
            });

            var addedCount = await service.ImportAsync(new JsonCatalogImporter(), importPath, cts.Token, progress);
            Console.WriteLine($"Успішно імпортовано нових записів: {addedCount}");
            return;

        case "report-top":
            Console.WriteLine("=== ТОП-5 ТОВАРІВ ЗА КІЛЬКІСТЮ ===");
            PrintLines(ReportService.TopByQuantity(items, 5));
            return;

        case "report-low":
            Console.WriteLine("=== ДЕФІЦИТНІ ТОВАРИ (ЗАЛИШОК < 10) ===");
            PrintLines(ReportService.LowStock(items, 10));
            return;

        case "report-groups":
            Console.WriteLine("=== ЗВІТ ЗА ОДИНИЦЯМИ ВИМІРУ ===");
            foreach (var g in ReportService.ByUnit(items))
            {
                Console.WriteLine($"  Одиниця: {g.Key,-10} | Позицій: {g.Count,3} | Всього: {g.TotalQuantity,6}");
            }
            return;

        case "report-sum":
            Console.WriteLine("=== ЗАГАЛЬНІ АГРЕГАТИ СКЛАДУ ===");
            var t = ReportService.Totals(items);
            Console.WriteLine($"  Позицій: {t.Items}, Всього: {t.Total}, Середнє: {t.Average:F2}, Макс: {t.Max}");
            return;

        case "report-demo":
            Console.WriteLine("=== ДЕМОНСТРАЦІЯ ВІДКЛАДЕНОГО ВИКОНАННЯ LINQ ===");
            var sourceList = service.FindAll(_ => true).ToList();
            var query = sourceList.Where(p => p.Quantity < 10);
            Console.WriteLine($"1. Дефіцитних до зміни джерела: {query.Count()}");
            sourceList.Add(Core.Domain.Product.Create("p999", "SKU-999", "Тестова скоба", "шт", 3));
            Console.WriteLine($"2. Дефіцитних після додавання: {query.Count()}");
            var snapshot = query.ToList();
            sourceList.Clear();
            Console.WriteLine($"3. У знімку лишилось: {snapshot.Count}");
            return;
    }

    // Основний сценарій подій
    service.Changed += OnCatalogChanged;
    int eventCount = 0;
    service.Changed += (_, _) => eventCount++;

    Console.WriteLine("==================================================");
    Console.WriteLine($"  Сховище: {store.GetType().Name}");
    Console.WriteLine("==================================================");

    var first = items.First();
    service.Receive(first.Id, 20);
    Console.WriteLine($"\nПодій отримано лічильником: {eventCount}");

    service.Changed -= OnCatalogChanged;
    service.Receive(first.Id, 10);
    Console.WriteLine($"Подій отримано лічильником після тихої операції: {eventCount}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Імпорт скасовано.");
}
catch (DomainException ex)
{
    Console.WriteLine($"Дані відхилено: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"Файл не знайдено: {ex.FileName}");
}

static void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    Console.WriteLine($"  [Event] {e.At:HH:mm:ss} {e.Kind,-8} дельта: {(e.Delta >= 0 ? "+" : "")}{e.Delta}");
}

static void PrintLines(System.Collections.Generic.IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
    {
        Console.WriteLine($"  * [{l.Sku,-8}] {l.Name,-32} : {l.Quantity,5} {l.Unit}");
    }
}