namespace PagePerfect.PdfScript.Reader;

/// <summary>
/// Then GraphicsObject enumeration lists the possible graphics objects that exist within
/// a PDF document's graphics state machine.
/// </summary>
[Flags]
public enum GraphicsObject
{
    /// <summary>
    /// Value representing none of the graphics objects.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// This is the page-level object.
    /// </summary>
    Page = 0x1,

    /// <summary>
    /// This is a path object.
    /// </summary>
    Path = 0x2,

    /// <summary>
    /// This is a text object.
    /// </summary>
    Text = 0x4,

    /// <summary>
    /// Value representing any graphics object.
    /// </summary>
    Any = 0x7,

}