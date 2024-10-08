using NSubstitute.Routing.AutoValues;
using PagePerfect.PdfScript.Processor.Text;
using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The TextFlowEngineTests class provides unit tests for the TextFlowEngine class.
/// </summary>
public class TextFlowEngineTests
{
    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// If the rectangle provided has no width or height, no lines are returned.
    /// </summary> 
    [Fact]
    public void ShouldOutputEmptyListIfNoRect()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        Assert.Empty(engine.FlowText(new List<Span>([new Span("Hello, world!", font, 12)]), new PdfRectangle(0, 0, 0, 100)));
        Assert.Empty(engine.FlowText(new List<Span>([new Span("Hello, world!", font, 12)]), new PdfRectangle(0, 0, 100, 0)));
    }

    /// <summary>
    /// The TextFlowEngine should return an empty list if a span is just an empty string.
    /// </summary> 
    [Fact]
    public void ShouldOutputEmptyListIfStringEmpty()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        Assert.Empty(engine.FlowText(new List<Span>([new Span("", font, 12)]), new PdfRectangle(0, 0, 100, 100)));
    }

    /// <summary>
    /// The TextFlowEngine should return an empty list if a span is just a whitespace sequence.
    /// </summary> 
    [Fact]
    public void ShouldOutputEmptyListIfStringOnlyWhitespace()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        Assert.Empty(engine.FlowText(new List<Span>([new Span("  ", font, 12)]), new PdfRectangle(0, 0, 100, 100)));
    }

    /// <summary>
    /// The TextFlowEngine should return a single line with a single line span, if the text fits within the rectangle.
    /// </summary>
    [Fact]
    public void ShouldOutputTrivialLine()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        var lines = engine.FlowText(new List<Span>([new Span("Hello, world!", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        // Expect a single line, one type
        Assert.Single(lines);
        Assert.Single(lines.First().Spans);

        // The text should match the input.
        Assert.Equal("Hello, world!", lines.First().Spans.First().Text);
        Assert.Equal(font, lines.First().Spans.First().Font);
        Assert.Equal(12, lines.First().Spans.First().FontSize);

        // Check the width of the span(s).
        var expected = font.MeasureString("Hello, world!", 12, 0, 1);
        Assert.Equal(0, lines.First().Spans.First().BoundingBox.Left);
        Assert.Equal(100 - 12, lines.First().Spans.First().BoundingBox.Bottom);
        Assert.Equal(100, lines.First().Spans.First().BoundingBox.Top);
        Assert.Equal(expected, lines.First().Spans.First().BoundingBox.Width, 2);
    }

    /// <summary>
    /// The TextFlowEngine should ignore leading whitespace.
    /// </summary> 
    [Fact]
    public void ShouldIgnoreLeadingWhitespace()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        // Text with leading whitespace
        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        var lines = engine.FlowText(new List<Span>([new Span("  Hello, world!", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        // Expect a single line, one type
        Assert.Single(lines);
        Assert.Single(lines.First().Spans);

        // The text should match the input.
        Assert.Equal("Hello, world!", lines.First().Spans.First().Text);
    }

    /// <summary>
    /// The TextFlowEngine should ignore trailing whitespace.
    /// </summary> 
    [Fact]
    public void ShouldIgnoreTrailingWhitespace()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        // Text with trailing whitespace
        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        var lines = engine.FlowText(new List<Span>([new Span("Hello, world!  ", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        // Expect a single line, one type
        Assert.Single(lines);
        Assert.Single(lines.First().Spans);

        // The text should match the input.
        Assert.Equal("Hello, world!", lines.First().Spans.First().Text);
    }

    /// <summary>
    /// The TextFlowEngine should collapse a whitespace sequence.
    /// </summary> 
    [Fact]
    public void ShouldCollapseWhitespace()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        // Text with a long whitespace sequence.
        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        var lines = engine.FlowText(new List<Span>([new Span("Hello,  \t   world!", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        // Expect a single line, one type
        Assert.Single(lines);
        Assert.Single(lines.First().Spans);

        // The text should match the input but with collapsed whitespace.
        // The width should be the width of the text with collapsed whitespace.
        var width = (float)font.MeasureString("Hello, world!", 12, 0, 1);
        Assert.Equal("Hello, world!", lines.First().Spans.First().Text);
        Assert.Equal(width, lines.First().Spans.First().BoundingBox.Width, 2);
    }

    /// <summary>
    /// The TextFlowEngine should wrap a span's text across multiple lines.
    /// </summary>
    [Fact]
    public void ShouldWrapSpan()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        var lines = engine.FlowText(new List<Span>([new Span("The quick brown fox jumps over the lazy dog.", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        Assert.Equal(3, lines.Count());

        var first = lines.First();
        Assert.Single(first.Spans);
        Assert.Equal("The quick brown", first.Spans.First().Text);
        Assert.Equal(font, first.Spans.First().Font);
        Assert.Equal(12, first.Spans.First().FontSize);
        var width = font.MeasureString("The quick brown", 12, 0, 1);
        Assert.Equal(0, first.Spans.First().BoundingBox.Left);
        Assert.Equal(100 - 12, first.Spans.First().BoundingBox.Bottom);
        Assert.Equal(100, first.Spans.First().BoundingBox.Top);
        Assert.Equal(width, first.Spans.First().BoundingBox.Width, 2);

        var second = lines.Skip(1).First();
        Assert.Single(second.Spans);
        Assert.Equal("fox jumps over the", second.Spans.First().Text);
        Assert.Equal(font, second.Spans.First().Font);
        Assert.Equal(12, second.Spans.First().FontSize);
        width = font.MeasureString("fox jumps over the", 12, 0, 1);
        Assert.Equal(0, second.Spans.First().BoundingBox.Left);
        Assert.Equal(100 - 24, second.Spans.First().BoundingBox.Bottom);
        Assert.Equal(100 - 12, second.Spans.First().BoundingBox.Top);
        Assert.Equal(Math.Round(width, 2), Math.Round(second.Spans.First().BoundingBox.Width, 2));

        var third = lines.Last();
        Assert.Single(third.Spans);
        Assert.Equal("lazy dog.", third.Spans.First().Text);
        Assert.Equal(font, third.Spans.First().Font);
        Assert.Equal(12, third.Spans.First().FontSize);
        width = font.MeasureString("lazy dog.", 12, 0, 1);
        Assert.Equal(0, third.Spans.First().BoundingBox.Left);
        Assert.Equal(100 - 36, third.Spans.First().BoundingBox.Bottom);
        Assert.Equal(100 - 24, third.Spans.First().BoundingBox.Top);
        Assert.Equal(Math.Round(width, 2), Math.Round(third.Spans.First().BoundingBox.Width, 2));
    }

    /// <summary>
    /// The TextFlowEngine should truncate when a subsequent line no longer fits within the rectangle.
    /// </summary> 
    [Fact]
    public void ShouldTruncateLines()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        var lines = engine.FlowText(new List<Span>([new Span("The quick brown fox jumps over the lazy dog.", font, 12)]), new PdfRectangle(0, 0, 100, 30));

        // Expect only two lines - third line shouldn't fit.
        Assert.Equal(2, lines.Count());

        var first = lines.First();
        Assert.Single(first.Spans);
        Assert.Equal("The quick brown", first.Spans.First().Text);

        var second = lines.Skip(1).First();
        Assert.Single(second.Spans);
        Assert.Equal("fox jumps over the", second.Spans.First().Text);
    }

    /// <summary>
    /// The TextFlowEngine should align text to the right.
    /// </summary>
    [Fact]
    public void ShouldCreateRightAlignedText()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var options = TextAlignmentOptions.Default;
        options.HorizontalAlignment = HorizontalTextAlignment.Right;
        var engine = new TextFlowEngine(options);
        var lines = engine.FlowText(new List<Span>([new Span("The quick brown fox jumps over the lazy dog.", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        Assert.Equal(3, lines.Count());

        var first = lines.First();
        var second = lines.Skip(1).First();
        var third = lines.Last();

        Assert.Equal("The quick brown", first.Spans.First().Text);
        Assert.Equal("fox jumps over the", second.Spans.First().Text);
        Assert.Equal("lazy dog.", third.Spans.First().Text);

        var firstWidth = font.MeasureString("The quick brown", 12, 0, 1);
        var secondWidth = font.MeasureString("fox jumps over the", 12, 0, 1);
        var thirdWidth = font.MeasureString("lazy dog.", 12, 0, 1);

        Assert.Equal(Math.Round(100 - firstWidth, 2), Math.Round(first.BoundingBox.Left, 2));
        Assert.Equal(Math.Round(100 - firstWidth, 2), Math.Round(first.Spans.First().BoundingBox.Left, 2));
        Assert.Equal(Math.Round(100 - secondWidth, 2), Math.Round(second.BoundingBox.Left, 2));
        Assert.Equal(Math.Round(100 - secondWidth, 2), Math.Round(second.Spans.First().BoundingBox.Left, 2));
        Assert.Equal(Math.Round(100 - thirdWidth, 2), Math.Round(third.BoundingBox.Left, 2));
        Assert.Equal(Math.Round(100 - thirdWidth, 2), Math.Round(third.Spans.First().BoundingBox.Left, 2));
    }

    /// <summary>
    /// The TextFlowEngine should create horizontally centered text.
    /// </summary> 
    [Fact]
    public void ShouldCreateCenteredText()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var options = TextAlignmentOptions.Default;
        options.HorizontalAlignment = HorizontalTextAlignment.Center;
        var engine = new TextFlowEngine(options);
        var lines = engine.FlowText(new List<Span>([new Span("The quick brown fox jumps over the lazy dog.", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        Assert.Equal(3, lines.Count());

        var first = lines.First();
        var second = lines.Skip(1).First();
        var third = lines.Last();

        Assert.Equal("The quick brown", first.Spans.First().Text);
        Assert.Equal("fox jumps over the", second.Spans.First().Text);
        Assert.Equal("lazy dog.", third.Spans.First().Text);

        var firstWidth = font.MeasureString("The quick brown", 12, 0, 1);
        var secondWidth = font.MeasureString("fox jumps over the", 12, 0, 1);
        var thirdWidth = font.MeasureString("lazy dog.", 12, 0, 1);

        Assert.Equal(Math.Round((100 - firstWidth) / 2, 2), Math.Round(first.BoundingBox.Left, 2));
        Assert.Equal(Math.Round((100 - firstWidth) / 2, 2), Math.Round(first.Spans.First().BoundingBox.Left, 2));
        Assert.Equal(Math.Round((100 - secondWidth) / 2, 2), Math.Round(second.BoundingBox.Left, 2));
        Assert.Equal(Math.Round((100 - secondWidth) / 2, 2), Math.Round(second.Spans.First().BoundingBox.Left, 2));
        Assert.Equal(Math.Round((100 - thirdWidth) / 2, 2), Math.Round(third.BoundingBox.Left, 2));
        Assert.Equal(Math.Round((100 - thirdWidth) / 2, 2), Math.Round(third.Spans.First().BoundingBox.Left, 2));
    }

    /// <summary>
    /// The TextFlowEngine should create fully justified text.
    /// </summary>
    [Fact]
    public void ShouldCreateFullyJustifiedText()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var options = TextAlignmentOptions.Default;
        options.HorizontalAlignment = HorizontalTextAlignment.FullyJustified;
        var engine = new TextFlowEngine(options);
        var lines = engine.FlowText(new List<Span>([new Span("The quick brown fox jumps over the lazy dog.", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        Assert.Equal(3, lines.Count());

        var first = lines.First();
        var second = lines.Skip(1).First();
        var third = lines.Last();

        // First line - we expect the first and last line spans to be on the far left and far right.
        // We expect equal spacing between each word.
        Assert.Equal(3, first.Spans.Length);
        Assert.Equal("The", first.Spans[0].Text);
        Assert.Equal(" quick", first.Spans[1].Text);
        Assert.Equal(" brown", first.Spans[2].Text);
        Assert.Equal(0, first.Spans[0].BoundingBox.Left);
        Assert.Equal(100, first.Spans[2].BoundingBox.Right);
        Assert.Equal(
            Math.Round(first.Spans[1].BoundingBox.Left - first.Spans[0].BoundingBox.Right, 2),
            Math.Round(first.Spans[2].BoundingBox.Left - first.Spans[1].BoundingBox.Right, 2));

        // Second line - we expect the first and last line spans to be on the far left and far right.
        // We expect equal spacing between each word.
        Assert.Equal(4, second.Spans.Length);
        Assert.Equal("fox", second.Spans[0].Text);
        Assert.Equal(" jumps", second.Spans[1].Text);
        Assert.Equal(" over", second.Spans[2].Text);
        Assert.Equal(" the", second.Spans[3].Text);
        Assert.Equal(0, second.Spans[0].BoundingBox.Left);
        Assert.Equal(100, second.Spans[3].BoundingBox.Right, 2);
        Assert.Equal(
            Math.Round(second.Spans[1].BoundingBox.Left - second.Spans[0].BoundingBox.Right, 2),
            Math.Round(second.Spans[2].BoundingBox.Left - second.Spans[1].BoundingBox.Right, 2));
        Assert.Equal(
            Math.Round(second.Spans[2].BoundingBox.Left - second.Spans[1].BoundingBox.Right, 2),
            Math.Round(second.Spans[3].BoundingBox.Left - second.Spans[2].BoundingBox.Right, 2));

        // Last line - we expect a single span with "lazy dog.", left-aligned.
        Assert.Single(third.Spans);
        Assert.Equal("lazy dog.", third.Spans[0].Text);
        Assert.Equal(0, third.Spans[0].BoundingBox.Left);
        Assert.NotEqual(100, third.Spans[0].BoundingBox.Right);
    }

    /// <summary>
    /// The TextFlowEngine should create fully justified text, and allow the
    /// last line to be right or center aligned.
    /// </summary>
    [Fact]
    public void ShouldCreateFullyJustifiedTextWithNonLeftLastLine()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var options = TextAlignmentOptions.Default;
        options.HorizontalAlignment = HorizontalTextAlignment.FullyJustified | HorizontalTextAlignment.Right;
        var engine = new TextFlowEngine(options);
        var lines = engine.FlowText(new List<Span>([new Span("The quick brown fox jumps over the lazy dog.", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        Assert.Equal(3, lines.Count());

        var first = lines.First();
        var second = lines.Skip(1).First();
        var third = lines.Last();

        var thirdWidth = font.MeasureString("lazy dog.", 12, 0, 1);

        // Last line - we expect a single span with "lazy dog.", right-aligned.
        Assert.Single(third.Spans);
        Assert.Equal("lazy dog.", third.Spans[0].Text);
        Assert.Equal(Math.Round(100 - thirdWidth, 2), Math.Round(third.Spans[0].BoundingBox.Left, 2));
        Assert.Equal(100, third.Spans[0].BoundingBox.Right);
    }

    /// <summary>
    /// The TextFlowEngine should vertically align the text so it's bottom-aligned.
    /// </summary> 
    [Fact]
    public void ShouldVerticallyAlignToBottom()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var options = TextAlignmentOptions.Default;
        options.HorizontalAlignment = HorizontalTextAlignment.Left;
        options.VerticalAlignment = VerticalTextAlignment.Bottom;
        var engine = new TextFlowEngine(options);
        var lines = engine.FlowText(new List<Span>([new Span("The quick brown fox jumps over the lazy dog.", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        Assert.Equal(3, lines.Count());

        var first = lines.First();
        var second = lines.Skip(1).First();
        var third = lines.Last();
        var descent = font.GetDescent(12);

        Assert.Equal(Math.Round(0 - descent, 2), Math.Round(third.BoundingBox.Bottom, 2));
        //Assert.Equal(0, third.Spans.First().BoundingBox.Bottom);
        Assert.Equal(Math.Round(12 - descent, 2), Math.Round(second.BoundingBox.Bottom, 2));
        //Assert.Equal(12, second.Spans.First().BoundingBox.Bottom);
        Assert.Equal(Math.Round(24 - descent, 2), Math.Round(first.BoundingBox.Bottom, 2));
        //Assert.Equal(24, first.Spans.First().BoundingBox.Bottom);
    }

    /// <summary>
    /// The TextFlowEngine should vertically align the text so it's in the middle of the rectangle.
    /// </summary> 
    [Fact]
    public void ShouldVerticallyAlignToMiddle()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var options = TextAlignmentOptions.Default;
        options.HorizontalAlignment = HorizontalTextAlignment.Left;
        options.VerticalAlignment = VerticalTextAlignment.Middle;
        var engine = new TextFlowEngine(options);
        var lines = engine.FlowText(new List<Span>([new Span("The quick brown fox jumps over the lazy dog.", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        Assert.Equal(3, lines.Count());

        var first = lines.First();
        var second = lines.Skip(1).First();
        var third = lines.Last();
        var descent = font.GetDescent(12);
        Assert.Equal(Math.Round(32 - descent, 2), Math.Round(third.BoundingBox.Bottom, 2));
        //Assert.Equal(0, third.Spans.First().BoundingBox.Bottom);
        Assert.Equal(Math.Round(44 - descent, 2), Math.Round(second.BoundingBox.Bottom, 2));
        //Assert.Equal(12, second.Spans.First().BoundingBox.Bottom);
        Assert.Equal(Math.Round(56 - descent, 2), Math.Round(first.BoundingBox.Bottom, 2));
        //Assert.Equal(24, first.Spans.First().BoundingBox.Bottom);
    }


    /// <summary>
    /// The TextFlowEngine should apply line spacing between lines, if specified.
    /// </summary> 
    [Fact]
    public void ShouldApplyLineSpacing()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var options = TextAlignmentOptions.Default;

        // Use line spacing of 3.0
        var engine = new TextFlowEngine(options, 3f);
        var lines = engine.FlowText(new List<Span>([new Span("The quick brown fox jumps over the lazy dog.", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        Assert.Equal(3, lines.Count());

        var first = lines.First();
        var second = lines.Skip(1).First();
        var third = lines.Last();
        var descent = font.GetDescent(12);

        Assert.Equal("The quick brown", first.Spans.First().Text);
        Assert.Equal("fox jumps over the", second.Spans.First().Text);
        Assert.Equal("lazy dog.", third.Spans.First().Text);

        // Expect space between the lines.
        Assert.Equal(Math.Round(100 - 12 - descent, 2), Math.Round(first.BoundingBox.Bottom, 2));
        Assert.Equal(Math.Round(100d - 12d - 15d - descent, 2), Math.Round(second.BoundingBox.Bottom, 2));
        Assert.Equal(Math.Round(100d - 12d - 15d - 15d - descent, 2), Math.Round(third.BoundingBox.Bottom, 2));
    }

    /// <summary>
    /// The TextFlowEngine should support a trivial line (where all content fits) with multiple spans.
    /// </summary>
    [Fact]
    public void ShouldSupportMultipleSpansOnTrivialLine()
    {
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        var lines = engine.FlowText([new Span("Hello, ", font, 12), new Span("world!", font, 12)], new PdfRectangle(0, 0, 100, 100));

        // Expect a single line, one type
        Assert.Single(lines);
        Assert.Single(lines.First().Spans);

        // The text should match the input.
        Assert.Equal("Hello, world!", lines.First().Spans.First().Text);
        Assert.Equal(font, lines.First().Spans.First().Font);
        Assert.Equal(12, lines.First().Spans.First().FontSize);

        // Check the width of the span(s).
        var expected = font.MeasureString("Hello, world!", 12, 0, 1);
        Assert.Equal(0, lines.First().Spans.First().BoundingBox.Left);
        Assert.Equal(100 - 12, lines.First().Spans.First().BoundingBox.Bottom);
        Assert.Equal(100, lines.First().Spans.First().BoundingBox.Top);
        Assert.Equal(expected, lines.First().Spans.First().BoundingBox.Width, 2);
    }

    /// <summary>
    /// The TextFlowEngine should wrap multiple spans across multiple lines.
    /// </summary>
    [Fact]
    public void ShouldWrapMultipleLines()
    {
        /*
        using var output = new MemoryStream();
        var writer = new PdfDocumentWriter(output);
        var font = writer.CreateStandardFont("Helvetica");

        var engine = new TextFlowEngine(TextAlignmentOptions.Default);
        var lines = engine.FlowText(new List<Span>([new Span("The quick brown fox jumps over the lazy dog.", font, 12)]), new PdfRectangle(0, 0, 100, 100));

        Assert.Equal(3, lines.Count());

        var first = lines.First();
        Assert.Single(first.Spans);
        Assert.Equal("The quick brown", first.Spans.First().Text);
        Assert.Equal(font, first.Spans.First().Font);
        Assert.Equal(12, first.Spans.First().FontSize);
        var width = font.MeasureString("The quick brown", 12, 0, 1);
        Assert.Equal(0, first.Spans.First().BoundingBox.Left);
        Assert.Equal(100 - 12, first.Spans.First().BoundingBox.Bottom);
        Assert.Equal(100, first.Spans.First().BoundingBox.Top);
        Assert.Equal(width, first.Spans.First().BoundingBox.Width, 2);

        var second = lines.Skip(1).First();
        Assert.Single(second.Spans);
        Assert.Equal("fox jumps over the", second.Spans.First().Text);
        Assert.Equal(font, second.Spans.First().Font);
        Assert.Equal(12, second.Spans.First().FontSize);
        width = font.MeasureString("fox jumps over the", 12, 0, 1);
        Assert.Equal(0, second.Spans.First().BoundingBox.Left);
        Assert.Equal(100 - 24, second.Spans.First().BoundingBox.Bottom);
        Assert.Equal(100 - 12, second.Spans.First().BoundingBox.Top);
        Assert.Equal(Math.Round(width, 2), Math.Round(second.Spans.First().BoundingBox.Width, 2));

        var third = lines.Last();
        Assert.Single(third.Spans);
        Assert.Equal("lazy dog.", third.Spans.First().Text);
        Assert.Equal(font, third.Spans.First().Font);
        Assert.Equal(12, third.Spans.First().FontSize);
        width = font.MeasureString("lazy dog.", 12, 0, 1);
        Assert.Equal(0, third.Spans.First().BoundingBox.Left);
        Assert.Equal(100 - 36, third.Spans.First().BoundingBox.Bottom);
        Assert.Equal(100 - 24, third.Spans.First().BoundingBox.Top);
        Assert.Equal(Math.Round(width, 2), Math.Round(third.Spans.First().BoundingBox.Width, 2));
        */
    }

    /// <summary>
    /// The TextFlowEngine should combine word chunks of two adjacent spans into a single word.
    /// </summary>
    [Fact]
    public void ShouldCombineSpanWords()
    {

    }
    #endregion
}