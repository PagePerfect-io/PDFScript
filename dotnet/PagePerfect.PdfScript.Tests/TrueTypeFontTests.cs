using PagePerfect.PdfScript.Writer;
using PagePerfect.PdfScript.Writer.Resources.Fonts;
using PagePerfect.PdfScript.Writer.Resources.Fonts.TrueType;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The TrueTypeFontTests class contains tests for the TrueTypeFont class.
/// </summary>
public class TrueTypeFontTests
{
    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// The TrueTypeFont class should parse a valid TTF file.
    /// </summary> 
    [Fact]
    public void ShouldParseTrueTypeFont()
    {
        var font = TrueTypeFont.Parse(new PdfObjectReference(2, 0), "F01", "Data/Andes-Black.ttf");

        Assert.Equal("AndesW05-Black", font.Typename);
        Assert.Equal(-4.8, font.GetDescent(12));
    }

    /// <summary>
    /// The TrueTypeFont class should measure a string.
    /// </summary>
    [Fact]
    public void ShouldMeasureString()
    {
        var info = new TrueTypeFontInfo();
        info.Parse("Data/Andes-Black.ttf");

        var expectedWidth = "Hello, World!".Sum(c => info.GetCharacterWidth(c) / (double)info.UnitsPerEm);

        var font = TrueTypeFont.Parse(new PdfObjectReference(2, 0), "F01", "Data/Andes-Black.ttf");

        Assert.Equal(Math.Round(12 * expectedWidth, 2), Math.Round(font.MeasureString("Hello, World!", 12, 0, 1), 2));
    }

    /// <summary>
    /// The TrueTypeFont class should measure a space character.
    /// </summary>
    [Fact]
    public void ShouldMeasureSpaceCharacter()
    {
        var info = new TrueTypeFontInfo();
        info.Parse("Data/Andes-Black.ttf");

        var expectedWidth = info.GetCharacterWidth(' ') / (double)info.UnitsPerEm;

        var font = TrueTypeFont.Parse(new PdfObjectReference(2, 0), "F01", "Data/Andes-Black.ttf");

        Assert.Equal(Math.Round(12 * expectedWidth, 2), Math.Round(font.MeasureSpace(12, 0, 1), 2));
    }

    /// <summary>
    /// The TrueTypeFont class should encode strings to glyph arrays.
    /// </summary>
    [Fact]
    public void ShouldEncodeStringToGlyphArray()
    {
        var info = new TrueTypeFontInfo();
        info.Parse("Data/Manrope-Regular.ttf");
        var font = TrueTypeFont.Parse(new PdfObjectReference(2, 0), "F01", "Data/Manrope-Regular.ttf");

        var str = "Hello \uE002 World!";
        var encoded = font.Encode(str);
        Assert.Equal([
            0, 58, 0, 202, 0, 245, 0, 245, 1, 1, 2, 135, 2, 139, 2, 135, 0, 150, 1, 1, 1, 27, 0, 245, 0, 198, 2, 78
        ], encoded);

        Assert.Equal([
            58,202,245,257,647,651,150,283,198,590
        ], font.UsedGlyphs.ToArray());

        str = "Wysołych Świąt!";
        encoded = font.Encode(str);
        Assert.Equal([
            0, 150, 1, 69, 1, 31, 1, 1, 0, 249, 1, 69, 0 ,192, 0, 226,
            2, 135,
            0, 119, 1, 63, 0, 229, 0, 187, 1, 38, 2, 78
        ], encoded);

        Assert.Equal([
            58,202,245,257,647,651,150,283,198,590,325,287,249,192,226,119,319,229,187, 294
        ], font.UsedGlyphs.ToArray());

    }
    #endregion
}