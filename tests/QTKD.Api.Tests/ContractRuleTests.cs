using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using QTKD.Api.Entities;
using QTKD.Api.Services;
using Xunit;

namespace QTKD.Api.Tests;

public class ContractRuleTests
{
    [Fact]
    public async Task Ctr01_DraftContractCanBeUpdated()
    {
        await using var db = CreateDbContext();
        var customer = new Customer { Name = "Active customer", Status = CustomerStatus.Active };
        var contract = new Contract { ContractNo = "DRAFT-1", Customer = customer };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        var service = new ContractService(db);

        var result = await service.UpdateAsync(contract.Id, new UpdateContractRequest
        {
            ContractNo = "DRAFT-1-UPDATED",
            CustomerId = customer.Id,
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2027, 10, 31)
        });

        Assert.Null(result.Error);
        Assert.Equal("DRAFT-1-UPDATED", result.Value!.ContractNo);
        Assert.Equal("DRAFT-1-UPDATED", (await db.Contracts.FindAsync(contract.Id))!.ContractNo);
    }

    [Fact]
    public async Task Ctr01_PendingApprovalContractCannotBeUpdated()
    {
        await using var db = CreateDbContext();
        var customer = new Customer { Name = "Active customer", Status = CustomerStatus.Active };
        var contract = new Contract
        {
            ContractNo = "PENDING-1",
            Customer = customer,
            Status = ContractStatus.PendingApproval
        };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        var service = new ContractService(db);

        var result = await service.UpdateAsync(contract.Id, new UpdateContractRequest
        {
            ContractNo = "CHANGED",
            CustomerId = customer.Id
        });

        Assert.Equal(ContractOperationError.InvalidStatus, result.Error);
        Assert.Equal("PENDING-1", (await db.Contracts.FindAsync(contract.Id))!.ContractNo);
    }

    [Fact]
    public async Task Ctr02_SubmitWithoutContentIsRejectedAndRemainsDraft()
    {
        await using var db = CreateDbContext();
        var contract = await CreateSubmitReadyContractAsync(db);
        var service = new ContractService(db);

        var result = await service.SubmitAsync(contract.Id);

        Assert.Equal(ContractOperationError.MissingRequiredContent, result.Error);
        Assert.Equal(ContractStatus.Draft, (await db.Contracts.FindAsync(contract.Id))!.Status);
    }

    [Fact]
    public async Task Ctr02_SubmitWithCustomerDatesAndContentSucceeds()
    {
        await using var db = CreateDbContext();
        var contract = await CreateSubmitReadyContractAsync(db);
        db.ContractContents.Add(new ContractContent { ContractId = contract.Id, Name = "Required content" });
        await db.SaveChangesAsync();
        var service = new ContractService(db);

        var result = await service.SubmitAsync(contract.Id);

        Assert.Null(result.Error);
        Assert.Equal(ContractStatus.PendingApproval, result.Value!.Status);
        Assert.Equal(ContractStatus.PendingApproval, (await db.Contracts.FindAsync(contract.Id))!.Status);
    }

    [Fact]
    public async Task Ctr03_SubmitMovesDraftToPendingApprovalNotApproved()
    {
        await using var db = CreateDbContext();
        var contract = await CreateSubmitReadyContractAsync(db);
        db.ContractContents.Add(new ContractContent { ContractId = contract.Id, Name = "Required content" });
        await db.SaveChangesAsync();
        var service = new ContractService(db);

        var result = await service.SubmitAsync(contract.Id);

        Assert.Equal(ContractStatus.PendingApproval, result.Value!.Status);
        Assert.NotEqual(ContractStatus.Approved, result.Value.Status);
    }

    private static async Task<Contract> CreateSubmitReadyContractAsync(QtkdDbContext db)
    {
        var customer = new Customer { Name = "Active customer", Status = CustomerStatus.Active };
        var contract = new Contract
        {
            ContractNo = $"SUBMIT-{Guid.NewGuid():N}",
            Customer = customer,
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2027, 10, 31),
            Status = ContractStatus.Draft
        };
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        return contract;
    }

    private static QtkdDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<QtkdDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new QtkdDbContext(options);
    }
}