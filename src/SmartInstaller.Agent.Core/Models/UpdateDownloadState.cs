namespace SmartInstaller.Agent.Core.Models;

public sealed record UpdateDownloadState(
    InstallerManifest Manifest,
    string FileName,
    string TemporaryPath,
    string FinalPath,
    bool FinalFileExists,
    long PartialBytes,
    long? ExpectedBytes)
{
    public bool HasPartialFile =>
        PartialBytes > 0 &&
        !FinalFileExists;

    public double Percentage =>
        ExpectedBytes is > 0
            ? Math.Min(
                100d,
                PartialBytes * 100d /
                ExpectedBytes.Value)
            : 0d;
}
