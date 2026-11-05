namespace QTKD.Api.DTOs;

public sealed record PriceListDto(int Id, string Name, DateOnly EffectiveDate, List<PriceItemDto> Items);