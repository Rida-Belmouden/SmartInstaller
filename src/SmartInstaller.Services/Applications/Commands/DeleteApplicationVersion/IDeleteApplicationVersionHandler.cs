namespace SmartInstaller.Services.Applications
    .Commands.DeleteApplicationVersion;

public interface IDeleteApplicationVersionHandler
{
    Task<DeleteApplicationVersionResult> HandleAsync(
        DeleteApplicationVersionCommand command,
        CancellationToken cancellationToken = default);
}