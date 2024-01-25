using System.Text;
using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Reader.Statements;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The PdfsReaderTests class contains tests for the PdfsReader class.
/// </summary>
public class PdfsReaderTests
{
    // Public tests
    // ============
    #region Overall tests
    /// <summary>
    /// The reader should return False when it cannot read a statement.
    /// </summary>
    [Fact]
    public async Task ShouldReturnFalseWhenNoStatementRead()
    {
        using var stream = S("20 (Hello, World)");
        var reader = new PdfsReader(stream);
        Assert.False(await reader.Read());
    }
    #endregion

    #region Flow control statement
    /// <summary>
    /// The reader should read a "endpage" statement.
    /// </summary>
    [Fact]
    public async Task ShouldReadEndPageStatement()
    {
        using var stream = S("endpage 10 20 m");
        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.EndPageStatement, reader.Statement.Type);
        Assert.IsType<EndPageStatement>(reader.Statement);
    }
    #endregion

    #region Reading Prologue statements
    /// <summary>
    /// The PdfsReader should read a "var" prolog statement.
    /// </summary>
    [Fact]
    public async Task ShouldReadVarPrologStatement()
    {
        using var stream = S("# var $MyFont /Name /Helvetica");
        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.PrologStatement, reader.Statement.Type);
        Assert.IsType<Reader.Statements.Prolog.VarDeclaration>(reader.Statement);
        var @var = (Reader.Statements.Prolog.VarDeclaration)reader.Statement;
        Assert.Equal(PrologStatementType.VarDeclaration, @var.PrologType);

    }

    /// <summary>
    /// The PdfsReader should throw when a "var" prolog statement
    /// is missing its value.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenValueMissingInVarPrologStatement()
    {
        using var stream = S("# var $MyFont /Name");
        var reader = new PdfsReader(stream);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
    }

    /// <summary>
    /// The PdfsReader should throw when a "var" prolog statement
    /// uses an invalid 'type' value.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenTypeInvalidInVarPrologStatement()
    {
        // Must use one of the known datatypes.
        using (var stream = S("# var $MyFont /Unknown /Helvetica"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }

        // Can't use a keyword or other token type, only a name.
        using (var stream = S("# var $MyFont name /Helvetica"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }
    }

    /// <summary>
    /// The PdfsReader should throw when a "var" prolog statement
    /// uses an invalid 'type' value.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenVariableNameInvalidInVarPrologStatement()
    {
        // Must use the $var notation, can't use a string or name or anything else.
        using (var stream = S("# var /MyFont /Name /Helvetica"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }
        using (var stream = S("# var myFont /Name /Helvetica"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }
    }

    /// <summary>
    /// The PdfsReader should throw when a "var" prolog statement
    /// uses a value whose type doesn't match the type specified.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenValueTypeMismatchInVarPrologStatement()
    {
        // Type is /Name, but value is a number.
        using (var stream = S("# var $MyFont /Name 20"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }
    }

    /// <summary>
    /// The PdfsReader should read a "resource" prolog statement.
    /// </summary>
    [Fact]
    public async Task ShouldReadResourcePrologStatement()
    {
        using var stream = S("# resource /MyImage /Image (https://example.com/image.png)");
        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.PrologStatement, reader.Statement.Type);
        Assert.IsType<Reader.Statements.Prolog.ResourceDeclaration>(reader.Statement);
        var @resource = (Reader.Statements.Prolog.ResourceDeclaration)reader.Statement;
        Assert.Equal(PrologStatementType.ResourceDeclaration, @resource.PrologType);
    }

    /// <summary>
    /// The PdfsReader class should throw an exception when the location for a resource
    /// is invalid. 
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenResourceLocationInvalidInResourcePrologStatement()
    {
        // Location is required.
        using (var stream = S("# resource /MyImage /Image"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }

        // Location must be a string.
        using (var stream = S("# resource /MyImage /Image 20"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }
    }

    /// <summary>
    /// The PdfsReader class should throw an exception when the resource
    /// type is invalid. 
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenResourceTypeInvalidInResourcePrologStatement()
    {
        // Resource type must be /Image or /Font, and is a required field.
        using (var stream = S("# resource /MyImage (https://example.com/image.png)"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }

        using (var stream = S("# resource /MyImage /Stuff (https://example.com/image.png)"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }
    }

    /// <summary>
    /// The PdfsReader class should throw an exception when the resource
    /// name is invalid. 
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenResourceNameInvalidInResourcePrologStatement()
    {
        // Resource name must be a name (/Name) and is a required field.
        using (var stream = S("# resource /Image (https://example.com/image.png)"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }

        using (var stream = S("# resource (MyImage) /Image (https://example.com/image.png)"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }

        using (var stream = S("# resource $MyImage /Image (https://example.com/image.png)"))
        {
            var reader = new PdfsReader(stream);
            await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
        }
    }
    #endregion

    #region Reading Graphics instructions - special graphics state
    /// <summary>
    /// The PdfsReader should read "q" and "Q" operations, which preserve and restore
    /// the graphics state, respectively.
    /// </summary>
    [Fact]
    public async Task ShouldReadPreserveAndRestoreStateOperations()
    {
        using var stream = S("q Q");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.q, op.Operator);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Q, op.Operator);
    }

    /// <summary>
    /// The PdfsReader should read "cm" operations, which set the current transformation
    /// matrix.
    /// </summary>
    [Fact]
    public async Task ShouldReadSetCTMOperation()
    {
        using var stream = S("1 0 0 1 10 20 cm");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.cm, op.Operator);
        Assert.Equal(6, op.Operands.Length);
        Assert.Equal(new PdfsValue(1), op.Operands[0]);
        Assert.Equal(new PdfsValue(0), op.Operands[1]);
        Assert.Equal(new PdfsValue(0), op.Operands[2]);
        Assert.Equal(new PdfsValue(1), op.Operands[3]);
        Assert.Equal(new PdfsValue(10), op.Operands[4]);
        Assert.Equal(new PdfsValue(20), op.Operands[5]);
    }
    #endregion

    #region Reading Graphics instructions - general graphics instructions
    /// <summary>
    /// The PdfsReader class should read operations that set the line width,
    /// join style, miter limit, and line cap style.
    /// </summary>
    [Fact]
    public async Task ShouldReadLineStyleOperations()
    {
        using var stream = S("10 w 1 J 1 j 10 M");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.w, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(10), op.Operands[0]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.J, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(1), op.Operands[0]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.j, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(1), op.Operands[0]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.M, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(10), op.Operands[0]);
    }

    /// <summary>
    /// The PdfsReader class should read a "d" operation that sets the current
    /// dash pattern.
    /// </summary>
    [Fact]
    public async Task ShouldReadLineDashPatternOperation()
    {
        using var stream = S("[1 2 3 4] 5 d");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.d, op.Operator);
        Assert.Equal(2, op.Operands.Length);
        Assert.Equal(new PdfsValue(5), op.Operands[1]);
        Assert.Equal(PdfsValueKind.Array, op.Operands[0].Kind);
        var array = op.Operands[0].GetArray();
        Assert.Equal(4, array.Length);
        Assert.Equal(new PdfsValue(1), array[0]);
        Assert.Equal(new PdfsValue(2), array[1]);
        Assert.Equal(new PdfsValue(3), array[2]);
        Assert.Equal(new PdfsValue(4), array[3]);
    }

    /// <summary>
    /// The PdfsReader class should read an "ri" operation that sets the
    /// rendering intent.
    /// </summary>
    [Fact]
    public async Task ShouldReadRenderingIntentOperation()
    {
        using var stream = S("/AbsoluteColormetric ri");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.ri, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue("/AbsoluteColormetric", PdfsValueKind.Name), op.Operands[0]);
    }

    /// <summary>
    /// The PdfsReader class should read the "i" operation that sets the
    /// flatness tolerance.
    /// </summary>
    [Fact]
    public async Task ShouldReadFlatnessToleranceOperation()
    {
        using var stream = S("10 i");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.i, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(10), op.Operands[0]);
    }

    /// <summary>
    /// The PdfsReader class should read the "gs" operation that injects
    /// a graphics state.
    /// </summary>
    [Fact]
    public async Task ShouldReadGraphicsStateOperation()
    {
        using var stream = S("/MyState gs");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.gs, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue("/MyState", PdfsValueKind.Name), op.Operands[0]);
    }
    #endregion

    #region Reading Graphics instructions - path construction instructions
    [Fact]
    public async Task ShouldReadMoveToOperation()
    {
        using var stream = S("10 20 m");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.m, op.Operator);
        Assert.Equal(2, op.Operands.Length);
        Assert.Equal(new PdfsValue(10), op.Operands[0]);
        Assert.Equal(new PdfsValue(20), op.Operands[1]);
    }

    [Fact]
    public async Task ShouldReadLineToOperation()
    {
        using var stream = S("100 20 l");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.l, op.Operator);
        Assert.Equal(2, op.Operands.Length);
        Assert.Equal(new PdfsValue(100), op.Operands[0]);
        Assert.Equal(new PdfsValue(20), op.Operands[1]);
    }

    [Fact]
    public async Task ShouldReadCubicBezierCurveOperations()
    {
        using (var stream = S("100 100 150 100 150 20 c"))
        {
            var reader = new PdfsReader(stream);
            Assert.True(await reader.Read());
            Assert.NotNull(reader.Statement);
            Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
            Assert.IsType<GraphicsOperation>(reader.Statement);
            var op = (GraphicsOperation)reader.Statement;
            Assert.Equal(Operator.c, op.Operator);
            Assert.Equal(6, op.Operands.Length);
            Assert.Equal(new PdfsValue(100), op.Operands[0]);
            Assert.Equal(new PdfsValue(100), op.Operands[1]);
            Assert.Equal(new PdfsValue(150), op.Operands[2]);
            Assert.Equal(new PdfsValue(100), op.Operands[3]);
            Assert.Equal(new PdfsValue(150), op.Operands[4]);
            Assert.Equal(new PdfsValue(20), op.Operands[5]);
        }
        using (var stream = S("125 100 150 20 v"))
        {

            var reader = new PdfsReader(stream);
            Assert.True(await reader.Read());
            Assert.NotNull(reader.Statement);
            Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
            Assert.IsType<GraphicsOperation>(reader.Statement);
            var op = (GraphicsOperation)reader.Statement;
            Assert.Equal(Operator.v, op.Operator);
            Assert.Equal(4, op.Operands.Length);
            Assert.Equal(new PdfsValue(125), op.Operands[0]);
            Assert.Equal(new PdfsValue(100), op.Operands[1]);
            Assert.Equal(new PdfsValue(150), op.Operands[2]);
            Assert.Equal(new PdfsValue(20), op.Operands[3]);
        }

        using (var stream = S("125 100 150 20 y"))
        {

            var reader = new PdfsReader(stream);
            Assert.True(await reader.Read());
            Assert.NotNull(reader.Statement);
            Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
            Assert.IsType<GraphicsOperation>(reader.Statement);
            var op = (GraphicsOperation)reader.Statement;
            Assert.Equal(Operator.y, op.Operator);
            Assert.Equal(4, op.Operands.Length);
            Assert.Equal(new PdfsValue(125), op.Operands[0]);
            Assert.Equal(new PdfsValue(100), op.Operands[1]);
            Assert.Equal(new PdfsValue(150), op.Operands[2]);
            Assert.Equal(new PdfsValue(20), op.Operands[3]);
        }
    }

    /// <summary>
    /// The PdfsReader class should read a close-path operation.
    /// </summary>
    [Fact]
    public async Task ShouldReadClosePathOperation()
    {
        using var stream = S("h");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.h, op.Operator);
        Assert.Empty(op.Operands);
    }

    /// <summary>
    /// The PdfsReader class should read a rectangle operation.
    /// </summary>
    [Fact]
    public async Task ShouldReadRectangleOperation()
    {
        using var stream = S("10 20 100 50 re");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.re, op.Operator);
        Assert.Equal(4, op.Operands.Length);
        Assert.Equal(new PdfsValue(10), op.Operands[0]);
        Assert.Equal(new PdfsValue(20), op.Operands[1]);
        Assert.Equal(new PdfsValue(100), op.Operands[2]);
        Assert.Equal(new PdfsValue(50), op.Operands[3]);
    }
    #endregion

    #region Reading Graphics instructions - path painting instructions
    /// <summary>
    /// The PdfsReader class should read stroking and filling operations.
    /// </summary>
    [Fact]
    public async Task ShouldReadPathPaintingOperations()
    {
        using var stream = S("s S f F f* B B* b b* n");

        var reader = new PdfsReader(stream);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.s, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.S, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.f, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.F, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.fStar, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.B, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.BStar, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.b, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.bStar, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.n, op.Operator);
        Assert.Empty(op.Operands);

    }

    #endregion

    #region Reading Graphics instructions - clipping path instructions
    [Fact]
    /// <summary>
    /// The PdfsReader should read the "W" and "W*" operations, which modify the clipping path.
    /// </summary>
    public async Task ShouldReadClippingPathOperations()
    {
        using var stream = S("W W*");

        var reader = new PdfsReader(stream);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.W, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.WStar, op.Operator);
        Assert.Empty(op.Operands);
    }
    #endregion

    #region Reading Graphics instructions - text object instructions
    /// <summary>
    /// The PdfsReader should read the "BT" and "ET" operations, which start and end a text block.
    /// </summary>
    [Fact]
    public async Task ShouldReadTextObjectOperations()
    {
        using var stream = S("BT ET");

        var reader = new PdfsReader(stream);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.BT, op.Operator);
        Assert.Empty(op.Operands);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.ET, op.Operator);
        Assert.Empty(op.Operands);
    }
    #endregion

    #region Text state instructions
    /// <summary>
    /// The PdfsReader class should read text state operations that set the character
    /// spacing, word spacing, text ratio, text leading, font name and font size.
    /// </summary>
    [Fact]
    public async Task ShouldReadTextStateInstructions()
    {
        using var stream = S("12 Tc 0 Tw 100 Tz -2 TL 0 Tr 3 Ts");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Tc, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(12), op.Operands[0]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Tw, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(0), op.Operands[0]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Tz, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(100), op.Operands[0]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.TL, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(-2), op.Operands[0]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Tr, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(0), op.Operands[0]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Ts, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue(3), op.Operands[0]);
    }

    /// <summary>
    /// The PdfsReader should be able to read the "Tf" operation, which sets the font
    /// name and font size. 
    /// </summary>
    [Fact]
    public async Task ShouldReadSetFontNameAndSizeOperation()
    {
        using var stream = S("/TimesRoman 12 Tf");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Tf, op.Operator);
        Assert.Equal(2, op.Operands.Length);
        Assert.Equal(new PdfsValue("/TimesRoman", PdfsValueKind.Name), op.Operands[0]);
        Assert.Equal(new PdfsValue(12), op.Operands[1]);
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