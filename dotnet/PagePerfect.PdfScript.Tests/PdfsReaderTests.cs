using System.Text;
using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Reader.Statements;
using PagePerfect.PdfScript.Reader.Statements.Prolog;

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

    /// <summary>
    /// The PdfsReader should be able to read a "pattern" prolog statement.
    /// </summary>
    [Fact]
    public async Task ShouldReadPatternResourcePrologStatement()
    {
        using var stream = S("# pattern /GreenYellow /LinearGradient /DeviceRGB <<" +
            "/Rect [0 0 595 842] " +
            "/C0 [0 1 0.2]  " +
            "/C1 [0.8 0.8 0.2]" +
            "/Stops [0.0 1.0]" +
            ">> \r\n");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.PrologStatement, reader.Statement.Type);
        Assert.IsType<Reader.Statements.Prolog.PatternDeclaration>(reader.Statement);
        var pattern = (Reader.Statements.Prolog.PatternDeclaration)reader.Statement;
        Assert.Equal(PrologStatementType.PatternDeclaration, pattern.PrologType);
        Assert.Equal(PatternType.LinearGradient, pattern.PatternType);
        Assert.Equal("/GreenYellow", pattern.Name);
        Assert.Equal(new PdfRectangle(0, 0, 595, 842), pattern.BoundingRectangle);
        Assert.Equal(2, pattern.Colours.Length);
        Assert.Equal(ColourSpace.DeviceRGB, pattern.ColourSpace);
        Assert.Equal(new Colour(ColourSpace.DeviceRGB, 0, 1, 0.2f), pattern.Colours[0]);
        Assert.Equal(new Colour(ColourSpace.DeviceRGB, 0.8f, 0.8f, 0.2f), pattern.Colours[1]);
        Assert.Equal(2, pattern.Stops.Length);
        Assert.Equal(0.0f, pattern.Stops[0]);
        Assert.Equal(1.0f, pattern.Stops[1]);
    }

    /// <summary>
    /// The PdfsReader should throw an exception when a "pattern" prolog statement
    /// is invalid.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenPatternDeclarationInvalid()
    {
        // Name is must be a name.
        using var stream = S("# pattern (GreenYellow) /LinearGradient /DeviceRGB <<" +
        "/Rect [0 0 595 842] " +
        "/C0 [0 1 0.2]  " +
        "/C1 [0.8 0.8 0.2]" +
        "/Stops [0.0 1.0]" +
        ">> \r\n");
        var reader = new PdfsReader(stream);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Type must be valid - LinearGradient or RadialGradient
        using var stream2 = S("# pattern /GreenYellow /Unknown /DeviceRGB <<" +
        "/Rect [0 0 595 842] " +
        "/C0 [0 1 0.2]  " +
        "/C1 [0.8 0.8 0.2]" +
        "/Stops [0.0 1.0]" +
        ">> \r\n");
        reader = new PdfsReader(stream2);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Colour space must be valid - DeviceRGB, DeviceCMYK, or DeviceGray
        using var stream3 = S("# pattern /GreenYellow /LinearGradient /DeviceInvalid <<" +
        "/Rect [0 0 595 842] " +
        "/C0 [0 1 0.2]  " +
        "/C1 [0.8 0.8 0.2]" +
        "/Stops [0.0 1.0]" +
        ">> \r\n");
        reader = new PdfsReader(stream3);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Missing required fields
        using var stream4 = S("# pattern /GreenYellow /LinearGradient /DeviceRGB \r\n");
        reader = new PdfsReader(stream4);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Missing required 'Rect' field
        using var stream5 = S("# pattern /GreenYellow /LinearGradient /DeviceRGB <<" +
        "/C0 [0 1 0.2]  " +
        "/C1 [0.8 0.8 0.2]" +
        "/Stops [0.0 1.0]" +
        ">> \r\n");
        reader = new PdfsReader(stream5);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Missing required 'Cn' colour fields
        using var stream6 = S("# pattern /GreenYellow /LinearGradient /DeviceRGB <<" +
        "/Rect [0 0 595 842] " +
        "/Stops [0.0 1.0]" +
        ">> \r\n");
        reader = new PdfsReader(stream6);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Incorrect number of components in a colour.
        using var stream7 = S("# pattern /GreenYellow /LinearGradient /DeviceRGB <<" +
            "/Rect [0 0 595 842] " +
            "/C0 [0 1 0.2 1]  " +
            "/C1 [0.8 0.8 0.2 1]" +
            "/Stops [0.0 1.0]" +
            ">> \r\n");
        reader = new PdfsReader(stream7);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Missing 'stops' field.
        using var stream8 = S("# pattern /GreenYellow /LinearGradient /DeviceRGB <<" +
        "/Rect [0 0 595 842] " +
        "/C0 [0 1 0.2]  " +
        "/C1 [0.8 0.8 0.2]" +
        ">> \r\n");
        reader = new PdfsReader(stream8);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
    }

    /// <summary>
    /// The PdfsReader should be able to read a "color" prolog statement.
    /// </summary>
    [Fact]
    public async Task ShouldReadColourResourcePrologStatement()
    {
        using var stream = S("# color /Green /DeviceRGB 0.2 0.8 0.2\r\n");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.PrologStatement, reader.Statement.Type);
        Assert.IsType<ColourDeclaration>(reader.Statement);
        var col = (ColourDeclaration)reader.Statement;
        Assert.Equal(PrologStatementType.ColourDeclaration, col.PrologType);
        Assert.Equal("/Green", col.Name);
        Assert.Equal(new Colour(ColourSpace.DeviceRGB, 0.2f, 0.8f, 0.2f), col.Colour);
    }

    /// <summary>
    /// The PdfsReader should throw an exception when a "color" prolog statement
    /// is invalid.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenColourDeclarationInvalid()
    {
        // Name is must be a name.
        using var stream = S("# pattern (Green) /DeviceRGB 0.2 0.8 0.2 \r\n");
        var reader = new PdfsReader(stream);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Colour space must be valid - DeviceRGB, DeviceCMYK, or DeviceGray
        using var stream3 = S("# pattern /Green /DeviceInvalid 0.2 0.8 0.2 \r\n");
        reader = new PdfsReader(stream3);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Missing components
        using var stream4 = S("# pattern /Green /DeviceRGB \r\n");
        reader = new PdfsReader(stream4);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Too few components
        using var stream5 = S("# pattern /Green /DeviceRGB 0.2 0.8\r\n");
        reader = new PdfsReader(stream5);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);

        // Invalid value for component
        using var stream6 = S("# pattern /Green /DeviceRGB 0.2 0.8 (Edwin)\r\n");
        reader = new PdfsReader(stream6);
        await Assert.ThrowsAsync<PdfsReaderException>(reader.Read);
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

    #region Reading Graphics instructions - Text state instructions
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

    #region Reading Graphics instructions - Text positioning instructions
    /// <summary>
    /// The PdfsReader class should be able to read "Td", "TD", and "T*" operations,
    /// which set the text position to a new line.
    /// </summary>
    [Fact]
    public async Task ShouldReadStartNextLineOperations()
    {
        using var stream = S("30 30 Td 0 20 TD T*");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Td, op.Operator);
        Assert.Equal(2, op.Operands.Length);
        Assert.Equal(new PdfsValue(30), op.Operands[0]);
        Assert.Equal(new PdfsValue(30), op.Operands[1]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.TD, op.Operator);
        Assert.Equal(2, op.Operands.Length);
        Assert.Equal(new PdfsValue(0), op.Operands[0]);
        Assert.Equal(new PdfsValue(20), op.Operands[1]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.TStar, op.Operator);
        Assert.Empty(op.Operands);
    }

    /// <summary>
    /// The PdfsReader class should read the Tm operation, which sets the text
    /// transformation matrix.
    /// </summary>
    [Fact]
    public async Task ShouldReadTextMatrixOperation()
    {
        using var stream = S("1 0 0 1 10 20 Tm");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Tm, op.Operator);
        Assert.Equal(6, op.Operands.Length);
        Assert.Equal(new PdfsValue(1), op.Operands[0]);
        Assert.Equal(new PdfsValue(0), op.Operands[1]);
        Assert.Equal(new PdfsValue(0), op.Operands[2]);
        Assert.Equal(new PdfsValue(1), op.Operands[3]);
        Assert.Equal(new PdfsValue(10), op.Operands[4]);
        Assert.Equal(new PdfsValue(20), op.Operands[5]);
    }
    #endregion

    #region Reading Graphics instructions - Text showing operations
    /// <summary>
    /// The PdfsReader class should read the "Tj" operation, which shows a string.
    /// </summary>
    [Fact]
    public async Task ShouldReadBasicTextShowingOperation()
    {
        using var stream = S("(Hello, World) Tj (Hello, again) '");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Tj, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue("Hello, World", PdfsValueKind.String), op.Operands[0]);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Apos, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(new PdfsValue("Hello, again", PdfsValueKind.String), op.Operands[0]);
    }

    /// <summary>
    /// The PdfsReader class should read the "\"" operation, which shows a string
    /// after setting character and word spacing, in one go. 
    /// </summary>
    [Fact]
    public async Task ShouldReadShortcutTextShowingOperation()
    {
        using var stream = S("15 10 (Hello, World) \"");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Quot, op.Operator);
        Assert.Equal(3, op.Operands.Length);
        Assert.Equal(new PdfsValue(15), op.Operands[0]);
        Assert.Equal(new PdfsValue(10), op.Operands[1]);
        Assert.Equal(new PdfsValue("Hello, World", PdfsValueKind.String), op.Operands[2]);
    }

    /// <summary>
    /// The PdfsReader class should read the "Tj" operation, which shows a positioned string.
    /// </summary>
    [Fact]
    public async Task ShouldReadPositionedTextShowingOperation()
    {
        using var stream = S("[(Hello, ) -100 (World)] TJ");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.TJ, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(PdfsValueKind.Array, op.Operands[0].Kind);
        var array = op.Operands[0].GetArray();
        Assert.Equal(3, array.Length);
        Assert.Equal(new PdfsValue("Hello, ", PdfsValueKind.String), array[0]);
        Assert.Equal(new PdfsValue(-100), array[1]);
        Assert.Equal(new PdfsValue("World", PdfsValueKind.String), array[2]);
    }
    #endregion



    #region Reading Graphics instructions - Color
    /// <summary>
    /// The PdfsReader class should read the "cs" and "CS" operations, which set a colour space.
    /// </summary>
    [Fact]
    public async Task ShouldReadColourSpaceOperations()
    {
        using var stream = S("/DeviceRGB cs /DeviceCMYK CS");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.cs, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(PdfsValueKind.Name, op.Operands[0].Kind);
        Assert.Equal("/DeviceRGB", op.Operands[0].GetString());

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.CS, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(PdfsValueKind.Name, op.Operands[0].Kind);
        Assert.Equal("/DeviceCMYK", op.Operands[0].GetString());
    }

    /// <summary>
    /// The PdfsReader class should read the "sc" and "SC" operations, which set a colour
    /// in the current colour space.
    /// </summary>
    [Fact]
    public async Task ShouldReadSetColourOperations()
    {
        using var stream = S("0.5 sc 1 0 0 sc 0 1 0.2 0.4 SC");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.sc, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(0.5f, op.Operands[0].GetNumber());

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.sc, op.Operator);
        Assert.Equal(3, op.Operands.Length);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(1, op.Operands[0].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[1].Kind);
        Assert.Equal(0, op.Operands[1].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[2].Kind);
        Assert.Equal(0, op.Operands[2].GetNumber());

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.SC, op.Operator);
        Assert.Equal(4, op.Operands.Length);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(0, op.Operands[0].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[1].Kind);
        Assert.Equal(1, op.Operands[1].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[2].Kind);
        Assert.Equal(0.2f, op.Operands[2].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[3].Kind);
        Assert.Equal(0.4f, op.Operands[3].GetNumber());

    }

    /// <summary>
    /// The PdfsReader class should read the "scn" and "SCN" operations, which set a colour
    /// in the current colour space and support pattern colour spaces.
    /// </summary>
    [Fact]
    public async Task ShouldReadSetColourNewOperations()
    {
        using var stream = S("0.5 /X scn 1 0 0 /X scn 0 1 0.2 0.4 /X SCN /X SCN");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.scn, op.Operator);
        Assert.Equal(2, op.Operands.Length);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(0.5f, op.Operands[0].GetNumber());
        Assert.Equal(PdfsValueKind.Name, op.Operands[1].Kind);
        Assert.Equal("/X", op.Operands[1].GetString());


        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.scn, op.Operator);
        Assert.Equal(4, op.Operands.Length);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(1, op.Operands[0].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[1].Kind);
        Assert.Equal(0, op.Operands[1].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[2].Kind);
        Assert.Equal(0, op.Operands[2].GetNumber());
        Assert.Equal(PdfsValueKind.Name, op.Operands[3].Kind);
        Assert.Equal("/X", op.Operands[3].GetString());

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.SCN, op.Operator);
        Assert.Equal(5, op.Operands.Length);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(0, op.Operands[0].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[1].Kind);
        Assert.Equal(1, op.Operands[1].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[2].Kind);
        Assert.Equal(0.2f, op.Operands[2].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[3].Kind);
        Assert.Equal(0.4f, op.Operands[3].GetNumber());
        Assert.Equal(PdfsValueKind.Name, op.Operands[4].Kind);
        Assert.Equal("/X", op.Operands[4].GetString());

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.SCN, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(PdfsValueKind.Name, op.Operands[0].Kind);
        Assert.Equal("/X", op.Operands[0].GetString());
    }

    /// <summary>
    /// The PdfsReader class should read the "g" and "G" operations, which set a colour
    /// in the DeviceGray colour space.
    /// </summary>
    [Fact]
    public async Task ShouldReadSetGrayColourOperations()
    {
        using var stream = S("0.5 g 0.7 G");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.g, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(0.5f, op.Operands[0].GetNumber());

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.G, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(0.7f, op.Operands[0].GetNumber());

    }

    /// <summary>
    /// The PdfsReader class should read the "rg" and "RG" operations, which set a colour
    /// in the DeviceRGB colour space.
    /// </summary>
    [Fact]
    public async Task ShouldReadSetRGBColourOperations()
    {
        using var stream = S("0.5 0.6 0.7 rg 0.7 0.8 0.9 RG");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.rg, op.Operator);
        Assert.Equal(3, op.Operands.Length);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(0.5f, op.Operands[0].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[1].Kind);
        Assert.Equal(0.6f, op.Operands[1].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[2].Kind);
        Assert.Equal(0.7f, op.Operands[2].GetNumber());

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.RG, op.Operator);
        Assert.Equal(3, op.Operands.Length);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(0.7f, op.Operands[0].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[1].Kind);
        Assert.Equal(0.8f, op.Operands[1].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[2].Kind);
        Assert.Equal(0.9f, op.Operands[2].GetNumber());
    }

    /// <summary>
    /// The PdfsReader class should read the "k" and "K" operations, which set a colour
    /// in the DeviceCMYK colour space.
    /// </summary>
    [Fact]
    public async Task ShouldReadSetCMYKColourOperations()
    {
        using var stream = S("0.5 0.6 0.7 1 k 0.7 0.8 0.9 1 K");

        var reader = new PdfsReader(stream);
        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.k, op.Operator);
        Assert.Equal(4, op.Operands.Length);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(0.5f, op.Operands[0].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[1].Kind);
        Assert.Equal(0.6f, op.Operands[1].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[2].Kind);
        Assert.Equal(0.7f, op.Operands[2].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[3].Kind);
        Assert.Equal(1f, op.Operands[3].GetNumber());

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.K, op.Operator);
        Assert.Equal(4, op.Operands.Length);
        Assert.Equal(PdfsValueKind.Number, op.Operands[0].Kind);
        Assert.Equal(0.7f, op.Operands[0].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[1].Kind);
        Assert.Equal(0.8f, op.Operands[1].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[2].Kind);
        Assert.Equal(0.9f, op.Operands[2].GetNumber());
        Assert.Equal(PdfsValueKind.Number, op.Operands[3].Kind);
        Assert.Equal(1f, op.Operands[3].GetNumber());
    }
    #endregion



    #region Reading Graphics instructions - Shading pattern
    /// <summary>
    /// The PdfsReader class should read the "sh" operation, which paints a
    /// shading and color object.
    /// </summary>
    [Fact]
    public async Task ShouldReadShadingPatternOperations()
    {
        using var stream = S("/Pattern sh");

        var reader = new PdfsReader(stream);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.sh, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(PdfsValueKind.Name, op.Operands[0].Kind);
        Assert.Equal("/Pattern", op.Operands[0].GetString());
    }
    #endregion



    #region Reading Graphics instructions - Place object
    /// <summary>
    /// The PdfsReader class should read the "Do" operation, which places
    /// an object onto the page, such as an image or a form.
    /// </summary>
    [Fact]
    public async Task ShouldReadPlaceObjectOperation()
    {
        using var stream = S("/Img1 Do");

        var reader = new PdfsReader(stream);

        Assert.True(await reader.Read());
        Assert.NotNull(reader.Statement);
        Assert.Equal(PdfsStatementType.GraphicsOperation, reader.Statement.Type);
        Assert.IsType<GraphicsOperation>(reader.Statement);
        var op = (GraphicsOperation)reader.Statement;
        Assert.Equal(Operator.Do, op.Operator);
        Assert.Single(op.Operands);
        Assert.Equal(PdfsValueKind.Name, op.Operands[0].Kind);
        Assert.Equal("/Img1", op.Operands[0].GetString());
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