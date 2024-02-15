using System.Diagnostics;

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