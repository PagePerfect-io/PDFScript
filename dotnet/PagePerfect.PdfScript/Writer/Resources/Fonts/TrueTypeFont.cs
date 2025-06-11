using PagePerfect.PdfScript.Writer.Resources.Fonts.TrueType;

namespace PagePerfect.PdfScript.Writer.Resources.Fonts;

/// <summary>
/// The TrueTypeFont class represents a TrueType font that can be used in a PDF document.
/// </summary>
public class TrueTypeFont : Font
{
    // Private fields
    // ==============
    #region Private fields
    private readonly TrueTypeFontInfo _info;
    private readonly HashSet<uint> _usedGlyphs = [];
    #endregion



    // Instance initialiser
    // ====================
    #region Instance initialiser
    /// <summary>
    /// Initialises a new PdfFont instance.
    /// </summary>
    /// <param name="obj">The PDF object that this resource refers to.</param>
    /// <param name="identifier">The identifier that the object will be known as in the current page</param>
    /// <param name="info">The TrueType information for this font.</param>
    /// <param name="program">The font program - in this case, the TTF file..</param>
    /// <param name="tag">Optionally, a tag to identify the resource.</param>
    /// <exception cref="ArgumentException">The obect reference, identifier or typename cannot be Null or empty.</exception>
    private TrueTypeFont(PdfObjectReference obj, string identifier, TrueTypeFontInfo info, Stream program, object? tag)
        : base(obj, identifier, info.PostscriptName ?? "Unknown", tag)
    {
        _info = info;
        Program = program;
    }
    #endregion



    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// Retrieves the TrueType information for this font.
    /// </summary>
    public TrueTypeFontInfo Info => _info;

    /// <summary>
    /// A stream that contains the font program.
    /// </summary>
    public Stream Program { get; }

    /// <summary>
    /// Retrieves the set of used glyphs in this font. This is used to determine which glyphs
    /// are actually used in the document, so that unused glyphs can be omitted from the CID font
    /// definition in the PDF file.
    /// </summary>
    public HashSet<uint> UsedGlyphs => _usedGlyphs;
    #endregion



    // Base class overrides
    // ====================
    #region Font overrides
    /// <summary>
    /// Retrieves the font's descent for the given font size.
    /// </summary>
    /// <param name="fontSize">The font size.</param>
    /// <returns>The descent.</returns>
    public override double GetDescent(double fontSize)
    {
        return _info.Descender * fontSize / _info.UnitsPerEm;
    }

    /// <summary>
    /// Measures the width of a space character at the given font size and with the
    /// specified character spacing and text ratio.
    /// </summary>
    /// <param name="fontSize">The font size.</param>
    /// <param name="characterSpacing">The character spacing.</param>
    /// <param name="textRatio">The text ratio. 1 is the default ratio, 0.5 is half-width.</param>
    /// <returns>The width.</returns>
    public override double MeasureSpace(double fontSize, double characterSpacing, double textRatio)
    {
        return textRatio * (fontSize * _info.GetCharacterWidth(' ') / _info.UnitsPerEm + characterSpacing);

        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Measures the width of the specified string, at the given font size and with the
    /// specified character spacing and text ratio.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="fontSize">The font size.</param>
    /// <param name="characterSpacing">The character spacing.</param>
    /// <param name="textRatio">The text ratio. 1 is the default ratio, 0.5 is half-width.</param>
    /// <returns>The width.</returns>
    public override double MeasureString(string str, double fontSize, double characterSpacing, double textRatio)
    {
        if (null == str) throw new ArgumentNullException(nameof(str));

        double width = 0;

        foreach (var c in str)
        {
            // We find the character width and add it. Take the
            // font size into account.
            width += fontSize * _info.GetCharacterWidth(c) / _info.UnitsPerEm;

            // Add character spacing.                
            width += characterSpacing;
        }

        return textRatio * (width - characterSpacing); // Take last char spacing off.
    }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Encodes a string into an array of 2-byte glyph codes. This will be used to write the
    /// string to the PDF file, using the writeHexString() method. The font will be wrapped in a
    /// CIDFont object.
    /// </summary>
    /// <param name="str">The string to encode into an array of 2-byte glyph codes</param>
    /// <returns>The byte array with glyph indices.</returns>
    public byte[] Encode(string str)
    {
        if (string.IsNullOrEmpty(str)) return [];

        var encoded = new byte[str.Length * 2];
        int e = 0;
        for (var i = 0; i < str.Length; i++)
        {
            var ch = str[i];
            if (!_info.Cmap.TryGetValue(ch, out var glyphIndex)) throw new InvalidOperationException(
                $"Character code {ch} not found in character map.");
            encoded[e++] = (byte)(glyphIndex >> 8);
            encoded[e++] = (byte)(glyphIndex & 0xFF);

            if (glyphIndex > 0 && !_usedGlyphs.Contains(glyphIndex))
            {
                _usedGlyphs.Add(glyphIndex);
            }
        }
        return encoded;
    }

    /// <summary>
    /// Parses a TrueType font from the specified file.
    /// </summary>
    /// <param name="obj">The PDF object that this resource refers to.</param>
    /// <param name="identifier">The identifier that the object will be known as in the current page</param>
    /// <param name="path">The path to the file.</param>
    /// <param name="tag">Optionally, a tag to identify the resource.</param>
    /// <returns>The parsed TrueType font.</returns>
    /// <exception cref="ArgumentNullException">The path cannot be a Null reference.</exception>
    public static TrueTypeFont Parse(PdfObjectReference obj, string identifier, string path, object? tag = null)
    {
        return Parse(obj, identifier, File.OpenRead(path), tag);
    }

    /// <summary>
    /// Parses a TrueType font from the specified stream.
    /// </summary>
    /// <param name="obj">The PDF object that this resource refers to.</param>
    /// <param name="identifier">The identifier that the object will be known as in the current page</param>
    /// <param name="stream">The stream that contains the font file data.</param>
    /// <param name="tag">Optionally, a tag to identify the resource.</param>
    /// <returns>The parsed TrueType font.</returns>
    /// <exception cref="ArgumentNullException">The stream cannot be a Null reference.</exception>
    public static TrueTypeFont Parse(PdfObjectReference obj, string identifier, Stream stream, object? tag = null)
    {
        var info = new TrueTypeFontInfo();
        info.Parse(stream);

        return new TrueTypeFont(obj, identifier, info, stream, tag);
    }

    /// <summary>
    /// Writes a font definition for a TrueType font that is used in the document.
    /// </summary>
    /// <param name="writer">The writer to use to write to the document.</param>
    public async Task WriteFontProgram(StreamWriter writer)
    {
        Program.Seek(0, SeekOrigin.Begin);

        await writer.WriteLineAsync($"\t/Length\t{Program.Length}");
        await writer.WriteLineAsync($"\t/Length1\t{Program.Length}");
        await writer.WriteLineAsync(">>\r\nstream");

        writer.Flush();
        await Program.CopyToAsync(writer.BaseStream);

        await writer.WriteLineAsync("endstream\r\nendobj");
    }

    #endregion
}