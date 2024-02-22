using System.Text;

namespace PagePerfect.PdfScript.Writer.Resources.Metrics;

/// <summary>
/// The AfmLexer class implements a basic AFM file reader.
/// Its main use is to read character widths from AFM files for the standard
/// 14 fonts supported by PDF.
/// </summary>
internal class AfmLexer
{
    // Private fields and constants
    // ============================
    #region Constants
    private const char WHITESPACE_ZERO = '\0';
    private const char WHITESPACE_TAB = '\t';
    private const char WHITESPACE_10 = '\xa';
    private const char WHITESPACE_12 = '\xc';
    private const char WHITESPACE_13 = '\xd';
    private const char WHITESPACE_CR = '\r';
    private const char WHITESPACE_LF = '\n';
    private const char WHITESPACE_SPACE = ' ';
    private const char SEMI_COLON = ';';
    private const char DASH = '-';
    private const char FULL_STOP = '.';
    #endregion

    #region Private fields
    private readonly StreamReader _reader;
    #endregion



    // Public initialisers
    // ===================
    #region Instance initialisers
    /// <summary>
    /// Initialises a new AfmReader instance.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public AfmLexer(Stream stream)
    {
        if (null == stream) throw new ArgumentNullException(nameof(stream));
        _reader = new StreamReader(stream);
    }

    /// <summary>
    /// Initialises a new AfmReader instance.
    /// </summary>
    /// <param name="path">The file to read from.</param>
    public AfmLexer(string path) : this(File.OpenRead(path)) { }

    #endregion



    // Public methods
    // ==============        
    #region Public methods
    /// <summary>
    /// Reads the next token. This method reads the next token off the
    /// stream, and returns the parsed token.
    /// </summary>
    /// <param name="token">(Out) the parsed token.</param>
    /// <returns>True if the next token could be read; False otherwise.</returns>        
    public bool Read(out AfmToken? token)
    {
        return InternalRead(out token);
    }

    /// <summary>
    /// Reads the next token in the stream, passing any number, whitespace
    /// or special characters.
    /// </summary>
    /// <param name="token">(Out) the token.</param>
    /// <returns>True if the reader encountered a token; false otherwise.</returns>
    public bool ReadNextToken(out AfmToken? token)
    {
        while (true == InternalRead(out var next))
        {
            if (AfmTokenType.Token == next!.Type)
            {
                token = next;
                return true;
            }
        }

        token = null;
        return false;
    }

    /// <summary>
    /// Reads the next token that matches the specified token value.
    /// </summary>
    /// <param name="value">The token value.</param>
    /// <returns>True if the token was found; false otherwiese.</returns>
    public bool ReadTo(string value)
    {
        while (true == ReadNextToken(out var token))
        {
            if (value == token!.Value) return true;
        }

        return false;
    }

    /// <summary>
    /// Reads the next token that matches one of the specified token values.
    /// </summary>
    /// <param name="values">The values to match.</param>
    /// <param name="tokenValue">(out) The token value.</param>
    /// <returns>True if the token was found; false otherwiese.</returns>
    public bool ReadTo(string[] values, out string? tokenValue)
    {
        tokenValue = null;
        if (null == values) return false;

        while (true == ReadNextToken(out var token))
        {
            if (values.Contains(token!.Value))
            {
                tokenValue = token.Value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to read a numeric token. This method reads the next token, and
    /// validates that it's a number. Any whitespace leading up to the number
    /// is ignored.
    /// </summary>
    /// <param name="number">(Out) the numeric value of the next token.</param>
    /// <returns>True if the next token could be read, and is a number; false otherwise.</returns>
    public bool TryReadNumber(out double number)
    {
        // Ignore whitespace
        var result = false;
        AfmToken? token = null;
        do
        {
            result = InternalRead(out token);
        } while (true == result && AfmTokenType.Whitespace == token!.Type);

        if (false == result || AfmTokenType.Number != token!.Type)
        {
            number = default;
            return false;
        }

        number = token.NumericValue;
        return true;
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Reads the next token. This method reads the next token off the
    /// stream, and returns the parsed token.
    /// </summary>
    /// <param name="token">(Out) the parsed token.</param>
    /// <returns>True if the next token could be read; False otherwise.</returns>
    private bool InternalRead(out AfmToken? token)
    {
        // We read the next token. This can be a token (name), semi-colon, number
        // or whitespace.
        var state = AfmLexerState.Free;
        var builder = new StringBuilder();
        int c;
        while ((c = _reader.Peek()) >= 0)
        {
            switch (state)
            {
                case AfmLexerState.Free:
                    // In the free state, we expect to find the start of
                    // whitespace, a number, or a token, or a semicolon character.
                    switch (c)
                    {
                        case WHITESPACE_ZERO:
                        case WHITESPACE_TAB:
                        case WHITESPACE_10:
                        case WHITESPACE_12:
                        case WHITESPACE_13:
                        case WHITESPACE_SPACE:
                            state = AfmLexerState.Whitespace;
                            break;

                        case SEMI_COLON:
                            _reader.Read(); // Consume
                            token = new AfmToken(AfmTokenType.SemiColon);
                            return true;

                        case DASH:
                        case FULL_STOP:
                            state = AfmLexerState.Number;
                            builder.Append((char)c);
                            break;

                        default:
                            if (c >= '0' && c <= '9')
                            {
                                state = AfmLexerState.Number;
                            }
                            else
                            {
                                state = AfmLexerState.Token;
                            }
                            builder.Append((char)c);
                            break;

                    }
                    _reader.Read(); // Consume
                    break;

                case AfmLexerState.Whitespace:
                    // In whitespace mode, we keep ignoring whitespace
                    // characters. As soon as we encounter a non-whitespace
                    // character, we're done.
                    switch (c)
                    {
                        case WHITESPACE_ZERO:
                        case WHITESPACE_TAB:
                        case WHITESPACE_10:
                        case WHITESPACE_12:
                        case WHITESPACE_13:
                        case WHITESPACE_SPACE:
                            _reader.Read(); // Consume
                            break;

                        default:
                            token = new AfmToken(AfmTokenType.Whitespace);
                            return true;
                    }
                    break;

                case AfmLexerState.Token:
                    // In the token state, we terminate when we
                    // find whitespace, or a semicolon, or a dash or full stop.\
                    // We also stop if we find a numeral.
                    switch (c)
                    {
                        case WHITESPACE_ZERO:
                        case WHITESPACE_TAB:
                        case WHITESPACE_10:
                        case WHITESPACE_12:
                        case WHITESPACE_13:
                        case WHITESPACE_SPACE:
                        case SEMI_COLON:
                        case DASH:
                        case FULL_STOP:
                            token = ParseToken(builder, state);
                            return true;

                        default:
                            if (c >= '0' && c <= '9')
                            {
                                token = ParseToken(builder, state);
                                return true;
                            }

                            // This is a token character so we add it to
                            // the builder.
                            builder.Append((char)_reader.Read());
                            break;
                    }
                    break;

                case AfmLexerState.Number:
                    // In the number state, we stop when encountering
                    // whitespace, or a semicolon, or a dash, or a non-numeral.
                    switch (c)
                    {
                        case WHITESPACE_ZERO:
                        case WHITESPACE_TAB:
                        case WHITESPACE_10:
                        case WHITESPACE_12:
                        case WHITESPACE_13:
                        case WHITESPACE_SPACE:
                        case SEMI_COLON:
                        case DASH:
                            token = ParseToken(builder, state);
                            return null != token;

                        case FULL_STOP:
                            builder.Append((char)_reader.Read());
                            break;

                        default:
                            if (c < '0' || c > '9')
                            {
                                token = ParseToken(builder, state);
                                return null != token;
                            }

                            // This is a numeral character so we add it to
                            // the builder.
                            builder.Append((char)_reader.Read());
                            break;
                    }
                    break;
            }
        }

        // If our current state is number or token, and we've got something
        // in the builder, then we finish it off.
        switch (state)
        {

            case AfmLexerState.Number:
            case AfmLexerState.Token:
                token = ParseToken(builder, state);
                return null != token;

            default:
                // We hit the end of the stream without a token.
                token = null;
                return false;
        }
    }

    /// <summary>
    /// Parses a token. This method parses a string and returns an AFM token.
    /// </summary>
    /// <param name="builder">The string builder containing the token string.</param>
    /// <param name="state">The state - number or token.</param>
    /// <returns>An AfmToken instance.</returns>
    private static AfmToken? ParseToken(StringBuilder builder, AfmLexerState state)
    {
        if (AfmLexerState.Number == state)
        {
            // If we've only read . or - then we've found a token instead.
            if (1 == builder.Length)
            {
                if ('.' == builder[0] || '-' == builder[0])
                    return new AfmToken(builder.ToString(), AfmTokenType.Token);
            }

            return double.TryParse(builder.ToString(), out var number)
                ? new AfmToken(number)
                : null;
        }

        return new AfmToken(builder.ToString(), AfmTokenType.Token);
    }
    #endregion



    // Internal types
    // ==============
    #region AfmLexerState enumeration
    /// <summary>
    /// The AfmLexerState enumeration lists the possible states that the lexer
    /// state machine can be in.
    /// </summary>
    private enum AfmLexerState
    {
        Free,
        Whitespace,
        Token,
        Number
    }
    #endregion
}
