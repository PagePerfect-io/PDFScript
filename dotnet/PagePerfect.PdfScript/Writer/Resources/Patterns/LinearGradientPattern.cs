namespace PagePerfect.PdfScript.Writer.Resources.Patterns;

/// <summary>
/// The LinearGradientPattern class represents a linear gradient pattern resource that can be
/// used in a PDF document. This is a simple linear gradient pattern, with a start and end
/// colour, and a set of stops.
/// This resource is used by the PdfWriter class to create a linear gradient pattern resource.
/// It assumed the writer has already done the appropriate checks to ensure the pattern is valid.
/// </summary>
public class LinearGradientPattern(PdfObjectReference obj, string identifier, ColourSpace cs, PdfRectangle rect, Colour[] colours, float[] stops, object? tag = null)
: Pattern(obj, identifier, PatternType.LinearGradient, tag)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The colour space for the pattern.
    /// </summary>
    public ColourSpace ColourSpace { get; } = cs;

    /// <summary>
    /// The bounds of the gradient.
    /// </summary>
    public PdfRectangle Rectangle { get; } = rect;

    /// <summary>
    /// The colours used in the gradient.
    /// </summary>
    public Colour[] Colours { get; } = colours;

    /// <summary>
    /// The stops used in the gradient.
    /// </summary>
    public float[] Stops { get; } = stops;

    #endregion
}