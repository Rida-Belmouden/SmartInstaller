namespace SmartInstaller.Agent.Core.Download.Cache;

public interface IFileCacheService
{
    string GetFinalPath(string fileName);

    string GetTemporaryPath(string fileName);

    bool IsReusable(
        string fileName,
        long? expectedFileSizeBytes);

    void EnsureCacheDirectoryExists();

    void DeleteTemporaryFile(string fileName);

    void DeleteFinalFile(string fileName);

    void PromoteTemporaryFile(
        string fileName,
        bool overwrite);

    long GetTemporaryFileSize(string fileName);
}
