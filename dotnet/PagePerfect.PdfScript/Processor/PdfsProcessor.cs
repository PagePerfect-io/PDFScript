using System.Runtime.CompilerServices;
using PagePerfect.PdfScript.Processor.Text;
using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Reader.Statements;
using PagePerfect.PdfScript.Reader.Statements.Prolog;
using PagePerfect.PdfScript.Writer;
using PagePerfect.PdfScript.Writer.Resources.Fonts;
using PagePerfect.PdfScript.Writer.Resources.Patterns;

namespace PagePerfect.PdfScript.Processor;

/// <summary>
/// The PdfsProcessor class is used to parse a .pdfs document. It uses a reader
/// to read statements off of a source stream, and processes them into PDF
/// content instructions and document layout objects. 
/// /// </summary>
public class PdfsProcessor(Stream source, IPdfDocumentWriter writer)
{
    // Private fields
    // ==============
    #region Private fields
    private readonly PdfsReader _reader = new(source);
    private readonly IPdfDocumentWriter _writer = writer;
    private PdfsProcessorState _state = PdfsProcessorState.Initial;
    private readonly Dictionary<string, ColourDeclaration> _colourDeclarations = [];
    private readonly Dictionary<string, PatternDeclaration> _patternDeclarations = [];
    private readonly Dictionary<string, ResourceDeclaration> _resourceDeclarations = [];
    private readonly Dictionary<string, PdfsVariable> _variables = [];
    private readonly Dictionary<string, PdfResourceReference> _localResources = [];
    private GraphicsObject _currentGraphicsObject = GraphicsObject.Page;
    private (float Width, float Height) _pageSize = (595, 842);
    private static readonly string[] _reservedNames;
    private static readonly PageTemplate[] _pageTemplates;
    private GraphicsState _graphicsState = new();
    private Stack<GraphicsState> _graphicsStateStack = new();
    private (float Width, float Height) _textBoxConstraint = (float.NaN, float.NaN);
    #endregion



    // Type initialiser
    // ================
    #region Type initialiser
    /// <summary>
    /// Initialises the static fields of the PdfsProcessor class.
    /// </summary>
    static PdfsProcessor()
    {
        // TODO add font names to reserved names.
        _reservedNames = ["/Type", "/Name", "/Length", "/Image", "/Font", "/Form", "/XObject", "/String", "/Number", "/Name", "/List", "/Dictionary", "/Boolean"];

        _pageTemplates = [new("/A0", 2384, 3370), new("/A1", 1684, 2384), new("/A2", 1191, 1684), new("/A3", 842, 1191), new("/A4", 595, 842), new("/A5", 420, 595), new("/A6", 298, 420), new("/Letter", 612, 791), new("/Legal", 612, 1009), new("/Tabloid", 791, 1225)];
    }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Processes the source stream into a PDF document.
    /// </summary>
    /// <param name="writer">The writer to use.</param>
    public async Task Process()
    {
        // We process statements from the reader until we reach the end of the stream.
        // Depending on the processor's state and the type of statement, we will
        // validate and output graphics instructions, prolog statements, or others.
        while (await _reader.Read())
        {
            // If we've not yet opened the document, do so now.
            if (!_writer.IsOpen) await _writer.Open();

            switch (_reader.Statement!.Type)
            {
                case PdfsStatementType.PrologStatement:
                    // If we are in the initial state, we can process a prolog statement.
                    if (_state != PdfsProcessorState.Initial) throw new
                        PdfsProcessorException("A Prolog statement must appear before any content statements.");

                    ProcessPrologStatement((_reader.Statement as PrologStatement)!);
                    break;

                case PdfsStatementType.EndPageStatement:
                    // Close the curent page, and change the state to BeforePage.
                    // If there is no current page, we will create an empty one.
                    switch (_state)
                    {
                        case PdfsProcessorState.BeforePage:
                        case PdfsProcessorState.Initial:
                            await OpenNewPage();
                            break;
                    }

                    await _writer.ClosePage();
                    _state = PdfsProcessorState.BeforePage;
                    break;

                case PdfsStatementType.PageStatement:
                    // We can do this at any stage as the statement
                    // affects subsequent content, not the current state.
                    ProcessPageStatement((_reader.Statement as PageStatement)!);
                    break;

                case PdfsStatementType.GraphicsOperation:
                    // We validate and output the graphics operation.
                    // If we're not on a page, we open a page.
                    if (_state != PdfsProcessorState.OnPage)
                    {
                        await OpenNewPage();
                        _state = PdfsProcessorState.OnPage;
                    }

                    // Output the graphics operation.
                    await WriteGraphicsOperation((_reader.Statement as GraphicsOperation)!);
                    break;
            }
        }

        // We're done, so we can close the current page and document.
        await _writer.CloseIfNeeded();
    }

    /// <summary>
    /// Processes the source stream into a PDF document.
    /// </summary>
    /// <param name="source">The source stream that contains a .pdfs script file.</param>
    /// <param name="writer">The writer to use.</param>
    public static async Task Process(Stream source, IPdfDocumentWriter writer)
    {
        var processor = new PdfsProcessor(source, writer);
        await processor.Process();
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// The EnsureLocalImage method ensures that an image is stored in the document.
    /// If the image hasn't already been added, this method will download the image
    /// and add it to the document.
    /// </summary>
    /// <param name="location">The location of the resource.</param>
    /// <returns>A resource reference.</returns>
    private async Task<PdfResourceReference> EnsureLocalImage(string location)
    {
        if (_localResources.TryGetValue(location, out var resource)) return resource;

        // Download the image locally.
        var localPath = await DownloadResourceToTempFile(location);

        // Add the image to the document.
        var image = _writer.CreateImage(localPath);
        _localResources[location] = image;

        return image;
    }

    /// <summary>
    /// The EnsureLocalTrueType method ensures that a TrueType font is stored in the document.
    /// If the font hasn't already been added, this method will download the font
    /// and add it to the document.
    /// </summary>
    /// <param name="location">The location of the resource.</param>
    /// <returns>A resource reference.</returns>
    private async Task<Font> EnsureLocalTrueTypeFont(string location)
    {
        if (_localResources.TryGetValue(location, out var resource)) return (Font)resource;

        // Download the font locally.
        var localPath = await DownloadResourceToTempFile(location);

        // Add the font to the document.
        var font = _writer.CreateTrueTypeFont(localPath);
        _localResources[location] = font;

        return font;
    }

    /// <summary>
    /// Downloads a resource to a temporary file. This method will use the
    /// specified path if it's a local path, or else it will attempt to download
    /// the remote contents into a temporary file.
    /// </summary>
    /// <param name="location">The location of the file.</param>
    /// <returns>The location of the downloaded file.</returns>
    private static async Task<string> DownloadResourceToTempFile(string location)
    {
        var localPath = Path.ChangeExtension(Path.GetTempFileName(), Path.GetExtension(location));

        if (File.Exists(location))
        {
            File.Copy(location, localPath, true);
            return localPath;
        }
        else
        {
            try
            {
                using var client = new HttpClient();
                using var stmIn = await client.GetStreamAsync(location);
                using var stmOut = File.OpenWrite(localPath);
                await stmIn.CopyToAsync(stmOut);

                return localPath;
            }
            catch (InvalidOperationException e)
            {
                throw new PdfsProcessorException($"Failed to download resource '{location}'.", e);
            }
            catch (HttpRequestException e)
            {
                throw new PdfsProcessorException($"Failed to download resource '{location}'.", e);
            }
            catch (IOException e)
            {
                throw new PdfsProcessorException($"Failed to download resource '{location}'.", e);
            }
            catch (TaskCanceledException e)
            {
                throw new PdfsProcessorException($"Failed to download resource '{location}'.", e);
            }
            catch (UriFormatException e)
            {
                throw new PdfsProcessorException($"Failed to download resource '{location}'.", e);
            }
        }
    }

    /// <summary>
    /// Determines if a resource name is unique. This method checks that the name
    /// isn't already in use by a resource, variable, or other declaration.
    /// </summary>
    /// <param name="name">The resource name.</param>
    /// <returns>True if the name isn't yet in use. False otherwise.</returns>
    private bool IsUniqueResourceName(string name)
    {
        return !_resourceDeclarations.ContainsKey(name) &&
               !_patternDeclarations.ContainsKey(name) &&
               !_colourDeclarations.ContainsKey(name);
    }

    /// <summary>
    /// Opens a new page in the document.
    /// </summary>
    private async Task OpenNewPage()
    {
        // If we're currently not on the Page object, we throw an exception -
        // you must close a text or path object before opening a new page.
        if (_currentGraphicsObject != GraphicsObject.Page) throw new
            PdfsProcessorException("You must close a text or path object before opening a new page.");

        await _writer.OpenPage(_pageSize.Width, _pageSize.Height, DisplayOrientation.Regular);
        await _writer.OpenContentStream();

        _currentGraphicsObject = GraphicsObject.Page;
    }

    /// <summary>
    /// Processes the page statement. This method will validate
    /// that a named page exists, or that the page dimensions are valid.
    /// </summary>
    /// <param name="page">The page statement.</param>
    private void ProcessPageStatement(PageStatement page)
    {
        if (page.Template is not null)
        {
            var template = _pageTemplates.FirstOrDefault(t => t.Name == page.Template) ?? throw new
                PdfsProcessorException($"Page template '{page.Template}' is not defined.");

            _pageSize = (template.Width, template.Height);
        }
        else
        {
            if (page.Width <= 0 || page.Height <= 0) throw new
                PdfsProcessorException("Page dimensions must be positive.");

            _pageSize = (page.Width, page.Height);
        }
    }

    private void ProcessPrologStatement(PrologStatement prolog)
    {
        switch (prolog.PrologType)
        {
            case PrologStatementType.VarDeclaration:
                TryAddVariableDeclaration((prolog as VarDeclaration)!);
                break;

            case PrologStatementType.ResourceDeclaration:
                TryAddResourceDeclaration((prolog as ResourceDeclaration)!);
                break;

            case PrologStatementType.PatternDeclaration:
                TryAddPatternDeclaration((prolog as PatternDeclaration)!);
                break;

            case PrologStatementType.ColourDeclaration:
                TryAddColourDeclaration((prolog as ColourDeclaration)!);
                break;
        }
    }

    /// <summary>
    /// Processes a Tb operation. This method will process the text box constraint,
    /// and set the current text box.
    /// </summary>
    /// <param name="op">The graphics operation.</param>
    private void ProcessTextBoxConstraint(GraphicsOperation op)
    {
        var w = ResolveOperand(op.Operands[0]);
        var h = ResolveOperand(op.Operands[1]);

        var width = w.Kind switch
        {
            PdfsValueKind.Number => Math.Max(0, w.GetNumber()),
            PdfsValueKind.Name => w.GetString() switch
            {
                "/Auto" => float.NaN,
                _ => throw new PdfsProcessorException($"Invalid width value for text box '{w.GetString()}'.")
            },
            _ => throw new PdfsProcessorException($"Invalid width value for text box '{w.Kind}'.")
        };

        var height = h.Kind switch
        {
            PdfsValueKind.Number => Math.Max(0, h.GetNumber()),
            PdfsValueKind.Name => h.GetString() switch
            {
                "/Auto" => float.NaN,
                _ => throw new PdfsProcessorException($"Invalid height value for text box '{h.GetString()}'.")
            },
            _ => throw new PdfsProcessorException($"Invalid height value for text box '{h.Kind}'.")
        };

        _textBoxConstraint = (width, height);
    }

    private PdfsValue ResolveArray(PdfsValue[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = ResolveOperand(array[i]);
        }

        return new PdfsValue(array);
    }

    private PdfsValue ResolveDictionary(Dictionary<string, PdfsValue> dictionary)
    {
        foreach (var (key, value) in dictionary)
        {
            dictionary[key] = ResolveOperand(value);
        }

        return new PdfsValue(dictionary);
    }

    /// <summary>
    /// Resolves a font. This method will resolve a font by name, and return
    /// a reference to the font resource. If the font is a standard font, this
    /// method will ask the writer to create a standard font resource. Otherwise,
    /// this method will resolve the resource, download the font from the
    /// resource location, and create a true type font resource. 
    /// /// </summary>
    /// <param name="fontName">The name of the font.</param>
    /// <returns>The font resource.</returns>
    private async Task<Font> ResolveFont(string fontName)
    {
        if (FontUtilities.IsStandardFont(fontName))
        {
            if (_localResources.TryGetValue(fontName, out var resource)) return (Font)resource!;

            var font = _writer.CreateStandardFont(fontName);
            _localResources[fontName] = font;

            return font;
        }

        // This is not a standard font. We support TrueType fonts.

        // Find the resource declaration and check the type.
        var location = ResolveResource(fontName, ResourceType.Font);

        // Add to the document if not already added.
        return await EnsureLocalTrueTypeFont(location);
    }

    /// <summary>
    /// Resolves an operand. This method will resolve a variable by name,
    /// if the operand is a variable reference. If the operand is a dictionary
    /// or array, it will resolve its elements recursively.
    /// </summary>
    /// <param name="operand">The operand value.</param>
    /// <returns>The resolved value.</returns>
    private PdfsValue ResolveOperand(PdfsValue operand)
    {
        return operand.Kind switch
        {
            PdfsValueKind.Variable => ResolveVariable(operand.GetString()),
            PdfsValueKind.Dictionary => ResolveDictionary(operand.GetDictionary()),
            PdfsValueKind.Array => ResolveArray(operand.GetArray()),
            _ => operand
        };
    }

    /// <summary>
    /// Tries to resolve a pattern. This method will find a pattern by name, and
    /// then check if a resource instance has been created yet. If not, it
    /// will create the resource and add it to the document.
    /// If the name does not match a pattern, we return a Null reference.
    /// </summary>
    /// <param name="name">The name of the pattern.</param>
    /// <returns>The Pattern resource.</returns>
    private bool TryResolvePattern(string name, out Pattern? pattern)
    {
        if (!_patternDeclarations.TryGetValue(name, out var declaration)) { pattern = null; return false; }

        if (_localResources.TryGetValue(name, out var resource)) { pattern = (Pattern)resource!; return true; }

        pattern = declaration.PatternType switch
        {
            Reader.Statements.Prolog.PatternType.RadialGradient => _writer.CreateRadialGradientPattern(declaration.BoundingRectangle, declaration.Colours, declaration.Stops),
            Reader.Statements.Prolog.PatternType.LinearGradient => _writer.CreateLinearGradientPattern(declaration.BoundingRectangle, declaration.Colours, declaration.Stops),
            _ => throw new PdfsProcessorException($"Pattern type '{declaration.PatternType}' is not supported.")
        };
        _localResources[name] = pattern;
        return true;
    }

    /// <summary>
    /// Resolves a resource. This method resolves a resource by name, and
    /// returns its location. If the resource cannot be found, or is not of
    /// the expected type, this method throws an exception.
    /// </summary>
    /// <param name="name">The resource name.</param>
    /// <param name="type">The type of resource.</param>
    /// <returns>The resource locaiton.</returns>
    private string ResolveResource(string name, ResourceType type)
    {
        if (!_resourceDeclarations.TryGetValue(name, out var resource))
        {
            throw new PdfsProcessorException($"Resource '{name}' is not defined.");
        }

        if (resource.ResourceType != type) throw new PdfsProcessorException($"Resource '{name}' is not of type '{type}'.");

        return resource.Location;
    }

    /// <summary>
    /// resolves a variable by name. This method locates the variable,
    /// case-sensitive, and returns its value.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>The value.</returns>
    /// <exception cref="PdfsProcessorException">The variable could not be found.</exception> <summary>
    private PdfsValue ResolveVariable(string name)
    {
        if (_variables.TryGetValue(name, out var variable))
        {
            return variable.Value;
        }

        throw new PdfsProcessorException($"Variable '{name}' is not defined.");
    }

    /// <summary>
    /// Tries to add a colour declaration to the state machine. This method checks that
    /// a colour, pattern or other resource does not already exist, and that the name is not
    /// a reserved name.
    /// </summary>
    /// <param name="statement"></param>
    private void TryAddColourDeclaration(ColourDeclaration statement)
    {
        if (!IsUniqueResourceName(statement.Name)) throw
            new PdfsProcessorException($"A resource with name '{statement.Name}' is already defined.");

        if (_reservedNames.Contains(statement.Name)) throw
            new PdfsProcessorException($"'{statement.Name}' is a reserved name.");

        _colourDeclarations[statement.Name] = statement;
    }

    /// <summary>
    /// Tries to add a pattern declaration to the state machine. This method checks that
    /// a pattern or other resource does not already exist, and that the name is not
    /// a reserved name.
    /// </summary>
    /// <param name="statement"></param>
    private void TryAddPatternDeclaration(PatternDeclaration statement)
    {
        if (!IsUniqueResourceName(statement.Name)) throw
            new PdfsProcessorException($"A resource with name '{statement.Name}' is already defined.");

        if (_reservedNames.Contains(statement.Name)) throw
            new PdfsProcessorException($"'{statement.Name}' is a reserved name.");

        _patternDeclarations[statement.Name] = statement;
    }

    private void TryAddResourceDeclaration(ResourceDeclaration statement)
    {
        if (!IsUniqueResourceName(statement.Name)) throw
            new PdfsProcessorException($"A resource with name '{statement.Name}' is already defined.");

        if (_reservedNames.Contains(statement.Name)) throw
            new PdfsProcessorException($"'{statement.Name}' is a reserved name.");

        _resourceDeclarations[statement.Name] = statement;
    }

    private void TryAddVariableDeclaration(VarDeclaration statement)
    {
        if (_variables.ContainsKey(statement.Name))
        {
            throw new PdfsProcessorException($"Variable '{statement.Name}' is already defined.");
        }

        _variables[statement.Name] = new PdfsVariable(statement.Name, statement.Value);
        _reader.SetVariableType(statement.Name, statement.Datatype);
    }

    /// <summary>
    /// Writes a Do operation. This method will resolve the resource,
    /// locate it, add it to the document, and output the Do operation. 
    /// </summary>
    /// <param name="op">The operation</param>
    private async Task WriteDoOperation(GraphicsOperation op)
    {
        // We only support images for now.
        var location = ResolveResource(op.Operands[0].GetString(), ResourceType.Image);

        // Add to the document if not already added.
        var stored = await EnsureLocalImage(location);

        // Add the image to the current page.
        _writer.AddResourceToPage(stored);

        // Write the Do operation.
        await _writer.WriteRawContent($"/{stored.Identifier} Do\r\n");
    }

    /// <summary>
    /// Writes an ellipse (ell) operation to the output stream. This method
    /// outputs path instructions that approximate a circle or ellipse.
    /// </summary>
    /// <param name="op">The operation.</param>
    private async Task WriteEllipseOperation(GraphicsOperation op)
    {
        // We resolve the operands.
        var x = ResolveOperand(op.Operands[0]).GetNumber();
        var y = ResolveOperand(op.Operands[1]).GetNumber();
        var w = ResolveOperand(op.Operands[2]).GetNumber();
        var h = ResolveOperand(op.Operands[3]).GetNumber();

        if (w <= 0 || h <= 0) return;

        var rx = w / 2f;
        var ry = h / 2f;
        var ellipseModifier = (float)(4 * ((Math.Sqrt(2) - 1) / 3f));
        var cx = rx * ellipseModifier;
        var cy = rx * ellipseModifier;

        // We output the path instructions.
        await _writer.WriteRawContent($"{x + rx:F2} {y:F2} m\r\n");
        await _writer.WriteRawContent($"{x + rx + cx:F2} {y:F2} {x + w:F2} {y + ry - cy:F2} {x + w:F2} {y + ry:F2} c\r\n");
        await _writer.WriteRawContent($"{x + w:F2} {y + ry + cy:F2} {x + rx + cx:F2} {y + h:F2} {x + rx:F2} {y + h:F2} c\r\n");
        await _writer.WriteRawContent($"{x + rx - cx:F2} {y + h:F2} {x:F2} {y + ry + cy:F2} {x:F2} {y + ry:F2} c\r\n");
        await _writer.WriteRawContent($"{x:F2} {y + ry - cy:F2} {x + rx - cx:F2} {y:F2} {x + rx:F2} {y:F2} c\r\n");
        await _writer.WriteRawContent($"h\r\n");
    }

    /// <summary>
    /// Writes a "scn" or "SCN" operation to the output stream.
    /// If this operation is of the format \Name scn, then we will
    /// treat it differently to the PDF standard:
    /// If the name resolves to a pattern, we create the necessary pattern
    /// resource in the document, as well as add it to the page. We also
    /// set the colour space to /Pattern.
    /// If the name resolves to a colour, we set the colour space
    /// to match the colour's space, and output the colour.
    /// /// </summary>
    /// <param name="op">The Graphics operation.</param>
    private async Task WriteSetColourNewOperation(GraphicsOperation op)
    {
        var name = ResolveOperand(op.Operands[0]);
        if (PdfsValueKind.Name != name.Kind || op.Operands.Length > 1)
        {
            await WriteStandardGraphicsOperation(op);
        }
        else
        {
            // Is this a pattern?
            if (TryResolvePattern(name.GetString(), out var pattern))
            {
                var csOp = op.Operator == Operator.scn ? Operator.cs : Operator.CS;

                // Add the pattern to the current page.
                _writer.AddResourceToPage(pattern!);

                // Set the colour space to /Pattern and output the pattern.
                await _writer.WriteRawContent($"/Pattern {csOp} /{pattern!.Identifier} {op.GetOperatorName()}\r\n");
            }
            else if (_colourDeclarations.TryGetValue(name.GetString(), out var colourDeclaration))
            {
                var col = colourDeclaration!.Colour;
                Operator colOp;
                switch (col.ColourSpace)
                {
                    case ColourSpace.DeviceRGB:
                        colOp = op.Operator == Operator.scn ? Operator.rg : Operator.RG;
                        await _writer.WriteRawContent($"{col.Components[0]:F2} {col.Components[1]:F2} {col.Components[2]:F2} {colOp}\r\n");
                        break;
                    case ColourSpace.DeviceCMYK:
                        colOp = op.Operator == Operator.scn ? Operator.k : Operator.K;
                        await _writer.WriteRawContent($"{col.Components[0]:F2} {col.Components[1]:F2} {col.Components[2]:F2} {col.Components[3]:F2} {colOp}\r\n");
                        break;
                    case ColourSpace.DeviceGray:
                        colOp = op.Operator == Operator.scn ? Operator.g : Operator.G;
                        await _writer.WriteRawContent($"{col.Components[0]:F2} {colOp}\r\n");
                        break;
                }
            }
            else throw new PdfsProcessorException($"Resource '{name.GetString()}' is not defined.");
        }
    }

    /// <summary>
    /// Writes a graphics operation to the output stream. This method detects
    /// special cases, such as Do and Tj, and dispatches appropriately, before
    /// defaulting to a standard output of the operands and operator.
    /// </summary>
    /// <param name="op">The operation.</param>
    private async Task WriteGraphicsOperation(GraphicsOperation op)
    {
        // Check if the operation is allowed in the current graphics object.
        if (!op.IsAllowedIn(_currentGraphicsObject)) throw
            new PdfsProcessorException($"Operation '{op.GetOperatorName()}' is not allowed in a {_currentGraphicsObject} object.");

        switch (op.Operator)
        {
            case Operator.Do:
                await WriteDoOperation(op);
                break;

            case Operator.q:
                _graphicsStateStack.Push(_graphicsState.Clone());
                await WriteStandardGraphicsOperation(op);
                break;

            case Operator.Q:
                if (_graphicsStateStack.Count == 0) throw new PdfsProcessorException("Cannot restore state - no state to restore.");
                _graphicsState = _graphicsStateStack.Pop();
                await WriteStandardGraphicsOperation(op);
                break;

            case Operator.Tf:
                await WriteTfOperation(op);
                break;

            case Operator.Ta:
                // Process horizontal text alignment.
                _graphicsState.HorizontalTextAlignment = (HorizontalTextAlignment)ResolveOperand(op.Operands[0]).GetNumber();
                break;

            case Operator.TA:
                // Process vertical text alignment.
                _graphicsState.VerticalTextAlignment = (VerticalTextAlignment)ResolveOperand(op.Operands[0]).GetNumber();
                break;

            case Operator.Tb:
                // Process the text box constraint.
                ProcessTextBoxConstraint(op);
                break;

            case Operator.Tc:
                // Process character spacing.
                _graphicsState.CharacterSpacing = ResolveOperand(op.Operands[0]).GetNumber();
                await WriteStandardGraphicsOperation(op);
                break;

            case Operator.Tw:
                // Process word spacing.
                _graphicsState.WordSpacing = ResolveOperand(op.Operands[0]).GetNumber();
                await WriteStandardGraphicsOperation(op);
                break;

            case Operator.Tz:
                // Process horizontal scaling.
                _graphicsState.HorizontalScaling = ResolveOperand(op.Operands[0]).GetNumber() / 100f;
                await WriteStandardGraphicsOperation(op);
                break;

            case Operator.TL:
                // Process leading.
                _graphicsState.Leading = ResolveOperand(op.Operands[0]).GetNumber();
                await WriteStandardGraphicsOperation(op);
                break;

            case Operator.Ts:
                // Process rise.
                _graphicsState.Rise = ResolveOperand(op.Operands[0]).GetNumber();
                await WriteStandardGraphicsOperation(op);
                break;

            case Operator.Tfl:
                await WriteTflOperation(op);
                break;

            //            case Operator.Tj:
            //              await WriteTjOperation(op);
            //            break;

            case Operator.rr:
                await WriteRoundedRectangleOperation(op);
                break;

            case Operator.ell:
                await WriteEllipseOperation(op);
                break;

            case Operator.scn:
            case Operator.SCN:
                await WriteSetColourNewOperation(op);
                break;

            default:
                await WriteStandardGraphicsOperation(op);
                break;
        }

        // Some operations change the current graphics object.
        switch (op.Operator)
        {
            case Operator.BT:
                _currentGraphicsObject = GraphicsObject.Text;
                break;

            case Operator.ET:
                _currentGraphicsObject = GraphicsObject.Page;
                break;

            case Operator.m:
            case Operator.re:
            case Operator.rr:
            case Operator.ell:
                _currentGraphicsObject = GraphicsObject.Path;
                break;

            case Operator.S:
            case Operator.s:
            case Operator.f:
            case Operator.F:
            case Operator.fStar:
            case Operator.B:
            case Operator.BStar:
            case Operator.b:
            case Operator.bStar:
            case Operator.n:
                _currentGraphicsObject = GraphicsObject.Page;
                break;
        }
    }

    /// <summary>
    /// Writes a rounded rectangle (rr) operation to the output stream. This method
    /// outputs path instructions that approximate a rounded rectangle.
    /// </summary>
    /// <param name="op">The operation.</param>
    private async Task WriteRoundedRectangleOperation(GraphicsOperation op)
    {
        // We resolve the operands.
        var x = ResolveOperand(op.Operands[0]).GetNumber();
        var y = ResolveOperand(op.Operands[1]).GetNumber();
        var w = ResolveOperand(op.Operands[2]).GetNumber();
        var h = ResolveOperand(op.Operands[3]).GetNumber();
        var rx = ResolveOperand(op.Operands[4]).GetNumber();
        var ry = op.Operands.Length > 5 ? ResolveOperand(op.Operands[5]).GetNumber() : rx;

        rx = (float)Math.Min(rx, w / 2f);
        ry = (float)Math.Min(ry, h / 2f);

        if (rx <= 0 || ry <= 0)
        {
            await _writer.WriteRawContent($"{x:F2} {y:F2} {w:F2} {h:F2} re\r\n");
            return;
        }

        var ellipseModifier = (float)(4 * ((Math.Sqrt(2) - 1) / 3f));
        var cx = rx * ellipseModifier;
        var cy = rx * ellipseModifier;

        // We output the path instructions.
        // Bottom edge
        await _writer.WriteRawContent($"{x + rx:F2} {y:F2} m\r\n");
        await _writer.WriteRawContent($"{x + w - rx:F2} {y:F2} l\r\n");

        // Bottom right curve
        await _writer.WriteRawContent($"{x + w - rx + cx:F2} {y:F2} {x + w:F2} {y + ry - cy:F2} {x + w:F2} {y + ry:F2} c\r\n");

        // Right hand edge
        await _writer.WriteRawContent($"{x + w:F2} {y + h - ry:F2} l\r\n");

        // Top right curve
        await _writer.WriteRawContent($"{x + w:F2} {y + h - ry + cy:F2} {x + w - rx + cx:F2} {y + h:F2} {x + w - rx:F2} {y + h:F2} c\r\n");

        // Top edge
        await _writer.WriteRawContent($"{x + rx:F2} {y + h:F2} l\r\n");

        // Top left curve
        await _writer.WriteRawContent($"{x + rx - cx:F2} {y + h:F2} {x:F2} {y + h - ry + cy:F2} {x:F2} {y + h - ry:F2} c\r\n");

        // Left hand edge
        await _writer.WriteRawContent($"{x:F2} {y + ry:F2} l\r\n");

        // Bottom left curve
        await _writer.WriteRawContent($"{x:F2} {y + ry - cy:F2} {x + rx - cx:F2} {y:F2} {x + rx:F2} {y:F2} c\r\n");

        // Close the path.
        await _writer.WriteRawContent($"h\r\n");
    }

    /// <summary>
    /// Writes a graphics operation to the output stream. This method
    /// outputs the operands and operator.
    /// </summary>
    /// <param name="op">The operation.</param>
    private async Task WriteStandardGraphicsOperation(GraphicsOperation op)
    {
        // Write the operands. If needed we resolve variables.
        foreach (var v in op.Operands)
        {
            await _writer.WriteValue(ResolveOperand(v));
            await _writer.WriteRawContent(" ");
        }

        await _writer.WriteRawContent($"{op.GetOperatorName()}\r\n");
    }

    private async Task WriteTfOperation(GraphicsOperation op)
    {
        // We resolve the font name and size.
        var fontName = ResolveOperand(op.Operands[0]).GetString();
        var fontSize = ResolveOperand(op.Operands[1]).GetNumber();

        // If the font name is a standard font, then we add it to the page
        // and proceed. Otherwise, we resolve the font resource and potentially
        // embed it in the document.
        var font = await ResolveFont(fontName);

        // Add the font to the current page.
        _writer.AddResourceToPage(font);

        // Write the font and size.
        await _writer.WriteRawContent($"/{font.Identifier} {fontSize} Tf\r\n");

        // Update the graphics state.
        _graphicsState.FontSize = fontSize;
        _graphicsState.Font = font;
    }

    /// <summary>
    /// Writes a Tfl operation. This method will perform a text-flow operation
    /// and then output text lines to the PDF document writer. As a side-effect,
    /// this method will also set the system variables for text width, text height,
    /// and number of text lines written. 
    /// /// </summary>
    /// <param name="op">The graphics operation.</param>
    private async Task WriteTflOperation(GraphicsOperation op)
    {
        if (null == _graphicsState.Font) throw new PdfsProcessorException("Set a font first before placing text.");
        if (_graphicsState.FontSize <= 0) throw new PdfsProcessorException("Set a font size first before placing text.");

        var text = ResolveOperand(op.Operands[0]);

        // If the text block constraint has auto width, we won't flow text
        // but simply write a single line.
        if (float.IsNaN(_textBoxConstraint.Width))
        {
            await _writer.WriteValue(text);
            await _writer.WriteRawContent(" Tj\r\n");
            return;

            // TODO: measure width, set height to font size, return 1 line
            // TODO: vertically center text
        }

        //TODO: use NaN for height of Rect and deal with this in Engine
        // TODO: Measure widest line, measure height, return number of lines.

        // With a set width, we need to flow the text.
        // If the text alignment is justified, then we need to force the
        // word spacing to zero in the output if it's not currently zero.
        var needsForcedWordSpacing = _graphicsState.WordSpacing != 0 && HorizontalTextAlignment.FullyJustified == (_graphicsState.HorizontalTextAlignment & HorizontalTextAlignment.FullyJustified);
        if (needsForcedWordSpacing) { await _writer.WriteRawContent($"q 0 Tw\r\n"); }

        var rect = new PdfRectangle(0, 0, _textBoxConstraint.Width, _textBoxConstraint.Height);
        var span = new Span(text.GetString(), _graphicsState.Font, _graphicsState.FontSize);
        var engine = new TextFlowEngine(
            new TextAlignmentOptions(
                _graphicsState.HorizontalTextAlignment,
                _graphicsState.VerticalTextAlignment),
            _graphicsState.Leading,
            _graphicsState.WordSpacing,
            _graphicsState.CharacterSpacing,
            _graphicsState.HorizontalScaling);
        var lines = engine.FlowText([span], rect);

        if (lines.Any()) { await _writer.WriteLines(lines); }

        // If we had to force 0 word spacing, we restore the state now.
        if (needsForcedWordSpacing)
        {
            await _writer.WriteRawContent($"Q\r\n");
        }
    }
    #endregion



    // Private types
    // =============
    #region Private types
    /// <summary>
    /// The GraphicsState class represents the state of the graphics object.
    /// This is used primarily to store the state of the text object, before
    /// drawing text lines, as we need to be able to feed parameters such as
    /// the character spacing, into the text flow engine.
    /// </summary>
    private class GraphicsState
    {
        // Public properties
        // =================
        #region Public properties
        /// <summary>
        /// The text leading.
        /// </summary>
        public float Leading { get; set; }

        /// <summary>
        /// The character spacing.
        /// </summary>
        public float CharacterSpacing { get; set; }

        /// <summary>
        /// The word spacing.
        /// </summary>
        public float WordSpacing { get; set; }

        /// <summary>
        /// The horizontal scaling for text.
        /// </summary>
        public float HorizontalScaling { get; set; } = 1;

        /// <summary>
        /// The text rise.
        /// </summary>
        public float Rise { get; set; }

        /// <summary>
        /// The font size.
        /// </summary>
        public float FontSize { get; set; }

        /// <summary>
        /// The current font.
        /// </summary>
        public Font? Font { get; set; }

        /// <summary>
        /// The horizontal text alignment.
        /// </summary>
        public HorizontalTextAlignment HorizontalTextAlignment { get; set; } = HorizontalTextAlignment.Left;

        /// <summary>
        /// The vertical text alignment.
        /// </summary>
        public VerticalTextAlignment VerticalTextAlignment { get; set; } = VerticalTextAlignment.Top;
        #endregion



        // Public methods
        // ==============
        #region Public methods
        /// <summary>
        /// Clones this graphics state.
        /// </summary>
        /// <returns>The cloned instance.</returns>
        public GraphicsState Clone()
        {
            return new GraphicsState
            {
                Leading = Leading,
                CharacterSpacing = CharacterSpacing,
                WordSpacing = WordSpacing,
                HorizontalScaling = HorizontalScaling,
                Rise = Rise,
                FontSize = FontSize,
                Font = Font,
                HorizontalTextAlignment = HorizontalTextAlignment,
                VerticalTextAlignment = VerticalTextAlignment
            };
        }
        #endregion
    }

    /// <summary>
    /// The PdfsProcessorState enumeration represents the state of the processor.
    /// This is used to determine how to process statements from the reader.
    /// </summary>
    private enum PdfsProcessorState
    {
        Initial,
        BeforePage,
        OnPage
    }
    #endregion
}