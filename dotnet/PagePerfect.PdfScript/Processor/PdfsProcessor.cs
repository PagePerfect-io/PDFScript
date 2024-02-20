using System.Runtime.CompilerServices;
using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Reader.Statements;
using PagePerfect.PdfScript.Reader.Statements.Prolog;
using PagePerfect.PdfScript.Writer;

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
    private PdfsReader _reader = new(source);
    private IPdfDocumentWriter _writer = writer;
    private PdfsProcessorState _state = PdfsProcessorState.Initial;
    private Dictionary<string, ResourceDeclaration> _resourceDeclarations = [];
    private Dictionary<string, PdfsVariable> _variables = [];
    private Dictionary<string, PdfResourceReference> _localResources = [];
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
    private PdfResourceReference EnsureLocalImage(string location)
    {
        if (_localResources.TryGetValue(location, out var resource)) return resource;

        // Download the image locally.


        // Add the image to the document.
        var image = new PdfImage(location);
        _writer.AddImage(image);
        _localResources[location] = image;
        return image;
    }

    /// <summary>
    /// Opens a new page in the document.
    /// </summary>
    private async Task OpenNewPage()
    {
        await _writer.OpenPage(595, 841, DisplayOrientation.Regular);
        await _writer.OpenContentStream();

    }

    private void ProcessPrologStatement(PrologStatement prolog)
    {
        switch (prolog.PrologType)
        {
            case PrologStatementType.VarDeclaration:
                TryAddVariableDeclaration((prolog as VarDeclaration)!);
                break;

            case PrologStatementType.ResourceDeclaration:
                //TryAddResource((prolog as ResourceDeclaration)!);
                break;
        }
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
        var stored = EnsureLocalImage(location);

        // Add the image to the current page.
        _writer.AddResourceToPage(stored.ObjectReference);

        // Write the Do operation.
        await _writer.WriteRawContent($"{stored.Identifier} Do\r\n");
    }

    /// <summary>
    /// Writes a graphics operation to the output stream. This method detects
    /// special cases, such as Do and Tj, and dispatches appropriately, before
    /// defaulting to a standard output of the operands and operator.
    /// </summary>
    /// <param name="op">The operation.</param>
    private async Task WriteGraphicsOperation(GraphicsOperation op)
    {
        switch (op.Operator)
        {
            case Operator.Do:
                await WriteDoOperation(op);
                break;

            //            case Operator.Tj:
            //              await WriteTjOperation(op);
            //            break;

            default:
                await WriteStandardGraphicsOperation(op);
                break;
        }
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
    #endregion



    // Private types
    // =============
    #region Private types
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