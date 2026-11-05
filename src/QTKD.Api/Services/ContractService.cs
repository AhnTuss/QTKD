using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using QTKD.Api.Entities;

namespace QTKD.Api.Services;

public class ContractService : IContractService
{
    private readonly QtkdDbContext _db;

    public ContractService(QtkdDbContext db)
    {
        _db = db;
    }

    public async Task<List<ContractDto>> GetAllAsync(CancellationToken ct = default)
    {
        var contracts = await _db.Contracts
            .AsNoTracking()
            .Include(c => c.Customer)
            .Include(c => c.Contents)
            .OrderBy(c => c.Id)
            .ToListAsync(ct);

        return contracts.Select(ToDto).ToList();
    }

    public async Task<ContractDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var contract = await _db.Contracts
            .AsNoTracking()
            .Include(c => c.Customer)
            .Include(c => c.Contents)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        return contract is null ? null : ToDto(contract);
    }

    public async Task<ContractOperationResult<ContractDto>> CreateAsync(
        CreateContractRequest request,
        CancellationToken ct = default)
    {
        var validation = await ValidateReferencesAndFieldsAsync(
            request.CustomerId,
            request.ContractNo,
            request.StartDate,
            request.EndDate,
            null,
            ct);
        if (validation is not null)
        {
            return ContractOperationResult<ContractDto>.Failure(validation.Value);
        }

        var customer = await _db.Customers.FirstAsync(c => c.Id == request.CustomerId, ct);
        var contract = new Contract
        {
            ContractNo = request.ContractNo.Trim(),
            CustomerId = customer.Id,
            Customer = customer,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = ContractStatus.Draft
        };

        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync(ct);
        return ContractOperationResult<ContractDto>.Success(ToDto(contract));
    }

    public async Task<ContractOperationResult<ContractDto>> UpdateAsync(
        int id,
        UpdateContractRequest request,
        CancellationToken ct = default)
    {
        var contract = await _db.Contracts
            .Include(c => c.Customer)
            .Include(c => c.Contents)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (contract is null)
        {
            return ContractOperationResult<ContractDto>.Failure(ContractOperationError.NotFound);
        }

        if (!IsEditable(contract.Status))
        {
            return ContractOperationResult<ContractDto>.Failure(ContractOperationError.InvalidStatus);
        }

        var validation = await ValidateReferencesAndFieldsAsync(
            request.CustomerId,
            request.ContractNo,
            request.StartDate,
            request.EndDate,
            id,
            ct);
        if (validation is not null)
        {
            return ContractOperationResult<ContractDto>.Failure(validation.Value);
        }

        var customer = await _db.Customers.FirstAsync(c => c.Id == request.CustomerId, ct);
        contract.ContractNo = request.ContractNo.Trim();
        contract.CustomerId = customer.Id;
        contract.Customer = customer;
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        await _db.SaveChangesAsync(ct);

        return ContractOperationResult<ContractDto>.Success(ToDto(contract));
    }

    public async Task<ContractOperationResult<ContractContentDto>> AddContentAsync(
        int id,
        CreateContractContentRequest request,
        CancellationToken ct = default)
    {
        var contract = await _db.Contracts.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (contract is null)
        {
            return ContractOperationResult<ContractContentDto>.Failure(ContractOperationError.NotFound);
        }

        if (!IsEditable(contract.Status))
        {
            return ContractOperationResult<ContractContentDto>.Failure(ContractOperationError.InvalidStatus);
        }

        var content = new ContractContent { ContractId = id, Name = request.Name.Trim() };
        _db.ContractContents.Add(content);
        await _db.SaveChangesAsync(ct);

        return ContractOperationResult<ContractContentDto>.Success(new ContractContentDto(content.Id, content.Name));
    }

    public async Task<ContractContentDto?> GetContentByIdAsync(int id, int contentId, CancellationToken ct = default)
    {
        return await _db.ContractContents
            .AsNoTracking()
            .Where(c => c.ContractId == id && c.Id == contentId)
            .Select(c => new ContractContentDto(c.Id, c.Name))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ContractOperationResult<ContractDto>> SubmitAsync(int id, CancellationToken ct = default)
    {
        var contract = await _db.Contracts
            .Include(c => c.Customer)
            .Include(c => c.Contents)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
        if (contract is null)
        {
            return ContractOperationResult<ContractDto>.Failure(ContractOperationError.NotFound);
        }

        if (contract.Status != ContractStatus.Draft)
        {
            return ContractOperationResult<ContractDto>.Failure(ContractOperationError.InvalidStatus);
        }

        if (contract.Customer.Status != CustomerStatus.Active)
        {
            return ContractOperationResult<ContractDto>.Failure(ContractOperationError.CustomerInactive);
        }

        if (contract.StartDate is null || contract.EndDate is null || contract.StartDate > contract.EndDate)
        {
            return ContractOperationResult<ContractDto>.Failure(ContractOperationError.InvalidDateRange);
        }

        if (contract.Contents.Count == 0)
        {
            return ContractOperationResult<ContractDto>.Failure(ContractOperationError.MissingRequiredContent);
        }

        contract.Status = ContractStatus.PendingApproval;
        await _db.SaveChangesAsync(ct);
        return ContractOperationResult<ContractDto>.Success(ToDto(contract));
    }

    private async Task<ContractOperationError?> ValidateReferencesAndFieldsAsync(
        int customerId,
        string contractNo,
        DateOnly? startDate,
        DateOnly? endDate,
        int? excludingContractId,
        CancellationToken ct)
    {
        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId, ct);
        if (customer is null)
        {
            return ContractOperationError.CustomerNotFound;
        }

        if (customer.Status != CustomerStatus.Active)
        {
            return ContractOperationError.CustomerInactive;
        }

        if (startDate is not null && endDate is not null && startDate > endDate)
        {
            return ContractOperationError.InvalidDateRange;
        }

        var duplicate = await _db.Contracts.AnyAsync(
            c => c.ContractNo == contractNo.Trim() && c.Id != excludingContractId,
            ct);
        return duplicate ? ContractOperationError.DuplicateContractNo : null;
    }

    private static bool IsEditable(ContractStatus status) =>
        status is ContractStatus.Draft or ContractStatus.RevisionRequested;

    private static ContractDto ToDto(Contract contract) => new(
        contract.Id,
        contract.ContractNo,
        contract.CustomerId,
        contract.Customer.Name,
        contract.StartDate,
        contract.EndDate,
        contract.Status,
        contract.Contents.Select(c => new ContractContentDto(c.Id, c.Name)).ToList());
}