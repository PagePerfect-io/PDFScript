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
    #endregion
}