namespace SmartInstaller.Services.Applications.DTOs;

public sealed record ApplicationVersionDto(
    Guid PublicId,
    string Version,
    DateTime? ReleaseDate,
    bool IsLatest);
