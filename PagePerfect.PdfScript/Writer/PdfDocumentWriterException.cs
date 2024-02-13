namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The PdfDocumentWriterException is thrown by the PdfDocumentWriter class.
/// </summary>
public class PdfDocumentWriterException : Exception
{
    // Public initialisers
    // ===================
    #region Public initialisers
    /// <summary>
    /// Initialises a new, empty PdfDocumentWriterException instance.
    /// </summary>
    public PdfDocumentWriterException() { }

    /// <summary>
    /// Initialises a new PdfDocumentWriterException instance based
    /// on the specified message.
    /// </summary>
    public PdfDocumentWriterException(string message) : base(message) { }

    /// <summary>
    /// Initialises a new PdfDocumentWriterException instance based
    /// on the specified message and source exception.
    /// </summary>
    public PdfDocumentWriterException(string message, Exception inner) : base(message, inner) { }
    #endregion
}