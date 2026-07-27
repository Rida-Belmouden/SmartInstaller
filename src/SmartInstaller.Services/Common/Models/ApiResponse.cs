namespace SmartInstaller.Services.Common.Models;

public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message = null)
{
    public static ApiResponse<T> Ok(
        T data,
        string? message = null)
    {
        return new ApiResponse<T>(
            Success: true,
            Data: data,
            Message: message);
    }

    public static ApiResponse<T> Failure(string message)
    {
        return new ApiResponse<T>(
            Success: false,
            Data: default,
            Message: message);
    }
}