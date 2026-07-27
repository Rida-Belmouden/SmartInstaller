namespace SmartInstaller.Services.Applications.DTOs;

public sealed record ApplicationDetailsDto(
    Guid PublicId,
    string Name,
    string Slug,
    string? Description,
    string? WebsiteUrl,
    string? IconUrl,
    bool IsFeatured,
    string Category,
    string Publisher,
    string Platform,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<ApplicationVersionDto> Versions);