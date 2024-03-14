

using PagePerfect.PdfScript.Processor.Text;
using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The TextUtilitiesTests class contains tests for the TextUtilities class.
/// </summary>
public class TextUtilitiesTests
{
    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// The WriteLines method should output a text block (BT..ET)
    /// when passing in at least one line.
    /// </summary>
    [Fact]
    public async Task ShouldOutputTextBlockInstructions()
    {
        using var output = new MemoryStream();

        var writer = new StreamWriter(output);
        var font = new TestFont("Helvetica");

        var lines = new[] { new Line(new PdfRectangle(10, 10, 100, 12), [
            new LineSpan(new PdfRectangle(10, 10, 100, 12), font, 12, "Hello, world") ]) };
        await writer.WriteLines(lines);

        // Check that we can read the document.
        var content = CS(output);
        Assert.Contains($"/{font.Identifier} 12 Tf", content);
        Assert.Contains("BT ", content);
        Assert.Contains(" ET", content);
    }

    /// <summary>
    /// The WriteLines method should output consecutive TJ instructions for consecutive spans.
    /// </summary>
    [Fact]
    public async Task ShouldOutputConsecutiveLineSpans()
    {
        using var output = new MemoryStream();

        var writer = new StreamWriter(output);
        var font = new TestFont("Helvetica");

        var lines = new[] { new Line(new PdfRectangle(10, 10, 200, 12), [
            new LineSpan(new PdfRectangle(10, 10, 100, 12), font, 12, "Hello, "),
            new LineSpan(new PdfRectangle(110, 10, 100, 12), font, 12, "world") ] ) };
        await writer.WriteLines(lines);

        // Get the content of the first page.
        var content = CS(output);

        // Check that there is only one Tf instruction, and two Tj instructions.
        var firstIndex = content.IndexOf($"/{font.Identifier} 12 Tf ");
        var lastIndex = content.LastIndexOf($"/{font.Identifier} 12 Tf ");
        Assert.Equal(firstIndex, lastIndex);
        Assert.Contains("(Hello, ) Tj ", content);
        Assert.Contains("(world) Tj ", content);
    }

    /// <summary>
    /// The WriteLines method should output text in WinAnsi encoding.
    /// </summary>
    [Fact]
    public async Task ShouldOutputWinAnsiEncodedText()
    {
        using var output = new MemoryStream();

        var writer = new StreamWriter(output);
        var font = new TestFont("Helvetica");

        // Text with some character > 127
        var lines = new[] { new Line(new PdfRectangle(10, 10, 200, 12), [
            new LineSpan(new PdfRectangle(10, 10, 100, 12), font, 12, "Hello, "),
            new LineSpan(new PdfRectangle(110, 10, 100, 12), font, 12, "£Money!") ] ) };
        await writer.WriteLines(lines);

        // Check that we can read the encoded text.
        var content = CS(output);
        Assert.Contains("(Hello, ) Tj ", content);
        Assert.Contains("(\\243Money!) Tj ", content);
    }

    /// <summary>
    /// The WriteLines method should output a positioned text (TJ) instruction
    /// when two or more line spans don't touch.
    /// </summary>
    [Fact]
    public async Task ShouldOutputPositionedTextWhenLineSpansDontTouch()
    {
        using var output = new MemoryStream();

        var writer = new StreamWriter(output);
        var font = new TestFont("Helvetica");

        var lines = new[] { new Line(new PdfRectangle(10, 10, 430, 12), [
            new LineSpan(new PdfRectangle(10, 10, 100, 12), font, 12, "The quick "),
            new LineSpan(new PdfRectangle(120, 10, 100, 12), font, 12, "brown fox "),
            new LineSpan(new PdfRectangle(230, 10, 100, 12), font, 12, "jumps over "),
            new LineSpan(new PdfRectangle(340, 10, 100, 12), font, 12, "the lazy dog.") ] ) };
        await writer.WriteLines(lines);

        // Get the content of the first page.
        var content = CS(output);

        // We expect only one Tf instruction, one Tj instruction, and three TJ instructions.
        var firstIndex = content.IndexOf($"/{font.Identifier} 12 Tf ");
        var lastIndex = content.LastIndexOf($"/{font.Identifier} 12 Tf ");
        Assert.Equal(firstIndex, lastIndex);

        Assert.Contains("(The quick ) Tj ", content);

        var gap = (int)(10 * 1000 / 12d);
        Assert.Contains($"[-{gap} (brown fox )] TJ ", content);
        Assert.Contains($"[-{gap} (jumps over )] TJ ", content);
        Assert.Contains($"[-{gap} (the lazy dog.)] TJ ", content);
    }

    /// <summary>
    /// The WriteLines method should output a Tf instruction each time a span has a
    /// different font or size to the previous span.
    [Fact]
    public async Task ShouldOutputMultipleFontIntructionsAcrossSpans()
    {
        using var output = new MemoryStream();

        var writer = new StreamWriter(output);
        var regular = new TestFont("Helvetica");
        var bold = new TestFont("Helvetica-Bold", "F2");

        var lines = new[] { new Line(new PdfRectangle(10, 10, 430, 12), [
            new LineSpan(new PdfRectangle(10, 10, 100, 12), regular, 12, "The quick "),
            new LineSpan(new PdfRectangle(120, 10, 100, 12), bold, 12, "brown fox "),
            new LineSpan(new PdfRectangle(230, 10, 100, 12), bold, 12, "jumps over "),
            new LineSpan(new PdfRectangle(340, 10, 100, 12), bold, 14, "the lazy dog.") ] ) };
        await writer.WriteLines(lines);

        // Get the content of the first page.
        var content = CS(output);

        // We expect three Tf instructions.
        Assert.Equal(1, CountInstances(content, $"/{regular.Identifier} 12 Tf "));
        Assert.Equal(1, CountInstances(content, $"/{bold.Identifier} 12 Tf "));
        Assert.Equal(1, CountInstances(content, $"/{bold.Identifier} 14 Tf "));
        Assert.Equal(3, CountInstances(content, " Tf "));

        Assert.Contains("(The quick ) Tj ", content);
        var gap = (int)(10 * 1000 / 12d);
        Assert.Contains($"[-{gap} (brown fox )] TJ ", content);
        Assert.Contains($"[-{gap} (jumps over )] TJ ", content);
        gap = (int)(10 * 1000 / 14d);
        Assert.Contains($"[-{gap} (the lazy dog.)] TJ ", content);
    }

    /// <summary>
    /// The WriteLines method should output multiple lines, with one or more TJ or Tj instructions.
    /// </summary>
    [Fact]
    public async Task ShouldWriteMultipleLines()
    {
        using var output = new MemoryStream();

        var writer = new StreamWriter(output);
        var regular = new TestFont("Helvetica");

        var lines = new[] {
            new Line(new PdfRectangle(10, 50, 200, 12.5), [
                new LineSpan(new PdfRectangle(10, 10, 100, 12.5), regular, 12.5, "The quick brown fox")
            ]),
            new Line(new PdfRectangle(10, 37.5, 200, 12.5), [
                new LineSpan(new PdfRectangle(10, 10, 100, 12.5), regular, 12.5, "jumps over the lazy dog.")
            ])
        };

        await writer.WriteLines(lines);

        // Get the content of the first page.
        var content = CS(output);

        // We expect one text block (BT..ET)
        Assert.Equal(1, CountInstances(content, "BT "));
        Assert.Equal(1, CountInstances(content, " ET"));

        // We expect a line break instruction. that also sets the leading - TD
        Assert.Equal(1, CountInstances(content, " 0 -12.5 TD "));

        // We expect two Tj instructions.
        Assert.Equal(2, CountInstances(content, " Tj "));
    }

    /// <summary>
    /// The WriteLines method should correctly output standard T* linebreaks, or TD instructions that set the
    /// text leading, based on the spacing of lines. 
    /// </summary>
    [Fact]
    public async Task ShouldOutputLinebreaksAndSetLeading()
    {
        using var output = new MemoryStream();

        var writer = new StreamWriter(output);
        var regular = new TestFont("Helvetica");

        // Four lines, with some inconsistnt spacing
        var lines = new[] {
            new Line(new PdfRectangle(10, 50, 200, 12.5), [
                new LineSpan(new PdfRectangle(10, 10, 100, 12.5), regular, 12.5, "The quick")
            ]),
            new Line(new PdfRectangle(10, 37.5, 200, 12.5), [
                new LineSpan(new PdfRectangle(10, 10, 100, 12.5), regular, 12.5, "brown fox")
            ]),
            new Line(new PdfRectangle(10, 25, 200, 12.5), [
                new LineSpan(new PdfRectangle(10, 10, 100, 12.5), regular, 12.5, "jumps over")
            ]),
            new Line(new PdfRectangle(10, 10, 200, 12.5), [
                new LineSpan(new PdfRectangle(10, 10, 100, 12.5), regular, 12.5, "the lazy dog.")
            ])
        };

        await writer.WriteLines(lines);

        // Get the content of the first page.
        var content = CS(output);

        // We expect one text block (BT..ET)
        Assert.Equal(1, CountInstances(content, "BT "));
        Assert.Equal(1, CountInstances(content, " ET"));

        // We expect a TD, a T*, and a TD.
        Assert.Equal(1, CountInstances(content, " 0 -12.5 TD "));
        Assert.Equal(1, CountInstances(content, " 0 -15 TD "));
        Assert.Equal(1, CountInstances(content, " T* "));
    }

    /// <summary>
    /// The WriteLines method should support lines with different horizontal starting points
    /// and output appropriate TD instructions.
    /// <returns>
    [Fact]
    public async Task ShouldSupportHorizontalOffsets()
    {
        using var output = new MemoryStream();

        var writer = new StreamWriter(output);
        var regular = new TestFont("Helvetica");

        // Four lines, with some inconsistnt leading and horizontal offsets
        var lines = new[] {
            new Line(new PdfRectangle(10, 50, 200, 12.5), [
                new LineSpan(new PdfRectangle(10, 10, 100, 12.5), regular, 12.5, "The quick")
            ]),
            new Line(new PdfRectangle(20, 37.5, 200, 12.5), [
                new LineSpan(new PdfRectangle(20, 10, 100, 12.5), regular, 12.5, "brown fox")
            ]),
            new Line(new PdfRectangle(30, 25, 200, 12.5), [
                new LineSpan(new PdfRectangle(30, 10, 100, 12.5), regular, 12.5, "jumps over")
            ]),
            new Line(new PdfRectangle(20, 10, 200, 12.5), [
                new LineSpan(new PdfRectangle(20, 10, 100, 12.5), regular, 12.5, "the lazy dog.")
            ])
        };

        await writer.WriteLines(lines);

        // Get the content of the first page.
        var content = CS(output);

        // We expect one text block (BT..ET)
        Assert.Equal(1, CountInstances(content, "BT "));
        Assert.Equal(1, CountInstances(content, " ET"));

        // We expect three TD instructions, because each line has a different horizontal offset.
        Assert.Equal(2, CountInstances(content, " 10 -12.5 TD "));
        Assert.Equal(1, CountInstances(content, " -10 -15 TD "));
        Assert.Equal(3, CountInstances(content, " TD "));

        // And we count four Tj instructions - no spacing within the line spans.
        Assert.Equal(4, CountInstances(content, " Tj "));
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Counts the number of instances a pattern was found in the string.
    /// </summary>
    /// <param name="text">The text string.</param>
    /// <param name="pattern">The pattern.</param>
    /// <returns>The number of times the pattern was found.</returns>
    private static int CountInstances(string text, string pattern)
    {
        var count = 0;
        var index = 0;
        while (true)
        {
            index = text.IndexOf(pattern, index);
            if (index == -1) break;
            count++;
            index += pattern.Length;
        }
        return count;
    }

    /// <summary>
    /// Gets the content of the specified stream. This method loads the content
    /// into a byte array and converts it to a string assuming the content 
    /// /// is ASCII-endcoded.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <returns>The string representation of the stream.</returns>
    private static string CS(MemoryStream stream)
    {
        return System.Text.Encoding.ASCII.GetString(stream.GetBuffer(), 0, (int)stream.Position);
    }

    #endregion


    // Private types
    // =============
    #region TestFont class
    /// <summary>
    /// The TestFont class is a test implementation of the Font class.
    /// It is used to test the WriteLines method.
    /// </summary>
    private class TestFont(string typename, string identifier = "F1") : Font(new PdfObjectReference(1, 1), identifier, typename, false)
    {
        public override double GetDescent(double fontSize)
        {
            throw new NotImplementedException();
        }

        public override double MeasureSpace(double fontSize, double characterSpacing, double textRatio)
        {
            throw new NotImplementedException();
        }

        public override double MeasureString(string str, double fontSize, double characterSpacing, double textRatio)
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}