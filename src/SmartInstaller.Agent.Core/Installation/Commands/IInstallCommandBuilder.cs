using SmartInstaller.Agent.Core.Installation.Models;

namespace SmartInstaller.Agent.Core.Installation.Commands;

public interface IInstallCommandBuilder
{
    InstallCommand Build(InstallRequest request);
}
