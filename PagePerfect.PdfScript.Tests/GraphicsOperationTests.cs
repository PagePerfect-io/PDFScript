using System.Text;
using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Reader.Statements;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The GraphicsOperationTests class contains tests for the GraphicsOperation class.
/// </summary>
public class GraphicsOperationTests
{
    // Public tests
    // ============
    #region Parsing tests
    /// <summary>
    /// The GraphicsOperation should parse a basic operator with no operands.
    /// </summary>
    [Fact]
    public void ShouldParseGraphicsOperationWithoutOperands()
    {
        var op = GraphicsOperation.Parse("q", []);
        Assert.NotNull(op);
        Assert.Equal(Operator.q, op.Operator);
        Assert.Empty(op.Operands);
        Assert.Equal("q", op.GetOperatorName());
    }

    /// <summary>
    /// The GraphicsOperation should parse an operator with a mapped name, and no operands.
    /// </summary>
    [Fact]
    public void ShouldParseMappedNameGraphicsOperationWithoutOperands()
    {
        var op = GraphicsOperation.Parse("f*", []);
        Assert.NotNull(op);
        Assert.Equal(Operator.fStar, op.Operator);
        Assert.Empty(op.Operands);
        Assert.Equal("f*", op.GetOperatorName());

        op = GraphicsOperation.Parse("T*", []);
        Assert.NotNull(op);
        Assert.Equal(Operator.TStar, op.Operator);
        Assert.Empty(op.Operands);
        Assert.Equal("T*", op.GetOperatorName());
    }

    /// <summary>
    /// The GraphicsOperation should parse an operator with operands.
    /// </summary>
    [Fact]
    public void ShouldParseGraphicsOperationWithOperands()
    {
        var operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(1));
        operands.Push(new PdfsValue(0));
        operands.Push(new PdfsValue(0));
        operands.Push(new PdfsValue(1));
        operands.Push(new PdfsValue(10));
        operands.Push(new PdfsValue(20));
        var op = GraphicsOperation.Parse("cm", operands);
        Assert.NotNull(op);
        Assert.Equal(Operator.cm, op.Operator);
        Assert.Equal("cm", op.GetOperatorName());
        Assert.Equal(6, op.Operands.Length);
        Assert.Equal(new PdfsValue(1), op.Operands[0]);
        Assert.Equal(new PdfsValue(0), op.Operands[1]);
        Assert.Equal(new PdfsValue(0), op.Operands[2]);
        Assert.Equal(new PdfsValue(1), op.Operands[3]);
        Assert.Equal(new PdfsValue(10), op.Operands[4]);
        Assert.Equal(new PdfsValue(20), op.Operands[5]);
    }

    /// <summary>
    /// The GraphicsOperation class should throw an exception when parsing an operator
    /// that does not exist.
    /// </summary>
    [Fact]
    public void ShouldThrowIfOperatorNotRecognised()
    {
        Assert.Throws<PdfsReaderException>(() => GraphicsOperation.Parse("foo", []));
    }

    /// <summary>
    /// The GraphicsOperation class should throw an exception when parsing an operator
    /// whose operands do not match the definition.
    /// </summary>
    [Fact]
    public void ShouldThrowIfOperandsDontMatch()
    {
        // Wrong type
        var operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(1));
        operands.Push(new PdfsValue("Hello, World!"));
        operands.Push(new PdfsValue(0));
        operands.Push(new PdfsValue(1));
        operands.Push(new PdfsValue(10));
        operands.Push(new PdfsValue(20));
        Assert.Throws<PdfsReaderException>(() => GraphicsOperation.Parse("cm", operands));

        // Too few operands
        operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(1));
        operands.Push(new PdfsValue(0));
        operands.Push(new PdfsValue(0));
        operands.Push(new PdfsValue(1));
        Assert.Throws<PdfsReaderException>(() => GraphicsOperation.Parse("cm", operands));
    }

    /// <summary>
    /// The GraphicsOperation class should parse an operation with multiple operand
    /// options - examples are sc, SC, scn, SCN.
    /// </summary>
    [Fact]
    public void ShouldParseGraphicsOperationWithMultipleOperandOptions()
    {
        // sc with one operand
        var operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(1));
        var op = GraphicsOperation.Parse("sc", operands);
        Assert.NotNull(op);
        Assert.Equal(Operator.sc, op.Operator);
        Assert.Equal("sc", op.GetOperatorName());
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(1), op.Operands[0]);

        // scn with one operand
        operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(1));
        op = GraphicsOperation.Parse("scn", operands);
        Assert.NotNull(op);
        Assert.Equal(Operator.scn, op.Operator);
        Assert.Equal("scn", op.GetOperatorName());
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(1), op.Operands[0]);

        // scn with four operands - three numbers and a name
        operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(1));
        operands.Push(new PdfsValue(0.9f));
        operands.Push(new PdfsValue(0.8f));
        operands.Push(new PdfsValue("Edwin", PdfsValueKind.Name));
        op = GraphicsOperation.Parse("scn", operands);
        Assert.NotNull(op);
        Assert.Equal(Operator.scn, op.Operator);
        Assert.Equal("scn", op.GetOperatorName());
        Assert.Equal(4, op.Operands.Length);
        Assert.Equal(new PdfsValue(1), op.Operands[0]);
        Assert.Equal(new PdfsValue(0.9f), op.Operands[1]);
        Assert.Equal(new PdfsValue(0.8f), op.Operands[2]);
        Assert.Equal(new PdfsValue("Edwin", PdfsValueKind.Name), op.Operands[3]);

        // scn with four operands - four numbers
        operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(1));
        operands.Push(new PdfsValue(0.9f));
        operands.Push(new PdfsValue(0.8f));
        operands.Push(new PdfsValue(0.7f));
        op = GraphicsOperation.Parse("scn", operands);
        Assert.NotNull(op);
        Assert.Equal(Operator.scn, op.Operator);
        Assert.Equal("scn", op.GetOperatorName());
        Assert.Equal(4, op.Operands.Length);
        Assert.Equal(new PdfsValue(1), op.Operands[0]);
        Assert.Equal(new PdfsValue(0.9f), op.Operands[1]);
        Assert.Equal(new PdfsValue(0.8f), op.Operands[2]);
        Assert.Equal(new PdfsValue(0.7f), op.Operands[3]);
    }

    /// <summary>
    /// The GraphicsOperation class should throw an exception when parsing an operation
    /// with multiple options for operands, when the operands do not match any of the
    /// options. 
    /// </summary>
    [Fact]
    public void ShouldThrowWhenNoMatchFoundForOperatorWithMultipleOperandOptions()
    {
        // Wrong type
        var operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(1));
        operands.Push(new PdfsValue(0.9f));
        operands.Push(new PdfsValue("Edwin", PdfsValueKind.String));
        Assert.Throws<PdfsReaderException>(() => GraphicsOperation.Parse("scn", operands));

        // No match for an operation with no operands
        operands = new Stack<PdfsValue>();
        Assert.Throws<PdfsReaderException>(() => GraphicsOperation.Parse("scn", operands));
    }

    /// <summary>
    /// Te GraphicsOperation class should parse an operation where one or more operands
    /// are type-resolved variables.
    [Fact]
    public void ShouldParseOperationWithVariableOperands()
    {
        // rg with three operands - one is a variable
        var operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(1));
        operands.Push(new TypeResolvedVariable("g", PdfsValueKind.Number));
        operands.Push(new PdfsValue(0.8f));

        var op = GraphicsOperation.Parse("rg", operands);
        Assert.NotNull(op);
        Assert.Equal(Operator.rg, op.Operator);
        Assert.Equal("rg", op.GetOperatorName());
        Assert.Equal(3, op.Operands.Length);
        Assert.Equal(new PdfsValue(1), op.Operands[0]);
        Assert.Equal(new PdfsValue(0.8f), op.Operands[2]);
    }

    /// <summary>
    /// Te GraphicsOperation class should throw an exception when an
    /// operand is a type-resolved variable that has an incorrect datatype.
    [Fact]
    public void ShouldThrowWhenVariableTypeIncorrect()
    {
        // rg with three operands - one is a variable with incorrext type
        var operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(1));
        operands.Push(new TypeResolvedVariable("g", PdfsValueKind.Name));
        operands.Push(new PdfsValue(0.8f));
        Assert.Throws<PdfsReaderException>(() => GraphicsOperation.Parse("rg", operands));
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Creates a memory stream out of a string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <returns>The memory stream.</returns>
    private static MemoryStream S(string source)
    {
        var bytes = Encoding.ASCII.GetBytes(source);
        return new MemoryStream(bytes);
    }
    #endregion
}