
using PagePerfect.PdfScript.Writer.Resources.Fonts.TrueType;

namespace PagePerfect.PdfScript.Tests;
/// <summary>
/// The TrueTypeFontInfoTests class contains tests for the TrueTypeFontInfo class.
/// </summary> 
public class TrueTypeFontInfoTests
{
    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// The TrueTypeFontInfo class should load a TrueType font file and return its information.
    /// </summary>
    [Fact]
    public void ShouldReadTrueTypeFont()
    {
        var info = new TrueTypeFontInfo();
        info.Parse("Data/Andes-Black.ttf");

        Assert.Equal("AndesW05-Black", info.PostscriptName);
        Assert.Equal(-400, info.Descender);
    }
    #endregion
}