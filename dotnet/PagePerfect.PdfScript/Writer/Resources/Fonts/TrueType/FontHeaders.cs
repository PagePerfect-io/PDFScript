namespace PagePerfect.PdfScript.Writer.Resources.Fonts.TrueType;

/// <summary>
/// The FontHeaders class encapsulates information contained within a TrueType 'head' table. Only information that is
/// useful for the parsing of fonts for their metrics is contained witin a FontHeaders structure. The FontHeaders class
/// is used internally by the TrueTypeFontInfo class.
/// </summary>

internal class FontHeaders
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// the number of units that make up a single em-point. This resolution is used in 
    /// the metrics tables of the font.
    /// </summary> 
    public int UnitsPerEm { get; set; }

    /// <summary>
    /// the format of the glyph location table used in the TrueType file
    /// </summary>
    public int IndexToLocFormat { get; set; }
    #endregion
}