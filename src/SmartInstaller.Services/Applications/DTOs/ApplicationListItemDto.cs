namespace SmartInstaller.Services.Applications.DTOs;

public sealed record ApplicationListItemDto(
    Guid PublicId,
    string Name,
    string Slug,
    string? Description,
    string? IconUrl,
    string Category,
    string Publisher,
    string Platform,
    string? LatestVersion,
    bool IsFeatured);