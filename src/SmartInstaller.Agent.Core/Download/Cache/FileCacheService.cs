namespace SmartInstaller.Agent.Core.Download.Cache;

public sealed class FileCacheService(
    ICachePathProvider pathProvider)
    : IFileCacheService
{
    public string GetFinalPath(string fileName)
    {
        return pathProvider.GetFinalPath(fileName);
    }

    public string GetTemporaryPath(string fileName)
    {
        return pathProvider.GetTemporaryPath(fileName);
    }

    public bool IsReusable(
        string fileName,
        long? expectedFileSizeBytes)
    {
        var finalPath = GetFinalPath(fileName);

        if (!File.Exists(finalPath))
        {
            return false;
        }

        return !expectedFileSizeBytes.HasValue ||
               new FileInfo(finalPath).Length ==
               expectedFileSizeBytes.Value;
    }

    public void EnsureCacheDirectoryExists()
    {
        pathProvider.EnsureCacheDirectoryExists();
    }

    public void DeleteTemporaryFile(string fileName)
    {
        DeleteIfExists(GetTemporaryPath(fileName));
    }

    public void DeleteFinalFile(string fileName)
    {
        DeleteIfExists(GetFinalPath(fileName));
    }

    public void PromoteTemporaryFile(
        string fileName,
        bool overwrite)
    {
        File.Move(
            GetTemporaryPath(fileName),
            GetFinalPath(fileName),
            overwrite);
    }

    public long GetTemporaryFileSize(string fileName)
    {
        return new FileInfo(
            GetTemporaryPath(fileName)).Length;
    }

    private static void DeleteIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
