using System.Text;

namespace PagePerfect.PdfScript.Reader.Statements;

/// <summary>
/// The PageStatement class represents a  'page' statement in a .pdfs file.
/// This statement accepts two number operands or a name operand, and
/// sets the page dimensions for any subsequent page.
/// </summary>
public class PageStatement : PdfsStatement
{
    // Public properties
    // =================
    #region Public properties
    public string? Template { get; }

    public float Width { get; }

    public float Height { get; }
    #endregion



    // Instance initialisers
    // =====================
    #region Instance initialisers
    /// <summary>
    /// Initialises a new instance of the PageStatement class.
    /// </summary>
    /// <param name="template">The page template to use.</param>
    public PageStatement(string template) : base(PdfsStatementType.PageStatement)
    {
        Template = template;
    }

    /// <summary>
    /// Initialises a new instance of the PageStatement class.
    /// </summary>
    /// <param name="width">The width of the page, in points.</param>
    /// <param name="height">The height of the page, in point.</param>
    public PageStatement(float width, float height) : base(PdfsStatementType.PageStatement)
    {
        Width = width;
        Height = height;
    }
    #endregion



    // Base class overrides
    // ====================
    #region Base class overrides
    /// <summary>
    /// Returns a string representation of this statement.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        return Template is not null ? $"page {Template}" : $"page {Width} {Height}";
    }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Parses a page statement. This method expects to find either a name on the stack,
    /// or a set of two numbers.
    /// </summary>
    /// <param name="operandStack">The operand stack.</param>
    /// <returns>The page statment instance.</returns>
    /// <exception cref="PdfsReaderException">The available operands did not match.</exception>
    public static PageStatement Parse(Stack<PdfsValue> operandStack)
    {
        if (operandStack.Count == 0) throw new PdfsReaderException("Page statement expected an operand.");

        var op = operandStack.Pop();
        switch (GetResolvedKind(op))
        {
            case PdfsValueKind.Name:
                return new PageStatement(op.GetString());
            case PdfsValueKind.Number:
                if (operandStack.Count == 0) throw new PdfsReaderException("Page statment expected a name operand or two number operands.");
                var op2 = operandStack.Pop();
                if (PdfsValueKind.Number != GetResolvedKind(op2)) throw new PdfsReaderException("Page statment expected a name operand or two number operands.");
                return new PageStatement(op2.GetNumber(), op.GetNumber());
            default:
                throw new PdfsReaderException("Page statment expected a name operand or two number operands.");
        };
    }
    #endregion

}