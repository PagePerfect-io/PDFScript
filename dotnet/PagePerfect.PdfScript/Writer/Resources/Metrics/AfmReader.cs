namespace PagePerfect.PdfScript.Writer.Resources.Metrics;

/// <summary>
/// The AfmReader class reads the AFM files that are embedded as resources
/// in the Harness.Pdf assembly. Specifically, it reads the glyph widths
/// and font descender information in the AFM file.
/// </summary>
public class AfmReader
{
    // Public properties
    // =================
    #region Public properties
    public double Descent { get; private set; }
    public int First { get; private set; }
    public int Last { get; private set; }
    public int[]? Widths { get; private set; }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Reads the AFM file for the specified font.
    /// </summary>
    /// <param name="name">The font name.</param>
    public void Read(string name)
    {
        var afmEmbeddedResource = GetAfmResource(name);
        var reader = new AfmLexer(afmEmbeddedResource);

        int? firstChar = null;
        var lastChar = 0;
        var widths = new List<int>();
        while (true == reader.ReadTo(new[] { "Descender", "C" }, out var token))
        {

            if ("Descender" == token)
            {
                if (reader.TryReadNumber(out var descent)) { Descent = descent; }
                continue;
            }

            // If there is no number after C, we keep looking for another one.
            if (false == reader.TryReadNumber(out var charNum)) continue;

            if (charNum < lastChar) break;

            if (false == reader.ReadTo("WX")) throw
                new PdfDocumentWriterException("Expected to find a WX token.");

            if (false == reader.TryReadNumber(out var width)) throw
                new PdfDocumentWriterException("Expected a number after a WX token.");

            // ... next character
            if (null == firstChar) { firstChar = lastChar = (int)charNum; }
            else
            {
                while (++lastChar < (int)charNum) widths.Add(0);
            }
            widths.Add((int)width);
        }

        if (widths.Count == 0) throw
            new PdfDocumentWriterException($"No character widths found in AFM file for font {name}.");
        if (firstChar is null) throw
            new PdfDocumentWriterException($"No character widths found in AFM file for font {name}.");

        Widths = [.. widths];
        First = firstChar.Value;
        Last = lastChar;
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Returns a AFM file as a stream from an embedded resource.
    /// </summary>
    /// <param name="name">The font name.</param>
    /// <returns></returns>
    private static Stream GetAfmResource(string name)
    {
        // Font names have a leading slash, which is not present in the
        // name of the AFM embedded resource.
        var nameWithoutSlash = $"{name.Replace("/", "")}.afm";
        // The embedded resource name contains the simple name of this
        // assembly.
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName().Name;
        // The AFM embedded resources are located in Resources/Metrics.
        var resourceName = $"{assemblyName}.Writer.Resources.Metrics.{nameWithoutSlash}";
        // Return the AFM embedded resource as a Stream.
        return assembly.GetManifestResourceStream(resourceName) ?? throw new
            PdfDocumentWriterException($"The AFM resource {resourceName} was not found.");
    }
    #endregion
}
