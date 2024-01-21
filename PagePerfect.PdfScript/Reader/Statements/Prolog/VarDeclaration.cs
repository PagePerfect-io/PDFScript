namespace PagePerfect.PdfScript.Reader.Statements.Prolog;

/// <summary>
/// The VarDeclaration class represents a  '# var' prolog statement in a .pdfs file.
/// </summary>
/// <remarks>
/// Initialises a new Var instance.
/// </remarks>
public class VarDeclaration(string name, PdfsValueKind datatype, PdfsValue value)
: PrologStatement(PrologStatementType.VarDeclaration)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The variable's name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The datatype of this variable.
    /// </summary>
    public PdfsValueKind Datatype { get; } = datatype;

    /// <summary>
    /// The initial value of this variable.
    /// </summary>
    public PdfsValue Value { get; } = value;
    #endregion
}