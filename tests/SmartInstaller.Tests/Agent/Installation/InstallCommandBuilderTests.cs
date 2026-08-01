using SmartInstaller.Agent.Core.Installation.Commands;
using SmartInstaller.Agent.Core.Installation.Models;

namespace SmartInstaller.Tests.Agent.Installation;

public sealed class InstallCommandBuilderTests
{
    private readonly InstallCommandBuilder _builder = new();

    [Fact]
    public void Build_Exe_UsesInstallerPathDirectly()
    {
        var command = _builder.Build(
            new InstallRequest(
                @"C:\Cache\setup.exe",
                InstallerKind.Exe,
                "/S",
                true));

        Assert.Equal(
            @"C:\Cache\setup.exe",
            command.FileName);

        Assert.Equal("/S", command.Arguments);
        Assert.True(command.RequiresAdministrator);
    }

    [Fact]
    public void Build_Msi_UsesMsiexecAndDefaultSilentArguments()
    {
        var command = _builder.Build(
            new InstallRequest(
                @"C:\Cache\setup.msi",
                InstallerKind.Msi));

        Assert.Equal(
            "msiexec.exe",
            command.FileName);

        Assert.Equal(
            @"/i ""C:\Cache\setup.msi"" /qn /norestart",
            command.Arguments);
    }

    [Fact]
    public void Build_Msi_UsesCustomArguments()
    {
        var command = _builder.Build(
            new InstallRequest(
                @"C:\Cache\setup.msi",
                InstallerKind.Msi,
                "/quiet REBOOT=ReallySuppress"));

        Assert.Equal(
            @"/i ""C:\Cache\setup.msi"" /quiet REBOOT=ReallySuppress",
            command.Arguments);
    }
}
