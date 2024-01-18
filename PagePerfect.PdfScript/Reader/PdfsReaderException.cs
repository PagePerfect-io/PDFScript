namespace PagePerfect.PdfScript.Reader;

/// <summary>
/// The PdfsReaderException is thrown by the PdfsReader class.
/// </summary>
public class PdfsReaderException : Exception
{
    // Public initialisers
    // ===================
    #region Public initialisers
    /// <summary>
    /// Initialises a new, empty PdfsReaderException instance.
    /// </summary>
    public PdfsReaderException() { }

    /// <summary>
    /// Initialises a new PdfsReaderException instance based
    /// on the specified message.
    /// </summary>
    public PdfsReaderException(string message) : base(message) { }

    /// <summary>
    /// Initialises a new PdfsReaderException instance based
    /// on the specified message and source exception.
    /// </summary>
    public PdfsReaderException(string message, Exception inner) : base(message, inner) { }
    #endregion
}