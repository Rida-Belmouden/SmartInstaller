namespace SmartInstaller.Agent.Core.Models;

public sealed record MatchedInstalledApplication(
    InstalledApplication InstalledApplication,
    AgentCatalogItem CatalogApplication);
