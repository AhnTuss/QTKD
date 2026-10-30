namespace QTKD.Api.Entities;

/// <summary>Bảng giá dịch vụ, có hiệu lực từ EffectiveDate.</summary>
public class PriceList
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }

    public List<PriceItem> Items { get; set; } = new();
}
