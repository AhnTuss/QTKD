using System.ComponentModel.DataAnnotations;
using QTKD.Api.Entities;

namespace QTKD.Api.DTOs;

public sealed record UpdateCustomerRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    public string Name { get; init; } = string.Empty;

    public CustomerStatus Status { get; init; } = CustomerStatus.Active;
}