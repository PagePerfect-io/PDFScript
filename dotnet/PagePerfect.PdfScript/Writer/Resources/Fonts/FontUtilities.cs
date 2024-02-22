namespace PagePerfect.PdfScript.Writer.Resources.Fonts;

/// <summary>
/// The FontUtilities class is a static class that provides utility methods for working with fonts.
/// </summary>
public static class FontUtilities
{
    // Private fields
    // ==============
    #region Private fields
    private static readonly List<string> s_safeFonts = [];
    private static readonly List<string> s_standardFonts = [];
    #endregion



    // Class initialiser
    // =================
    #region Class initialiser
    /// <summary>
    /// Sets up the font utilities class.
    /// </summary>
    static FontUtilities()
    {
        SetupStandardFonts();
    }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Indicates if the specified font name is one of the 14 standard fonts in the PDF specification.
    /// </summary>
    /// <param name="fontName">The font name</param>
    /// <returns>A boolean indicating if the font name is a standard one (true) or not</returns>
    /// <exception cref="ArgumentNullException">The fontName argument is null</exception>
    public static bool IsStandardFont(string fontName)
    {
        return s_standardFonts.Exists(font => fontName.TrimStart('/').Equals(font, StringComparison.InvariantCultureIgnoreCase));
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Adds a font to the list of fonts that are safe to externalise.
    /// </summary>
    /// <param name="font">The font.</param>
    private static void AddSafeFont(string font)
    {
        s_safeFonts.Add(font);
    }

    /// <summary>
    /// Adds a font to the list of standard PDF fonts.
    /// </summary>
    /// <param name="font">The font.</param>
    private static void AddStandardFont(string font)
    {
        s_standardFonts.Add(font);
    }

    /// <summary>
    /// Sets up the standard PDF fonts.
    /// </summary>
    private static void SetupStandardFonts()
    {
        // PDF standard list
        AddStandardFont("Times-Roman");
        AddStandardFont("Times-Bold");
        AddStandardFont("Times-BoldItalic");
        AddStandardFont("Times-Italic");
        AddStandardFont("Helvetica");
        AddStandardFont("Helvetica-Bold");
        AddStandardFont("Helvetica-BoldOblique");
        AddStandardFont("Helvetica-Oblique");
        AddStandardFont("Courier");
        AddStandardFont("Courier-Bold");
        AddStandardFont("Courier-BoldOblique");
        AddStandardFont("Courier-Oblique");
        AddStandardFont("Symbol");
        AddStandardFont("ZapfDingbats");

        // External by default
        AddSafeFont("Arial");
        AddSafeFont("Arial Bold");
        AddSafeFont("Arial Bold Italic");
        AddSafeFont("Arial Italic");

        AddSafeFont("Courier New");
        AddSafeFont("Courier New Bold");
        AddSafeFont("Courier New Bold Italic");
        AddSafeFont("Courier New Italic");

        AddSafeFont("Times New Roman");
        AddSafeFont("Times New Roman Bold");
        AddSafeFont("Times New Roman Bold Italic");
        AddSafeFont("Times New Roman Italic");
    }
    #endregion
}
