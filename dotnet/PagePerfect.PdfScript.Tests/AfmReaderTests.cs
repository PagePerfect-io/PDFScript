using PagePerfect.PdfScript.Writer.Resources.Metrics;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The AfmReaderTests class provides unit tests for the AfmReader class.
/// </summary> 
public class AfmReaderTests
{

    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// The AfmReader class should read an AFM file for a standard PDF font.
    /// </summary>
    [Fact]
    public void ShouldReadAfmFile()
    {
        var reader = new AfmReader();
        reader.Read("Helvetica");

        Assert.Equal(-207, reader.Descent);
        Assert.Equal(32, reader.First);
        Assert.Equal(251, reader.Last);
        Assert.Equal(220, reader.Widths!.Length);
        Assert.Equal(278, reader.Widths[0]);
    }
    #endregion
}