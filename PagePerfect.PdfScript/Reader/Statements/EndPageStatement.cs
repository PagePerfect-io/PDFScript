namespace PagePerfect.PdfScript.Reader.Statements;

/// <summary>
/// The EndPageStatement class represents a  'endpage' statement in a .pdfs file.
/// </summary>
public class EndPageStatement : PdfsStatement
{
    // Instance initialisers
    // =====================
    #region Instance initialisers
    /// <summary>
    /// Initialises a new EndPageStatement instance.
    /// </summary>
    public EndPageStatement() : base(PdfsStatementType.EndPageStatement) { }
    #endregion
}