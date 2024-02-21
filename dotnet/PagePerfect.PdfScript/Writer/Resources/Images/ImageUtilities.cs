namespace PagePerfect.PdfScript.Writer.Resources.Images;

/// <summary>
/// The ImageUtilities class is a static library class that provides convenience methods that deal
/// with images.
/// </summary>
public static class ImageUtilities
{
    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Retrieves the type of the image contained in the specified file. The type matching is based on filename only, not
    /// on the actual contents of the file stream.
    /// </summary>
    /// <param name="path">The file to retrieve the image information from.</param>
    /// <returns>The type of image.</returns>
    public static ImageType GetImageType(string path) => Path.GetExtension(path)?.ToLower() switch
    {
        ".jpeg" => ImageType.Jpeg,
        ".jpg" => ImageType.Jpeg,
        _ => ImageType.Unknown
    };

    #endregion
}