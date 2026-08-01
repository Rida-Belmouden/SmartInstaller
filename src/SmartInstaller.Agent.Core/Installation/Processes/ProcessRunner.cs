using System.Diagnostics;

namespace SmartInstaller.Agent.Core.Installation.Processes;

public sealed class ProcessRunner : IProcessRunner
{
    public async Task<ProcessExecutionResult> RunAsync(
        ProcessExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var process = new Process
            {
                StartInfo = CreateStartInfo(request),
                EnableRaisingEvents = true
            };

            if (!process.Start())
            {
                return new ProcessExecutionResult(
                    false,
                    false,
                    false,
                    null,
                    stopwatch.Elapsed,
                    "The installer process could not be started.");
            }

            using var timeoutSource =
                new CancellationTokenSource(request.Timeout);

            using var linkedSource =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    timeoutSource.Token);

            try
            {
                await process.WaitForExitAsync(
                    linkedSource.Token);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);

                return new ProcessExecutionResult(
                    true,
                    cancellationToken.IsCancellationRequested,
                    timeoutSource.IsCancellationRequested &&
                    !cancellationToken.IsCancellationRequested,
                    null,
                    stopwatch.Elapsed,
                    cancellationToken.IsCancellationRequested
                        ? "The installation was cancelled."
                        : "The installation timed out.");
            }

            return new ProcessExecutionResult(
                true,
                false,
                false,
                process.ExitCode,
                stopwatch.Elapsed,
                null);
        }
        catch (Exception exception)
            when (exception is
                InvalidOperationException or
                System.ComponentModel.Win32Exception)
        {
            return new ProcessExecutionResult(
                false,
                false,
                false,
                null,
                stopwatch.Elapsed,
                exception.Message);
        }
    }

    private static ProcessStartInfo CreateStartInfo(
        ProcessExecutionRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.FileName,
            Arguments = request.Arguments,
            UseShellExecute =
                request.RequiresAdministrator,
            CreateNoWindow =
                request.CreateNoWindow &&
                !request.RequiresAdministrator
        };

        if (request.RequiresAdministrator)
        {
            startInfo.Verb = "runas";
            startInfo.WindowStyle =
                ProcessWindowStyle.Hidden;
        }

        return startInfo;
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(
                    entireProcessTree: true);
            }
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
