namespace QTKD.Api.Services;

public enum PriceOperationError
{
    NotFound,
    DuplicateServiceCode
}

public sealed record PriceOperationResult<T>(T? Value, PriceOperationError? Error)
{
    public static PriceOperationResult<T> Success(T value) => new(value, null);
    public static PriceOperationResult<T> Failure(PriceOperationError error) => new(default, error);
}