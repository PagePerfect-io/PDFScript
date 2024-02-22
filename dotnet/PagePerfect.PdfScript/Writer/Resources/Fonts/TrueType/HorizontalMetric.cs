namespace PagePerfect.PdfScript.Writer.Resources.Fonts.TrueType;

/// <summary>
/// The HorizontalMetric class encapsulates metrics for glyphs in a TrueType font file. It is used internally by the
/// TrueTypeFontInfo class.
/// </summary>
internal class HorizontalMetric
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The 'advance width' - how much should the text cursor advance after this glyph is drawn.
    /// </summary>
    public int AdvanceWidth { get; set; }

    /// <summary>
    /// The left side bearing - how much should the text cursor move before this glyph is drawn.
    /// </summary>
    public int LeftSideBearing { get; set; }
    #endregion   
}