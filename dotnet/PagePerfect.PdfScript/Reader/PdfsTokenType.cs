namespace PagePerfect.PdfScript.Reader;

/// <summary>
/// The PdfsTokenType enumeration lists the possible token types in a .pdfs document.
/// </summary>
public enum PdfsTokenType
{
    ///  ArrayEnd
    ArrayEnd,

    /// ArrayStart
    ArrayStart,

    /// Comment
    Comment,

    /// DictionaryEnd
    DictionaryEnd,

    /// DictionaryStart
    DictionaryStart,

    /// Name
    Name,

    /// Null
    Null,

    /// Number
    Number,

    /// R
    R,

    /// String
    String,

    /// A keyword
    Keyword,

    /// Whitespace
    Whitespace,

    /// <summary>
    /// A variable, such as $name
    /// </summary>
    Variable,

    /// <summary>
    /// This is a prologue fragment, #
    /// </summary>
    PrologFragment
}
