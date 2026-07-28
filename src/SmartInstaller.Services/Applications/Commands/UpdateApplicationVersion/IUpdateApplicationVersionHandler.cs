namespace SmartInstaller.Services.Applications
    .Commands.UpdateApplicationVersion;

public interface IUpdateApplicationVersionHandler
{
    Task<UpdateApplicationVersionResult> HandleAsync(
        UpdateApplicationVersionCommand command,
        CancellationToken cancellationToken = default);
}