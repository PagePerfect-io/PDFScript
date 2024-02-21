namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The Image class represents an image that can be embedded in a PDF document.
/// </summary>
/// <remarks>
/// Initialises a new PdfFont instance.
/// </remarks>
/// <param name="obj">The PDF object that this resource refers to.</param>
/// <param name="identifier">The identifier that the object will be known as in the current page</param>
/// <param name="filename">The image's file name.</param>
/// <exception cref="ArgumentException">The obect reference, identifier or typename cannot be Null or empty.</exception>
public class Image(PdfObjectReference obj, string identifier, string filename, object? tag = null)
: PdfResourceReference(obj, identifier, PdfResourceType.Image, tag)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The file name for the image.
    /// </summary>
    /// <value></value>
    public string Filename { get; } = filename;
    #endregion
}