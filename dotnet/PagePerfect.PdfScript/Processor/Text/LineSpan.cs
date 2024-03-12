using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript.Processor.Text;

/// <summary>
/// The LineSpan class represents a congiuous span of text within a text block. It can cover
/// part of a word, all the way to an entire line of text with many words.
/// A Span instance consists of a bounding box, the font details and the text. It is used by the
/// PdfDocumentWriter class to create TJ and Tj operations. 
/// </summary>
/// <remarks>
/// Instantiates a new LineSpan instance.
/// </remarks>
/// <param name="boundingBox">The bounding box.</param>
/// <param name="font">The font.</param>
/// <param name="fontSize">The font size.</param>
/// <param name="text">The text of this line span.</param>
public class LineSpan(PdfRectangle boundingBox, Font font, double fontSize, string text)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The line span's bounding box, in absolute coordinates.
    /// /// </summary>
    public PdfRectangle BoundingBox { get; } = boundingBox;

    /// <summary>
    /// The text of this line span.
    /// </summary>
    public string Text { get; } = text;

    /// <summary>
    /// The font.
    /// </summary>
    public Font Font { get; } = font;

    /// <summary>
    /// The font size, in points.
    /// </summary>
    /// <value></value>
    public double FontSize { get; } = fontSize;
    #endregion
}