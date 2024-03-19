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
    private static readonly Dictionary<string, string> s_standardFontMap = new(StringComparer.OrdinalIgnoreCase);
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

    /// <summary>
    /// Tries to get a standardised, standard font name from the specified font name.
    /// This method accepts font names that are similar to the standard names, 
    /// and returns the standard name. Specifically it supports the form 'TimesRoman'
    /// for 'Times-Roman' and performs a case-insensitive comparison.
    /// </summary>
    /// <param name="fontName">The specified font name.</param>
    /// <param name="standardFontName">The matching standard name.</param>
    /// <returns>True if a match was found; False otherwise.</returns>
    public static bool TryGetStandardFontName(string fontName, out string? standardFontName)
    {
        return s_standardFontMap.TryGetValue(fontName, out standardFontName);
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
        s_standardFontMap[font.Replace("-", "")] = font;
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
