using SmartInstaller.Agent.Core.Models;

namespace SmartInstaller.Agent.Core.Services;

public interface IInstallerFileNameResolver
{
    string Resolve(
        InstallerManifest manifest,
        Uri downloadUri);
}
