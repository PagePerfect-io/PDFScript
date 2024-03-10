using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript.Reader.Statements.Prolog;

/// <summary>
/// The PatternDeclaration class represents a  '# pattern' prolog statement in a .pdfs file.
/// </summary>
/// <remarks>
/// Initialises a new PatternDeclaration instance.
/// </remarks>
public class PatternDeclaration(
    string name,
    PatternType patternType,
    ColourSpace colourSpace,
    PdfRectangle boundingRectangle,
    Colour[] colours,
    float[] stops)
: PrologStatement(PrologStatementType.PatternDeclaration)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The pattern name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The type of pattern - linear gradient, etc.
    /// </summary>
    public PatternType PatternType { get; } = patternType;

    /// <summary>
    /// The colour space for this pattern.
    /// </summary>
    public ColourSpace ColourSpace { get; } = colourSpace;

    /// <summary>
    /// The bounding rectangle for this pattern.
    /// </summary>
    public PdfRectangle BoundingRectangle { get; } = boundingRectangle;

    /// <summary>
    /// The colours used in the pattern.
    /// </summary>
    public Colour[] Colours { get; } = colours;

    /// <summary>
    /// The stops in this pattern.
    /// </summary>
    public float[] Stops { get; } = stops;
    #endregion
}