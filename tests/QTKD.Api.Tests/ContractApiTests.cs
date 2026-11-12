using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QTKD.Api.Data;
using QTKD.Api.Entities;
using Xunit;

namespace QTKD.Api.Tests;

public class ContractApiTests
{
    [Fact]
    public async Task Tc03_UpdateDraftContract_SucceedsAndPersistsChanges()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        await factory.EnsureDatabaseAsync();
        var (customerId, contractId) = await SeedContractAsync(factory);

        var response = await client.PutAsJsonAsync($"/api/contracts/{contractId}", new
        {
            contractNo = "CONTRACT-UPDATED",
            customerId,
            startDate = "2026-12-01",
            endDate = "2027-11-30"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var stored = await db.Contracts.SingleAsync(c => c.Id == contractId);
        Assert.Equal("CONTRACT-UPDATED", stored.ContractNo);
        Assert.Equal(new DateOnly(2026, 12, 1), stored.StartDate);
    }

    [Fact]
    public async Task Tc04_UpdatePendingContract_IsRejectedAndLeavesDataUnchanged()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        await factory.EnsureDatabaseAsync();
        var (customerId, contractId) = await SeedContractAsync(factory, ContractStatus.PendingApproval);

        var response = await client.PutAsJsonAsync($"/api/contracts/{contractId}", new
        {
            contractNo = "SHOULD-NOT-CHANGE",
            customerId,
            startDate = "2026-12-01",
            endDate = "2027-11-30"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var stored = await db.Contracts.SingleAsync(c => c.Id == contractId);
        Assert.Equal("CONTRACT-1", stored.ContractNo);
        Assert.Equal(ContractStatus.PendingApproval, stored.Status);
    }

    [Fact]
    public async Task Tc05_SubmitWithoutContent_IsRejectedAndRemainsDraft()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        await factory.EnsureDatabaseAsync();
        var (_, contractId) = await SeedContractAsync(factory);

        var response = await client.PostAsync($"/api/contracts/{contractId}/submit", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        Assert.Equal(ContractStatus.Draft, (await db.Contracts.SingleAsync()).Status);
    }

    [Fact]
    public async Task Tc06_SubmitValidContract_ChangesAndReturnsPendingApproval()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        await factory.EnsureDatabaseAsync();
        var (_, contractId) = await SeedContractAsync(factory, withContent: true);

        var response = await client.PostAsync($"/api/contracts/{contractId}/submit", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("PendingApproval", json.RootElement.GetProperty("status").GetString());
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        Assert.Equal(ContractStatus.PendingApproval, (await db.Contracts.SingleAsync()).Status);
    }

    [Fact]
    public async Task Tc07_RequestingApprovedDirectly_IsRejectedAndRemainsDraft()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        await factory.EnsureDatabaseAsync();
        var (customerId, contractId) = await SeedContractAsync(factory);

        var response = await client.PutAsJsonAsync($"/api/contracts/{contractId}", new
        {
            contractNo = "CONTRACT-1",
            customerId,
            status = "Approved"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        Assert.Equal(ContractStatus.Draft, (await db.Contracts.SingleAsync()).Status);
    }

    [Fact]
    public async Task Tc08_RepeatedSubmit_IsRejectedWithoutDuplicateContract()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        await factory.EnsureDatabaseAsync();
        var (_, contractId) = await SeedContractAsync(factory, withContent: true);

        var firstResponse = await client.PostAsync($"/api/contracts/{contractId}/submit", null);
        var repeatedResponse = await client.PostAsync($"/api/contracts/{contractId}/submit", null);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, repeatedResponse.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        Assert.Equal(1, await db.Contracts.CountAsync());
        Assert.Equal(ContractStatus.PendingApproval, (await db.Contracts.SingleAsync()).Status);
    }

    private static async Task<(int CustomerId, int ContractId)> SeedContractAsync(
        TestApiFactory factory,
        ContractStatus status = ContractStatus.Draft,
        bool withContent = false)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QtkdDbContext>();
        var customer = new Customer { Name = "Integration customer", Status = CustomerStatus.Active };
        var contract = new Contract
        {
            ContractNo = "CONTRACT-1",
            Customer = customer,
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2027, 10, 31),
            Status = status
        };
        if (withContent)
        {
            contract.Contents.Add(new ContractContent { Name = "Required content" });
        }

        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        return (customer.Id, contract.Id);
    }
}