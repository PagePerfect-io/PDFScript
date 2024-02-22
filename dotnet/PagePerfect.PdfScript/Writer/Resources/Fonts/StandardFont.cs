using PagePerfect.PdfScript.Writer.Resources.Metrics;

namespace PagePerfect.PdfScript.Writer.Resources.Fonts;

/// <summary>
/// The StandardFont class represents a standard PDF font - one of the 14 fonts defined in the PDF specification.
/// This class inherits from the base Font class.
/// </summary>
public class StandardFont : Font
{
    // Private fields
    // ==============
    #region Private fields
    private readonly AfmReader _afm;
    #endregion



    // Instance initialiser
    // ====================
    #region Instance initialiser
    /// <summary>
    /// Initialises a new PdfFont instance.
    /// </summary>
    /// <param name="obj">The PDF object that this resource refers to.</param>
    /// <param name="identifier">The identifier that the object will be known as in the current page</param>
    /// <param name="typename">The font's type name.</param>
    /// <param name="tag">An object reference to help identify the resource.</param>
    /// <exception cref="ArgumentException">The obect reference, identifier or typename cannot be Null or empty.</exception>
    public StandardFont(PdfObjectReference obj, string identifier, string typename, object? tag = null)
        : base(obj, identifier, typename, tag)
    {
        if (!FontUtilities.IsStandardFont(typename)) throw
            new ArgumentException("The font type name must be one of the standard PDF fonts.", nameof(typename));

        _afm = new AfmReader();
        _afm.Read(typename);
    }
    #endregion



    // Base class overrides
    // ====================
    #region Font class overrides
    /// <summary>
    /// Retrieves the font's descent for the given font size. The descent is the amount of space
    /// that a character may descend below the baseline, such as the bottom of a 'g' character.
    /// This method is used to align text to the baseline, at the end of the text flow engine's
    /// processes. 
    /// </summary>
    /// <param name="fontSize">The font size.</param>
    /// <returns>The descent.</returns>
    public override double GetDescent(double fontSize)
    {
        return fontSize * _afm.Descent / 1000d;
    }

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
    public override double MeasureSpace(double fontSize, double characterSpacing, double textRatio)
    {
        return textRatio * (fontSize * GetWidth(' ') / 1000d + characterSpacing);
    }

    /// <summary>
    /// Measures a string. This method returns the width of the string in points, taking into account
    /// the glyph widths, the font size, the character spacing and the text ratio.
    /// </summary>
    /// <param name="str">The text string.</param>
    /// <param name="fontSize">The font size.</param>
    /// <param name="characterSpacing">The character spacing.</param>
    /// <param name="textRatio">The text ratio.</param>
    /// <returns>The width of the string, in points.</returns>
    public override double MeasureString(string str, double fontSize, double characterSpacing, double textRatio)
    {
        if (null == str) throw new ArgumentNullException(nameof(str));

        double width = 0;

        foreach (var c in str)
        {
            // We find the character width and add it. Take the
            // font size into account.
            width += fontSize * GetWidth(c) / 1000d;

            // Add character spacing.                
            width += characterSpacing;
        }

        return textRatio * (width - characterSpacing); // Take last char spacing off.
    }
    #endregion


    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Returns the width of the glyph indicated by the character index or
    /// CID.
    /// </summary>
    /// <param name="index">The character index or CID.</param>
    /// <returns>The width.</returns>
    private int GetWidth(int index)
    {
        return index >= _afm.First && index <= _afm.Last
            ? _afm.Widths![index - _afm.First] : 1000;
    }
    #endregion
}