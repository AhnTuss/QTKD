using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

public sealed record CreatePriceListRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    public string Name { get; init; } = string.Empty;

    [Required]
    public DateOnly? EffectiveDate { get; init; }
}