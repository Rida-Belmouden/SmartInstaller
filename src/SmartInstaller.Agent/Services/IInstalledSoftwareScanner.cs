using SmartInstaller.Agent.Models;

namespace SmartInstaller.Agent.Services;

public interface IInstalledSoftwareScanner
{
    Task<IReadOnlyList<InstalledApplication>> ScanAsync(
        CancellationToken cancellationToken = default);
}
