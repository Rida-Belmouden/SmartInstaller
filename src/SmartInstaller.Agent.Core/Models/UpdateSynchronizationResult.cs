namespace SmartInstaller.Agent.Core.Models;

public sealed record UpdateSynchronizationResult(
    IReadOnlyList<UpdateCheckItem> Items,
    int InstalledApplicationCount,
    int MatchedApplicationCount)
{
    public int UpdateCount => Items.Count(item => item.UpdateAvailable);
}
