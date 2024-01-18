using System.Text;

namespace PagePerfect.PdfScript;

/// <summary>
/// The PdfsLexer is a .pdfs document lexer. It reads tokens off of a stream.
/// It is used by the Document class to parse a .pdfs document and output a PDF.
/// The PdfsLexer is a state machine that reads token instances, which can
/// be numbers, strings, names, variables, keywords or markup such as brackets.
/// The PdfsLexer will read the stream as a series of UTF-8 encoded characters.
/// </summary> 
/// <remarks>
/// Initialises a new PdfsLexer instance.
/// </remarks>
/// <param name="stream">The stream to read from.</param>
public class PdfsLexer(Stream stream)
{
    // Private constants
    // =================
    #region Private constants
    private const int BUFFER_SIZE = 0x20000;
    private const int PDF_STRING_MAXLENGTH = 0xffff;
    private const char WHITESPACE_ZERO = '\0';
    private const char WHITESPACE_TAB = '\t';
    private const char WHITESPACE_10 = '\xa';
    private const char WHITESPACE_12 = '\xc';
    private const char WHITESPACE_13 = '\xd';
    private const char WHITESPACE_CR = '\r';
    private const char WHITESPACE_LF = '\n';

    private const char ESCAPE_LF = 'n';
    private const char ESCAPE_CR = 'r';
    private const char ESCAPE_TAB = 't';
    private const char ESCAPE_BACKSPACE = 'b';
    private const char ESCAPE_FORMFEED = 'f';
    private const char WHITESPACE_SPACE = ' ';
    private const char COMMENT_START = '%';
    private const char ARRAY_OPEN = '[';
    private const char ANGLE_OPEN = '<';
    private const char ARRAY_CLOSE = ']';
    private const char ANGLE_CLOSE = '>';
    private const char NAME_START = '/';
    private const char STRING_OPEN = '(';
    private const char STRING_CLOSE = ')';
    private const char STRING_ESCAPE = '\\';
    private const char OBJECT_REFERENCE = 'R';
    private const char PLUS = '+';
    private const char MINUS = '-';
    private const char FULL_STOP = '.';
    private const char FRAGMENT = '#';
    private const char VARIABLE_START = '$';
    #endregion



    // Private fields
    // ==============
    #region Private fields
    private readonly StreamReader _reader = new StreamReader(stream, Encoding.UTF8, false, BUFFER_SIZE, true);
    private char[] _buffer = new char[BUFFER_SIZE];
    private int _pointer;
    private int _size;
    private readonly Stream _stream = stream;
    private PdfsTokenType _tokenType = PdfsTokenType.Null;
    private Stack<long> _stateStack = new();
    private float _number;
    #endregion



    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// Does the lexer read numbers with leading zeroes?
    /// (True by default)
    /// The GraphicsStreamReader sets this to false as content
    /// streams may have non-leading zeroes - e.g. a value of
    /// 025 should be read as 0 25 in content streams.
    /// </summary>
    public bool AllowLeadingZeroes { get; set; } = true;

    /// <summary>
    /// Retrieves the type of the token that was just read.
    /// </summary>
    public PdfsTokenType TokenType
    {
        get { return _tokenType; }
    }

    /// <summary>
    /// Retrieves the numeric value of the token that was just read.
    /// This property is NaN if the token isn't a number.
    /// </summary>
    /// <value>The number.</value>
    public float Number
    {
        get
        {
            if (PdfsTokenType.Number != TokenType) return float.NaN;

            return _number;
        }
    }

    /// <summary>
    /// Retrieves the token's string value.
    /// This converts a string or hex string's bytes to a string.
    /// This method will return eithern an ASCII or UTF-16BE string,
    /// </summary>
    public string? String
    {
        get
        {
            switch (_tokenType)
            {
                case PdfsTokenType.String:
                case PdfsTokenType.Keyword:
                case PdfsTokenType.Name:
                case PdfsTokenType.Variable:
                    return new string(TokenBuffer[..TokenLength]);
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// Retrieves the token buffer.
    /// </summary> 
    public char[] TokenBuffer { get; } = new char[PDF_STRING_MAXLENGTH];

    /// <summary>
    /// Retrieves the lenfth of the token buffer.
    /// </summary> 
    public int TokenLength { get; private set; }
    #endregion



    // Region public methods
    // =====================
    #region Public methods
    /// <summary>
    /// Returns a character array with the token characters.
    /// This method returns a copy of the token characters,
    /// into a new array. 
    /// </summary>
    /// <returns>The character array.</returns>
    public char[] GetCharacters()
    {
        return [.. TokenBuffer[..TokenLength]];
    }

    /// <summary>
    /// Preserves the current stream position.
    /// </summary>
    public void PreserveState()
    {
        _stateStack.Push(_stream.Position);
    }

    /// <summary>
    /// Reads the next token off the stream, and indicates if there are more tokens left in the stream.
    /// </summary>
    /// <returns>A value that indicates if there are more tokens to be read.</returns>
    public async Task<bool> Read()
    {
        bool finished = false;
        LexerReadState readState = LexerReadState.Free;
        char current = (char)0;
        var nestedParentheses = 0;
        var octalDigitsRead = 0;
        var octalValue = 0;
        var isLeadingZero = false;
        TokenLength = 0;

        while (false == finished)
        {
            char previous = current;
            if (_pointer == _size)
            {
                _pointer = 0;
                _size = await _reader.ReadAsync(_buffer, 0, BUFFER_SIZE);
                if (_size == 0) break;
            }
            current = _buffer[_pointer++];

            switch (readState)
            {
                case LexerReadState.Free:
                    // In the free state, we handle the following characters:
                    // whitespace - we enter the whitespace state
                    // / - we enter the keyword state
                    // % - we enter the comment state
                    // < - we enter the AfterAngleOpen state
                    // > - we enter the AfterAngleClose state
                    // ( - we enter the string state
                    // [ - we have found an ArrayStart token
                    // ] - we have found an ArrayEnd token
                    // + - we have found a number, possibly
                    // - - we have found a number, possibly
                    // . - we have found a number, possibly
                    // # - we have found a prologue fragment
                    // $ - we enter the variable state
                    // any other - we enter the token state
                    switch (current)
                    {
                        case WHITESPACE_ZERO:
                        case WHITESPACE_TAB:
                        case WHITESPACE_10:
                        case WHITESPACE_12:
                        case WHITESPACE_13:
                        case WHITESPACE_SPACE:
                            readState = LexerReadState.WhiteSpace;
                            break;

                        case NAME_START:
                            readState = LexerReadState.Keyword;
                            TokenBuffer[TokenLength++] = current;
                            break;

                        case COMMENT_START:
                            readState = LexerReadState.Comment;
                            break;

                        case ANGLE_OPEN:
                            readState = LexerReadState.AfterAngleOpen;
                            break;

                        case ANGLE_CLOSE:
                            readState = LexerReadState.AfterAngleClose;
                            break;

                        case STRING_OPEN:
                            readState = LexerReadState.String;
                            nestedParentheses = 0;
                            break;

                        case ARRAY_OPEN:
                            _tokenType = PdfsTokenType.ArrayStart;
                            finished = true;
                            break;

                        case ARRAY_CLOSE:
                            _tokenType = PdfsTokenType.ArrayEnd;
                            finished = true;
                            break;

                        case PLUS:
                        case MINUS:
                        case FULL_STOP:
                            readState = LexerReadState.Number;
                            isLeadingZero = false;
                            TokenBuffer[TokenLength++] = current;
                            break;

                        case FRAGMENT:
                            _tokenType = PdfsTokenType.PrologFragment;
                            finished = true;
                            break;

                        case VARIABLE_START:
                            readState = LexerReadState.Variable;
                            break;

                        default:
                            if (current >= '0' && current <= '9')
                            {
                                readState = LexerReadState.Number;
                                isLeadingZero = current == '0';
                            }
                            else readState = LexerReadState.Keyword;
                            TokenBuffer[TokenLength++] = current;
                            break;
                    }
                    break;

                case LexerReadState.String:
                    // In the string state we read until we find a ) token. The ) token
                    // indicates the end of the string but only if not matched by 
                    // a previous ( token.
                    // We also look out for the escape character, and
                    // for CR and/or LF tokens as we normalise them
                    // into a single LF token.                                
                    switch (current)
                    {
                        case WHITESPACE_CR:
                            // If this CR or was preceded by
                            // an escape token, then we ignore it.
                            if (STRING_ESCAPE != previous)
                            {
                                TokenBuffer[TokenLength++] = '\n';
                            }
                            break;

                        case WHITESPACE_LF:
                            // If this LF or was preceded by
                            // an escape token, or a CR, then we ignore it.
                            if (STRING_ESCAPE != previous && WHITESPACE_CR != previous)
                            {
                                TokenBuffer[TokenLength++] = '\n';
                            }
                            break;

                        case STRING_OPEN:
                            // If we started an octal sequence
                            // we need to complete it.
                            if (octalDigitsRead > 0)
                            {
                                TokenBuffer[TokenLength++] = (char)octalValue;
                                octalValue = octalDigitsRead = 0;
                            }

                            // If this is a ( token and not part
                            // of an escape sequence, we add to the
                            // string and record the nesting.
                            if (STRING_ESCAPE != previous)
                                nestedParentheses++;

                            // Otherwise, if it was escaped then
                            // we don't look at nesting.
                            TokenBuffer[TokenLength++] = current;
                            break;

                        case STRING_CLOSE:
                            // If we started an octal sequence
                            // we need to complete it.
                            if (octalDigitsRead > 0)
                            {
                                TokenBuffer[TokenLength++] = (char)octalValue;
                                octalValue = octalDigitsRead = 0;
                            }

                            // If this was an un-escaped ) token
                            // then we may have found the end of the
                            // string.
                            if (STRING_ESCAPE != previous && --nestedParentheses < 0)
                            {
                                _tokenType = PdfsTokenType.String;
                                finished = true;
                            }
                            else
                            {

                                // If it was escaped, or it wasn't the end
                                // of the string, we add the token.
                                TokenBuffer[TokenLength++] = current;
                            }
                            break;

                        case STRING_ESCAPE:
                            // If we started an octal sequence
                            // we need to complete it.
                            if (octalDigitsRead > 0)
                            {
                                TokenBuffer[TokenLength++] = (char)octalValue;
                                octalValue = octalDigitsRead = 0;
                            }

                            // An escape character. We don't record
                            // this in the string unless the previous
                            // character was an escape character.
                            if (STRING_ESCAPE == previous)
                            {
                                TokenBuffer[TokenLength++] = current;
                                // We need to destroy the curent
                                // character so we don't assume it's
                                // an escape character next time round!
                                current = (char)0;
                            }
                            break;

                        case ESCAPE_LF:
                            // If we started an octal sequence
                            // we need to complete it.
                            if (octalDigitsRead > 0)
                            {
                                TokenBuffer[TokenLength++] = (char)octalValue;
                                octalValue = octalDigitsRead = 0;
                            }

                            // If this character follows an
                            // escape character, we add the
                            // escape sequence - otherwise
                            // we just add the character.
                            if (STRING_ESCAPE == previous)
                                TokenBuffer[TokenLength++] = WHITESPACE_LF;
                            else
                                TokenBuffer[TokenLength++] = current;
                            break;

                        case ESCAPE_CR:
                            // If we started an octal sequence
                            // we need to complete it.
                            if (octalDigitsRead > 0)
                            {
                                TokenBuffer[TokenLength++] = (char)octalValue;
                                octalValue = octalDigitsRead = 0;
                            }

                            // If this character follows an
                            // escape character, we add the
                            // escape sequence - otherwise
                            // we just add the character.
                            if (STRING_ESCAPE == previous)
                                TokenBuffer[TokenLength++] = WHITESPACE_CR;
                            else
                                TokenBuffer[TokenLength++] = current;
                            break;

                        case ESCAPE_TAB:
                            // If we started an octal sequence
                            // we need to complete it.
                            if (octalDigitsRead > 0)
                            {
                                TokenBuffer[TokenLength++] = (char)octalValue;
                                octalValue = octalDigitsRead = 0;
                            }

                            // If this character follows an
                            // escape character, we add the
                            // escape sequence - otherwise
                            // we just add the character.
                            if (STRING_ESCAPE == previous)
                                TokenBuffer[TokenLength++] = WHITESPACE_TAB;
                            else
                                TokenBuffer[TokenLength++] = current;
                            break;

                        case ESCAPE_BACKSPACE:
                            // If we started an octal sequence
                            // we need to complete it.
                            if (octalDigitsRead > 0)
                            {
                                TokenBuffer[TokenLength++] = (char)octalValue;
                                octalValue = octalDigitsRead = 0;
                            }

                            // If this character follows an
                            // escape character, we add the
                            // escape sequence - otherwise
                            // we just add the character.
                            if (STRING_ESCAPE == previous)
                                TokenBuffer[TokenLength++] = '\b';
                            else
                                TokenBuffer[TokenLength++] = current;
                            break;

                        case ESCAPE_FORMFEED:
                            // If we started an octal sequence
                            // we need to complete it.
                            if (octalDigitsRead > 0)
                            {
                                TokenBuffer[TokenLength++] = (char)octalValue;
                                octalValue = octalDigitsRead = 0;
                            }

                            // If this character follows an
                            // escape character, we add the
                            // escape sequence - otherwise
                            // we just add the character.
                            if (STRING_ESCAPE == previous)
                                TokenBuffer[TokenLength++] = '\f';
                            else
                                TokenBuffer[TokenLength++] = current;
                            break;

                        default:
                            // If this is a numeral in the range 0..8,
                            // and we're current escaping an octal sequence,
                            // then we add to that.
                            if (current >= '0' && current <= '8' && (STRING_ESCAPE == previous || 1 == octalDigitsRead || 2 == octalDigitsRead))
                            {
                                octalDigitsRead++;
                                octalValue = (octalValue << 3) + (current - '0');
                            }
                            else
                            {
                                // Otherwise, if the previous character was
                                // an escape token then we've found an invalid
                                // string - we'll just keep reading.

                                // We do need to finish an existing octal
                                // sequence.
                                if (octalDigitsRead > 0)
                                {
                                    TokenBuffer[TokenLength++] = (char)octalValue;
                                    octalValue = octalDigitsRead = 0;
                                }

                                TokenBuffer[TokenLength++] = current;
                            }
                            break;
                    }
                    break;

                case LexerReadState.Comment:
                    // When we are in the comment state we read until we have read a \r\n sequence.
                    if (WHITESPACE_CR == current && WHITESPACE_LF == previous)
                    {
                        _tokenType = PdfsTokenType.Comment;
                        finished = true;
                    }
                    break;

                case LexerReadState.Keyword:
                    // In the keyword state, we continue to read data until we find:
                    // - whitespace: we end the current token
                    // - %: we end the current token
                    // - [: we end the current token
                    // - <: we end the current token
                    // - >: we end the current token
                    // - ]: we end the current token
                    // - /: we end the current token
                    // - (: we end the current token
                    // - #: we end the current token
                    // - $: we end the current token
                    switch (current)
                    {
                        case WHITESPACE_ZERO:
                        case WHITESPACE_TAB:
                        case WHITESPACE_10:
                        case WHITESPACE_12:
                        case WHITESPACE_13:
                        case WHITESPACE_SPACE:
                        case COMMENT_START:
                        case ARRAY_OPEN:
                        case ANGLE_OPEN:
                        case ARRAY_CLOSE:
                        case ANGLE_CLOSE:
                        case NAME_START:
                        case STRING_OPEN:
                        case FRAGMENT:
                        case VARIABLE_START:
                            // We're done. This keyword could be a reference (if the keyword is 'R')
                            // or a name, if it starts with a / character. Otherwise, it's some
                            // other keyword.
                            TryParseKeyword();
                            finished = true;
                            --_pointer; // We want to re-read this character next time.
                            break;

                        default:
                            TokenBuffer[TokenLength++] = current;
                            break;

                    }
                    break;

                case LexerReadState.Number:
                    // In the number state, we only accept:
                    // . - we keep reading. Possibly thisn't a number.
                    // numerals - keep reading
                    // Anything else (any other non-numeral, or
                    // whitespace, or a + or - character) means 
                    // the end of the number.
                    switch (current)
                    {
                        case FULL_STOP:
                            isLeadingZero = false;
                            TokenBuffer[TokenLength++] = current;
                            break;

                        default:
                            // If the previous token is a 0
                            // then we've found the end of
                            // a numger with value 0, unless we allow
                            // leading zeroes.
                            if ((AllowLeadingZeroes || !isLeadingZero) && current >= 48 && current <= 57)
                            {
                                TokenBuffer[TokenLength++] = current;
                            }
                            else
                            {
                                finished = true;
                                // It's only a number if we can parse
                                // it. Otherwise, it's a keyword.
                                TryParseNumber();

                                --_pointer; // We want to re-read this character next time.

                            }
                            break;
                    }
                    break;

                case LexerReadState.AfterAngleOpen:
                    // If we opened a '<' character, then if we find another one we will have found
                    // a dictionaryStart token. Otherwise, we are in the HexString state.
                    if (ANGLE_OPEN == current)
                    {
                        _tokenType = PdfsTokenType.DictionaryStart;
                        finished = true;
                    }
                    else if (ANGLE_CLOSE == current)
                    {
                        // A < followed by > is an empty hex string.
                        TryParseHexString();
                        finished = true;
                    }
                    else
                    {
                        TokenBuffer[TokenLength++] = current;
                        readState = LexerReadState.HexString;
                    }
                    break;

                case LexerReadState.AfterAngleClose:
                    // If we read a '>' character, then if we find another one we will have found
                    // a dictionaryEnd token. Otherwise, we are reading a normal token.
                    if (ANGLE_CLOSE == current)
                    {
                        _tokenType = PdfsTokenType.DictionaryEnd;
                        finished = true;
                    }
                    else
                    {
                        TokenBuffer[TokenLength++] = previous;
                        TokenBuffer[TokenLength++] = current;
                        readState = LexerReadState.Keyword;
                    }
                    break;

                case LexerReadState.HexString:
                    // If we are inside a hex string, the string will end with the '>' token.
                    if (ANGLE_CLOSE == current)
                    {
                        TryParseHexString();
                        finished = true;
                    }
                    else
                    {
                        TokenBuffer[TokenLength++] = current;
                    }
                    break;

                case LexerReadState.Variable:
                    // In the variable state, we only accept:
                    // - a digit - keep reading
                    // - A-Z or a-z - keep reading
                    // - underscore - keep reading
                    // Anything else, we end the token.
                    if (current >= '0' && current <= '9' || current >= 'A' && current <= 'Z' || current >= 'a' && current <= 'z' || current == '_')
                    {
                        TokenBuffer[TokenLength++] = current;
                    }
                    else
                    {
                        finished = true;
                        _tokenType = PdfsTokenType.Variable;
                        --_pointer; // We want to re-read this character next time.
                    }
                    break;

                case LexerReadState.WhiteSpace:
                    // In the WhiteSpace state, if we find more whitespace we append it.
                    // If we find anything else we will have found the entire whitespace token.
                    switch (current)
                    {
                        case WHITESPACE_ZERO:
                        case WHITESPACE_TAB:
                        case WHITESPACE_10:
                        case WHITESPACE_12:
                        case WHITESPACE_13:
                        case WHITESPACE_SPACE:
                            break;

                        default:
                            _tokenType = PdfsTokenType.Whitespace;
                            finished = true;
                            --_pointer; // We want to re-read this character next time.
                            break;
                    }
                    break;

            } // switch readState
        }

        // If we're finished then that's great.
        if (true == finished) return true;

        // Otherwise, if we were working on a keyword, variable, whitespace, or number
        // then we complete it.
        switch (readState)
        {
            case LexerReadState.Number:
                TryParseNumber();
                return true;

            case LexerReadState.Keyword:
                TryParseKeyword();
                return true;

            case LexerReadState.Variable:
                _tokenType = PdfsTokenType.Variable;
                return true;

            case LexerReadState.WhiteSpace:
                _tokenType = PdfsTokenType.Whitespace;
                return true;

            default:
                return false;
        }

    }

    /// <summary>
    /// Restores the stream position to the last preserved state.
    /// </summary>
    public void RestoreState()
    {
        _stream.Position = _stateStack.Pop();
    }
    #endregion

    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Tries to parse the current token as one of the keyword types.
    /// The keyword could be an 'R', or a name, or else a keyword.
    /// </summary>
    private void TryParseKeyword()
    {
        if (TokenLength == 1 && OBJECT_REFERENCE == TokenBuffer[0])
            _tokenType = PdfsTokenType.R;
        else if (NAME_START == TokenBuffer[0])
            _tokenType = PdfsTokenType.Name;
        else
            _tokenType = PdfsTokenType.Keyword;
    }

    /// <summary>
    /// Tries to parse the current token as a hex string.
    /// This method overwrites the token buffer with the bytes
    /// unpacked from the hex string, and sets the token type to
    /// String. 
    /// </summary>
    private void TryParseHexString()
    {
        _tokenType = PdfsTokenType.String;

        if (TokenLength < 2) { TokenLength = 0; return; }

        var i = 0; var o = 0;
        while (i < TokenLength - 1)
        {
            var first = TokenBuffer[i];
            if (first < '0') { ++i; continue; }

            var second = TokenBuffer[i + 1];
            var ms = (byte)((first >= '0' && first <= '9')
                ? first - '0' : (first >= 'a' && first <= 'f')
                    ? first - 'a' + 10 : first - 'A' + 10);
            var ls = (byte)((second >= '0' && second <= '9')
                ? second - '0' : (second >= 'a' && second <= 'f')
                    ? second - 'a' + 10 : second - 'A' + 10);

            TokenBuffer[o++] = (char)((ms << 4) + ls);

            i += 2;
        }

        TokenLength = o;
    }

    /// <summary>
    /// Tries to parse the current token as a number.
    /// When successful, this method sets the token type to Number, and
    /// set the number field. Otherwise, this method sets the token type
    /// to Token.
    /// </summary>
    private void TryParseNumber()
    {
        var value = new string(TokenBuffer[..TokenLength]);
        if (float.TryParse(value, out _number))
            _tokenType = PdfsTokenType.Number;
        else
            _tokenType = PdfsTokenType.Keyword;
    }
    #endregion



    // Private types
    // =============
    #region LexerReadState
    /// <summary>
    /// The LexerReadState enumeration lists the possible states that the lexer
    /// /// can be in while reading bytes off the stream.
    /// </summary>
    private enum LexerReadState
    {
        AfterAngleClose,
        AfterAngleOpen,
        Comment,
        Free,
        HexString,
        String,
        Keyword,
        WhiteSpace,
        Number,
        BinaryToken,
        Variable
    }
    #endregion
}