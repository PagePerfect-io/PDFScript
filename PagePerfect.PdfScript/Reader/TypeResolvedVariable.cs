namespace PagePerfect.PdfScript.Reader;

/// <summary>
/// The TypeResolvedVariable is a PdfsValue that represents a variable that has been resolved 
/// to a specific type. It is used by the PdfsReader to substitute variable references with their
/// type-resolved versions for the purpose of type-checking the operands of graphics operations.
/// </summary>
public class TypeResolvedVariable : PdfsValue
{
    // Instance initialiser
    // ====================
    #region Instance initialiser
    /// <summary>
    /// Initialises a new TypeResolvedVariable instance.
    /// </summary>
    /// <param name="variable">The original variable's name.</param>
    /// <param name="datatype">The resolved type of the variable.</param>
    /// <exception cref="PdfsReaderException">The original value must be of kind 'variable'.</exception>
    public TypeResolvedVariable(string variable, PdfsValueKind datatype) : base(variable, PdfsValueKind.Variable)
    {
        if (datatype == PdfsValueKind.Variable) throw new ArgumentException("A variable cannot be of type variable");

        ResolvedDatatype = datatype;
    }
    #endregion



    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The resolved type of this variable.
    /// </summary>
    public PdfsValueKind ResolvedDatatype { get; }
    #endregion 
}

