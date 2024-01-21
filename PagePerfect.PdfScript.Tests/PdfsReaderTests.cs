using System.Text;
using PagePerfect.PdfScript.Reader;

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
        Assert.IsType<Reader.Statements.EndPageStatement>(reader.Statement);

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