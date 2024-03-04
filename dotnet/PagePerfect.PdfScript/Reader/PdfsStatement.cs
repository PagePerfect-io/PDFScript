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



    // Protected implementation
    // ========================
    #region Protected implementation
    /// <summary>
    /// Retrieves the kind of PDFs value. If the value is a type-resolved variable,
    /// this method returns the resolved datatype. Otherwise, this method returns
    /// the value's kind.
    /// </summary>
    /// <param name="val">The value.</param>
    /// <returns>The resolved kind.</returns>
    public static PdfsValueKind GetResolvedKind(PdfsValue val)
    {
        return val.Kind switch
        {
            PdfsValueKind.Variable => ((TypeResolvedVariable)val).ResolvedDatatype,
            _ => val.Kind
        };
    }
    #endregion
}