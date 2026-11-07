using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Events;
using Core.Reports;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class CatalogTests
{
    [Fact] // 1) Інваріант: відмова і незмінний стан при помилці
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var p = service.Add("SKU-1", "Кабель", "м", 10);

        Assert.ThrowsAny<ArgumentException>(() => service.Receive(p.Id, -5));
        Assert.Equal(10, store.GetById(p.Id)!.Quantity);
    }

    [Fact] // 2) Перевірка генерації події Changed (Lab 6)
    public void Add_RaisesChangedExactlyOnce()
    {
        var service = new CatalogService(new InMemoryCatalogStore());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.Add("SKU-2", "Гвинт", "шт", 7);

        Assert.Single(events);
        Assert.Equal(CatalogChangeKind.Added, events[0].Kind);
    }

    [Theory] // 3) Перевірка LINQ-звіту на кількох наборах вхідних даних (Lab 7)
    [InlineData(10, 1)]
    [InlineData(60, 2)]
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected) =>
        Assert.Equal(expected, ReportService.LowStock(TestData.Items(), threshold).Count);

    [Fact] // 4) Детермінований топ-N (Lab 7)
    public void TopByQuantity_IsDeterministic() =>
        Assert.Equal(new[] { "SKU-BIG", "SKU-MID" },
            ReportService.TopByQuantity(TestData.Items(), 2).Select(l => l.Sku));

    [Fact] // 5) Скасування операції асинхронного імпорту
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, "[]");
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var service = new CatalogService(new InMemoryCatalogStore());

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.ImportAsync(new JsonCatalogImporter(), tempFile, cts.Token));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact] // 6) Тимчасовий файл із зіпсованими даними -> DomainException
    public async Task ImportAsync_InvalidData_ThrowsDomainException()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, "[{\"Id\":\"p1\",\"Sku\":\"SKU-1\",\"Name\":\"X\",\"Unit\":\"шт\",\"Quantity\":-10}]");
            var service = new CatalogService(new InMemoryCatalogStore());

            await Assert.ThrowsAsync<DomainException>(
                () => service.ImportAsync(new JsonCatalogImporter(), tempFile));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}