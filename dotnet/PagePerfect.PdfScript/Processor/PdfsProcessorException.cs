namespace PagePerfect.PdfScript.Processor;

/// <summary>
/// The PdfsProcessorException is thrown by the PdfsProcessor class.
/// </summary>
public class PdfsProcessorException : Exception
{
    // Public initialisers
    // ===================
    #region Public initialisers
    /// <summary>
    /// Initialises a new, empty PdfsProcessorException instance.
    /// </summary>
    public PdfsProcessorException() { }

    /// <summary>
    /// Initialises a new PdfsProcessorException instance based
    /// on the specified message.
    /// </summary>
    public PdfsProcessorException(string message) : base(message) { }

    /// <summary>
    /// Initialises a new PdfsProcessorException instance based
    /// on the specified message and source exception.
    /// </summary>
    public PdfsProcessorException(string message, Exception inner) : base(message, inner) { }
    #endregion
}