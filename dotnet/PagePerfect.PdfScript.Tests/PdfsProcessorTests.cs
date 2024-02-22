using System.Text;
using NSubstitute;
using PagePerfect.PdfScript.Processor;
using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Writer;
using PagePerfect.PdfScript.Writer.Resources.Fonts;

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

    #region Prolog statements - resources
    /// <summary>
    /// The processor should throw an exception when declaring a resource twice.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenDeclaringResourcesTwice()
    {
        using var stream = S("# resource /Img1 /Image () # resource /Img1 /Image ()");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));

    }

    /// <summary>
    /// The processor should throw an exception when the type is not Font or Image.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenResourceTypeIsInvalidInDeclaration()
    {
        using var stream = S("# resource /Img1 /Unknown () ");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsReaderException>(() => PdfsProcessor.Process(stream, writer));
    }

    /// <summary>
    /// The processor should throw an exception when the resource name is a
    /// reserved name.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenResourcNameIsReserved()
    {
        using var stream = S("# resource /Image /Image () ");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));
    }
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
    /// <summary>
    /// The processor should place an image using the /Do operation.
    /// </summary>
    [Fact]
    public async Task ShouldPlaceImage()
    {
        using var stream = S("# resource /Img1 /Image (Data/pageperfect-logo.jpg)\r\n100 0 0 100 200 300 cm /Img1 Do");

        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateImage(Arg.Any<string>()).Returns(new Image(new PdfObjectReference(1, 0), "Img1", "Data/pageperfect-logo.jpg", null));
        await PdfsProcessor.Process(stream, writer);

        // We expect calls to AddResourceToPage and CreateImage
        writer.Received(1).CreateImage(Arg.Any<string>());
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());

        // We expect a call to write a 'cm' and a call to write a 'Do'.
        await writer.Received(2).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 100));
        await writer.Received(2).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 0));
        await writer.Received(1).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 200));
        await writer.Received(1).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 300));
        await writer.Received(1).WriteRawContent("cm\r\n");
        await writer.Received(1).WriteRawContent("/Img1 Do\r\n");
    }

    /// <summary>
    /// The Processor should embed an image only once, even when used multiple times
    /// across pages.
    /// </summary>
    [Fact]
    public async Task ShouldEmbedImageOnceWhenUsedMultipleTimes()
    {
        using var stream = S("# resource /Img1 /Image (Data/pageperfect-logo.jpg)\r\n" +
            "100 0 0 100 200 300 cm /Img1 Do " +
            "100 0 0 100 200 500 cm /Img1 Do " +
            "endpage " +
            "100 0 0 100 200 300 cm /Img1 Do "
            );

        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateImage(Arg.Any<string>()).Returns(new Image(new PdfObjectReference(1, 0), "Img1", "Data/pageperfect-logo.jpg", null));
        await PdfsProcessor.Process(stream, writer);

        // We expect three calls to AddResourceToPage 
        // We expect one call to  CreateImage
        writer.Received(1).CreateImage(Arg.Any<string>());
        writer.Received(3).AddResourceToPage(Arg.Any<PdfResourceReference>());

        // We expect three writes of the 'Do' operation.
        await writer.Received(3).WriteRawContent("/Img1 Do\r\n");

    }
    /// <summary>
    /// The processor should throw an exception when the named image cannot be
    /// found, when placing an image.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenImageNotFoundWhenPlacingImage()
    {
        using var stream = S("/Img1 Do");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));

    }

    /// <summary>
    /// The processor should throw an exception when the name
    /// does not match the /Image resource type.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenResourceNotImageWhenPlacingImage()
    {
        using var stream = S("# resource /Img1 /Font (https://font.com/fake)\r\n/Img1 Do");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));

    }

    /// <summary>
    /// The processor should throw an exception when the image
    /// could not be located.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenImageNotLocatedWhenPlacingImage()
    {
        using var stream = S("# resource /Img1 /Image (https://image.unkonwn)\r\n/Img1 Do");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));

    }

    /// <summary>
    /// The processor should throw an exception when the image
    /// is not a JPEG image.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenImageNotJPEGWhenPlacingImage()
    {
        using var stream = S("# resource /Img1 /Image (Resource/Test.png)\r\n/Img1 Do");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));

    }

    #endregion

    #region Text and standard font
    /// <summary>
    /// The processor should include a standard font in the document.
    /// </summary>
    [Fact]
    public async Task ShouldUseStandardFont()
    {
        using var stream = S("BT /Helvetica 24 Tf 100 100 Td (Hello, world!) Tj ET");

        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateStandardFont("/Helvetica").Returns(new StandardFont(new PdfObjectReference(1, 0), "F1", "Helvetica", null));
        await PdfsProcessor.Process(stream, writer);

        // We expect a call to CreateStandardFont and AddResourceToPage
        writer.Received(1).CreateStandardFont("/Helvetica");
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());
        await writer.Received(1).WriteRawContent("BT\r\n");
        await writer.Received(1).WriteRawContent("/F1 24 Tf\r\n");
    }

    /// <summary>
    /// The processor should output a text object between Bt..ET operations.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ShouldOutputTextObject()
    {
        using var stream = S("BT /Helvetica 24 Tf 100 100 Td (Hello, world!) Tj ET");

        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateStandardFont("/Helvetica").Returns(new StandardFont(new PdfObjectReference(1, 0), "F1", "Helvetica", null));
        await PdfsProcessor.Process(stream, writer);

        // We expect a call to CreateStandardFont and AddResourceToPage
        writer.Received(1).CreateStandardFont("/Helvetica");
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());
        await writer.Received(1).WriteRawContent("BT\r\n");
        await writer.Received(1).WriteRawContent("/F1 24 Tf\r\n");
        await writer.Received(2).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 100));
        await writer.Received(1).WriteRawContent("Td\r\n");
        await writer.Received(1).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.String && v.GetString() == "Hello, world!"));
        await writer.Received(1).WriteRawContent("Tj\r\n");
        await writer.Received(1).WriteRawContent("ET\r\n");
    }

    /// <summary>
    /// The processor should throw an exception when it encounters text operations
    /// outside of a text object.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenTextOperationsPlacedOutsideTextObject()
    {
        using var stream = S("100 100 Td");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));
    }

    /// <summary>
    /// The processor should throw an exception when it encounters text operations
    /// inside of a text object that are not allowed.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenInvalidOperationsInsideTextObject()
    {
        using var stream = S("BT BT ET ET");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));

        using var stream2 = S("BT 1 0 0 1 200 150 cm ET");

        writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream2, writer));

    }
    #endregion

    #region TrueType Fonts
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