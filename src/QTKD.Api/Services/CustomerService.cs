using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly QtkdDbContext _db;

    public CustomerService(QtkdDbContext db)
    {
        _db = db;
    }

    public async Task<List<CustomerDto>> GetAllAsync(CancellationToken ct = default)
    {
        // AsNoTracking: chỉ đọc, không cần EF theo dõi thay đổi.
        return await _db.Customers
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .Select(c => new CustomerDto(c.Id, c.Name, c.Status))
            .ToListAsync(ct);
    }
}
