namespace SmartInstaller.Services.Applications.DTOs;

public sealed record UpdateApplicationVersionRequest(
    string Version,
    DateTime? ReleaseDate,
    bool IsLatest);