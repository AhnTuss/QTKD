using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public interface IPriceListService
{
    Task<List<PriceListDto>> GetAllAsync(CancellationToken ct = default);
    Task<PriceListDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PriceOperationResult<PriceListDto>> CreateAsync(CreatePriceListRequest request, CancellationToken ct = default);
    Task<List<PriceItemDto>?> GetItemsAsync(int priceListId, CancellationToken ct = default);
    Task<PriceOperationResult<PriceItemDto>> AddItemAsync(int priceListId, CreatePriceItemRequest request, CancellationToken ct = default);
}