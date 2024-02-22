namespace PagePerfect.PdfScript.Writer.Resources.Fonts.TrueType;

/// <summary>
/// The GlyphInfo class encapsulates information about a glyph in a TrueType font file. It is used internally by
/// the TrueTypeFontInfo class.
/// </summary>
internal class GlyphInfo(HorizontalMetric metric)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The min X value of the glyph outline.
    /// </summary>
    public int XMin { get; set; }

    /// <summary>
    /// The max X value of the glyph outline.
    /// </summary>
    public int XMax { get; set; }

    /// <summary>
    /// The min Y value of the glyph outline.
    /// </summary>
    public int YMin { get; set; }

    /// <summary>
    /// The max Y value of the glyph outline.
    /// </summary>
    public int YMax { get; set; }

    /// <summary>
    /// The HorizontalMetric for this glyph.
    /// </summary>
    public HorizontalMetric Metric { get; set; } = metric;
    #endregion
}