namespace PagePerfect.PdfScript.Writer.Resources.Fonts.TrueType;

/// <summary>
/// The table class encapsulates the information required to locate a table within a TrueType font file. It is used internally
/// by the TrueTypeFontInfo class.
/// </summary>
/// <remarks>
/// Initialises a new Table instance.
/// </remarks>
/// <param name="tag">The tag to search the table by.</param>
/// <param name="checksum">The table's checksum.</param>
/// <param name="offset">The offset of the table in the file.</param>
/// <param name="length">The length of the table, in bytes.</param>
internal class Table(string tag, long checksum, long offset, long length)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The tag to search the table by.
    /// </summary> 
    public string Tag { get; } = tag;

    /// <summary>
    /// The table's checksum.
    /// </summary>
    public long Checksum { get; } = checksum;

    /// <summary>
    /// The table's offset in the file.
    /// </summary>
    public long Offset { get; } = offset;

    /// <summary>
    /// The table's length, in bytes.
    /// </summary>
    public long Length { get; } = length;
    #endregion
}