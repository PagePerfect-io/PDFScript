using PagePerfect.PdfScript.Writer.Resources.Patterns;

namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The Pattern class represents a pattern that can be embedded in a PDF document.
/// It is an abstract class that is used as the base class for all pattern resources.
/// </summary>
/// <remarks>
/// Initialises a new Pattern instance.
/// </remarks>
/// <param name="obj">The PDF object that this resource refers to.</param>
/// <param name="identifier">The identifier that the object will be known as in the current page</param>
/// <param name="patternType">The patter type - linear or radial gradient</param>
/// <exception cref="ArgumentException">The obect reference or identifier cannot be Null.</exception>
public abstract class Pattern(PdfObjectReference obj, string identifier, PatternType patternType, object? tag = null)
: PdfResourceReference(obj, identifier, PdfResourceType.Pattern, tag)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The type of pattern.
    /// </summary>
    public PatternType PatternType { get; } = patternType;
    #endregion
}