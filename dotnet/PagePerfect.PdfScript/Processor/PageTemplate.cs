namespace PagePerfect.PdfScript.Processor;

/// <summary>
/// The PageTemplate class represents a named set of page dimensions for use with
/// the "page" statement.
/// </summary>
/// <param name="name">The name of the template.</param>
/// <param name="width">The page width.</param>
/// <param name="height">The page height.</param>
public class PageTemplate(string name, float width, float height)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The name of the template.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The page width in points.
    /// </summary>
    public float Width { get; } = width;

    /// <summary>
    /// The page height in points.
    /// </summary>
    public float Height { get; } = height;
    #endregion
}
