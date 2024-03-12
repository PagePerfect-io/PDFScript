namespace PagePerfect.PdfScript.Processor.Text;

/// <summary>
/// The Line class represents a line of text positioned by the TextFlowEngine.
/// It includes a bounding box for the line, in absolute coordinates, and a list of
/// words that make up the line. 
/// </summary>
/// <remarks>
/// Initialises a new instance of the Line class.
/// </remarks>
/// <param name="boundingBox">The bounding box of the line.</param>
/// <param name="spans">The spans in this line.</param>
public class Line(PdfRectangle boundingBox, IEnumerable<LineSpan> spans)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The bounding box of the line, in absolute coordinates.
    /// </summary>
    public PdfRectangle BoundingBox { get; set; } = boundingBox;

    /// <summary>
    /// The list of spans that make up the line.
    /// </summary> 
    public LineSpan[] Spans { get; } = spans.ToArray();
    #endregion
}