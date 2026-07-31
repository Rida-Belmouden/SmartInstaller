using System.Runtime.InteropServices;

namespace SmartInstaller.Agent.Core.Services;

public sealed class SystemArchitectureDetector
    : ISystemArchitectureDetector
{
    public string Detect() =>
        RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant()
        };
}
