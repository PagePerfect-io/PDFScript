namespace PagePerfect.PdfScript.Processor.Text;

/// <summary>
/// The VerticalTextAlignment enumeration lists the possible vertical alignment values
/// for the TextFormattingOptions class.
/// </summary>
public enum VerticalTextAlignment
{
    /// <summary>
    /// The text is placed at the top of the bounding box. The top of the first line matches the top
    /// of the bounding box.
    /// </summary>
    Top,

    /// <summary>
    /// The text is placed in the middle. The centre of the middle line of text matches the centre of the
    /// bounding box.
    /// </summary>
    Middle,

    /// <summary>
    /// The text is placed at the bottom of the bounding box. The bottom of the last line matches the
    /// bottom of the bounding box.
    /// </summary>
    Bottom
}

