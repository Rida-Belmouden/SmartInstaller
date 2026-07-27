namespace SmartInstaller.Services.Catalog.DTOs;

public sealed record CatalogItemDto(
    Guid PublicId,
    string Name,
    string Slug,
    string? Description,
    int ApplicationCount);