using QTKD.Api.DTOs;

namespace QTKD.Api.Services;

public enum ContractOperationError
{
    NotFound,
    InvalidStatus,
    CustomerNotFound,
    CustomerInactive,
    DuplicateContractNo,
    InvalidDateRange,
    MissingRequiredContent
}

public sealed record ContractOperationResult<T>(T? Value, ContractOperationError? Error)
{
    public static ContractOperationResult<T> Success(T value) => new(value, null);
    public static ContractOperationResult<T> Failure(ContractOperationError error) => new(default, error);
}