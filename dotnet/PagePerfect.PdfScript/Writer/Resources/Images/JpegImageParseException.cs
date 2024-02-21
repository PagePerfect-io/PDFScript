using System.Runtime.Serialization;

namespace PagePerfect.PdfScript.Writer.Resources.Images;

/// <summary>
/// The JpegImageParseException class is thrown by the JpegImageInfo class when an error occurs during parsing of JPEG image
/// information.
/// </summary>
public class JpegImageParseException : PdfDocumentWriterException
{
    // Public constructors
    // ===================
    #region Public constructors
    /// <summary>
    /// Constructs a new JpegImageParseException instance.
    /// </summary>
    public JpegImageParseException() : base() { }

    /// <summary>
    /// Constructs a new JpegImageParseException instance with the specified message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public JpegImageParseException(string message) : base(message) { }

    /// <summary>
    /// Constructs a new JpegImageParseException instance with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception</param>
    public JpegImageParseException(string message, Exception innerException) : base(message, innerException) { }
    #endregion

}
