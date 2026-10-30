namespace QTKD.Api.Entities;

/// <summary>
/// Một dòng giá trong bảng giá. Khóa chính ghép (PriceListId, ServiceCode):
/// trong một bảng giá, mỗi mã dịch vụ chỉ có một đơn giá.
/// </summary>
public class PriceItem
{
    public int PriceListId { get; set; }
    public PriceList PriceList { get; set; } = null!;

    public string ServiceCode { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
