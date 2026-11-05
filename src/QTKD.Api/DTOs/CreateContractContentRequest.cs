using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

public sealed record CreateContractContentRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Content name cannot be empty or whitespace.")]
    public string Name { get; init; } = string.Empty;
}