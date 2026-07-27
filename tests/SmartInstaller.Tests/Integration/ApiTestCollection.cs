namespace SmartInstaller.Tests.Integration;

[CollectionDefinition(Name)]
public sealed class ApiTestCollection
    : ICollectionFixture<SmartInstallerApiFactory>
{
    public const string Name = "SmartInstaller API";
}