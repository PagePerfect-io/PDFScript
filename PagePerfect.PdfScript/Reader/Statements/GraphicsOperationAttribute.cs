
namespace PagePerfect.PdfScript.Reader.Statements;

/// <summary>
/// The GraphicsOperationAttribute is an attribute that can be applied to an Operator enumerated
/// value. It is used to type-check operations in a .pdfs file, and to provide specific mappings
/// between the enumerated values and the operator names, where they are not the same
/// </summary>
/// <param name="operands">The operands</param>
[AttributeUsage(AttributeTargets.Field)]
public class GraphicsOperationAttribute(params PdfsValueKind[] operands) : Attribute
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The types of the operands.
    /// </summary>
    public PdfsValueKind[] Operands { get; } = operands;

    /// <summary>
    /// Optionally, the operator name if it doesn't match the enumerated value's name.
    /// E.g. The T* operator is listed in the Operator enumeration as 'TStar'. 
    /// </summary>
    public string? Operator { get; set; }
    #endregion
}