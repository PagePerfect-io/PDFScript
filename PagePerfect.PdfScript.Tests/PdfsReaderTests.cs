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