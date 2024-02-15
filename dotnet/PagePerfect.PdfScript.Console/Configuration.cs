namespace PagePerfect.PdfScript.Console;

/// <summary>
/// The Configuration class encapsulates the configuration of CLI process.
/// </summary>
public class Configuration
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The command, e.g. "run" or "watch".
    /// </summary> 
    public string? Command { get; set; }

    /// <summary>
    /// The path to the input file.
    /// </summary>
    public string? InputFile { get; set; }

    /// <summary>
    /// The path to the output file.
    /// </summary>
    public string? OutputFile { get; set; }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Parses a list of arguments and returns a configuration object.
    /// </summary>
    /// <param name="args">The arguments provided by the user.</param>
    /// <returns>The configuration.</returns>
    /// <exception cref="ArgumentException">One or more arguments are invalid.</exception>
    public static Configuration Parse(string[] args)
    {
        string? command = null;
        string? inputFile = null;
        string? outputFile = null;

        var a = 0;
        var state = ConfigurationState.None;
        while (a < args.Length)
        {

            var arg = args[a++];
            switch (state)
            {
                case ConfigurationState.None:
                    // We are expecting a command
                    if (arg.StartsWith('-')) throw new ArgumentException($"Expected a command, but received {arg}");
                    command = ValidateCommand(arg);
                    state = ConfigurationState.Command;
                    break;

                case ConfigurationState.Command:
                    // If we receive an option then we parse the option.
                    // Otherwise, we assume that the argument is the input file.
                    // If we have already received an input file, then we assume that the argument is the output file.
                    // It is an error to receive an option after the input file.
                    if (arg.StartsWith('-'))
                    {
                        if (null != inputFile) throw new ArgumentException($"Cannot specify an option after the input file");

                        // state = ValidateOption();
                    }
                    else
                    {
                        // An input and output file are only valid if we have received a "run" command.
                        if ("run" != command) throw new ArgumentException($"Invalid argument {arg} for command {command}");

                        if (null == inputFile) inputFile = arg;
                        else if (null == outputFile) outputFile = arg;
                        else throw new ArgumentException($"Too many arguments");
                    }
                    break;
            }
        }

        if (null == command) throw new ArgumentException("No command specified");
        if ("run" == command && null == inputFile) throw new ArgumentException("No input file specified");

        return new Configuration
        {
            Command = command,
            InputFile = inputFile,
            OutputFile = outputFile
        };
    }
    #endregion


    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Validates the command argument. This method checks that the command is one
    /// of the recognised commands.
    /// </summary>
    /// <param name="arg">The argument.</param>
    /// <returns>The same argument.</returns>
    /// <exception cref="ArgumentException">The command is not recognised.</exception>
    private static string ValidateCommand(string arg)
    {
        if (arg == "run") return arg;
        if (arg == "watch") return arg;
        throw new ArgumentException($"Invalid command {arg}");
    }
    #endregion



    // Private types
    // =============
    #region ConfigurationState enumeration
    /// <summary>
    /// The ConfigurationState enumeration lists the possible states the configuration parser can be in.
    /// </summary>
    private enum ConfigurationState
    {
        None,
        Command,
    }
    #endregion
}