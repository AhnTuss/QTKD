using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

public sealed record UpdateContractRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Contract number cannot be empty or whitespace.")]
    public string ContractNo { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CustomerId { get; init; }

    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}