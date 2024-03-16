namespace PagePerfect.PdfScript.Processor.Text;

/// <summary>
/// The HorizontalTextAlignment enumeration lists the possible horizontal aligment values for the
/// TextFormattingOptions class.
/// </summary>
[Flags]
public enum HorizontalTextAlignment
{
    /// <summary>
    /// The text is left-aligned. Each line starts on the left-hand side of the bounding box.
    /// </summary>
    Left = 0,

    /// <summary>
    /// The text is centered. Each line will be centered horizontally.
    /// </summary>
    Center = 1,

    /// <summary>
    /// The text is right-aligned. Each line ends on the right-hand side of the bounding box.
    /// </summary>
    Right = 2,

    /// <summary>
    /// The text is fully justified. Each line will span the full width of the bounding box. The
    /// space between words is adjusted to achieve this.
    /// </summary>
    FullyJustified = 4
}