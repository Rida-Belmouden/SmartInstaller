namespace SmartInstaller.Agent.Core.Download.Cache;

public interface ICachePathProvider
{
    string CacheDirectory { get; }

    string GetFinalPath(string fileName);

    string GetTemporaryPath(string fileName);

    void EnsureCacheDirectoryExists();
}
