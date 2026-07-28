namespace SmartInstaller.Services.Applications
    .Commands.SetLatestApplicationVersion;

public interface ISetLatestApplicationVersionHandler
{
    Task<SetLatestApplicationVersionResult> HandleAsync(
        SetLatestApplicationVersionCommand command,
        CancellationToken cancellationToken = default);
}