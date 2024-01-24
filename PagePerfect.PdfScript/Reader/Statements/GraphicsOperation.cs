using System.Text;
using PagePerfect.PdfScript.Utilities;

namespace PagePerfect.PdfScript.Reader.Statements;

/// <summary>
/// The GraphicsOperation class represents a graphics operation,
/// composeed of zero, one or more operands and a single operator. 
/// </summary>
public class GraphicsOperation(Operator @operator, PdfsValue[] operands)
 : PdfsStatement(PdfsStatementType.GraphicsOperation)
{
    // Private static fields
    // =====================
    #region Private static fields
    /// <summary>
    /// The operator names.
    /// </summary>
    private static readonly Dictionary<string, Operator> _operatorSymbols;
    private static readonly Dictionary<Operator, string> _operatorNames;
    private static readonly Dictionary<Operator, PdfsValueKind[]> _operatorOperands;
    #endregion



    // Type initialiser
    // ==================
    #region Type initialiser
    /// <summary>
    /// Initialises the type.
    /// </summary>
    static GraphicsOperation()
    {
        _operatorSymbols = [];
        _operatorNames = [];
        _operatorOperands = [];

        CreateOperatorLookups();
    }
    #endregion



    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The operator for this graphics operation.
    /// </summary>
    public Operator Operator { get; } = @operator;

    /// <summary>
    /// The operands for this graphics operation.
    /// </summary>
    public PdfsValue[] Operands { get; } = operands;
    #endregion



    // Base class overrides
    // ====================
    #region Base class overrides
    /// <summary>
    /// Returns a string representation of this graphics operation.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        foreach (var operand in Operands)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(operand);
        }

        sb.Append(' ');
        sb.Append(GetOperatorName());

        return sb.ToString();
    }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Retrieves this operation's operator name as it appears in a PDF
    /// content stream, or a .pdfs file.
    /// </summary>
    /// <returns>The operator name.</returns>
    public string GetOperatorName()
    {
        return _operatorNames[Operator];
    }
    #endregion



    // Internal implementation
    // =======================
    #region Internal implementation
    /// <summary>
    /// Parses a graphics operation from the specified operator and operand stack.
    /// This method will validate that the operator exists, and that the operands'
    /// types match the definition of the operator. 
    /// </summary>
    /// <param name="op">The operator name.</param>
    /// <param name="operandStack">The operands on the stack.</param>
    /// <returns>The GraphicsOperation instance.</returns>
    /// <exception cref="PdfsReaderException">The operator was not recognised, or the operands did not match.</exception>
    public static GraphicsOperation Parse(string op, Stack<PdfsValue> operandStack)
    {
        // Find the operator symbol, and throw if it does not exist.
        if (!_operatorSymbols.TryGetValue(op, out var @operator))
            throw new PdfsReaderException($"Invalid operator '{op}'.");

        // Get the expected operands for this operator.
        var operands = new Stack<PdfsValue>();
        var expectedOperands = _operatorOperands[@operator];
        foreach (var expected in expectedOperands)
        {
            if (operandStack.Count == 0)
                throw new PdfsReaderException($"Expected {expectedOperands.Length} operands.");

            var operand = operandStack.Pop();
            if (operand.Kind == PdfsValueKind.Variable)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (operand.Kind != expected)
                    throw new PdfsReaderException($"Expected operand of type '{expected}', but found '{operand.Kind}'.");

                operands.Push(operand);
            }
        }

        return new GraphicsOperation(@operator, [.. operands]);
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Creates the dictionaries that map operator names to symbols and vice versa,
    /// and list types for their operands.
    /// </summary>
    private static void CreateOperatorLookups()
    {
        foreach (var op in Enum.GetValues<Operator>())
        {
            var attr = op.GetAttribute<GraphicsOperationAttribute>();
            var name = attr?.Operator ?? op.ToString();

            _operatorNames.Add(op, name);
            _operatorSymbols.Add(name, op);
            _operatorOperands.Add(op, attr?.Operands.Reverse().ToArray() ?? []);
        }
    }
    #endregion
}