namespace QTKD.Api.Entities;

public class ContractContent
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
}