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
    private static readonly Dictionary<Operator, GraphicsObject> _operatorObjects;
    private static readonly Dictionary<Operator, PdfsValueKind[][]> _operatorOperands;
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
        _operatorObjects = [];
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
    /// Determines if the operation is allowed in the specified graphics object.
    /// </summary>
    /// <param name="graphicsObject">The graphics object.</param>
    /// <returns>True if the operation is allowed; false otherwise.</returns>
    public bool IsAllowedIn(GraphicsObject graphicsObject) => (_operatorObjects[Operator] & graphicsObject) == graphicsObject;

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

        // Retrieve the options for operands. If there is just the one option, we can
        // parse the operands immediately. If there are multiple options, we try to find
        // a match first, and then base operands on the match.
        var options = _operatorOperands[@operator];
        if (options.Length == 0) return new GraphicsOperation(@operator, []);
        if (options.Length == 1) return TryParseOperands(@operator, operandStack, options[0]);

        // Try to find a match for the operands.
        var match = TryFindOperandMatch(operandStack, options);
        if (null == match) throw new PdfsReaderException($"No match found for operands of operator '{op}'.");

        // Parse the operands.
        var operands = new Stack<PdfsValue>();
        foreach (var expected in match)
        {
            var operand = operandStack.Pop();
            operands.Push(operand);

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
            var attr = op.GetAttributes<GraphicsOperationAttribute>();
            if (attr.Length == 0)
            {
                _operatorNames.Add(op, op.ToString());
                _operatorSymbols.Add(op.ToString(), op);
                _operatorOperands.Add(op, []);
                _operatorObjects.Add(op, GraphicsObject.Any);
                continue;
            }

            // Pick the name and allowed-in based on the first attribute's operator name, if available.
            var name = attr[0].Operator ?? op.ToString();
            _operatorObjects.Add(op, attr[0].AllowedIn);
            _operatorNames.Add(op, name);
            _operatorSymbols.Add(name, op);

            var options = attr
                .Select(a => a.Operands.Reverse().ToArray())
                .OrderByDescending(a => a.Length)
                .ToArray();

            _operatorOperands.Add(op, options);
        }
    }

    private static PdfsValueKind[]? TryFindOperandMatch(
        Stack<PdfsValue> operandStack,
        PdfsValueKind[][] options)
    {
        var copy = operandStack.ToArray();

        foreach (var option in options)
        {
            if (copy.Length < option.Length) continue;

            int n;
            for (n = 0; n < option.Length; n++)
            {
                var kind = copy[n].Kind;
                if (kind == PdfsValueKind.Variable)
                {
                    var resolved = copy[n] as TypeResolvedVariable;
                    kind = resolved!.ResolvedDatatype;
                }
                if (kind != option[n]) break;
            }
            if (n == option.Length) return option;
        }
        return null;
    }

    private static GraphicsOperation TryParseOperands(
        Operator @operator,
        Stack<PdfsValue> operandStack,
        PdfsValueKind[] expectedOperands)
    {
        var operands = new Stack<PdfsValue>();
        foreach (var expected in expectedOperands)
        {
            if (operandStack.Count == 0)
                throw new PdfsReaderException($"Expected {expectedOperands.Length} operands.");

            var operand = operandStack.Pop();
            if (operand.Kind == PdfsValueKind.Variable)
            {
                var resolved = operand as TypeResolvedVariable;
                if (resolved!.ResolvedDatatype != expected)
                    throw new PdfsReaderException($"Expected operand of type '{expected}', but found a variable of type '{resolved.ResolvedDatatype}'.");
            }
            else if (operand.Kind != expected)
                throw new PdfsReaderException($"Expected operand of type '{expected}', but found '{operand.Kind}'.");

            operands.Push(operand);

        }

        return new GraphicsOperation(@operator, [.. operands]);
    }
    #endregion
}