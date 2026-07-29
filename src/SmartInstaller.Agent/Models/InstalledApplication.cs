namespace SmartInstaller.Agent.Models;

public sealed record InstalledApplication(
    string Name,
    string? Version,
    string? Publisher,
    string? InstallLocation,
    string? UninstallString,
    DateOnly? InstallDate,
    string NormalizedName,
    string RegistryPath,
    string RegistryHive,
    string RegistryView);
