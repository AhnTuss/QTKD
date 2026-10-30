using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IContractService
{
    /// <summary>Trả về null nếu không tìm thấy hợp đồng.</summary>
    Task<ContractDto?> GetByIdAsync(int id, CancellationToken ct = default);
}
