using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IContractService
{
    Task<List<ContractDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Trả về null nếu không tìm thấy hợp đồng.</summary>
    Task<ContractDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<ContractOperationResult<ContractDto>> CreateAsync(CreateContractRequest request, CancellationToken ct = default);
    Task<ContractOperationResult<ContractDto>> UpdateAsync(int id, UpdateContractRequest request, CancellationToken ct = default);
    Task<ContractOperationResult<ContractContentDto>> AddContentAsync(int id, CreateContractContentRequest request, CancellationToken ct = default);
    Task<ContractContentDto?> GetContentByIdAsync(int id, int contentId, CancellationToken ct = default);
    Task<ContractOperationResult<ContractDto>> SubmitAsync(int id, CancellationToken ct = default);
}
