using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync(CancellationToken ct = default);
    Task<CustomerDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken ct = default);
}
