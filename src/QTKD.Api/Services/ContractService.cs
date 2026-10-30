using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public class ContractService : IContractService
{
    private readonly QtkdDbContext _db;

    public ContractService(QtkdDbContext db)
    {
        _db = db;
    }

    public async Task<ContractDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Contracts
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new ContractDto(
                c.Id,
                c.ContractNo,
                c.CustomerId,
                c.Customer.Name,
                c.StartDate,
                c.EndDate,
                c.Status))
            .FirstOrDefaultAsync(ct);
    }
}
