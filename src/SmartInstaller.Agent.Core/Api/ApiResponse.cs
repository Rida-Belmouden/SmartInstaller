namespace SmartInstaller.Agent.Core.Api;

internal sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
}
