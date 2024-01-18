namespace PagePerfect.PdfScript.Reader;

/// <summary>
/// The PdfsValueKind enumeration lists the possible kinds of values for operands -
/// numbers, strings, booleans, names, arrays, dictionaries and variables - as well
/// as keywords.
/// </summary>
public enum PdfsValueKind
{
    Number,

    String,

    Boolean,

    Name,

    Array,

    Dictionary,

    Variable,

    Keyword
}