using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;
using Xunit;

namespace QTKD.Api.Tests;

public class PriceListApiTests
{
    [Fact]
    public async Task CreatePriceList_AddItemAndReadItems_PersistsPricingData()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        await factory.EnsureDatabaseAsync();

        var createResponse = await client.PostAsJsonAsync("/api/pricelists", new
        {
            name = "Integration prices",
            effectiveDate = "2026-11-01"
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        using var priceListJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var priceListId = priceListJson.RootElement.GetProperty("id").GetInt32();

        var addItemResponse = await client.PostAsJsonAsync($"/api/pricelists/{priceListId}/items", new
        {
            serviceCode = "SVC-TEST",
            unitPrice = 2500.50m
        });
        Assert.Equal(HttpStatusCode.Created, addItemResponse.StatusCode);

        var getItemsResponse = await client.GetAsync($"/api/pricelists/{priceListId}/items");
        Assert.Equal(HttpStatusCode.OK, getItemsResponse.StatusCode);
        using var itemsJson = JsonDocument.Parse(await getItemsResponse.Content.ReadAsStringAsync());
        var item = Assert.Single(itemsJson.RootElement.EnumerateArray().ToArray());
        Assert.Equal("SVC-TEST", item.GetProperty("serviceCode").GetString());
        Assert.Equal(2500.50m, item.GetProperty("unitPrice").GetDecimal());

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        Assert.Equal(1, await db.PriceItems.CountAsync());
    }

    [Fact]
    public async Task AddItemWithInvalidPriceOrDuplicateCode_IsRejected()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        await factory.EnsureDatabaseAsync();
        var createResponse = await client.PostAsJsonAsync("/api/pricelists", new
        {
            name = "Validation prices",
            effectiveDate = "2026-11-01"
        });
        using var priceListJson = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var priceListId = priceListJson.RootElement.GetProperty("id").GetInt32();

        var invalidPriceResponse = await client.PostAsJsonAsync($"/api/pricelists/{priceListId}/items", new
        {
            serviceCode = "SVC-ZERO",
            unitPrice = 0m
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidPriceResponse.StatusCode);

        var validItem = new { serviceCode = "SVC-UNIQUE", unitPrice = 10m };
        var firstResponse = await client.PostAsJsonAsync($"/api/pricelists/{priceListId}/items", validItem);
        var duplicateResponse = await client.PostAsJsonAsync($"/api/pricelists/{priceListId}/items", validItem);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        Assert.Equal(1, await db.PriceItems.CountAsync());
    }
}