using PagePerfect.PdfScript.Writer.Resources.Metrics;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The AfmLexerTests class contains the tests for the AfmLexer class.
/// </summary>
public class AfmLexerTests
{
    // Public tests
    // ============
    #region Overall tests
    /// <summary>
    /// The lexer should read a token off the stream.
    /// </summary>
    [Fact]
    public void ShouldReadToken()
    {
        var lexer = new AfmLexer(S("StartFontMetrics 4.1"));
        Assert.True(lexer.Read(out var token));
        Assert.NotNull(token);
        Assert.Equal(AfmTokenType.Token, token.Type);
    }

    /// <summary>
    /// The lexer should not read beyond the end of the stream.
    /// </summary>
    [Fact]
    public void ShouldNotReadBeyondEOF()
    {
        var lexer = new AfmLexer(S("StartFontMetrics 4.1"));

        Assert.True(lexer.Read(out var token1));
        Assert.Equal("StartFontMetrics", token1!.Value);
        Assert.True(lexer.Read(out var token2));
        Assert.Equal(AfmTokenType.Whitespace, token2!.Type);
        Assert.True(lexer.Read(out var token3));
        Assert.Equal(4.1, token3!.NumericValue);
        Assert.False(lexer.Read(out _));
    }
    #endregion

    #region Reading tokens
    /// <summary>
    /// The lexer should read a whitespace token.
    /// </summary>
    [Fact]
    public void ShouldReadWhitespace()
    {
        var lexer = new AfmLexer(S("StartFontMetrics 4.1"));

        Assert.True(lexer.Read(out var token1));
        Assert.NotEqual(AfmTokenType.Whitespace, token1!.Type);
        Assert.Equal("StartFontMetrics", token1.Value);

        Assert.True(lexer.Read(out var whitespace));
        Assert.Equal(AfmTokenType.Whitespace, whitespace!.Type);

        Assert.True(lexer.Read(out var token2));
        Assert.NotEqual(AfmTokenType.Whitespace, token2!.Type);
        Assert.Equal(4.1, token2.NumericValue);
    }

    [Fact]
    /// <summary>
    /// The lexer should read a numeric token.
    /// </summary>
    public void ShouldReadNumber()
    {
        // Testing:
        // (1) an integer
        // (2) a negative integer
        // (3) a double
        // (4) a negative double
        // (5) a double without leading 0
        // (6) a negative double without leading 0
        var lexer = new AfmLexer(S("12 -25 4.1 -2.3 .7 -.5"));

        Assert.True(lexer.Read(out AfmToken? token));
        Assert.Equal(12, token!.NumericValue);

        Assert.True(lexer.Read(out _)); // Whitespace
        Assert.True(lexer.Read(out token));
        Assert.Equal(-25, token!.NumericValue);

        Assert.True(lexer.Read(out _)); // Whitespace
        Assert.True(lexer.Read(out token));
        Assert.Equal(4.1, token!.NumericValue);

        Assert.True(lexer.Read(out _)); // Whitespace
        Assert.True(lexer.Read(out token));
        Assert.Equal(-2.3, token!.NumericValue);

        Assert.True(lexer.Read(out _)); // Whitespace
        Assert.True(lexer.Read(out token));
        Assert.Equal(0.7, token!.NumericValue);

        Assert.True(lexer.Read(out _)); // Whitespace
        Assert.True(lexer.Read(out token));
        Assert.Equal(-0.5, token!.NumericValue);

        // Testing: Invalid numbers
        lexer = new AfmLexer(S("1-2 2.5.5 2..5"));

        // Two numbers: 1 and -2
        Assert.True(lexer.Read(out token));
        Assert.Equal(1, token!.NumericValue);
        Assert.True(lexer.Read(out token));
        Assert.Equal(-2, token!.NumericValue);

        Assert.True(lexer.Read(out _)); // Whitespace

        Assert.False(lexer.Read(out _)); // 2.5.5 is a bad number

        Assert.True(lexer.Read(out _)); // Whitespace

        Assert.False(lexer.Read(out _)); // 2..5 is a bad number

    }

    /// <summary>
    /// The lexer should read a semi-colon token.
    /// </summary>
    [Fact]
    public void ShouldReadSemiColon()
    {
        // Testing:
        // (1) an integer
        // (2) a negative integer
        // (3) a double
        // (4) a negative double
        // (5) a double without leading 0
        // (6) a negative double without leading 0
        var lexer = new AfmLexer(S("12 ; 6"));

        Assert.True(lexer.Read(out _)); // Number
        Assert.True(lexer.Read(out _)); // Whitespace
        Assert.True(lexer.Read(out var token)); // Semi-colon
        Assert.Equal(AfmTokenType.SemiColon, token!.Type);
    }

    /// <summary>
    /// The lexer should read a 'token' token.
    /// </summary>
    [Fact]
    public void ShouldReadTokenValue()
    {
        // Testing: tokens including funny characters.
        var lexer = new AfmLexer(S("StartFontMetrics (c) :Edwin:"));

        Assert.True(lexer.Read(out var token));
        Assert.Equal("StartFontMetrics", token!.Value);

        Assert.True(lexer.Read(out _)); // Whitespace

        Assert.True(lexer.Read(out token));
        Assert.Equal("(c)", token!.Value);

        Assert.True(lexer.Read(out _)); // Whitespace

        Assert.True(lexer.Read(out token));
        Assert.Equal(":Edwin:", token!.Value);
    }

    /// <summary>
    /// The lexer should be able to skip to the next token.
    /// </summary>
    [Fact]
    public void ShouldReadNextToken()
    {
        // Testing: tokens including funny characters.
        var lexer = new AfmLexer(S("12 0.5 ; 100 ; Edwin 132 200"));

        Assert.True(lexer.ReadNextToken(out var token));
        Assert.Equal("Edwin", token!.Value);

        // Trying to read another token - fails.
        Assert.False(lexer.ReadNextToken(out _));
    }

    /// <summary>
    /// The lexer should be able to skip to the next token.
    /// </summary>
    [Fact]
    public void ShouldReadNextMatchingToken()
    {
        // Testing: tokens including funny characters.
        var lexer = new AfmLexer(S("StdHW 84 StdVW 106 StartCharMetrics 315 C 32 ; WX 600 ;"));

        Assert.True(lexer.ReadTo("C"));
        Assert.True(lexer.Read(out _));
        Assert.True(lexer.Read(out var token)); // Whitespace
        Assert.Equal(32, token!.NumericValue);

        // Next time, it fails
        Assert.False(lexer.ReadTo("C"));
    }

    /// <summary>
    /// The lexer should be able to read a numeric token, if available.
    /// </summary>
    [Fact]
    public void ShouldTryReadNumber()
    {
        // (1) Read a number
        // (2) Read a number, ignoring whitespace
        // (3) Fail to read a number
        var lexer = new AfmLexer(S("12.5 30 Edwin"));

        Assert.True(lexer.TryReadNumber(out var number));
        Assert.Equal(12.5, number);
        Assert.True(lexer.TryReadNumber(out number));
        Assert.Equal(30, number);
        Assert.False(lexer.TryReadNumber(out _));
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
    private Stream S(string source)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(source);
        return new MemoryStream(bytes);
    }
    #endregion
}
