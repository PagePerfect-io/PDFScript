namespace PagePerfect.PdfScript.Writer.Resources.Images;

/// <summary>
/// The ImageInfo class encapulates information about an image that needs
/// to be written to a PDF document.
/// </summary>
public class ImageInfo
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The colour space of the image.
    /// </summary>
    public ColourSpace ColourSpace { get; set; }

    /// <summary>
    /// The intrinsic width of the image.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// The intrinsic height of the image.
    /// </summary>
    public int Height { get; set; }
    #endregion
}