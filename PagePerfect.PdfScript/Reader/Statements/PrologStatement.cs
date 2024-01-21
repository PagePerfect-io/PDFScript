using System.Security.Cryptography;
using PagePerfect.PdfScript.Reader.Statements.Prolog;

namespace PagePerfect.PdfScript.Reader.Statements;

/// <summary>
/// The PrologStatement class represents a # prolog statement in a .pdfs document.
/// This is an abstract class.
/// </summary>
/// <param name="type">The type of prolog statement.</param>
public abstract class PrologStatement(PrologStatementType type) : PdfsStatement(PdfsStatementType.PrologStatement)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The type of prolog statement.
    /// </summary>
    public PrologStatementType PrologType { get; } = type;
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Parses a prolog statement from the specified lexer. This method assumes the lexer
    /// is already in a position having read the prolog fragment. This method returns the
    /// appropriate PrologStatement instance. If the statement's syntax is invalid, this
    /// method will throw an exception.
    /// </summary>
    /// <param name="lexer">The lexer to use.</param>
    /// <returns>The PrologStatement instance.</returns>
    public static async Task<PrologStatement> Parse(PdfsLexer lexer)
    {
        var keyword = await lexer.ReadKeyword();
        if (null == keyword)
            throw new PdfsReaderException($"Expected 'var' or 'resource', but found '{lexer.TokenType}'.");

        return keyword switch
        {
            "var" => await ParseVarDeclaration(lexer),
            "resource" => await ParseResourceDeclaration(lexer),
            _ => throw new PdfsReaderException($"Expected 'var' or 'resource', but found '{keyword}'.")
        };
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Parses a var declaration (# var ...) from the specified lexer, after the 'var' keyword
    /// has already been read. 
    /// </summary>
    /// <param name="lexer">The lexer to read tokens from.</param>
    /// <returns>The VarDeclaration instance.</returns>
    private static async Task<PrologStatement> ParseVarDeclaration(PdfsLexer lexer)
    {
        // Read the variable name.
        var name = await lexer.ReadVariable();
        if (null == name)
            throw new PdfsReaderException($"Expected variable name, but found '{lexer.TokenType}'.");

        // Read the variable type.
        var type = await lexer.ReadName();
        if (null == type)
            throw new PdfsReaderException($"Expected variable type, but found '{lexer.TokenType}'.");

        // We read the value based on the type. We only support a number of types.
        switch (type)
        {
            case "/Number":
                var num = await lexer.ReadNumber();
                if (null == num)
                    throw new PdfsReaderException($"Type mismatch: Expected a numeric value for variable, but found '{lexer.TokenType}'.");

                return new VarDeclaration(name, PdfsValueKind.Number, new PdfsValue(num.Value));

            case "/String":
                var str = await lexer.ReadString();
                if (null == str)
                    throw new PdfsReaderException($"Type mismatch: Expected a string value for variable, but found '{lexer.TokenType}'.");

                return new VarDeclaration(name, PdfsValueKind.String, new PdfsValue(str!));

            case "/Boolean":
                var keyword = await lexer.ReadKeyword();
                if (null == keyword)
                    throw new PdfsReaderException($"Type mismatch: Expected a boolean value for variable, but found '{lexer.TokenType}'.");

                return keyword switch
                {
                    "true" => new VarDeclaration(name, PdfsValueKind.Boolean, new PdfsValue(true)),
                    "false" => new VarDeclaration(name, PdfsValueKind.Boolean, new PdfsValue(false)),
                    _ => throw new PdfsReaderException($"Type mismatch: Expected a boolean value for variable, but found '{keyword}'."),
                };
            case "/Name":
                var nameValue = await lexer.ReadName();
                if (null == nameValue)
                    throw new PdfsReaderException($"Type mismatch: Expected a name value for variable, but found '{lexer.TokenType}'.");

                return new VarDeclaration(name, PdfsValueKind.Name, new PdfsValue(nameValue!, PdfsValueKind.Name));

            case "/Array":
                throw new NotImplementedException();

            case "/Dictionary":
                throw new NotImplementedException();

            default:
                throw new PdfsReaderException($"Invalid variable type '{type}'.");
        }
    }

    private static async Task<PrologStatement> ParseResourceDeclaration(PdfsLexer lexer)
    {
        throw new NotImplementedException();
    }

    #endregion
}