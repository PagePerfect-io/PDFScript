using System.Diagnostics;
using System.Text;
using NSubstitute.Routing.AutoValues;

namespace PagePerfect.PdfScript.Tests.Console;

/// <summary>
/// The IntegrationTests class contains tests for the CLI process.
/// </summary>
public class IntegrationTests
{
    // Constants
    // =========
    #region Constants
    private const string consolePath = "../../../../PagePerfect.PdfScript.Console/bin/Debug/net8.0";
    private const string consoleExecutable = "PagePerfect.PdfScript.Console";
    #endregion



    // Test methods
    // ============
    #region Test methods
    /// <summary>
    /// The CLI process should process a .pdfs file.
    /// </summary>
    [Fact]
    public void ShouldProcessPdfsFile()
    {
        var consoleApp = StartApplication("run", "Console/Data/input.pdfs", "output.pdf");
        var err = consoleApp.StandardError.ReadToEnd();
        var output = consoleApp.StandardOutput.ReadToEnd();
        consoleApp.WaitForExit();

        Assert.Empty(err);
        Assert.True(File.Exists("output.pdf"));
    }

    /// <summary>
    /// The CLI process should watch a single .pdfs file.
    /// </summary>
    [Fact]
    public void ShouldWatchPdfsFile()
    {
        var consoleApp = StartApplication("watch", "Console/Data/input.pdfs", "output.pdf");
        var err = new StringBuilder();
        var output = new StringBuilder();
        consoleApp.OutputDataReceived += (sender, args) =>
        {
            output.AppendLine(args.Data);
        };
        consoleApp.ErrorDataReceived += (sender, args) =>
        {
            err.AppendLine(args.Data);
        };
        consoleApp.BeginOutputReadLine();
        consoleApp.BeginErrorReadLine();

        //        File.SetLastWriteTimeUtc("Console/Data/input.pdfs", DateTime.UtcNow);
        // Give the process time to react to the file change
        Thread.Sleep(200);
        File.AppendAllText("Console/Data/input.pdfs", " ");
        Thread.Sleep(200);

        consoleApp.Kill();
        consoleApp.WaitForExit();

        Assert.Empty(err.ToString().Trim());
        Assert.True(File.Exists("output.pdf"));
        Assert.Contains("has changed. Re-running...", output.ToString());
    }

    /// <summary>
    /// The CLI process should watch a directory for changes to .pdfs files.
    /// </summary>
    [Fact]
    public void ShouldWatchDirectory()
    {
        var consoleApp = StartApplication("watch", "Console/Data");
        var err = new StringBuilder();
        var output = new StringBuilder();
        consoleApp.OutputDataReceived += (sender, args) =>
        {
            output.AppendLine(args.Data);
        };
        consoleApp.ErrorDataReceived += (sender, args) =>
        {
            err.AppendLine(args.Data);
        };
        consoleApp.BeginOutputReadLine();
        consoleApp.BeginErrorReadLine();

        //        File.SetLastWriteTimeUtc("Console/Data/input.pdfs", DateTime.UtcNow);
        // Give the process time to react to the file change
        Thread.Sleep(200);
        File.AppendAllText("Console/Data/input.pdfs", " ");
        Thread.Sleep(200);

        consoleApp.Kill();
        consoleApp.WaitForExit();

        Assert.Empty(err.ToString().Trim());
        Assert.True(File.Exists("output.pdf"));
        Assert.Contains("input.pdfs has changed. Re-running...", output.ToString());
    }

    /// <summary>
    /// The CLI process should watch a target directory's subdirecties for changes to .pdfs files.
    /// </summary>
    [Fact]
    public void ShouldWatchSubDirectories()
    {
        var consoleApp = StartApplication("watch", "Console");
        var err = new StringBuilder();
        var output = new StringBuilder();
        consoleApp.OutputDataReceived += (sender, args) =>
        {
            output.AppendLine(args.Data);
        };
        consoleApp.ErrorDataReceived += (sender, args) =>
        {
            err.AppendLine(args.Data);
        };
        consoleApp.BeginOutputReadLine();
        consoleApp.BeginErrorReadLine();

        //        File.SetLastWriteTimeUtc("Console/Data/input.pdfs", DateTime.UtcNow);
        // Give the process time to react to the file change
        Thread.Sleep(200);
        File.AppendAllText("Console/Data/input.pdfs", " ");
        Thread.Sleep(1000);

        consoleApp.Kill();
        consoleApp.WaitForExit();

        Assert.Empty(err.ToString().Trim());
        Assert.True(File.Exists("output.pdf"));
        Assert.Contains("input.pdfs has changed. Re-running...", output.ToString());
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Starts the CLI application with the specified arguments.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns>A reference to the running process.</returns>
    private static Process StartApplication(params string[] args)
    {
        var info = new ProcessStartInfo
        {
            FileName = Path.Join(consolePath, consoleExecutable),
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var arg in args)
        {
            info.ArgumentList.Add(arg);
        }

        return Process.Start(info)!;
    }
    #endregion
}