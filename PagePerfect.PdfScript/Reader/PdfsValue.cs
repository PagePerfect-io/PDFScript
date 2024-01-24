using Microsoft.VisualBasic;

namespace PagePerfect.PdfScript.Reader;

/// <summary>
/// The PdfsValue class encapsulates a value in a .pdfs document. This is an immutable class.
/// The value can be a boolean, number, string, name, variable, keyword, dictionary or array. 
/// </summary>
public class PdfsValue
{
    // Private fields
    // ==============
    #region Private fields
    private readonly bool _booleanValue;

    private readonly float _numberValue;

    private readonly string? _stringValue;

    private readonly Dictionary<string, PdfsValue>? _dictionaryValue;

    private readonly PdfsValue[]? _arrayValue;
    #endregion



    // Instance initialisers
    // =====================
    #region Instance initialisers
    /// <summary>
    /// Initialises a new PdfsValue of the number kind.
    /// </summary>
    /// <param name="value">The numerical value.</param>
    public PdfsValue(float value)
    {
        Kind = PdfsValueKind.Number;
        _numberValue = value;
    }

    /// <summary>
    /// Initialises a new PdfsValue of the boolean kind.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public PdfsValue(bool value)
    {
        Kind = PdfsValueKind.Boolean;
        _booleanValue = value;
    }

    /// <summary>
    /// Initialises a new PdfsValue of the string kind.
    /// </summary>
    /// <param name="value">The string value.</param>

    public PdfsValue(string value)
    {
        Kind = PdfsValueKind.String;
        _stringValue = value;
    }

    /// <summary>
    /// Initialises a new PdfsValue with the specified kind and string value.
    /// This initialiser is only valid for the string, name, variable or keyword kinds. 
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <param name="kind">The kind of value.</param> 
    public PdfsValue(string value, PdfsValueKind kind)
    {
        if (kind != PdfsValueKind.String && kind != PdfsValueKind.Name && kind != PdfsValueKind.Variable && kind != PdfsValueKind.Keyword)
            throw new ArgumentException("A string value must be a string, name, variable or keyword.");
        Kind = kind;
        _stringValue = value;
    }

    /// <summary>
    /// Initialises a new PdfsValue of the dictionary kind.
    /// </summary>
    /// <param name="value">The dictionary value.</param>
    public PdfsValue(Dictionary<string, PdfsValue> value)
    {
        Kind = PdfsValueKind.Dictionary;
        _dictionaryValue = value;
    }

    /// <summary>
    /// Initialises a new PdfsValue of the array kind.
    /// </summary>
    /// <param name="value">The array value.</param>
    public PdfsValue(PdfsValue[] value)
    {
        Kind = PdfsValueKind.Array;
        _arrayValue = value;
    }
    #endregion



    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The kind of value.
    /// </summary>
    public PdfsValueKind Kind { get; }
    #endregion


    public override bool Equals(object? obj)
    {
        if (false == obj is PdfsValue other) return false;
        if (Kind != other.Kind) return false;

        return Kind switch
        {
            PdfsValueKind.Number => _numberValue == other._numberValue,
            PdfsValueKind.Boolean => _booleanValue == other._booleanValue,
            PdfsValueKind.String => _stringValue == other._stringValue,
            PdfsValueKind.Name => _stringValue == other._stringValue,
            PdfsValueKind.Variable => _stringValue == other._stringValue,
            PdfsValueKind.Keyword => _stringValue == other._stringValue,
            PdfsValueKind.Dictionary => _dictionaryValue == other._dictionaryValue,
            PdfsValueKind.Array => _arrayValue == other._arrayValue,
            _ => throw new InvalidOperationException("Value is not a string, name, keyword or variable."),
        };

    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Kind, _booleanValue, _numberValue, _stringValue, _dictionaryValue, _arrayValue);
    }

    public override string ToString()
    {
        return Kind switch
        {
            PdfsValueKind.Number => _numberValue.ToString(),
            PdfsValueKind.String => _stringValue!,
            PdfsValueKind.Boolean => _booleanValue.ToString(),
            PdfsValueKind.Name => _stringValue!,
            PdfsValueKind.Variable => _stringValue!,
            PdfsValueKind.Keyword => _stringValue!,
            PdfsValueKind.Dictionary => "{Dictionary}",
            PdfsValueKind.Array => "{Array}",
            _ => throw new InvalidOperationException("Value is not a string, name, keyword or variable."),
        };
    }


    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Retrieves the numeric value, if the kind of value is a number.
    /// </summary>
    /// <returns>The number.</returns>
    /// <exception cref="InvalidOperationException">The value is not of the number kind.</exception>
    public float GetNumber() => Kind switch
    {
        PdfsValueKind.Number => _numberValue,
        _ => throw new InvalidOperationException("Value is not a number"),
    };

    /// <summary>
    /// Retrieves the string value, if the kind of value is a string, name, variable or keyword.
    /// </summary>
    /// <returns>The string value.</returns>
    /// <exception cref="InvalidOperationException">The value is not of the string, name, variable or keyword kind.</exception>
    public string GetString() => Kind switch
    {
        PdfsValueKind.String => _stringValue!,
        PdfsValueKind.Name => _stringValue!,
        PdfsValueKind.Variable => _stringValue!,
        PdfsValueKind.Keyword => _stringValue!,
        _ => throw new InvalidOperationException("Value is not a string, name, keyword or variable."),
    };

    /// <summary>
    /// Retrieves the boolean value, if the kind of value is a boolean.
    /// </summary>
    /// <returns>The boolean value.</returns>
    /// <exception cref="InvalidOperationException">The value is not of the boolean kind.</exception>
    public bool GetBoolean() => Kind switch
    {
        PdfsValueKind.Boolean => _booleanValue,
        _ => throw new InvalidOperationException("Value is not a boolean"),
    };

    /// <summary>
    /// Retrieves the dictionary value, if the kind of value is a dictionary.
    /// </summary>
    /// <returns>The dictionary.</returns>
    /// <exception cref="InvalidOperationException">The value is not of the dictionary kind.</exception>

    public Dictionary<string, PdfsValue> GetDictionary() => Kind switch
    {
        PdfsValueKind.Dictionary => _dictionaryValue!,
        _ => throw new InvalidOperationException("Value is not a dictionary"),
    };

    /// <summary>
    /// Retrieves the array value, if the kind of value is an array.
    /// </summary>
    /// <returns>The array.</returns>
    /// <exception cref="InvalidOperationException">The value is not of the array kind.</exception>

    public PdfsValue[] GetArray() => Kind switch
    {
        PdfsValueKind.Array => _arrayValue!,
        _ => throw new InvalidOperationException("Value is not an array"),
    };

    /// <summary>
    /// Reads an array of PDFs value from the specified lexer. This method
    /// assumes the lexer has already been used to read the array-start token. 
    /// This method will throw an exception when the syntax of the array is
    /// invalid. This method will return a Null reference if the end of the file
    /// is reached before the array is complete.
    /// </summary>
    /// <param name="lexer">The lexer.</param>
    /// <param name="val">(Out) The array value.</param>
    /// <returns>The array PdfsValue instance. A Null reference, otherwise.</returns>
    public static async Task<PdfsValue?> ReadArray(PdfsLexer lexer)
    {
        var array = new List<PdfsValue>();
        var finished = false;

        while (false == finished && await lexer.Read())
        {
            // In the array state, we treat the following tokens and names as special:
            // [ or << - parse a nested array or dictionary.
            // ] - end of the array.
            // # - invalid, a prolog fragment cannot appear in an array.            
            switch (lexer.TokenType)
            {
                case PdfsTokenType.ArrayEnd:
                    finished = true;
                    break;

                case PdfsTokenType.ArrayStart:
                    var nested = await ReadArray(lexer);
                    if (nested != null) array.Add(nested);
                    else return null;
                    break;

                case PdfsTokenType.Number:
                    array.Add(new PdfsValue(lexer.Number));
                    break;

                case PdfsTokenType.R:
                    throw new PdfsReaderException("Object reference not allowed in PDFScript documents.");

                case PdfsTokenType.PrologFragment:
                    throw new PdfsReaderException("Unexpected Prolog fragments in array.");

                case PdfsTokenType.DictionaryStart:
                    var dict = await ReadDictionary(lexer);
                    if (null != dict) array.Add(dict!);
                    else return null;
                    break;

                case PdfsTokenType.DictionaryEnd:
                    throw new PdfsReaderException("Unexpected >> token in array.");

                case PdfsTokenType.String:
                    array.Add(new PdfsValue(lexer.String!));
                    break;

                case PdfsTokenType.Name:
                    array.Add(new PdfsValue(lexer.String!, PdfsValueKind.Name));
                    break;

                case PdfsTokenType.Keyword:
                    throw new PdfsReaderException($"Unexpected keyword {lexer.String!} in array.");

                case PdfsTokenType.Variable:
                    array.Add(new PdfsValue(lexer.String!, PdfsValueKind.Variable));
                    break;
            }
        }

        if (!finished) return null;
        return new PdfsValue(array.ToArray());
    }

    /// <summary>
    /// Reads a dictionary of PDFs value from the specified lexer. This method
    /// assumes the lexer has already been used to read the dictionary-start token. 
    /// This method will throw an exception when the syntax of the dictionary is
    /// invalid. This method will return a Null reference if the end of the file
    /// is reached before the dictionary is complete.
    /// </summary>
    /// <param name="lexer">The lexer.</param>
    /// <param name="val">(Out) The dictionary value.</param>
    /// <returns>The dictionary PDF value instance if successful. Otherwise, a Null reference.</returns>
    public static async Task<PdfsValue?> ReadDictionary(PdfsLexer lexer)
    {
        var dictionary = new Dictionary<string, PdfsValue>();
        var finished = false;
        string? dictKey = null;

        while (false == finished && await lexer.Read())
        {
            if (null == dictKey)
            {
                // If we've not yet read the dictionary key, then we expect a name or the
                // dictionary end token. Any other non-whitespace token is invalid.
                switch (lexer.TokenType)
                {
                    // If this token is a dictionary end token, then we're done.
                    case PdfsTokenType.DictionaryEnd:
                        finished = true;
                        break;

                    // If this token is a name then we remember it - this is our
                    // dictionary key.
                    case PdfsTokenType.Name:
                        dictKey = lexer.String;
                        break;

                    case PdfsTokenType.Whitespace:
                        // We ignore whitespace and comments.
                        break;

                    default:
                        throw new PdfsReaderException(
                            $"Unexpected {lexer.TokenType} token in dictionary - expected name or >> token.");
                }
            }
            else
            {
                // We've read the dictionary key, so now we read the value(s).
                switch (lexer.TokenType)
                {
                    case PdfsTokenType.ArrayEnd:
                        throw new PdfsReaderException("Unexpected ] token in dictionary");

                    case PdfsTokenType.ArrayStart:
                        var arr = await ReadArray(lexer);
                        if (null == arr) return null;

                        dictionary.Add(dictKey, arr);
                        dictKey = null;
                        break;

                    case PdfsTokenType.Name:
                        // This is a name. We use it as the value of the dictionary entry.
                        dictionary.Add(dictKey, new PdfsValue(lexer.String!, PdfsValueKind.Name));
                        dictKey = null;
                        break;

                    case PdfsTokenType.Number:
                        // This is a number. We use it as the value of the dictionary entry.
                        dictionary.Add(dictKey, new PdfsValue(lexer.Number));
                        dictKey = null;
                        break;

                    case PdfsTokenType.R:
                        throw new PdfsReaderException("Object reference not allowed in PDFScript documents.");

                    case PdfsTokenType.PrologFragment:
                        throw new PdfsReaderException("Unexpected Prolog fragments in array.");

                    case PdfsTokenType.DictionaryStart:
                        var nested = await ReadDictionary(lexer);
                        if (null == nested) return null;

                        dictionary.Add(dictKey, nested);
                        dictKey = null;

                        break;

                    case PdfsTokenType.DictionaryEnd:
                        throw new PdfsReaderException(
                            $"Unexpected {lexer.TokenType} token in dictionary - expected name or >> token.");

                    case PdfsTokenType.String:
                        // This is a string. We use it as the value of the dictionary entry.
                        dictionary.Add(dictKey, new PdfsValue(lexer.String!));
                        dictKey = null;
                        break;

                    case PdfsTokenType.Keyword:
                        throw new PdfsReaderException($"Unexpected keyword {lexer.String!} in array.");

                    case PdfsTokenType.Variable:
                        // This is a variable. We use it as the value of the dictionary entry.
                        dictionary.Add(dictKey, new PdfsValue(lexer.String!, PdfsValueKind.Variable));
                        dictKey = null;
                        break;
                }
            }
        }

        return !finished ? null : new PdfsValue(dictionary);
    }
    #endregion
}
