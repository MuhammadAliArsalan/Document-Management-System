using Microsoft.AspNetCore.Mvc;

namespace DocumentSearch.Helpers;

public enum ServiceResultType
{
    Success,
    Created,
    ValidationError,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    UnexpectedError
}

public sealed class ServiceResult<T>
{
    private ServiceResult(
        ServiceResultType resultType,
        string message,
        T? data = default,
        IReadOnlyCollection<string>? errors = null)
    {
        ResultType = resultType;
        Message = message;
        Data = data;
        Errors = errors;
    }

    public bool Succeeded =>
        ResultType is ServiceResultType.Success or ServiceResultType.Created;

    public ServiceResultType ResultType { get; }
    public string Message { get; }
    public T? Data { get; }
    public IReadOnlyCollection<string>? Errors { get; }

    public static ServiceResult<T> Success(T data, string message) =>
        new(ServiceResultType.Success, message, data);

    public static ServiceResult<T> Created(T data, string message) =>
        new(ServiceResultType.Created, message, data);

    public static ServiceResult<T> ValidationError(
        string message,
        IReadOnlyCollection<string>? errors = null) =>
        new(ServiceResultType.ValidationError, message, errors: errors);

    public static ServiceResult<T> Unauthorized(string message) =>
        new(ServiceResultType.Unauthorized, message);

    public static ServiceResult<T> Forbidden(string message) =>
        new(ServiceResultType.Forbidden, message);

    public static ServiceResult<T> NotFound(string message) =>
        new(ServiceResultType.NotFound, message);

    public static ServiceResult<T> Conflict(string message) =>
        new(ServiceResultType.Conflict, message);

    public static ServiceResult<T> UnexpectedError(string message) =>
        new(ServiceResultType.UnexpectedError, message);
}

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyCollection<string>? Errors { get; init; }
}

public static class ApiHelper
{
    public static IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        var statusCode = result.ResultType switch
        {
            ServiceResultType.Success => StatusCodes.Status200OK,
            ServiceResultType.Created => StatusCodes.Status201Created,
            ServiceResultType.ValidationError => StatusCodes.Status400BadRequest,
            ServiceResultType.Unauthorized => StatusCodes.Status401Unauthorized,
            ServiceResultType.Forbidden => StatusCodes.Status403Forbidden,
            ServiceResultType.NotFound => StatusCodes.Status404NotFound,
            ServiceResultType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var response = new ApiResponse<T>
        {
            Success = result.Succeeded,
            Message = result.Message,
            Data = result.Data,
            Errors = result.Errors
        };

        return new ObjectResult(response)
        {
            StatusCode = statusCode
        };
    }
}
