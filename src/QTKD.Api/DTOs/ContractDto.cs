using QTKD.Api.Entities;

namespace QTKD.Api.DTOs;

public record ContractDto(
    int Id,
    string ContractNo,
    int CustomerId,
    string CustomerName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    ContractStatus Status);
