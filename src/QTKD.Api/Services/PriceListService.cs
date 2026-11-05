using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.DTOs;
using QTKD.Api.Entities;

namespace QTKD.Api.Services;

public class PriceListService : IPriceListService
{
    private readonly QtkdDbContext _db;

    public PriceListService(QtkdDbContext db)
    {
        _db = db;
    }

    public async Task<List<PriceListDto>> GetAllAsync(CancellationToken ct = default)
    {
        var priceLists = await _db.PriceLists
            .AsNoTracking()
            .Include(p => p.Items)
            .OrderBy(p => p.Id)
            .ToListAsync(ct);

        return priceLists.Select(ToDto).ToList();
    }

    public async Task<PriceListDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var priceList = await _db.PriceLists
            .AsNoTracking()
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return priceList is null ? null : ToDto(priceList);
    }

    public async Task<PriceOperationResult<PriceListDto>> CreateAsync(
        CreatePriceListRequest request,
        CancellationToken ct = default)
    {
        var priceList = new PriceList
        {
            Name = request.Name.Trim(),
            EffectiveDate = request.EffectiveDate!.Value
        };

        _db.PriceLists.Add(priceList);
        await _db.SaveChangesAsync(ct);
        return PriceOperationResult<PriceListDto>.Success(ToDto(priceList));
    }

    public async Task<List<PriceItemDto>?> GetItemsAsync(int priceListId, CancellationToken ct = default)
    {
        var exists = await _db.PriceLists.AnyAsync(p => p.Id == priceListId, ct);
        if (!exists)
        {
            return null;
        }

        return await _db.PriceItems
            .AsNoTracking()
            .Where(item => item.PriceListId == priceListId)
            .OrderBy(item => item.ServiceCode)
            .Select(item => new PriceItemDto(item.PriceListId, item.ServiceCode, item.UnitPrice))
            .ToListAsync(ct);
    }

    public async Task<PriceOperationResult<PriceItemDto>> AddItemAsync(
        int priceListId,
        CreatePriceItemRequest request,
        CancellationToken ct = default)
    {
        var priceListExists = await _db.PriceLists.AnyAsync(p => p.Id == priceListId, ct);
        if (!priceListExists)
        {
            return PriceOperationResult<PriceItemDto>.Failure(PriceOperationError.NotFound);
        }

        var serviceCode = request.ServiceCode.Trim();
        var alreadyExists = await _db.PriceItems.AnyAsync(
            item => item.PriceListId == priceListId && item.ServiceCode == serviceCode,
            ct);
        if (alreadyExists)
        {
            return PriceOperationResult<PriceItemDto>.Failure(PriceOperationError.DuplicateServiceCode);
        }

        var item = new PriceItem
        {
            PriceListId = priceListId,
            ServiceCode = serviceCode,
            UnitPrice = request.UnitPrice
        };

        _db.PriceItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return PriceOperationResult<PriceItemDto>.Success(
            new PriceItemDto(item.PriceListId, item.ServiceCode, item.UnitPrice));
    }

    private static PriceListDto ToDto(PriceList priceList) => new(
        priceList.Id,
        priceList.Name,
        priceList.EffectiveDate,
        priceList.Items
            .OrderBy(item => item.ServiceCode)
            .Select(item => new PriceItemDto(item.PriceListId, item.ServiceCode, item.UnitPrice))
            .ToList());
}