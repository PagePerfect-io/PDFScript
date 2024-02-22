namespace PagePerfect.PdfScript.Writer.Resources.Fonts.TrueType;

/// <summary>
/// The TrueTypeFontParseException class is thrown by the TrueTypeFontInfo class when a failure occurred during parsing of the font
/// file. Exception caught by the TrueTypeFontInfo class (such as I/O exceptions) are rethrown as TrueTypeFontParseException instances
/// with the original exception included for reference.
/// </summary>
public class TrueTypeFontParseException : Exception
{
    // Public constructors
    // ===================
    #region Public constructors
    /// <summary>
    /// Constructs a new TrueTypeFontParseException instance.
    /// </summary>
    public TrueTypeFontParseException() : base() { }

    /// <summary>
    /// Constructs a new TrueTypeFontParseException instance with the specified message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public TrueTypeFontParseException(string message) : base(message) { }

    /// <summary>
    /// Constructs a new TrueTypeFontParseException instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception</param>
    public TrueTypeFontParseException(string message, Exception innerException) : base(message, innerException) { }
    #endregion

}
