namespace QTKD.Api.Entities;

public enum CustomerStatus
{
    Active = 1,
    Inactive = 2
}

/// <summary>Khách hàng của công ty (bên ký hợp đồng).</summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    public List<Contract> Contracts { get; set; } = new();
}
