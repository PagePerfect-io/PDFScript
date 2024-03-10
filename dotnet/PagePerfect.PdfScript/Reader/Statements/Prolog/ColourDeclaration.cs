using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript.Reader.Statements.Prolog;

/// <summary>
/// The ColourDeclaration class represents a '# color' prolog statement in a .pdfs file.
/// </summary>
/// <remarks>
/// Initialises a new ColourDeclaration instance.
/// </remarks>
public class ColourDeclaration(
    string name,
    Colour colour)
: PrologStatement(PrologStatementType.ColourDeclaration)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The colour's name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The colour value.
    /// </summary>
    public Colour Colour { get; } = colour;
    #endregion
}