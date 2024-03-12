
using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript.Processor.Text;

/// <summary>
/// The Span class represents a span of text, with a font and a size.
/// </summary> 
/// <remarks>
/// Initialises a new Span instance.
/// </remarks>
/// <param name="text">The text in this span.</param>
/// <param name="font">The font.</param>
/// <param name="fontSize">The font size.</param>
public class Span(string text, Font font, double fontSize)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The font.
    /// </summary>
    public Font Font { get; set; } = font;

    /// <summary>
    /// The font size.
    /// </summary>
    public double FontSize { get; set; } = fontSize;

    /// <summary>
    /// The text of the span.
    /// </summary>
    public string Text { get; set; } = text;
    #endregion
}