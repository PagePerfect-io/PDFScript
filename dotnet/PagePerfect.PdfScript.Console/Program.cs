
using System.Net;

namespace PagePerfect.PdfScript.Console;

/// <summary>
/// The Programm class contains the entry point of the application.
/// </summary>
class Program
{
    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    static async Task Main(string[] args)
    {
        try
        {
            System.Console.WriteLine("PDFScript CLI");
            System.Console.WriteLine("Usage: pdfs run [arguments] <input-file> [output-file]");
            System.Console.WriteLine("       pdfs watch [arguments] [input-file]");
            System.Console.WriteLine();
            System.Console.WriteLine("Runs a PDFScript file or watches for changes and re-runs the script.");

            var config = Configuration.Parse(args);

            switch (config.Command)
            {
                case "run":
                    await Run(config);
                    break;

                case "watch":
                    Watch(config);
                    break;

                default:
                    throw new ArgumentException($"Unknown command: {config.Command}");
            }
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(ex.Message);
        }
    }
    #endregion



    // Private implementation
    // ======================
    #region Privagte implementation
    /// <summary>
    /// Runs the PDFScript file. This method is called when the user provides the "run" command,
    /// or when the user provides the "watch" command and a file-change is detected.
    /// </summary>
    /// <param name="config">The configuration.</param>
    private static async Task Run(Configuration config)
    {
        try
        {
            var outputFile = config.OutputFile ?? Path.ChangeExtension(config.InputFile, ".pdf");
            var doc = new Document(File.OpenRead(config.InputFile!));
            await doc.SaveAs(outputFile!);
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// Watches for changes in the input file or directory.
    /// This method is called when the user provides the "watch" command.
    /// </summary>
    /// <param name="config">The configuration.</param>
    private static void Watch(Configuration config)
    {
        // If the input is a directory, then we watch the directory.
        // If the input is a file, then we watch the file.
        // If not input is provided, then we watch the current directory.        
        if (config.InputFile == null)
        {
            WatchDirectory(config, Directory.GetCurrentDirectory());
        }
        else
        {
            if (Directory.Exists(config.InputFile)) { WatchDirectory(config, config.InputFile); }
            else WatchFile(config);
        }
    }

    /// <summary>
    /// Watches a directory for changes.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="directory">The directory.</param>
    private static void WatchDirectory(Configuration config, string directory)
    {
        var dir = Path.GetFullPath(directory);
        var watcher = new FileSystemWatcher(dir)
        {
            Filter = "*.pdfs"
        };
        watcher.Changed += async (sender, e) =>
        {
            System.Console.WriteLine($"File {e.FullPath} has changed. Re-running...");
            config.InputFile = e.FullPath;
            config.OutputFile = Path.ChangeExtension(e.FullPath, ".pdf");
            await Run(config);
        };
        watcher.Error += (sender, e) =>
        {
            System.Console.Error.WriteLine($"Error: {e.GetException()}");
        };

        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = true;

        System.Console.WriteLine("Watching for changes...");
        System.Console.WriteLine($"Watching {dir} ...");
        System.Console.ReadLine();
    }

    /// <summary>
    /// Watches a file for changes.
    /// </summary>
    /// <param name="config">The configuration.</param>
    private static void WatchFile(Configuration config)
    {
        config.OutputFile ??= Path.ChangeExtension(config.InputFile, ".pdf");

        var full = Path.GetFullPath(config.InputFile!);
        var dir = Path.GetDirectoryName(full)!;
        var watcher = new FileSystemWatcher(dir)
        {
            Filter = $"*{Path.GetExtension(config.InputFile)!}"// Path.GetFileName(config.InputFile)!
        };
        watcher.Changed += async (sender, e) =>
        {
            if (e.FullPath != full) { return; }

            System.Console.WriteLine($"File {e.FullPath} has changed. Re-running...");
            await Run(config);
        };
        watcher.Error += (sender, e) =>
        {
            System.Console.Error.WriteLine($"Error: {e.GetException()}");
        };
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = false;

        System.Console.WriteLine($"Watching for changes to {Path.GetFileName(config.InputFile)} ...");
        System.Console.WriteLine($"Watching {dir} ...");
        System.Console.ReadLine();
        System.Console.WriteLine($"Exiting...");
    }
    #endregion
}
