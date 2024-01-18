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
    #endregion
}
