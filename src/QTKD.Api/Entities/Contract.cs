namespace QTKD.Api.Entities;


public enum ContractStatus
{
    Draft = 1,
    PendingApproval = 2,
    RevisionRequested = 3,
    Approved = 4
}

public class Contract
{
    public int Id { get; set; }
    public string ContractNo { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    // Nullable có chủ đích: hợp đồng Draft được phép chưa đủ thông tin.
    // Quy tắc CTR-02 (tuần 06) sẽ chặn việc gửi trình duyệt khi còn thiếu.
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public ContractStatus Status { get; set; } = ContractStatus.Draft;
}
