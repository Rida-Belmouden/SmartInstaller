namespace SmartInstaller.Services.Applications
    .Commands.CreateApplicationVersion;

public interface ICreateApplicationVersionHandler
{
    Task<CreateApplicationVersionResult> HandleAsync(
        CreateApplicationVersionCommand command,
        CancellationToken cancellationToken = default);
}