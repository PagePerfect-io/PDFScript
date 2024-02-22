namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The Font class represents a font that can be used within the content of a PDF document.
/// This is an abstract class, and should be inherited by a specific font type.
/// </summary>
/// <remarks>
/// Initialises a new PdfFont instance.
/// </remarks>
/// <param name="obj">The PDF object that this resource refers to.</param>
/// <param name="identifier">The identifier that the object will be known as in the current page</param>
/// <param name="typename">The font's name.</param>
/// <exception cref="ArgumentException">The obect reference, identifier or typename cannot be Null or empty.</exception>
public abstract class Font(PdfObjectReference obj, string identifier, string typename, object? tag = null)
: PdfResourceReference(obj, identifier, PdfResourceType.Font, tag)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The font's name.
    /// </summary>
    public string Typename { get; } = typename;
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Retrieves the font's descent for the given font size. The descent is the amount of space
    /// that a character may descend below the baseline, such as the bottom of a 'g' character.
    /// This method is used to align text to the baseline, at the end of the text flow engine's
    /// processes. 
    /// </summary>
    /// <param name="fontSize">The font size.</param>
    /// <returns>The descent.</returns>
    public abstract double GetDescent(double fontSize);

    /// <summary>
    /// Measures a space character. This method returns the width of the space character in points, taking into account
    /// the glyph widths, the font size, the character spacing and the text ratio. Even though this is a single
    /// character, we do include character space as we will always use the space in conjunction with other characters,
    /// in front of a word for example.
    /// </summary>
    /// <param name="fontSize">The font size.</param>
    /// <param name="characterSpacing">The character spacing.</param>
    /// <param name="textRatio">The text ratio.</param>
    /// <returns>The width of the string, in points.</returns>
    public abstract double MeasureSpace(double fontSize, double characterSpacing, double textRatio);

    /// <summary>
    /// Measures a string. This method returns the width of the string in points, taking into account
    /// the glyph widths, the font size, the character spacing and the text ratio.
    /// </summary>
    /// <param name="str">The text string.</param>
    /// <param name="fontSize">The font size.</param>
    /// <param name="characterSpacing">The character spacing.</param>
    /// <param name="textRatio">The text ratio.</param>
    /// <returns>The width of the string, in points.</returns>
    public abstract double MeasureString(string str, double fontSize, double characterSpacing, double textRatio);
    #endregion
}