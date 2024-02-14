
namespace PagePerfect.PdfScript.Console;

class Program
{
    static async Task Main(string[] args)
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

    private static async Task Run(Configuration config)
    {
        var outputFile = config.OutputFile ?? Path.ChangeExtension(config.InputFile, ".pdf");
        var doc = new Document(File.OpenRead(config.InputFile!));
        await doc.SaveAs(outputFile!);
    }

    private static void Watch(Configuration config)
    {
        var watcher = new FileSystemWatcher(Path.GetDirectoryName(config.InputFile)!)
        {
            Filter = Path.GetFileName(config.InputFile)!
        };
        watcher.Changed += async (sender, e) => await Run(config);
        watcher.EnableRaisingEvents = true;
        System.Console.WriteLine("Watching for changes...");
        System.Console.ReadLine();
    }
}
