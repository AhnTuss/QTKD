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

    public async Task<CustomerDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Customers
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto(c.Id, c.Name, c.Status))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = new Entities.Customer
        {
            Name = request.Name.Trim(),
            Status = request.Status
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        return new CustomerDto(customer.Id, customer.Name, customer.Status);
    }

    public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (customer is null)
        {
            return null;
        }

        customer.Name = request.Name.Trim();
        customer.Status = request.Status;
        await _db.SaveChangesAsync(ct);

        return new CustomerDto(customer.Id, customer.Name, customer.Status);
    }
}
