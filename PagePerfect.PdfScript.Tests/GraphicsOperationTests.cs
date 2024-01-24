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
        var op = GraphicsOperation.Parse("m", []);
        Assert.NotNull(op);
        Assert.Equal(Operator.m, op.Operator);
        Assert.Empty(op.Operands);
        Assert.Equal("m", op.GetOperatorName());
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

        op = GraphicsOperation.Parse("'", []);
        Assert.NotNull(op);
        Assert.Equal(Operator.Apos, op.Operator);
        Assert.Empty(op.Operands);
        Assert.Equal("'", op.GetOperatorName());

        op = GraphicsOperation.Parse("\"", []);
        Assert.NotNull(op);
        Assert.Equal(Operator.Quot, op.Operator);
        Assert.Empty(op.Operands);
        Assert.Equal("\"", op.GetOperatorName());
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