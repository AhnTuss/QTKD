namespace QTKD.Api.DTOs;

public sealed record PriceItemDto(int PriceListId, string ServiceCode, decimal UnitPrice);