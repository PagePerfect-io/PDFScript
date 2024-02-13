namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The DisplayOrientation enumeration determines how a page is shown on the screen in PDF Readers.
/// /// This affects the way the page is shown in PDF viewers, but does not affect the page size.
/// </summary>
public enum DisplayOrientation
{
    /// <summary>
    /// The page is shown rotated clockwise. This means a portrait page appears a landscape page.
    /// </summary>
    RotateClockwise = 270,

    /// <summary>
    /// The page is not rotated. So, a portrait page would be shown as a portrait page.
    /// </summary>
    Regular = 0,

    /// <summary>
    /// The page is shown rotated 90 degrees counter clockwise. This means a portrait page 
    /// appears as an upside-down landscape page.
    /// </summary>
    RotateCounterClockwise = 90,

    /// <summary>
    /// The page is displayed rotated 180 degrees. So, a portrait page would be shown
    /// upside-down.
    /// </summary>
    Rotate180 = 180
}
