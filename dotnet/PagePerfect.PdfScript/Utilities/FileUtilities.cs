namespace PagePerfect.PdfScript.Utilities;

/// <summary>
/// The FileUtilities class contains utility methods for working with files.
/// </summary>
public static class FileUtilities
{
    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Normalises a path, returning the absolute path and its type. For a relative local-systen path,
    /// this method returns the absolute path. For an aboslute local-system path, this method returns
    /// the path as is. For an internet path, this method returns the path as is. This method also returns
    /// a value indicating if the path is a local file, an internet file, or an unknown file type.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>A type with the normaised path and its type.</returns>
    public static (string, FileLocationType) NormalisePath(string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            return uri.Scheme switch
            {
                "file" => (Path.GetFullPath(path), FileLocationType.LocalFile),
                "http" => (path, FileLocationType.Internet),
                "https" => (path, FileLocationType.Internet),
                _ => (path, FileLocationType.Unknown)
            };
        }

        try
        {
            return (Path.GetFullPath(path), FileLocationType.LocalFile);
        }
        catch (ArgumentException)
        {
            return (path, FileLocationType.Unknown);
        }
    }
    #endregion
}
