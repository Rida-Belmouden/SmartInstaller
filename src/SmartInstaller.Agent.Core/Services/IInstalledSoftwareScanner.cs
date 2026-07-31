using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public interface IInstalledSoftwareScanner
{
    Task<IReadOnlyList<InstalledApplication>> ScanAsync(
        CancellationToken cancellationToken = default);
}
