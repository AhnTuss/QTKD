using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;
using Xunit;

namespace QTKD.Api.Tests;

public class CustomerApiTests
{
    [Fact]
    public async Task Tc01_CreateCustomer_StoresAndReturnsCustomer()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        await factory.EnsureDatabaseAsync();

        var response = await client.PostAsJsonAsync("/api/customers", new
        {
            name = "Integration customer",
            status = "Active"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(customer);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var stored = await db.Customers.SingleAsync(c => c.Id == customer.Id);
        Assert.Equal("Integration customer", stored.Name);
        Assert.Equal(customer.Id, stored.Id);
    }

    [Fact]
    public async Task Tc02_CreateCustomerWithoutName_ReturnsBadRequestAndStoresNothing()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        await factory.EnsureDatabaseAsync();

        var response = await client.PostAsJsonAsync("/api/customers", new { status = "Active" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        Assert.Empty(await db.Customers.ToListAsync());
    }

    private sealed record CustomerResponse(int Id, string Name, string Status);
}