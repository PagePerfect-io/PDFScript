using System.Text;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The PdfsLexerTests class contains tests for the PdfsLexer class.
/// </summary>
public class PdfsLexerTests
{
    // Public tests
    // ============
    #region Parsing overall
    /// <summary>
    /// The lexer should parse all tokens in a source stream, including
    /// the last token before EOF.
    /// </summary>
    [Fact]
    public async Task ShouldParseLastToken()
    {
        using var stream = S("0.03 Tc");
        var lexer = new PdfsLexer(stream);
        Assert.True(await lexer.Read());
        Assert.Equal(PdfsTokenType.Number, lexer.TokenType);
        Assert.Equal(0.03f, lexer.Number);

        Assert.True(await lexer.Read());
        Assert.Equal(PdfsTokenType.Whitespace, lexer.TokenType);

        Assert.True(await lexer.Read());
        Assert.Equal(PdfsTokenType.Keyword, lexer.TokenType);
        Assert.Equal("Tc", lexer.String);
    }
    #endregion

    #region Number parsing
    /// <summary>
    /// The lexer should parse an integer number.
    /// </summary>
    [Fact]
    public async Task ShouldParseInteger()
    {
        using var stream = S("2 Tc endstream");
        var lexer = new PdfsLexer(stream);
        Assert.True(await lexer.Read());
        Assert.Equal(PdfsTokenType.Number, lexer.TokenType);

        Assert.Equal(2, lexer.Number);
    }

    /// <summary>
    /// The lexer should parse a negative integer number.
    /// </summary>
    [Fact]
    public async Task ShouldParseNegativeInteger()
    {
        using var stream = S("-5 Tc endstream");
        var lexer = new PdfsLexer(stream);
        Assert.True(await lexer.Read());
        Assert.Equal(PdfsTokenType.Number, lexer.TokenType);

        Assert.Equal(-5, lexer.Number);
    }

    /// <summary>
    /// The lexer should parse positive and negative doubles.
    /// </summary>
    [Fact]
    public async Task ShouldParseDouble()
    {
        using (var stream = S("2.5 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);

            Assert.Equal(2.5d, lexer.Number);
        }

        using (var stream = S("+2.5 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);

            Assert.Equal(2.5d, lexer.Number);
        }

        using (var stream = S(".5 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);

            Assert.Equal(0.5d, lexer.Number);
        }

        using (var stream = S("+.5 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);

            Assert.Equal(0.5d, lexer.Number);
        }

        using (var stream = S("4. Tc endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);

            Assert.Equal(4d, lexer.Number);
        }

        using (var stream = S("-2.5 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);

            Assert.Equal(-2.5d, lexer.Number);
        }

        using (var stream = S("-.5 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);

            Assert.Equal(-0.5d, lexer.Number);
        }

        using (var stream = S("-3-5 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);
            Assert.Equal(-3, lexer.Number);

            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);
            Assert.Equal(-5, lexer.Number);
        }

        using (var stream = S("2.5.3 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Keyword, lexer.TokenType);

            Assert.Equal("2.5.3", lexer.String);
        }
    }

    /// <summary>
    /// The PDF lexer should treat a leading 0 as 0. So,
    /// the sequence 025 is treated as 0 25.
    /// </summary>
    [Fact]
    public async Task ShouldTreatLeadingZeroAsZero()
    {
        // By default, leading zeroes are allowed
        // and 025 is read as 25.
        using (var stream = S("025 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream);

            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);
            Assert.Equal(25, lexer.Number);
        }

        // When leading zeroes are not allowed, the sequence
        // 025 is read as 0 25.
        using (var stream = S("025 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream)
            {
                AllowLeadingZeroes = false
            };

            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);
            Assert.Equal(0, lexer.Number);

            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);
            Assert.Equal(25, lexer.Number);
        }

        // Either way, a 0 after a full stop is OK.
        using (var stream = S(".025 Tc endstream"))
        {
            var lexer = new PdfsLexer(stream)
            {
                AllowLeadingZeroes = false
            };

            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);
            Assert.Equal(0.025f, lexer.Number);
        }
    }
    #endregion


    #region String parsing
    /// <summary>
    /// The lexer should parse a basic ASCII string.
    /// </summary>
    [Fact]
    public async Task ShouldParseBasicString()
    {
        using (var stream = S("(Hello, World!) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("Hello, World!", lexer.String);
        }

        using (var stream = S("() Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("", lexer.String);
        }
    }

    /// <summary>
    /// Should parse a string across multiple liens
    /// </summary>
    [Fact]
    public async Task ShouldParseStringOnMultipleLines()
    {
        // A slash followed by a newline
        using (var stream = S(
            "(The quick brown fox \\\njumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox jumped over the lazy dog", lexer.String);
        }

        // A slash followed by a carriage return + newline
        using (var stream = S(
            "(The quick brown fox \\\r\njumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox jumped over the lazy dog", lexer.String);
        }

        // A slash followed by a carriage return
        using (var stream = S(
            "(The quick brown fox \\\rjumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox jumped over the lazy dog", lexer.String);
        }

    }

    /// <summary>
    /// The lexer should understand the end-of-line marker, as represented
    /// by CR, CR+LF, or LF.
    /// </summary>
    [Fact]
    public async Task ShouldParseStringWithEndOfLineMarker()
    {
        // A newline
        using (var stream = S(
            "(The quick brown fox \njumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox \njumped over the lazy dog", lexer.String);
        }

        // A carriage return + newline
        using (var stream = S(
            "(The quick brown fox \r\njumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox \njumped over the lazy dog", lexer.String);
        }

        // A carriage return
        using (var stream = S(
            "(The quick brown fox \rjumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox \njumped over the lazy dog", lexer.String);
        }
    }

    /// <summary>
    /// The lexer should understand escaping of control characters.
    /// </summary>
    [Fact]
    public async Task ShouldEscapeCommonEscapeCharacters()
    {
        using (var stream = S(
           "(The quick brown fox \\njumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox \njumped over the lazy dog", lexer.String);
        }

        using (var stream = S(
           "(The quick brown fox \\rjumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox \rjumped over the lazy dog", lexer.String);
        }

        using (var stream = S(
           "(The quick brown fox \\tjumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox \tjumped over the lazy dog", lexer.String);
        }

        using (var stream = S(
           "(The quick brown fox \\bjumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox \bjumped over the lazy dog", lexer.String);
        }

        using (var stream = S(
           "(The quick brown fox \\fjumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox \fjumped over the lazy dog", lexer.String);
        }

        using (var stream = S(
           "(The quick brown fox \\(jumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox (jumped over the lazy dog", lexer.String);
        }

        using (var stream = S(
           "(The quick brown fox \\)jumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox )jumped over the lazy dog", lexer.String);
        }

        using (var stream = S(
           "(The quick brown fox \\\\jumped over the lazy dog) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("The quick brown fox \\jumped over the lazy dog", lexer.String);
        }
    }

    /// <summary>
    /// The lexer should escape octal character codes.
    /// </summary>
    [Fact]
    public async Task ShouldEscapeOctalCharacterCodes()
    {
        using (var stream = S(
           "(Escaping \\101\\102\\103) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("Escaping ABC", lexer.String);
        }

        using (var stream = S(
           "(Escaping \\61\\62\\63) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("Escaping 123", lexer.String);
        }

        using (var stream = S(
           "(Escaping \\5) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("Escaping " + ((char)5), lexer.String);
        }

        // Content after an octal code.
        using (var stream = S(
           "(Escaping \\1234 \\61banana \\529 apple \\45\\nNewline) Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            var expected = $"Escaping {(char)83}4 {(char)49}banana {(char)42}9 apple {(char)37}\nNewline";

            Assert.Equal(expected, lexer.String);
        }

    }
    #endregion

    #region Hex string parsing
    /// <summary>
    /// The lexer should parse a hex string.
    /// </summary>
    [Fact]
    public async Task ShouldParseHexString()
    {
        using (var stream = S("<414243>Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("ABC", lexer.String);
        }

        using (var stream = S("<4a4b4C>Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.String, lexer.TokenType);

            Assert.Equal("JKL", lexer.String);
        }
    }

    /// <summary>
    /// The lexer should ignore whitespace inside a hex string.
    /// </summary>
    [Fact]
    public async Task ShouldIgnoreWhitespaceInHexString()
    {
        using var stream = S("<41\r42\n43\t44   45\f46>Tj endstream");
        var lexer = new PdfsLexer(stream);
        Assert.True(await lexer.Read());
        Assert.Equal(PdfsTokenType.String, lexer.TokenType);

        Assert.Equal("ABCDEF", lexer.String);
    }

    /// <summary>
    /// The lexer should read an empty hex string.
    /// </summary>
    [Fact]
    public async Task ShouldReadEmptyHexString()
    {
        using var stream = S("<>Tj endstream");
        var lexer = new PdfsLexer(stream);
        Assert.True(await lexer.Read());
        Assert.Equal(PdfsTokenType.String, lexer.TokenType);

        Assert.Equal("", lexer.String);
    }

    #endregion

    #region Variable parsing
    /// <summary>
    /// The lexer should be able to read a variable.
    /// </summary>
    [Fact]
    public async Task ShouldReadVariable()
    {
        using (var stream = S("$name Tj endstream"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Variable, lexer.TokenType);

            Assert.Equal("name", lexer.String);
        }

        using (var stream = S("$var_with_underscores $var10 $var_10 $Var20B"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Variable, lexer.TokenType);
            Assert.Equal("var_with_underscores", lexer.String);

            Assert.True(await lexer.Read());
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Variable, lexer.TokenType);
            Assert.Equal("var10", lexer.String);

            Assert.True(await lexer.Read());
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Variable, lexer.TokenType);
            Assert.Equal("var_10", lexer.String);

            Assert.True(await lexer.Read());
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Variable, lexer.TokenType);
            Assert.Equal("Var20B", lexer.String);
        }

        using (var stream = S("$var/Name $var.03 $var#fragment"))
        {
            var lexer = new PdfsLexer(stream);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Variable, lexer.TokenType);
            Assert.Equal("var", lexer.String);

            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Name, lexer.TokenType);
            Assert.Equal("/Name", lexer.String);

            Assert.True(await lexer.Read());
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Variable, lexer.TokenType);
            Assert.Equal("var", lexer.String);

            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Number, lexer.TokenType);
            Assert.Equal(0.03f, lexer.Number);

            Assert.True(await lexer.Read());
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Variable, lexer.TokenType);
            Assert.Equal("var", lexer.String);

            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.PrologFragment, lexer.TokenType);
            Assert.True(await lexer.Read());
            Assert.Equal(PdfsTokenType.Keyword, lexer.TokenType);
            Assert.Equal("fragment", lexer.String);

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