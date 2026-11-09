using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Reports;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class CatalogServiceTests
{
    [Fact]
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState() //інваріант
    {
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var p = service.Add("SKU-1", "Кабель", "м", 10);

        Assert.Throws<ArgumentException>(() => service.Receive(p.Id, -5));
        Assert.Equal(10, store.GetById(p.Id)!.Quantity);
    }

    [Fact]
    public void Add_RaisesChangedExactlyOnce()// подія
    {
        var service = new CatalogService(new InMemoryCatalogStore());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.Add("SKU-2", "Гвинт", "шт", 7);

        Assert.Single(events);
        Assert.Equal(CatalogChangeKind.Added, events[0].Kind);
    }

    [Theory]
    [InlineData(10, 2)]
    [InlineData(1, 0)]
    public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected)//звіт
    {
        var result = ReportService.LowStock(TestData.Items(), threshold);
        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void TopByQuantity_IsDeterministic()//звіт
    {
        var top = ReportService.TopByQuantity(TestData.Items(), 2);
        Assert.Equal(new[] { "SKU-BIG", "SKU-MID" }, top.Select(l => l.Sku));
    }

    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()//скасування
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, "[]");

        try
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            var service = new CatalogService(new InMemoryCatalogStore());

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.ImportAsync(new JsonCatalogImporter(), tempFile, cts.Token));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportAsync_WithInvalidData_ThrowsDomainException() //перевірка інваріантів, доменні вийнятки
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        try
        {
            var invalidJson = "[{\"Id\":\"1\",\"Sku\":\"SKU-1\",\"Name\":\"Test\",\"Unit\":\"шт\",\"Quantity\":-10}]";
            await File.WriteAllTextAsync(tempFile, invalidJson);

            var service = new CatalogService(new InMemoryCatalogStore());

            await Assert.ThrowsAsync<DomainException>(() =>
                service.ImportAsync(new JsonCatalogImporter(), tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ImportStreamAsync_WhenTimeoutOccurs_CancelsByTimeoutToken()//скасування з таймаутом
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        await File.WriteAllTextAsync(tempFile, "[{\"Id\":\"id-200\",\"Sku\":\"SKU-200\",\"Name\":\"Test\",\"Unit\":\"шт\",\"Quantity\":10}]");

        try
        {
            using var userCts = new CancellationTokenSource();
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));
            
            await Task.Delay(10);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);
            var service = new CatalogService(new InMemoryCatalogStore());

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.ImportStreamAsync(new JsonCatalogImporter(), tempFile, null, linkedCts.Token));

            Assert.False(userCts.IsCancellationRequested);
            Assert.True(timeoutCts.IsCancellationRequested);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

[Fact]
public async Task ImportStreamAsync_ReportsProgress()//перевірка прогресу
{
    var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
    var json = "[{\"Id\":\"id-100\",\"Sku\":\"SKU-100\",\"Name\":\"Test1\",\"Unit\":\"шт\",\"Quantity\":5}," +
               "{\"Id\":\"id-101\",\"Sku\":\"SKU-101\",\"Name\":\"Test2\",\"Unit\":\"шт\",\"Quantity\":10}]";
    await File.WriteAllTextAsync(tempFile, json);

    try
    {
        var service = new CatalogService(new InMemoryCatalogStore());
        var reportedValues = new List<int>();
        
        // Синхронний IProgress для надійного фіксування кожного виклику в тестах
        var progress = new SyncProgress<int>(reportedValues.Add);

        await service.ImportStreamAsync(new JsonCatalogImporter(), tempFile, progress);

        Assert.NotEmpty(reportedValues);
        Assert.Equal(2, reportedValues.Count);
        Assert.Equal(2, reportedValues.Last());
    }
    finally
    {
        if (File.Exists(tempFile))
            File.Delete(tempFile);
    }
}

// Допоміжний синхронний IProgress для тестів
private sealed class SyncProgress<T> : IProgress<T>
{
    private readonly Action<T> _handler;
    public SyncProgress(Action<T> handler) => _handler = handler;
    public void Report(T value) => _handler(value);
}
}