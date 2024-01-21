using PagePerfect.PdfScript.Reader.Statements;

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
                    // A keyword. We take care of reserved keywords (true, false) and then
                    // we try to match the keyword against known statements and graphics instructions.
                    var keyword = _lexer.String!;
                    switch (keyword)
                    {
                        case "true":
                            _operandStack.Push(new PdfsValue(true));
                            break;
                        case "false":
                            _operandStack.Push(new PdfsValue(false));
                            break;
                        default:
                            // This keyword must be a statement such as a
                            // conditional statement, function statement, or
                            // a graphics operation.
                            // So we parse the keyword to find out.
                            Statement = ParseStatement(keyword);
                            finished = true;
                            break;
                    }
                    break;

                case PdfsTokenType.Name:
                    // A name can go onto the operand stack.
                    _operandStack.Push(new PdfsValue(_lexer.String!, PdfsValueKind.Name));
                    break;

                case PdfsTokenType.Number:
                    // A number can go onto the operand stack.
                    _operandStack.Push(new PdfsValue(_lexer.Number));
                    break;

                case PdfsTokenType.PrologFragment:
                    // If we find a # prolog fragement, then we will try to parse
                    // a prolog statement from it.
                    Statement = await PrologStatement.Parse(_lexer);
                    finished = true;
                    break;

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
                case PdfsTokenType.ArrayEnd:
                case PdfsTokenType.DictionaryEnd:
                case PdfsTokenType.R:
                default:
                    throw new PdfsReaderException($"Unexpected token type: {_lexer.TokenType}");
            }
        }

        return finished;
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Parses a statement. This method will determine the nature of the statement
    /// through the current keyword, and validates the operand stack contains the
    /// right sort of values before constructing a statement.
    /// If the statment could not be created, or was found to be invalid, this
    /// method throws an exception. 
    /// </summary>
    /// <param name="keyword">The current keyword</param>
    /// <returns>The PDF statement.</returns>
    private PdfsStatement ParseStatement(string keyword)
    {
        return keyword switch
        {
            // Look for flow control statement, such as "endpage"
            "endpage" => new EndPageStatement(),
            // Look for conditional statements, such as "if"

            // Look for graphics operations, such as "re" and "Q"
            // as per table 4.1 of PDF-1.7 (p.196)
            /*
                "q" => SaveGraphicsStateInstruction.Parse(operands),
                "Q" => RestoreGraphicsStateInstruction.Parse(operands),
                "gs" => GraphicsStateInstruction.Parse(operands),
                "cm" => TransformationMatrixInstruction.Parse(operands),
                "BX" => new[] { new BeginCompatibilityInstruction() },
                "EX" => new[] { new EndCompatibilityInstruction() },
                "BT" => TextInstructions.ParseBT(operands),
                "ET" => TextInstructions.ParseET(operands),
                "Tc" => TextInstructions.ParseTc(operands),
                "Tw" => TextInstructions.ParseTw(operands),
                "Tz" => TextInstructions.ParseTz(operands),
                "TL" => TextInstructions.ParseTL(operands),
                "Tf" => TextInstructions.ParseTf(operands),
                "Tr" => TextInstructions.ParseTr(operands),
                "Ts" => TextInstructions.ParseTs(operands),
                "Td" => TextInstructions.ParseTd(operands),
                "TD" => TextInstructions.ParseTD(operands),
                "Tm" => TextInstructions.ParseTm(operands),
                "T*" => TextInstructions.ParseTStar(operands),
                "Tj" => TextInstructions.ParseTj(operands),
                "'" => TextInstructions.ParseSQuot(operands),
                "\"" => TextInstructions.ParseDQuot(operands),
                "TJ" => TextInstructions.ParseTJ(operands),
                "Do" => XObjectInstruction.Parse(operands),
                "MP" => MarkerInstructions.ParseMP(operands),
                "DP" => MarkerInstructions.ParseDP(operands),
                "BMC" => MarkerInstructions.ParseBMC(operands),
                "BDC" => MarkerInstructions.ParseBDC(operands),
                "EMC" => MarkerInstructions.ParseEMC(operands),
                "m" => PathInstructions.ParseM(operands),
                "l" => PathInstructions.ParseL(operands),
                "c" => PathInstructions.ParseC(operands),
                "v" => PathInstructions.ParseV(operands),
                "y" => PathInstructions.ParseY(operands),
                "h" => PathInstructions.ParseH(operands),
                "re" => PathInstructions.ParseRe(operands),
                "S" => PathInstructions.ParseStroke(operands),
                "s" => PathInstructions.ParseCloseAndStroke(operands),
                "f" or "F" => PathInstructions.ParseFillNonZeroWinding(operands),
                "f*" => PathInstructions.ParseFillEvenOdd(operands),
                "B" => PathInstructions.ParseFillStrokeNonZeroWinding(operands),
                "B*" => PathInstructions.ParseFillStrokeEvenOdd(operands),
                "b" => PathInstructions.ParseCloseFillStrokeNonZeroWinding(operands),
                "b*" => PathInstructions.ParseCloseFillStrokeEvenOdd(operands),
                "n" => PathInstructions.ParseN(operands),
                "W" => PathInstructions.ParseClipNonZeroWinding(operands),
                "W*" => PathInstructions.ParseClipEvenOdd(operands),
                "CS" => ColourInstruction.ParseCS(operands),
                "cs" => ColourInstruction.Parsecs(operands),
                "SC" => ColourInstruction.ParseSC(operands),
                "sc" => ColourInstruction.Parsesc(operands),
                "SCN" => ColourInstruction.ParseSCN(operands),
                "scn" => ColourInstruction.Parsescn(operands),
                "G" => ColourInstruction.ParseG(operands),
                "g" => ColourInstruction.Parseg(operands),
                "RG" => ColourInstruction.ParseRG(operands),
                "rg" => ColourInstruction.Parserg(operands),
                "K" => ColourInstruction.ParseK(operands),
                "k" => ColourInstruction.Parsek(operands),
                "w" => LineWidthInstruction.Parse(operands),
                "J" => LineCapInstruction.Parse(operands),
                "j" => LineJoinInstruction.Parse(operands),
                "M" => MiterLimitInstruction.Parse(operands),
                "d" => LineDashPatternInstruction.Parse(operands),
                "ri" => RenderingIntentInstruction.Parse(operands),
                "i" => FlatnessToleranceInstruction.Parse(operands),
                _ => new[] { new UnknownInstruction(operands) },
            */
            _ => throw new PdfsReaderException($"Unknown statement or keyword at '{keyword}")
        };
    }
    #endregion
}