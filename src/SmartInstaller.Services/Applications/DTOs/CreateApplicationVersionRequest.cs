namespace SmartInstaller.Services.Applications.DTOs;

public sealed record CreateApplicationVersionRequest(
    string Version,
    DateTime? ReleaseDate,
    bool IsLatest = false);