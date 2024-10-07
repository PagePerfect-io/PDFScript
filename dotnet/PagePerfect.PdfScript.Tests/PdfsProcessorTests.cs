using System.Text;
using NSubstitute;
using PagePerfect.PdfScript.Processor;
using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Writer;
using PagePerfect.PdfScript.Writer.Resources.Fonts;
using PagePerfect.PdfScript.Writer.Resources.Patterns;

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
        await writer.Received(1).OpenPage(595, 842, DisplayOrientation.Regular);
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
        await writer.Received(2).OpenPage(595, 842, DisplayOrientation.Regular);
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
        await writer.Received(3).OpenPage(595, 842, DisplayOrientation.Regular);
        await writer.Received(2).ClosePage();
        await writer.Received(1).CloseIfNeeded();
    }

    #endregion

    #region Page sizes and templates

    /// <summary>
    /// The processor should create A3 pages using the "page" statement.
    /// </summary>
    [Fact]
    public async Task ShouldCreateA3Page()
    {
        using var stream = S("/A3 page 10 10 m 100 100 l S");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We expect a new page instruction, with an A3 size.
        await writer.Received(1).OpenPage(842, 1191, DisplayOrientation.Regular);
    }

    /// <summary>
    /// The processor should create custom-size pages using the "page" statement.
    /// </summary>
    [Fact]
    public async Task ShouldCreateCustomSizePage()
    {
        using var stream = S("1000 1000 page 10 10 m 100 100 l S");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We expect a new page instruction, with an 1000x1000 pt size.
        await writer.Received(1).OpenPage(1000, 1000, DisplayOrientation.Regular);
    }

    /// <summary>
    /// The processor should throw an exception when a page size is invalid.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenPageSizeInvalid()
    {
        using var stream = S("/Unknown page 10 10 m 100 100 l S");
        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));

        using var stream2 = S("0 0 page 10 10 m 100 100 l S");
        writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream2, writer));

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

    /// <summary>
    /// The processor should be able to use an injected variable value in place of the value in the declaration.
    /// </summary>
    [Fact]
    public async Task ShouldResolveInjectedVariable()
    {
        using var stream = S("# var $width /Number 100 # var $height /Number 200 10 10 $width $height re f");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer, new { width = 111, height = new PdfsValue(222) });

        // We expect calls to write values 10, 10, 111, 222.
        await writer.Received(2).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 10));
        await writer.Received(1).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 111));
        await writer.Received(1).WriteValue(Arg.Is<PdfsValue>(v => v.Kind == PdfsValueKind.Number && v.GetNumber() == 222));
        await writer.Received(1).WriteRawContent("re\r\n");
        await writer.Received(1).WriteRawContent("f\r\n");
    }

    /// <summary>
    /// The processor should throw an exception when an injected value does not have the correct type.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenInjectedVariableIsOfIncorrectType()
    {
        using var stream = S("# var $width /Number 100 # var $height /Number 200 10 10 $width $height re f");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer, new { width = 111, height = "Edwin" }));
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
    /// The processor should include a standard font in the document
    /// and be able to refer to Times-Roman as /TimesRoman, and so on.
    /// </summary>
    [Fact]
    public async Task ShouldUseStandardFontWithDash()
    {
        using var stream = S("BT /TimesRoman 24 Tf 100 100 Td (Hello, world!) Tj ET");

        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateStandardFont("/Times-Roman").Returns(new StandardFont(new PdfObjectReference(1, 0), "F1", "Times-Roman", null));
        await PdfsProcessor.Process(stream, writer);

        // We expect a call to CreateStandardFont and AddResourceToPage
        writer.Received(1).CreateStandardFont("/Times-Roman");
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
    /// <summary>
    /// The processor should include a standard font in the document.
    /// </summary>
    [Fact]
    public async Task ShouldPlaceTextWithTrueTypeFont()
    {
        using var stream = S("# resource /ManropeRegular /Font (Data/Manrope-Regular.ttf)\r\n" +
            "BT /ManropeRegular 24 Tf 100 100 Td (Hello, world!) Tj ET");

        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateTrueTypeFont(Arg.Any<string>()).Returns(TrueTypeFont.Parse(new PdfObjectReference(1, 0), "F1", "Data/Manrope-Regular.ttf"));
        await PdfsProcessor.Process(stream, writer);

        // We expect a call to CreateTrueTypeFont and AddResourceToPage
        writer.Received(1).CreateTrueTypeFont(Arg.Any<string>());
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());
        await writer.Received(1).WriteRawContent("BT\r\n");
        await writer.Received(1).WriteRawContent("/F1 24 Tf\r\n");
    }

    /// <summary>
    /// The Processor should embed a TrueType font only once, even when used multiple times
    /// across pages.
    /// </summary>
    [Fact]
    public async Task ShouldEmbedTrueTypeFontOnceWhenUsedMultipleTimes()
    {
        using var stream = S("# resource /ManropeRegular /Font (Data/Manrope-Regular.ttf)\r\n" +
    "BT /ManropeRegular 24 Tf 100 100 Td (Hello, world!) Tj ET " +
    "BT /ManropeRegular 24 Tf 100 100 Td (Hello, world!) Tj ET " +
    "endpage " +
    "BT /ManropeRegular 24 Tf 100 100 Td (Hello, world!) Tj ET ");


        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateTrueTypeFont(Arg.Any<string>()).Returns(TrueTypeFont.Parse(new PdfObjectReference(1, 0), "F1", "Data/Manrope-Regular.ttf"));
        await PdfsProcessor.Process(stream, writer);

        // We expect three calls to AddResourceToPage 
        // We expect one call to  CreateTrueTypeFont
        writer.Received(1).CreateTrueTypeFont(Arg.Any<string>());
        writer.Received(3).AddResourceToPage(Arg.Any<PdfResourceReference>());

        // We expect three writes of the 'Tf' operation.
        await writer.Received(3).WriteRawContent("/F1 24 Tf\r\n");

    }

    /// <summary>
    /// The processor should throw an exception when the named image cannot be
    /// found, when placing an image.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenTrueTypeFontNotFoundWhenPlacingText()
    {
        using var stream = S("BT /ManropeRegular 24 Tf 100 100 Td (Hello, world!) Tj ET");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));

    }

    /// <summary>
    /// The processor should throw an exception when the name
    /// does not match the /Image resource type.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenResourceNotFontWhenPlacingText()
    {
        using var stream = S("# resource /ManropeRegular /Image (https://font.com/fake)\r\n" +
        "BT /ManropeRegular 24 Tf 100 100 Td (Hello, world!) Tj ET");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));
    }

    /// <summary>
    /// The processor should throw an exception when the image
    /// could not be located.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenResourceNotLocatedWhenPlacingText()
    {
        using var stream = S("# resource /ManropeRegular /Font (https://font.unknown)\r\n" +
        "BT /ManropeRegular 24 Tf 100 100 Td (Hello, world!) Tj ET");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));
    }
    #endregion

    #region Unicode text
    #endregion

    #region Additional graphics instructions
    /// <summary>
    /// The processor should support the 'rr' operation, which is used to
    /// draw or fill a rounded rectangle.
    /// </summary>
    [Fact]
    public async Task ShouldSupportRoundedRectangles()
    {

        using var stream = S("10 100 300 300 16 rr f");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We expect calls to write curves and lines that approximate
        // a rounded rectangle.
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("26.00 100.00 m\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("294.00 100.00 l\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("310.00 116.00 c\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("310.00 384.00 l\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("294.00 400.00 c\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("26.00 400.00 l\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("10.00 384.00 c\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("10.00 116.00 l\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("26.00 100.00 c\r\n")));
        await writer.Received(1).WriteRawContent("h\r\n");
    }

    /// <summary>
    /// The processor should default to outputting a 're' 
    /// if the 'rr' operation has 0 radius.
    /// </summary>
    [Fact]
    public async Task ShouldDefaultToUnroundedRectangle()
    {
        using var stream = S("10 100 300 300 0 rr f");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We expect a 're' in the output because this rectangle isn't rounded.
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("re\r\n")));
        await writer.DidNotReceive().WriteRawContent(Arg.Is<string>(s => s.Contains("rr\r\n")));
    }

    /// <summary>
    /// The processor should support the 'ell' operation, which is used to
    /// draw or fill a circle or ellipse.
    /// </summary>
    [Fact]
    public async Task ShouldSupportEllipse()
    {

        using var stream = S("10 100 300 300 ell S");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We expect calls to write curves that approximate
        // an ellipse
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("160.00 100.00 m\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("310.00 250.00 c\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("160.00 400.00 c\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains(" 10.00 250.00 c\r\n")));
        await writer.Received(1).WriteRawContent(Arg.Is<string>(s => s.Contains("160.00 100.00 c\r\n")));
        await writer.Received(1).WriteRawContent("h\r\n");
    }
    #endregion

    #region Linear and radial gradient patterns
    /// <summary>
    /// The processor should throw an exception when a pattern is declared
    /// with the same name as an existing pattern.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenPatternNameInUse()
    {
        using var stream = S("# pattern /GreenYellow /LinearGradient /DeviceRGB <<" +
            "/Rect [0 0 595 842] " +
            "/C0 [0 1 0.2]  " +
            "/C1 [0.8 0.8 0.2]" +
            "/Stops [0.0 1.0]" +
            ">>\r\n" +
            "# pattern /GreenYellow /LinearGradient /DeviceRGB <<" +
            "/Rect [0 0 595 842] " +
            "/C0 [0 1 0.2]  " +
            "/C1 [0.8 0.8 0.2]" +
            "/Stops [0.0 1.0]" +
            ">> " +
            "/Pattern cs /GreenYellow scn " +
            "10 10 100 100 re f");

        var writer = Substitute.For<IPdfDocumentWriter>();

        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));
    }

    /// <summary>
    /// The processor should throw an exception when declaring a pattern with the same name as a
    /// resource, or vice-versa.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenPatternNameClashWithResourceName()
    {
        using var stream = S("# resource /GreenYellow /Image (greenyellow.jpg)\r\n" +
            "# pattern /GreenYellow /LinearGradient /DeviceRGB <<" +
            "/Rect [0 0 595 842] " +
            "/C0 [0 1 0.2]  " +
            "/C1 [0.8 0.8 0.2]" +
            "/Stops [0.0 1.0]" +
            ">> " +
            "/Pattern cs /GreenYellow scn " +
            "10 10 100 100 re f");

        var writer = Substitute.For<IPdfDocumentWriter>();

        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream, writer));

        using var stream2 = S(
            "# pattern /GreenYellow /LinearGradient /DeviceRGB <<" +
            "/Rect [0 0 595 842] " +
            "/C0 [0 1 0.2]  " +
            "/C1 [0.8 0.8 0.2]" +
            "/Stops [0.0 1.0]" +
            ">> " +
            "# resource /GreenYellow /Image (greenyellow.jpg)\r\n" +
            "/Pattern cs /GreenYellow scn " +
            "10 10 100 100 re f");

        await Assert.ThrowsAsync<PdfsProcessorException>(() => PdfsProcessor.Process(stream2, writer));

    }

    /// <summary>
    /// The processor should support creation of a linear gradient pattern.
    /// </summary>
    [Fact]
    public async Task ShouldSupportLinearGradientPattern()
    {
        using var stream = S("# pattern /GreenYellow /LinearGradient /DeviceRGB <<" +
            "/Rect [0 0 595 842] " +
            "/C0 [0 1 0.2]  " +
            "/C1 [0.8 0.8 0.2]" +
            "/Stops [0.0 1.0]" +
            ">> " +
            "/GreenYellow scn " +
            "10 10 100 100 re f");

        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateLinearGradientPattern(Arg.Any<PdfRectangle>(), Arg.Any<Colour[]>(), Arg.Any<float[]>())
            .Returns(new LinearGradientPattern(new PdfObjectReference(1, 0), "GreenYellow", ColourSpace.DeviceRGB, new PdfRectangle(0, 0, 595, 842), [Colour.RGB(0, 1, 0.2f), Colour.RGB(0.8f, 0.8f, 0.2f)], [0.0f, 1.0f]));

        await PdfsProcessor.Process(stream, writer);

        // We expect calls to create a linear gradient resource, and add it to the page.
        // We expect a call to set the pattern as the fill colour.
        writer.Received(1).CreateLinearGradientPattern(Arg.Any<PdfRectangle>(), Arg.Any<Colour[]>(), Arg.Any<float[]>());
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());
        await writer.Received(1).WriteRawContent("/Pattern cs /GreenYellow scn\r\n");
    }

    /// <summary>
    /// The processor should support creation of a radial gradient pattern.
    /// </summary>
    [Fact]
    public async Task ShouldSupportRadialGradientPattern()
    {
        using var stream = S("# pattern /GreenYellow /RadialGradient /DeviceRGB <<" +
            "/Rect [0 0 595 842] " +
            "/C0 [0 1 0.2]  " +
            "/C1 [0.8 0.8 0.2]" +
            "/Stops [0.0 1.0]" +
            ">> " +
            "/GreenYellow scn " +
            "10 10 100 100 re f");

        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateRadialGradientPattern(Arg.Any<PdfRectangle>(), Arg.Any<Colour[]>(), Arg.Any<float[]>())
            .Returns(new RadialGradientPattern(new PdfObjectReference(1, 0), "GreenYellow", ColourSpace.DeviceRGB, new PdfRectangle(0, 0, 595, 842), [Colour.RGB(0, 1, 0.2f), Colour.RGB(0.8f, 0.8f, 0.2f)], [0.0f, 1.0f]));

        await PdfsProcessor.Process(stream, writer);

        // We expect calls to create a linear gradient resource, and add it to the page.
        // We expect a call to set the pattern as the fill colour.
        writer.Received(1).CreateRadialGradientPattern(Arg.Any<PdfRectangle>(), Arg.Any<Colour[]>(), Arg.Any<float[]>());
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());
        await writer.Received(1).WriteRawContent("/Pattern cs /GreenYellow scn\r\n");
    }
    #endregion

    #region Colour resource declarations
    /// <summary>
    /// The processor should support declaration of colour resources, which can be used in
    /// the scn and SCN operations.
    /// </summary>
    [Fact]
    public async Task ShouldSupportColourResources()
    {
        using var stream = S("# color /Green /DeviceRGB 0.2 0.9 0.2 " +
            "/Green SCN /Green scn " +
            "10 10 100 100 re f");

        var writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream, writer);

        // We expect calls to the rg and RG operations.
        await writer.Received(1).WriteRawContent("0.20 0.90 0.20 rg\r\n");
        await writer.Received(1).WriteRawContent("0.20 0.90 0.20 RG\r\n");

        using var stream2 = S(
            "# color /Gray500 /DeviceGray 0.5 " +
            "# color /Magenta /DeviceCMYK 0.2 0.9 0.2 1 " +
            "/Magenta SCN /Gray500 scn " +
            "10 10 100 100 re f");

        writer = Substitute.For<IPdfDocumentWriter>();
        await PdfsProcessor.Process(stream2, writer);

        // We expect calls to the g and K operations.
        await writer.Received(1).WriteRawContent("0.50 g\r\n");
        await writer.Received(1).WriteRawContent("0.20 0.90 0.20 1.00 K\r\n");
    }
    #endregion

    #region Drawing text lines
    /// <summary>
    /// The processor should support the Tfl operation, which is used to flow text along a width.
    /// It should flow a short piece of text over a single line.
    /// </summary>
    [Fact]
    public async Task ShouldFlowSingleTextLine()
    {
        using var stream = S(
            "# resource /ManropeRegular /Font (Data/Manrope-Regular.ttf)\r\n" +
            "BT /ManropeRegular 24 Tf 1 0 0 1 100 100 Tm (Hello, world!) Tfl ET");

        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateTrueTypeFont(Arg.Any<string>()).Returns(TrueTypeFont.Parse(new PdfObjectReference(1, 0), "F1", "Data/Manrope-Regular.ttf"));
        await PdfsProcessor.Process(stream, writer);

        // We expect a single call to draw text as the content fits on a line.
        writer.Received(1).CreateTrueTypeFont(Arg.Any<string>());
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());
        await writer.Received(1).WriteRawContent("BT\r\n");
        await writer.Received(1).WriteRawContent("/F1 24 Tf\r\n");
        await writer.Received(1).WriteValue(new PdfsValue("Hello, world!"));
        await writer.Received(1).WriteRawContent(" Tj\r\n");
        await writer.Received(1).WriteRawContent("BT\r\n");
    }

    /// <summary>
    /// The processor should support the Tfl operation, which is used to flow text along a width.
    /// It should flow a piece of text over multiple text lines.
    /// </summary>
    [Fact]
    public async Task ShouldFlowTextLines()
    {
        using var stream = S(
            "# resource /ManropeRegular /Font (Data/Manrope-Regular.ttf)\r\n" +
            "BT /ManropeRegular 24 Tf 1 0 0 1 100 100 Tm 300 /Auto Tb (The quick brown fox jumps over the lazy dog.) Tfl ET");

        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateTrueTypeFont(Arg.Any<string>()).Returns(TrueTypeFont.Parse(new PdfObjectReference(1, 0), "F1", "Data/Manrope-Regular.ttf"));
        await PdfsProcessor.Process(stream, writer);

        // We expect a multiple calls to write text
        writer.Received(1).CreateTrueTypeFont(Arg.Any<string>());
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());
        await writer.Received(1).WriteRawContent("BT\r\n");
        await writer.Received(1).WriteRawContent("/F1 24 Tf\r\n");
        await writer.Received(1).WriteValue(new PdfsValue("The quick brown fox jumps"));
        await writer.Received(1).WriteRawContent("0 -24 TD\r\n");
        await writer.Received(1).WriteValue(new PdfsValue("over the lazy dog."));
        await writer.Received(2).WriteRawContent(" Tj\r\n");
        await writer.Received(1).WriteRawContent("ET\r\n");
    }

    /// <summary>
    /// The processor should align text horizontally, when given an explicit width.
    /// </summary>
    [Fact]
    public async Task ShouldAlignTextHorizontally()
    {
        using var stream = S(
            "# resource /ManropeRegular /Font (Data/Manrope-Regular.ttf)\r\n" +
            "BT /ManropeRegular 24 Tf 1 Ta 1 0 0 1 100 100 Tm 300 /Auto Tb (The quick brown fox jumps over the lazy dog.) Tfl ET");

        var manrope = TrueTypeFont.Parse(new PdfObjectReference(1, 0), "F1", "Data/Manrope-Regular.ttf");
        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateTrueTypeFont(Arg.Any<string>()).Returns(manrope);
        await PdfsProcessor.Process(stream, writer);

        // We expect calls to offset the text horizontally
        writer.Received(1).CreateTrueTypeFont(Arg.Any<string>());
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());
        await writer.Received(1).WriteRawContent("BT\r\n");
        await writer.Received(1).WriteRawContent("/F1 24 Tf\r\n");
        var width = manrope.MeasureString("The quick brown fox jumps", 24, 0, 1f);
        var descent = manrope.GetDescent(24f);
        await writer.Received(1).WriteRawContent($"{(300 - width) / 2:F2} {-24 - descent} Td\r\n");
        await writer.Received(1).WriteValue(new PdfsValue("The quick brown fox jumps"));

        var width2 = manrope.MeasureString("over the lazy dog.", 24, 0, 1f);
        await writer.Received(1).WriteRawContent($"{(width - width2) / 2:F3} -24 TD\r\n");
        await writer.Received(1).WriteValue(new PdfsValue("over the lazy dog."));
        await writer.Received(2).WriteRawContent(" Tj\r\n");
        await writer.Received(1).WriteRawContent("ET\r\n");
    }

    /// <summary>
    /// The processor should align text vertically, when given an explicit height.
    /// </summary>
    [Fact]
    public async Task ShouldAlignTextVertically()
    {
        using var stream = S(
            "# resource /ManropeRegular /Font (Data/Manrope-Regular.ttf)\r\n" +
            "BT /ManropeRegular 24 Tf 1 Ta 1 TA 1 0 0 1 100 500 Tm 300 200 Tb (The quick brown fox jumps over the lazy dog.) Tfl ET");

        var manrope = TrueTypeFont.Parse(new PdfObjectReference(1, 0), "F1", "Data/Manrope-Regular.ttf");
        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateTrueTypeFont(Arg.Any<string>()).Returns(manrope);
        await PdfsProcessor.Process(stream, writer);

        // We expect calls to offset the text vertically
        writer.Received(1).CreateTrueTypeFont(Arg.Any<string>());
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());
        await writer.Received(1).WriteRawContent("BT\r\n");
        await writer.Received(1).WriteRawContent("/F1 24 Tf\r\n");
        var width = manrope.MeasureString("The quick brown fox jumps", 24, 0, 1f);
        var descent = manrope.GetDescent(24f);
        await writer.Received(1).WriteRawContent($"{(300 - width) / 2:F2} {200 - ((200 - 48) / 2) - 24 - descent} Td\r\n");

        var width2 = manrope.MeasureString("over the lazy dog.", 24, 0, 1f);
        await writer.Received(1).WriteRawContent($"{(width - width2) / 2:F3} -24 TD\r\n");
        await writer.Received(1).WriteValue(new PdfsValue("over the lazy dog."));
        await writer.Received(2).WriteRawContent(" Tj\r\n");
        await writer.Received(1).WriteRawContent("ET\r\n");
    }

    /// <summary>
    /// The processor should align text vertically, when the text box has a set height but
    /// an auto width.
    /// </summary>
    [Fact]
    public async Task ShouldAlignSingleLineVertically()
    {
        using var stream = S(
            "# resource /ManropeRegular /Font (Data/Manrope-Regular.ttf)\r\n" +
            "BT /ManropeRegular 24 Tf 1 0 0 1 100 100 Tm 1 TA /Auto 300 Tb (Hello, world!) Tfl ET");

        var manrope = TrueTypeFont.Parse(new PdfObjectReference(1, 0), "F1", "Data/Manrope-Regular.ttf");
        var writer = Substitute.For<IPdfDocumentWriter>();
        writer.CreateTrueTypeFont(Arg.Any<string>()).Returns(manrope);
        await PdfsProcessor.Process(stream, writer);

        // We expect a single call to draw text as the content fits on a line.
        // We also expect a Td operation that moves the text vertically.
        writer.Received(1).CreateTrueTypeFont(Arg.Any<string>());
        writer.Received(1).AddResourceToPage(Arg.Any<PdfResourceReference>());
        await writer.Received(1).WriteRawContent("BT\r\n");
        await writer.Received(1).WriteRawContent("/F1 24 Tf\r\n");

        var descent = manrope.GetDescent(24f);
        var offset = 300 - (300 - 24) / 2;
        await writer.Received(1).WriteRawContent($"0 {Math.Round(offset - 24 - descent, 3)} Td\r\n");
        await writer.Received(1).WriteValue(new PdfsValue("Hello, world!"));
        await writer.Received(1).WriteRawContent(" Tj\r\n");
        await writer.Received(1).WriteRawContent("BT\r\n");
    }

    /// <summary>
    /// The processor should place text on a single line, when given a set of text spans
    /// with various styles.
    /// </summary>
    //[Fact]
    //public async Task ShouldFlowSingleLineTextSpans()
    //  {
    //
    //}
    #endregion

    #region Measuring text lines
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