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
            "pattern" => await ParsePatternDeclaration(lexer),
            "color" => await ParseColourDeclaration(lexer),
            _ => throw new PdfsReaderException($"Expected 'var' or 'resource', but found '{keyword}'.")
        };
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Parses a colour declaration (# color ...) from the specified lexer, after the 'color' keyword
    /// has already been read.
    /// </summary>
    /// <param name="lexer">The lexer to read tokens from.</param>
    /// <returns>The ColourDeclaration instance.</returns>
    private static async Task<PrologStatement> ParseColourDeclaration(PdfsLexer lexer)
    {
        var name = await lexer.ReadName() ?? throw
            new PdfsReaderException($"Expected a name, but found '{lexer.TokenType}'.");

        var cs = await lexer.ReadName() ?? throw
            new PdfsReaderException($"Expected a name for colour space, but found '{lexer.TokenType}'.");

        // We support a few colour spaces.
        var colourSpace = cs switch
        {
            "/DeviceRGB" => ColourSpace.DeviceRGB,
            "/DeviceCMYK" => ColourSpace.DeviceCMYK,
            "/DeviceGray" => ColourSpace.DeviceGray,
            _ => throw new PdfsReaderException($"Invalid colour space '{cs}'."),
        };

        // We expect one, three, or four numbers for the colour components.
        var components = new List<float>();
        var numOfComponents = colourSpace switch
        {
            ColourSpace.DeviceRGB => 3,
            ColourSpace.DeviceCMYK => 4,
            ColourSpace.DeviceGray => 1,
            _ => throw new PdfsReaderException($"Invalid colour space '{cs}'."),
        };

        do
        {
            var num = await lexer.ReadNumber();
            if (null == num) throw new PdfsReaderException($"Expected a number for colour component {components.Count}.");
            components.Add(num.Value);
        } while (components.Count < numOfComponents);

        if (components.Count != numOfComponents)
            throw new PdfsReaderException($"Expected {numOfComponents} components for colours in the {colourSpace} colour space.");

        return new ColourDeclaration(name, new(colourSpace, [.. components]));
    }

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
                throw new PdfsReaderException($"Type mismatch: Arrays are not supported yet as variable types.");

            case "/Dictionary":
                throw new PdfsReaderException($"Type mismatch: Dictionaries are not supported yet as variable types.");

            default:
                throw new PdfsReaderException($"Invalid variable type '{type}'.");
        }
    }

    /// <summary>
    /// Parses a pattern declaration (# pattern ...) from the specified lexer, after
    /// the 'pattern' keyword has already been read.
    /// </summary>
    /// <param name="lexer">The lexer to read tokens from.</param>
    /// <returns>The PatternDeclaration instance.</returns>
    private static async Task<PrologStatement> ParsePatternDeclaration(PdfsLexer lexer)
    {
        var name = await lexer.ReadName() ?? throw
            new PdfsReaderException($"Expected a name, but found '{lexer.TokenType}'.");

        var type = await lexer.ReadName() ?? throw
            new PdfsReaderException($"Expected a name for pattern type, but found '{lexer.TokenType}'.");

        var cs = await lexer.ReadName() ?? throw
            new PdfsReaderException($"Expected a name for colour space, but found '{lexer.TokenType}'.");

        // We support a few pattern types.
        var patternType = type switch
        {
            "/LinearGradient" => PatternType.LinearGradient,
            "/RadialGradient" => PatternType.RadialGradient,
            _ => throw new PdfsReaderException($"Invalid pattern type '{type}'."),
        };

        // We support a few colour spaces.
        var colourSpace = cs switch
        {
            "/DeviceRGB" => ColourSpace.DeviceRGB,
            "/DeviceCMYK" => ColourSpace.DeviceCMYK,
            "/DeviceGray" => ColourSpace.DeviceGray,
            _ => throw new PdfsReaderException($"Invalid colour space '{cs}'."),
        };

        // We expect a dictionary with a bounding box, and an array of colours and stops.
        if (false == await lexer.ReadToken(PdfsTokenType.DictionaryStart)) throw
            new PdfsReaderException("Expected a dictionary for pattern declaration.");

        var dict = (await PdfsValue.ReadDictionary(lexer))?.GetDictionary() ?? throw
            new PdfsReaderException("Expected a dictionary for pattern declaration.");

        // Parse the /Rect array.
        if (!dict.TryGetValue("/Rect", out var rectValue) || PdfsValueKind.Array != rectValue.Kind)
            throw new PdfsReaderException("Expected a bounding box for pattern declaration.");
        var rectArray = rectValue.GetArray();
        var rect = new PdfRectangle(
            rectArray[0].GetNumber(),
            rectArray[1].GetNumber(),
            rectArray[2].GetNumber(),
            rectArray[3].GetNumber());

        // Parse the /C0, /C1, /C2, etc. colours.
        var componentCount = colourSpace switch
        {
            ColourSpace.DeviceRGB => 3,
            ColourSpace.DeviceCMYK => 4,
            ColourSpace.DeviceGray => 1,
            _ => throw new PdfsReaderException($"Invalid colour space '{cs}'."),
        };

        var colours = new List<Colour>();
        do
        {
            if (!dict.TryGetValue($"/C{colours.Count}", out var colourValue))
                break;

            if (PdfsValueKind.Array != colourValue.Kind)
                throw new PdfsReaderException($"Expected an array for colour {colours.Count}.");

            var colourArray = colourValue.GetArray();
            var components = colourArray.Select(c => c.GetNumber()).ToArray();
            if (components.Length != componentCount)
                throw new PdfsReaderException($"Expected {componentCount} components for colours in the {colourSpace} colour space.");
            colours.Add(new Colour(colourSpace, components));
        } while (true);

        // Parse the stops.
        if (!dict.TryGetValue("/Stops", out var stopsValue) || PdfsValueKind.Array != stopsValue.Kind)
            throw new PdfsReaderException("Expected an array for stops.");

        var stops = stopsValue.GetArray().Select(s => s.GetNumber()).ToArray();
        if (stops.Length != colours.Count)
            throw new PdfsReaderException("The number of stops must match the number of colours.");

        return new PatternDeclaration(name, patternType, colourSpace, rect, [.. colours], stops);
    }

    /// <summary>
    /// Parses a resource declaration (# resource ...) from the specified lexer, after 
    /// the 'resource' keyword has already been read. 
    /// </summary>
    /// <param name="lexer">The lexer to read tokens from.</param>
    /// <returns>The ResourceDeclaration instance.</returns>
    private static async Task<PrologStatement> ParseResourceDeclaration(PdfsLexer lexer)
    {
        // Read the resource name.
        var name = await lexer.ReadName();
        if (null == name)
            throw new PdfsReaderException($"Expected a name, but found '{lexer.TokenType}'.");

        // Read the resource type.
        var type = await lexer.ReadName();
        if (null == type)
            throw new PdfsReaderException($"Expected a name for resource type, but found '{lexer.TokenType}'.");

        // Read the resource location.
        var location = await lexer.ReadString();
        if (null == location)
            throw new PdfsReaderException($"Expected a string for location, but found '{lexer.TokenType}'.");

        // We support Image and Font types.
        var resourceType = type switch
        {
            "/Image" => ResourceType.Image,
            "/Font" => ResourceType.Font,
            _ => throw new PdfsReaderException($"Invalid resource type '{type}'."),
        };

        return new ResourceDeclaration(name, resourceType, location!);
    }
    #endregion
}