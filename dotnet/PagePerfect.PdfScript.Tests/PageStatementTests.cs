using System.Text;
using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Reader.Statements;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The PageStatementTests class contains tests for the PageStatement class.
/// </summary>
public class PageStatementTests
{
    // Public tests
    // ============
    #region Parsing tests
    /// <summary>
    /// The PageStatement class should parse a page statement with a name.
    /// </summary>
    [Fact]
    public void ShouldParsePageStatementWithName()
    {
        var operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue("/A4", PdfsValueKind.Name));

        var page = PageStatement.Parse(operands);
        Assert.NotNull(page);
        Assert.Equal("/A4", page.Template);
    }

    /// <summary>
    /// The PageStatement class should parse a page statement with a name.
    /// </summary>
    [Fact]
    public void ShouldParsePageStatementWithNumbers()
    {
        var operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(595));
        operands.Push(new PdfsValue(841));

        var page = PageStatement.Parse(operands);
        Assert.NotNull(page);
        Assert.Null(page.Template);
        Assert.Equal(595, page.Width);
        Assert.Equal(841, page.Height);
    }

    /// <summary>
    /// The PageStatement class should throw an exception when no operands are available
    /// on the stack.
    /// </summary>
    [Fact]
    public void ShouldThrowWhenPageStatementHasNoOperands()
    {
        Assert.Throws<PdfsReaderException>(() => PageStatement.Parse([]));
    }

    /// <summary>
    /// The PageStatement class should throw an exception when operands are invalid.
    /// </summary>
    [Fact]
    public void ShouldThrowWhenPageStatementHasInvalidOperands()
    {
        // A number, but only one
        var operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(595));
        Assert.Throws<PdfsReaderException>(() => PageStatement.Parse(operands));

        // Not a number or a name
        operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue("Edwin", PdfsValueKind.String));
        Assert.Throws<PdfsReaderException>(() => PageStatement.Parse(operands));

        // A number followed by a non-number operand
        operands = new Stack<PdfsValue>();
        operands.Push(new PdfsValue(595));
        operands.Push(new PdfsValue("Edwin", PdfsValueKind.String));
        Assert.Throws<PdfsReaderException>(() => PageStatement.Parse(operands));
    }
    #endregion
}