namespace PagePerfect.PdfScript.Reader;

/// <summary>
/// The PdfsReader class is used to read a .pdfs document. It uses a lexer
/// to read tokens from the stream, and parses them into graphics operations,
/// prolog statements, and other operations such as page breaks, function
/// definitions and so on.
/// The PdfsReader is a state machine that outputs an statement each time
/// it is called. The statement can be a graphics operation, a prolog
/// statement, etc. It is also a syntax validator, and will throw an
/// exception if an expression is invalid.
/// The PdfsReader is used by the Document class to process the contents
/// of a .pdfs document into a PDF document. 
/// </summary>
public class PdfsReader(Stream stream)
{
    // Private fields
    // ==============
    #region Private fields
    private readonly PdfsLexer _lexer = new(stream);
    private readonly Stack<PdfsValue> _operandStack = new();
    #endregion



    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The statement that was read, or a Null reference if no statement was read.
    /// </summary>
    public PdfsStatement? Statement { get; private set; }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Reads the next statement off of the stream. This method returns a boolean
    /// that determines if the operation was successful. If successful, this
    /// method will set the Statement property to the statement that was read.
    /// Otherwise, Statement will be a Null reference. 
    /// </summary>
    /// <returns>True if a statement could be read. False otherwise.</returns>
    public async Task<bool> Read()
    {
        Statement = null;
        var finished = false;
        while (!finished && await _lexer.Read())
        {
            // Ihe the initial state, we expect any token. It could be
            // an operand, or the start of an operand, or it could be a
            // keyword that needs to be parsed into an operator, or it
            // could be a prolog fragment.
            switch (_lexer.TokenType)
            {
                case PdfsTokenType.ArrayStart:
                    // If this is the start of an array, then we need to
                    // read the array, and put it on the operand stack.
                    var array = await PdfsValue.ReadArray(_lexer);
                    if (null != array) _operandStack.Push(array);
                    else throw new PdfsReaderException("EOF reached while reading array value.");
                    break;

                case PdfsTokenType.DictionaryStart:
                    // If we read the start of a dictionary, then we need to
                    // read the entire dictionary, and put it on the operand
                    // stack.
                    var dict = await PdfsValue.ReadDictionary(_lexer);
                    if (null != dict) _operandStack.Push(dict);
                    else throw new PdfsReaderException("EOF reached while reading dictionary value.");
                    break;

                case PdfsTokenType.Keyword:
                    throw new NotImplementedException();
                //await ReadKeyword();

                case PdfsTokenType.Name:
                    // A name can go onto the operand stack.
                    _operandStack.Push(new PdfsValue(_lexer.String!, PdfsValueKind.Name));
                    break;

                case PdfsTokenType.Number:
                    // A number can go onto the operand stack.
                    _operandStack.Push(new PdfsValue(_lexer.Number));
                    break;

                case PdfsTokenType.PrologFragment:
                    throw new NotImplementedException();
                //await ReadPrologFragment();

                case PdfsTokenType.String:
                    // A string can go onto the operand stack.
                    _operandStack.Push(new PdfsValue(_lexer.String!));
                    break;

                case PdfsTokenType.Variable:
                    // A variable can go onto the operand stack.
                    _operandStack.Push(new PdfsValue(_lexer.String!, PdfsValueKind.Variable));
                    break;

                case PdfsTokenType.Whitespace:
                    // Ignore whitespace.
                    break;

                case PdfsTokenType.Comment:
                    // Ignore comments.
                    break;

                case PdfsTokenType.Null:
                default:
                    throw new PdfsReaderException($"Unexpected token type: {_lexer.TokenType}");
            }
        }

        return finished;
    }
    #endregion
}