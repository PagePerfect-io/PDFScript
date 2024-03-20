using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The PdfDocumentTests class contains the tests
/// for the PdfDocumentWriter class.
/// </summary>
public class PdfDocumentWriterTests
{
    // Public test methods
    // ==================
    #region Basic PDF writing tests
    /// <summary>
    /// The PdfDocumentWriter should throw an exception if the stream or file is null.
    /// </summary>
    [Fact]
    public void ShouldThrowWhenStreamOrFileNull()
    {
        Assert.Throws<ArgumentNullException>(() => new PdfDocumentWriter((Stream)null!));
        Assert.Throws<ArgumentNullException>(() => new PdfDocumentWriter((string)null!));
    }

    /// <summary>
    /// The PdfDocumentWriter should throw an exception if the current state is not valid
    /// for opening a document, e.g. if the document is already open.
    /// </summary> 
    [Fact]
    public async Task ShouldThrowWhenInvalidStateWhenOpening()
    {
        var writer = new PdfDocumentWriter("test.pdf");
        await writer.Open();
        await Assert.ThrowsAsync<PdfDocumentWriterException>(writer.Open);
    }

    /*
    /// <summary>
    /// The PdfDocumentWriter should be able to write a Hello, World! document.
    /// </summary>
    [Fact]
    public async void ShouldWriteHelloWorldDocument()
    {
        using var stream = new MemoryStream();
        var writer = new PdfDocumentWriter(stream);
        await writer.Open();
        await writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.OpenContentStream();
        var font = writer.CreateFont("Helvetica", false);
        await writer.WriteRawContent($"BT /{font.Identifier} 24 Tf 100 100 Td (Hello, world!) Tj ET\r\n");
        writer.AddResourceToPage(font);
        await writer.CloseContentStream();
        await writer.ClosePage();
        await writer.Close();

        stream.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes("test.pdf", stream.ToArray());
    }
    */
    #endregion

    #region Page and content stream tests
    /// <summary>
    /// When using single-stream syntax, the PdfDocumentWriter should not allow
    /// multiple content streams to be opened.
    /// </summary>
    [Fact]
    public async Task ShouldNotAllowMultipleSingleContentStreams()
    {
        using (var stream = new MemoryStream())
        {
            var writer = new PdfDocumentWriter(stream);
            await writer.Open();
            await writer.OpenPage(595, 841, DisplayOrientation.Regular);
            await writer.OpenContentStream();
            await writer.CloseContentStream();
            await Assert.ThrowsAsync<PdfDocumentWriterException>(async () => await writer.OpenContentStream());
        }
    }

    /// <summary>
    /// The PdfDocumentWriter class should not allow mixing of single and multiple
    /// content streams - not mixing of NextContentStream() and OpenContentStream() calls.
    /// </summary>
    [Fact]
    public async Task ShouldNotAllowMixingOfSingleAndMultipleContentStreams()
    {
        using (var stream = new MemoryStream())
        {
            var writer = new PdfDocumentWriter(stream);
            await writer.Open();
            await writer.OpenPage(595, 841, DisplayOrientation.Regular);
            await writer.OpenContentStream();
            await writer.CloseContentStream();
            await Assert.ThrowsAsync<PdfDocumentWriterException>(async () => await writer.NextContentStream());
        }

        using (var stream = new MemoryStream())
        {
            var writer = new PdfDocumentWriter(stream);
            await writer.Open();
            await writer.OpenPage(595, 841, DisplayOrientation.Regular);
            await writer.NextContentStream();
            await Assert.ThrowsAsync<PdfDocumentWriterException>(async () => await writer.OpenContentStream());
        }
    }

    /*
    /// <summary>
    /// The PdfDocumentWriter should allow multiple content streams when using NextContentStream().
    /// </summary>
    [Fact]
    public async Task ShouldAllowMultipleContentStreams()
    {
        using (var stream = new MemoryStream())
        {

            var writer = new PdfDocumentWriter(stream);
            await writer.Open();
            await writer.OpenPage(595, 841, DisplayOrientation.Regular);
            await writer.NextContentStream();
            var font = writer.CreateFont("Helvetica", false);
            await writer.WriteRawContent($"BT /{font.Identifier} 24 Tf 100 100 Td ");
            writer.AddResourceToPage(font);
            await writer.CloseContentStream();
            await writer.NextContentStream();
            await writer.WriteRawContent($"(Hello, world!) Tj ET\r\n");
            await writer.CloseContentStream();
            await writer.ClosePage();
            await writer.Close();

            stream.Seek(0, SeekOrigin.Begin);
        }
    }
    */
    #endregion

    #region Form tests
    /*
    /// <summary>
    /// The PdfDocumentWriter should write a form XObject when one is added to
    /// the document.
    /// </summary>
    [Fact]
    public async Task ShouldWriteForm()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);
        await writer.Open();
        await writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.OpenContentStream();
        var font = writer.CreateFont("Helvetica", false);
        var form = writer.CreateForm(new PdfRectangle(0, 0, 595, 841));

        form.AddResource(font);
        await form.WriteContent($"BT /{font.Identifier} 24 Tf 100 100 Td (Hello, world from a form!) Tj ET\r\n");
        writer.AddResourceToPage(font);
        writer.AddResourceToPage(form);

        await writer.WriteRawContent($"/{form.Identifier} Do\r\n");
        await writer.CloseContentStream();
        await writer.ClosePage();
        await writer.Close();

        // Check that the document has written the form correctly.
        stream.Seek(0, SeekOrigin.Begin);
        //File.WriteAllBytes("form-writer-test.pdf", stream.ToArray());

    }
    */
    #endregion

    #region Image tests
    /// <summary>
    /// The PdfDocumentWriter class should support drawing of JPEG images.
    /// </summary>
    [Fact]
    public async Task ShouldDrawJpegImage()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);
        await writer.Open();
        await writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.NextContentStream();

        var img = writer.CreateImage("Data/pageperfect-logo.jpg");
        await writer.WriteRawContent($"100 0 0 100 200 300 cm\r\n");
        await writer.WriteRawContent($"/{img.Identifier} Do\r\n");
        writer.AddResourceToPage(img);
        await writer.CloseContentStream();
        await writer.ClosePage();
        await writer.Close();

        stream.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes("Data/image-write-test.pdf", stream.ToArray());
    }

    /// <summary>
    /// The PdfDocumentWriter should throw an exception when the image
    /// is not a recognised JPEG image.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ShouldThrowWhenImageNotJpeg()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);
        await writer.Open();
        await writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.NextContentStream();

        var img = writer.CreateImage("Data/pageperfect-logo.png");
        await writer.WriteRawContent($"100 0 0 100 200 300 cm\r\n");
        await writer.WriteRawContent($"/{img.Identifier} Do\r\n");
        writer.AddResourceToPage(img);
        await writer.CloseContentStream();
        await writer.ClosePage();

        await Assert.ThrowsAsync<NotSupportedException>(writer.Close);
    }

    /// <summary>
    /// The PdfDocumentWriter class should support JPEG images
    /// regardless of the file extension.
    /// </summary>
    [Fact]
    public async Task ShouldNotRelyOnJpegExtension()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);
        await writer.Open();
        await writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.NextContentStream();

        var img = writer.CreateImage("Data/pageperfect-logo");
        await writer.WriteRawContent($"100 0 0 100 200 300 cm\r\n");
        await writer.WriteRawContent($"/{img.Identifier} Do\r\n");
        writer.AddResourceToPage(img);
        await writer.CloseContentStream();
        await writer.ClosePage();
        await writer.Close(); // Does not throw
    }

    #endregion

    #region Standard font tests
    /// <summary>
    /// The PdfDocumentWriter should add a standard font to the document.
    /// </summary>
    [Fact]
    public async Task ShouldAddStandardFont()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);
        await writer.Open();
        await writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.NextContentStream();

        var helvetica = writer.CreateStandardFont("Helvetica");
        var courier = writer.CreateStandardFont("Courier");

        writer.AddResourceToPage(helvetica);
        writer.AddResourceToPage(courier);

        await writer.WriteRawContent($"BT /{helvetica.Identifier} 24 Tf 100 100 Td (Hello, world!) Tj ET\r\n");
        await writer.WriteRawContent($"BT /{courier.Identifier} 24 Tf 100 200 Td (Hello, world!) Tj ET\r\n");

        await writer.CloseContentStream();
        await writer.ClosePage();
        await writer.Close();

        stream.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes("Data/standard-font-test.pdf", stream.ToArray());

    }
    #endregion

    #region TrueType font tests
    /// <summary>
    /// The PdfDocumentWriter should support a TrueType font resource and should output it
    /// in the document.
    /// </summary>
    [Fact]
    public async Task ShouldOutputTrueTypeFontInFile()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);
        await writer.Open();
        await writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.NextContentStream();

        var andes = writer.CreateTrueTypeFont("Data/Andes-Black.ttf");
        writer.AddResourceToPage(andes);
        await writer.WriteRawContent($"BT /{andes.Identifier} 24 Tf 100 100 Td (Hello, world!) Tj ET\r\n");
        await writer.CloseContentStream();
        await writer.ClosePage();
        await writer.Close();

        stream.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes("Data/truetype-font-test.pdf", stream.ToArray());
    }
    #endregion

    #region Linear and radial gradient pattern tests
    /// <summary>
    /// The PdfDocumentWriter should support adding a linear gradient pattern to the document.
    /// </summary>
    [Fact]
    public async Task ShouldAddLinearGradientPattern()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);
        await writer.Open();
        await writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.NextContentStream();

        var pattern = writer.CreateLinearGradientPattern(
            new PdfRectangle(0, 0, 100, 100),
            [new(ColourSpace.DeviceRGB, 1, 0, 0), new(ColourSpace.DeviceRGB, 0, 1, 0)],
            [0, 1.0f]);

        writer.AddResourceToPage(pattern);
        await writer.WriteRawContent($"/Pattern cs /{pattern.Identifier} scn 0 0 100 100 re f\r\n");
        await writer.CloseContentStream();
        await writer.ClosePage();
        await writer.Close();

        stream.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes("Data/linear-gradient-pattern-test.pdf", stream.ToArray());
    }

    /// <summary>
    /// The PdfDocumentWriter should throw an exception when creating a linear gradient
    /// that's invalid.
    /// </summary>
    [Fact]
    public void ShouldThrowWhenLinearGradientInvalid()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);

        // Mixed colour spaces
        Assert.Throws<ArgumentException>(() => writer.CreateLinearGradientPattern(
            new PdfRectangle(0, 0, 100, 00),
            [new(ColourSpace.DeviceRGB, 1, 0, 0), new(ColourSpace.DeviceCMYK, 0, 1, 0, 1)],
            [0, 1.0f]));

        // Number of stops and colours don't match
        Assert.Throws<ArgumentException>(() => writer.CreateLinearGradientPattern(
            new PdfRectangle(0, 0, 100, 00),
            [new(ColourSpace.DeviceRGB, 1, 0, 0), new(ColourSpace.DeviceRGB, 0, 1, 0, 1)],
            [0, 0.5f, 1.0f]));

        // Not enough colours
        Assert.Throws<ArgumentException>(() => writer.CreateLinearGradientPattern(
            new PdfRectangle(0, 0, 100, 00),
            [new(ColourSpace.DeviceRGB, 1, 0, 0)],
            [0]));
    }

    /// <summary>
    /// The PdfDocumentWriter should support adding a radial gradient pattern to the document.
    /// </summary>
    [Fact]
    public async Task ShouldAddRadialGradientPattern()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);
        await writer.Open();
        await writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.NextContentStream();

        var pattern = writer.CreateRadialGradientPattern(
            new PdfRectangle(0, 0, 100, 100),
            [new(ColourSpace.DeviceRGB, 1, 0, 0), new(ColourSpace.DeviceRGB, 0, 1, 0)],
            [0, 1.0f]);

        writer.AddResourceToPage(pattern);
        await writer.WriteRawContent($"/Pattern cs /{pattern.Identifier} scn 0 0 100 100 re f\r\n");
        await writer.CloseContentStream();
        await writer.ClosePage();
        await writer.Close();

        stream.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes("Data/radial-gradient-pattern-test.pdf", stream.ToArray());
    }

    /// <summary>
    /// The PdfDocumentWriter should throw an exception when creating a linear gradient
    /// that's invalid.
    /// </summary>
    [Fact]
    public void ShouldThrowWhenRadialGradientInvalid()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);

        // Mixed colour spaces
        Assert.Throws<ArgumentException>(() => writer.CreateRadialGradientPattern(
            new PdfRectangle(0, 0, 100, 00),
            [new(ColourSpace.DeviceRGB, 1, 0, 0), new(ColourSpace.DeviceCMYK, 0, 1, 0, 1)],
            [0, 1.0f]));

        // Number of stops and colours don't match
        Assert.Throws<ArgumentException>(() => writer.CreateRadialGradientPattern(
            new PdfRectangle(0, 0, 100, 00),
            [new(ColourSpace.DeviceRGB, 1, 0, 0), new(ColourSpace.DeviceRGB, 0, 1, 0, 1)],
            [0, 0.5f, 1.0f]));

        // Not enough colours
        Assert.Throws<ArgumentException>(() => writer.CreateRadialGradientPattern(
            new PdfRectangle(0, 0, 100, 00),
            [new(ColourSpace.DeviceRGB, 1, 0, 0)],
            [0]));
    }
    #endregion

    #region Latin-1 encoding tests
    /// <summary>
    /// The PdfDocumentWriter should write Latin-1 encoded text. Any string value passed into
    /// WriteValue() should be encoded using Latin-1.
    /// </summary>
    [Fact]
    public async Task ShouldWriteLatin1EncodedText()
    {
        using var stream = new MemoryStream();

        var writer = new PdfDocumentWriter(stream);
        await writer.Open();
        await writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await writer.OpenContentStream();
        var font = writer.CreateStandardFont("Helvetica");
        await writer.WriteRawContent($"BT /{font.Identifier} 24 Tf 100 100 Td ");
        await writer.WriteValue(new Reader.PdfsValue("Hasta la mañana!"));
        await writer.WriteRawContent($" Tj ET\r\n");
        writer.AddResourceToPage(font);
        await writer.CloseContentStream();
        await writer.ClosePage();
        await writer.Close();

        stream.Seek(0, SeekOrigin.Begin);
        File.WriteAllBytes("Data/Latin-1 text.pdf", stream.ToArray());

    }
    #endregion


}
