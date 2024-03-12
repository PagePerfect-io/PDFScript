namespace PagePerfect.PdfScript.Processor.Text;

/// <summary>
/// The TextAlignmentOptions class provides options for aligning text.
/// It used by the DrawTextLines method overrides of the Canvas class.
/// </summary>
/// <remarks>
/// Initialises a new instance of the TextAlignmentOptions class.
/// </remarks>
/// <param name="horizontalAlignment">The horizontal alignment.</param>
/// <param name="verticalAlignment">The vertical alignment.</param>
public class TextAlignmentOptions(
    HorizontalTextAlignment horizontalAlignment,
    VerticalTextAlignment verticalAlignment)
{

    #region Instance initialiser
    #endregion



    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The horizontal alignment within a line.
    /// </summary> 
    public HorizontalTextAlignment HorizontalAlignment { get; set; } = horizontalAlignment;

    /// <summary>
    /// The vertical alignment of the lines.
    /// </summary>
    public VerticalTextAlignment VerticalAlignment { get; set; } = verticalAlignment;

    /// <summary>
    /// Retrieves the default options.
    /// </summary>
    public static TextAlignmentOptions Default
    {
        get => new(HorizontalTextAlignment.Left, VerticalTextAlignment.Top);
    }
    #endregion



    // Base class overrides
    // ====================
    #region Base class overrides
    /// <summary>
    /// Determines if this instance is equal to another instance.
    /// Two TextAlignmentOptions instances are equal if their HorizontalAlignment and VerticalAlignment
    /// properties are equal. 
    /// </summary>
    /// <param name="obj">The other instance.</param>
    /// <returns>True if the instances are equal; false otherwise.</returns>
    public override bool Equals(object? obj) => obj is TextAlignmentOptions options &&
               HorizontalAlignment == options.HorizontalAlignment &&
               VerticalAlignment == options.VerticalAlignment;

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HorizontalAlignment, VerticalAlignment);

    /// <summary>
    /// Returns a string represenrtation of this instance.
    /// </summary>
    /// <returns>The string.</returns>
    public override string ToString()
    {
        return $"({HorizontalAlignment}, {VerticalAlignment})";
    }

    #endregion


    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Clones the options. This method returns a new instance based on the current instance.
    /// </summary>
    /// <returns>The new instance.</returns>
    public TextAlignmentOptions Clone()
    {
        return new TextAlignmentOptions(HorizontalAlignment, VerticalAlignment);
    }
    #endregion
}