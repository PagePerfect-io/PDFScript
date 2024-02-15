using PagePerfect.PdfScript.Console;

namespace PagePerfect.PdfScript.Tests.Console;

/// <summary>
/// The ConfigurationTests class contains tests for the Configuration class.
/// </summary> <summary>
/// 
/// </summary>
public class ConfigurationTests
{
    // Test methods
    // ============
    #region Test methods
    /// <summary>
    /// The Configuration class should throw an ArgumentException when the command is invalid.
    /// </summary>
    [Fact]
    public void ShouldThrowWhenCommandInvalid()
    {
        Assert.Throws<ArgumentException>(() => Configuration.Parse(["invalid"]));
    }

    /// <summary>
    /// The Configuration class should parse a 'run' command.
    /// </summary>
    [Fact]
    public void ShouldParseRunCommand()
    {
        // Input and explicit output.
        Configuration config = Configuration.Parse(["run", "input.pdfs", "output.pdf"]);
        Assert.Equal("run", config.Command);
        Assert.Equal("input.pdfs", config.InputFile);
        Assert.Equal("output.pdf", config.OutputFile);

        // Input and implicit output.
        config = Configuration.Parse(["run", "helloworld.pdfs"]);
        Assert.Equal("run", config.Command);
        Assert.Equal("helloworld.pdfs", config.InputFile);
        Assert.Equal("helloworld.pdf", config.OutputFile);
    }

    /// <summary>
    /// The Configuration class should throw an ArgumentException when 
    /// the input path is missing in a 'run' command.
    /// </summary>
    [Fact]
    public void ShouldThrowArgumentExceptionWhenInputPathMissingInRunCommand()
    {
        Assert.Throws<ArgumentException>(() => Configuration.Parse(["run"]));
    }
    #endregion
}