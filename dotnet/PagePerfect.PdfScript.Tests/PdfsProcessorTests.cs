using System.Text;
using NSubstitute;
using PagePerfect.PdfScript.Processor;
using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The PdfsProcessorTests class contains tests for the PdfsProcessor class.
/// </summary>
public class PdfsProcessorTests
{
    // Public tests
    // ============
    #region Document structure
    /// <summary>
    /// The processor should not create a document when no statements are present.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ShouldNotCreateDocumentWhenNoStatements()
    {
        using var stream = S("");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We should not have opened a page or document.
        await writer.DidNotReceive().Open();
        await writer.DidNotReceive().OpenPage(Arg.Any<double>(), Arg.Any<double>(), Arg.Any<DisplayOrientation>());
        await writer.Received(1).CloseIfNeeded();
    }

    /// <summary>
    /// The processor should correctly open and close a page and document.
    /// </summary>
    [Fact]
    public async Task ShouldOpenAndClosePageAndDocument()
    {
        using var stream = S("10 10 m 100 100 l S");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We should have opened a page and closed it.
        // We should also have closed the document.
        await writer.Received(1).OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.Received(1).CloseIfNeeded();
    }

    /// <summary>
    /// The processor should open additional pages and close them, using the 'endpage' instruction.
    /// </summary>
    [Fact]
    public async Task ShouldOpenAdditionalPages()
    {
        using var stream = S("10 10 m 100 100 l S endpage 10 10 m 100 100 l S");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We expect two pages to be opened, and one of those explicitly closed.
        await writer.Received(2).OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.Received(1).ClosePage();
        await writer.Received(1).CloseIfNeeded();
    }

    /// <summary>
    /// The processor should create empty pages when using subsequent 'endpage' statements.
    /// </summary>
    [Fact]
    public async Task ShouldCreateEmptyPage()
    {
        using var stream = S("10 10 m 100 100 l S endpage endpage 10 10 m 100 100 l S");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We expect three pages.
        await writer.Received(3).OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.Received(2).ClosePage();
        await writer.Received(1).CloseIfNeeded();
    }

    #endregion

    #region Basic graphics instructions
    /// <summary>
    /// </summary>
    [Fact]
    public async Task ShouldProcessBasicGraphicsInstruction()
    {
        using var stream = S("0.03 Tc");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We expect a call to WriteValue.
        await writer.Received(1).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 0.03f));
        await writer.Received(1).WriteRawContent("Tc\r\n");
    }
    #endregion

    #region Prolog statements
    #endregion

    #region Variables
    /// <summary>
    /// The processor should throw an exception when a variable is declared multiple times.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenDeclaringVariableMultipleTimes()
    {
        using var stream = S("# var $width /Number 100 # var $width /Number 200");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));
    }

    /// <summary>
    /// The processor should throw an exception when an undeclared variable is used.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenUsingUndeclaredVariable()
    {
        using var stream = S("10 10 $width $height re f");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsReaderException>(() => PdfsProcessor.Process(stream, writer));
    }

    /// <summary>
    /// The processor should resolve a variable.
    /// </summary>
    [Fact]
    public async Task ShouldResolveVariable()
    {
        using var stream = S("# var $width /Number 100 # var $height /Number 200 10 10 $width $height re f");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We expect calls to write values 10, 10, 100, 200.
        await writer.Received(2).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 10));
        await writer.Received(1).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 100));
        await writer.Received(1).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 200));
        await writer.Received(1).WriteRawContent("re\r\n");
        await writer.Received(1).WriteRawContent("f\r\n");
    }

    #endregion

    #region Images and the Do operator
    #endregion

    #region Text and standard fonts
    #endregion

    #region Fonts
    #endregion

    #region Unicode text
    #endregion

    #region Real world examples
    /// <summary>
    /// The processor should output a PDF with some lines on it.
    /// </summary>
    [Fact]
    public async Task ShouldWriteADocument()
    {
        using var stream = S("10 10 m 100 100 l S endpage endpage 10 10 m 100 100 l S");

        var writer = new PdfDocumentWriter("test.pdf");

        await PdfsProcessor.Process(stream, writer);

    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Creates a memory stream out of a string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <returns>The memory stream.</returns>
    private static MemoryStream S(string source)
    {
        var bytes = Encoding.ASCII.GetBytes(source);
        return new MemoryStream(bytes);
    }
    #endregion
}