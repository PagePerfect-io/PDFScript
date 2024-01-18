namespace PagePerfect.PdfScript.Reader;

/// <summary>
/// The PdfsStatement class represents a statement in a .pdfs document. This is an abstract class.
/// </summary>
/// <param name="type">The type of statement.</param>
public abstract class PdfsStatement(PdfsStatementType type)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The type of statement.
    /// </summary>
    public PdfsStatementType Type { get; } = type;
    #endregion
}