using System.ComponentModel.DataAnnotations;

namespace QTKD.Api.DTOs;

public sealed record CreatePriceItemRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Service code cannot be empty or whitespace.")]
    public string ServiceCode { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal UnitPrice { get; init; }
}