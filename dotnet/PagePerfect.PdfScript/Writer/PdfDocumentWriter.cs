using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Writer.Resources.Fonts;
using PagePerfect.PdfScript.Writer.Resources.Images;

namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The PdfDocumentWriter class is used to write a PDF document to a stream.
/// It is used by the PdfsProcessor class to output a PDF document based on a 
/// .pdfs script file.
/// </summary>
public class PdfDocumentWriter : IPdfDocumentWriter
{
    // Private fields
    // ==============
    #region Private fields
    private WriterState _state;
    private readonly CrossReferenceTable _xref;
    private readonly PdfObjectReferenceManager _objects;
    private readonly List<PdfObjectReference> _pages;
    private readonly Stream _stream;
    private readonly StreamWriter _writer;
    private PdfObjectReference? _pagesReference;
    private PdfObjectReference? _catalogReference;
    private PageState? _currentPage;
    //private readonly Dictionary<PdfObjectReference, Func<CustomResource, PdfDocumentWriter, Task>> _customResourceCallbacks;
    private readonly List<PdfResourceReference> _documentResources;
    #endregion



    // Initialisers
    // ============
    #region Initialisers
    /// <summary>
    /// Initialises a new instance of the PdfDocumentWriter class
    /// using the specified stream to write to.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    public PdfDocumentWriter(Stream stream)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _writer = new(_stream);
        _state = WriterState.None;
        _xref = new();
        _objects = new();
        _pages = [];
        _documentResources = [];
        //_customResourceCallbacks = [];
    }

    /// <summary>
    /// Initialises a new instance of the PdfDocumentWriter class
    /// using a file path.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    public PdfDocumentWriter(string path) : this(new FileStream(path, FileMode.Create)) { }
    #endregion



    // Interface implementation
    // ========================
    #region IPdfDocumentWriter implementation
    /// <summary>
    /// Indicates if the document is currently open.
    /// </summary>
    public bool IsOpen { get => _state != WriterState.None && _state != WriterState.Closed; }

    /// <summary>
    /// Adds the specified resource to the page's resources.
    /// This operation is only valid if a page is currently open.
    /// Calling this method twice with the same resource reference on the same page is safe; the resource will only be added once.
    /// </summary>
    /// <exception cref="ArgumentNullException">The specified resource is a Null reference.</exception>
    /// <exception cref="PdfDocumentWriterException">The specified resource does not exist in the document,
    /// so it cannot be added as a page resource. Either the resource was not created through one of the
    /// resource creation methods (such as CreateFont), or it was created in a different Writer instance.
    /// </exception>
    /// <exception cref="PdfDocumentWriterException">A page has not been opened. This operation is only valid when a page has been
    /// opened as it applies to the current page.</exception>
    /// <param name="resource">The resource to add</param>
    public void AddResourceToPage(PdfResourceReference resource)
    {
        if (WriterState.Page != _state && WriterState.Content != _state)
            throw new PdfDocumentWriterException("Invalid state. A page must be opened before adding resources.");

        if (!_documentResources.Contains(resource))
            throw new PdfDocumentWriterException("The resource does not exist in this document");

        if (!_currentPage!.Resources.Contains(resource))
            _currentPage.Resources.Add(resource);
    }

    /// <summary>
    /// Finalises the document. This method wraps up any pending
    /// operations, closes the current page if applicable, and any
    /// other state, and closes the file stream. This instance can
    /// no longer be used after calling this method.
    /// </summary>
    public async Task Close()
    {
        if (_state == WriterState.None)
            throw new PdfDocumentWriterException("Invalid state. The document is not open.");
        if (_state == WriterState.Closed)
            throw new PdfDocumentWriterException("Invalid state. The document is already closed.");

        await TryClosePage();

        await WriteDocumentResources();
        await WritePages();
        var xrefPointer = await WriteCrossReferenceTable();
        await WriteTrailer();
        await WriteFooter(xrefPointer);

        _writer.Flush();
        _stream.Flush();

        _state = WriterState.Closed;
    }

    /// <summary>
    /// Closes the content stream, if one is open. It is not an error to attempt to
    /// close a stream if one isn't open - the call will be ignored instead.
    /// </summary>
    public async Task CloseContentStream()
    {
        if (_state != WriterState.Content)
            return;

        // Remember the length of the stream content.
        _writer.Flush();
        long streamLength = _stream.Position - _currentPage!.StreamMarker;

        // This ends the stream and the contents.
        await _writer.WriteLineAsync("endstream");
        await _writer.WriteLineAsync("endobj");

        // Write the object that contains the stream length.
        _writer.Flush();
        _xref.Add(_stream.Position + 2, _currentPage.ContentStreamLengthReference!);

        await _writer.WriteLineAsync(
            $"\r\n{_currentPage.ContentStreamLengthReference!.ToString(PdfObjectNotation.Declaration)} {streamLength} endobj");

        _state = WriterState.Page;
    }

    /// <summary>
    /// Finalises the document. This method wraps up any pending
    /// operations, closes the current page if applicable, and any
    /// other state, and closes the file stream. This instance can
    /// no longer be used after calling this method.
    /// This method will do nothing if the document isn't yet open,
    /// or was already closed. To throw an exception if the document
    /// is in an invalid state, use the Close() method instead.
    /// /// </summary>
    public async Task CloseIfNeeded()
    {
        if (_state == WriterState.None || _state == WriterState.Closed) return;

        await Close();
    }

    /// <summary>
    /// Closes the currently opened object. This call is only valid in the Free or Page state.
    /// </summary>
    public async Task CloseObject()
    {
        if (_state != WriterState.Page && _state != WriterState.Free)
            throw new PdfDocumentWriterException("Invalid state. The document must be open, not closed, and not be in the Content state.");

        await _writer.WriteLineAsync(">>");
        await _writer.WriteLineAsync("endobj");
    }

    /// <summary>
    /// Closes the page. This method closes a currently open page, if one is open.
    /// If no page is open, this methid throws an error.
    /// Closing a page also closes any other state, such as a Text Frame state.
    /// </summary>
    public async Task ClosePage()
    {
        if (_state != WriterState.Page && _state != WriterState.Content)
            throw new PdfDocumentWriterException("Invalid state. No page is currently open.");

        await TryClosePage();
    }


    /// <summary>
    /// Creates a new Image resource and returns a reference that identifies the image.
    /// </summary>
    /// <param name="filename">The image filename</param>
    /// <param name="tag">An optional tag that can be used to identify the image.</param>
    /// <returns>Reference to the newly created image.</returns>
    /// <exception cref="ArgumentNullException">The filename argument is null</exception>
    public Image CreateImage(string filename, object? tag = null)
    {
        var imageRef = CreateObjectReference();
        var image = new Image(imageRef, CreateResourceName("I"), filename, tag);

        _documentResources.Add(image);

        return image;
    }

    /// <summary>
    /// Creates a new object reference that is valid within this document.
    /// The object reference must be used to write an object to the PDF stream at some point,
    /// otherwise an exception is thrown when the document is finalised.
    /// </summary>
    /// <returns>The new object reference.</returns>
    public PdfObjectReference CreateObjectReference()
    {
        return _objects.Next();
    }

    /// <summary>
    /// Creates a new Font resource and returns a reference that identifies the font.
    /// This method will create a font based on one of the 14 standard PDF fonts.
    /// If the type name is not one of the standard fonts, this method will throw
    /// an exception.
    /// </summary>
    /// <param name="typename">The font's typename</param>
    /// <returns>Reference to the newly created font.</returns>
    /// <exception cref="ArgumentNullException">The 'typename' argument is null</exception>
    public Font CreateStandardFont(string typename, object? tag = null)
    {
        if (!FontUtilities.IsStandardFont(typename)) throw
            new ArgumentException("The font type name must be one of the standard PDF fonts.", nameof(typename));

        var fontRef = CreateObjectReference();
        var font = new StandardFont(fontRef, CreateResourceName("F"), typename.TrimStart('/'), tag);

        _documentResources.Add(font);

        return font;
    }

    /// <summary>
    /// Flushes the PDF stream.
    /// </summary>
    public void Flush()
    {
        _writer.Flush();
    }

    /// <summary>
    /// Opens the next content stream. This method is an alternative to OpenContentStream(),
    /// and is used to support multip[le content streams in one page. It is not permitted to
    /// combine OpenContentStream() and NextContentStream().
    /// </summary>
    public async Task<PdfObjectReference> NextContentStream()
    {
        // If we have a page context we can open a stream, otherwise we throw an exception.
        // We also can't open a content stream if one already exists.
        if (WriterState.Page != _state)
            throw new PdfDocumentWriterException("Invalid state - cannot open a content stream in the current state.");
        if (HasSingleContent())
            throw new PdfDocumentWriterException("Invalid state - cannot open multiple content streams if a single one already exists.");

        // Open a brand new contents object.
        var nextContentRef = CreateObjectReference();
        await OpenObject(nextContentRef);
        var length = CreateObjectReference();
        await _writer.WriteLineAsync($"\t/Length {length.ToString(PdfObjectNotation.Reference)}");

        await _writer.WriteLineAsync(">>");
        await _writer.WriteLineAsync("stream");
        _writer.Flush();

        _currentPage!.NextContent(length, _stream.Position, nextContentRef);

        // We are now in the Content stream
        _state = WriterState.Content;

        return nextContentRef;
    }

    /// <summary>
    /// Opens the document. This means the document is ready to accept
    /// content and resources.
    /// </summary>
    public async Task Open()
    {
        if (_state != WriterState.None)
            throw new PdfDocumentWriterException("Invalid state. The document is already open.");

        await WriteHeader();

        // Create the catalog, pages and outlines objects.
        _pagesReference = _objects.Next();
        var outlinesRef = await WriteNewObject("<< /Type /Outlines /Count 0 >>");
        _catalogReference = await WriteNewObject($"<< /Type /Catalog /Pages {_pagesReference} /Outlines {outlinesRef} >>");

        _state = WriterState.Free;
    }

    /// <summary>
    /// Opens a new content stream. This operation is only valid in the Page state. It is an error
    /// to attempt to open a content stream in the Free or Content mode. It is also an error to
    /// attempt to open a content stream if a page has already had content written.
    /// </summary>
    public async Task<PdfObjectReference> OpenContentStream()
    {
        // If we have a page context we can open a stream, otherwise we throw an exception.
        // We also can't open a content stream if one already exists.
        if (WriterState.Page != _state)
            throw new PdfDocumentWriterException("Invalid state - cannot open a content stream in the current state.");
        if (HasContent())
            throw new PdfDocumentWriterException("Invalid state - cannot open a content stream if one already exists.");

        // Open a new contents object.
        await OpenObject(_currentPage!.ContentsReference);
        var length = CreateObjectReference();
        await _writer.WriteLineAsync($"\t/Length {length.ToString(PdfObjectNotation.Reference)}");

        await _writer.WriteLineAsync(">>");
        await _writer.WriteLineAsync("stream");
        _writer.Flush();

        _currentPage.StartContent(length, _stream.Position);

        // We are now in the Content stream
        _state = WriterState.Content;

        return _currentPage.ContentsReference;

    } // OpenContentStream()

    /// <summary>
    /// Opens a new object and returns its object reference. This operation only works in the Free or Page state.
    /// This method records the object reference in the cross reference table.
    /// </summary>
    /// <param name="type">Optionally, the type of the object to add as a dictionary entry.</param>
    /// <returns>The object reference</returns>
    /// <exception cref="PdfDocumentWriterException">This operation is not valid in the current state.</exception>
    public async Task<PdfObjectReference> OpenObject(string? type = null)
    {
        if (_state != WriterState.Page && _state != WriterState.Free)
            throw new PdfDocumentWriterException("Invalid state. The document must be open, not closed, and not be in the Content state.");

        PdfObjectReference newRef = CreateObjectReference();
        await OpenObject(newRef, type);

        return newRef;
    }

    /// <summary>
    /// Opens a new object with the specified object reference as its identifier.
    /// This operation only works in the Free or Page states.
    /// This method records the object reference in the cross reference table.
    /// </summary>
    /// <param name="reference">The object's reference</param>
    /// <param name="type">Optionally, the type of the object to add as a dictionary entry.</param>
    /// <exception cref="PdfDocumentWriterException">This operation is not valid in the current state.</exception>
    public async Task OpenObject(PdfObjectReference reference, string? type = null)
    {
        if (_state != WriterState.Page && _state != WriterState.Free)
            throw new PdfDocumentWriterException("Invalid state. The document must be open, not closed, and not be in the Content state.");

        _writer.Flush();

        // Store this object in the cross-reference.
        _xref.Add(_stream.Position + 2, reference);

        // Open the object.
        await _writer.WriteLineAsync($"\r\n{reference.ToString(PdfObjectNotation.Declaration)}");
        await _writer.WriteLineAsync("<<");
        if (null != type) await _writer.WriteLineAsync($"\t/Type\t/{type}");

    }

    /// <summary>
    /// Opens a new page in the PDF document. If a page is currently open, this method will close
    /// that page, and close any other state (such as a Text Frame state).
    /// </summary>
    /// <param name="width">The width of the page, in points.</param>
    /// <param name="height">The height of the page, in points.</param>
    /// <param name="display">The display orientation of the page. Defaults to a regular orientation.</param>
    public async Task OpenPage(double width, double height, DisplayOrientation display)
    {
        if (_state == WriterState.None)
            throw new PdfDocumentWriterException("Invalid state. The document is not open.");
        if (_state == WriterState.Closed)
            throw new PdfDocumentWriterException("Invalid state. The document is closed.");

        await TryClosePage();

        await AddPage(new PdfRectangle(0, 0, width, height), display);
    }

    /// <summary>
    /// Outputs the contents of the specified buffer to the current contents stream.
    /// This operation is valid in the Free, Page and Content state. The buffer is written
    /// into the output stream as-is, so it is the responsibility of the caller to ensure
    /// that the buffer is valid PDF content.
    /// </summary>
    /// <param name="buffer">The buffer</param>
    /// <param name="start">The index to start writing from.</param>
    /// <param name="length">The number of bytes to write.</param>
    public async Task WriteBuffer(byte[] buffer, int start, int length)
    {
        if (_state == WriterState.None || _state == WriterState.Closed)
            throw new PdfDocumentWriterException("Invalid state. The document must be open, and not yet closed.");

        _writer.Flush();
        await _stream.WriteAsync(buffer, start, length);
    }

    /// <summary>
    /// Outputs the specified string directly to the current contents stream. No checking is done on this
    /// string so if it doesn't match the PDF specification, the document will not load.
    /// This method can be called regardless of the current state of the Writer.
    /// </summary>
    /// <param name="content">The content</param>
    public async Task WriteRawContent(string content)
    {
        await _writer.WriteAsync(content);
    }

    /// <summary>
    /// Writes the specified PdfsValue to the current content stream.
    /// This is used by the processor to write graphics instructions to the output stream.
    /// </summary>
    /// <param name="value">The value.</param>
    public async Task WriteValue(PdfsValue value)
    {
        switch (value.Kind)
        {
            case PdfsValueKind.Boolean:
            case PdfsValueKind.Name:
            case PdfsValueKind.Number:
            case PdfsValueKind.Keyword:
                await _writer.WriteAsync(value.ToString());
                break;

            case PdfsValueKind.String:
                await _writer.WriteAsync($"({value.GetString()})");
                break;

            case PdfsValueKind.Array:
                await _writer.WriteAsync("[");
                foreach (var v in value.GetArray())
                {
                    await WriteValue(v);
                    await _writer.WriteAsync(" ");
                }
                await _writer.WriteAsync("]");
                break;

            case PdfsValueKind.Dictionary:
                await _writer.WriteAsync("<<");
                foreach (var kv in value.GetDictionary())
                {
                    await _writer.WriteAsync($"/{kv.Key} ");
                    await WriteValue(kv.Value);
                    await _writer.WriteAsync(" ");
                }
                await _writer.WriteAsync(">>");
                break;

            default:
                throw new PdfDocumentWriterException($"Invalid value kind: {value.Kind}");
        }
    }
    #endregion


    // Public methods
    // ==============
    #region Public methods
    /*

    

    
    /// <summary>
    /// Creates a custom resource in the document. This resource is based on a supplied object reference and allows the object to be
    /// used as a resource in pages.
    /// This object needs to exist in the document and it is the caller's responsibility to do so. This object can be added to the document
    /// at any stage, so it is not required for this object to actually exist in the document when this method is called.
    /// A new name will be created for this resource, and a resource reference returned. This resource reference can
    /// be used in any subsequent content stream of this document.
    /// When the PDF document is finalised, the WriteCustomResource event is raised so that clients can insert objects if they didn't
    /// do this before. Failure to include all objects in the PDF document will result in an exception at the final stage of
    /// document finalisation.
    /// </summary>
    /// <param name="objectRef">The object to add as a resource.</param>
    /// <param name="originalType">The original type of the resource.</param>
    /// <returns>the resource reference</returns>
    /// <exception cref="ArgumentException">The 'objectRef' argument must be a non-empty object.</exception>
    /// <exception cref="ArgumentNullException">The resourceType cannot be null.</exception>
    public PdfResourceReference CreateCustomResource(
        PdfObjectReference objectRef,
        string originalType,
        object? resourceObject,
        Func<CustomResource, PdfDocumentWriter, Task>? writeCallback = null)
    {
        if (objectRef.IsEmpty()) throw new ArgumentException("objectRef must be non-empty", nameof(objectRef));
        if (string.IsNullOrEmpty(originalType)) throw new ArgumentException("The original type cannot be null or empty.", nameof(originalType));

        var resourceRef = new CustomResource(
            objectRef,
            CreateResourceName(char.ToUpper(originalType[0]).ToString()),
            originalType,
            resourceObject);

        if (null != writeCallback)
            _customResourceCallbacks.Add(resourceRef.ObjectReference, writeCallback);

        _documentResources.Add(resourceRef);

        return resourceRef;
    }



    /// <summary>
    /// Creates a new Form resource and returns a reference that identifies the form XObject.
    /// This form can be used to draw reusable content, by adding it to a page via a Do operation.
    /// Alternatively, use the PlaceForm method on a DrawingCanvas instance. 
    /// /// </summary>
    /// <param name="bbox">The bounding box of the form.</param>
    /// <returns>Reference to the newly created form.</returns>
    public Form CreateForm(PdfRectangle bbox)
    {
        var formRef = CreateObjectReference();
        var form = new Form(this, formRef, CreateResourceName("Fm"), bbox);
        _documentResources.Add(form);

        return form;
    }

    /// <summary>
    /// Creates a new TrueType Font resource and returns a reference that identifies the font.
    /// </summary>
    /// <param name="path">The path to the font's program.</param>
    /// <returns>Reference to the newly created font.</returns>
    public Font CreateTrueTypeFont(string path)
    {
        if (null == path) throw new ArgumentNullException(nameof(path));

        var fontRef = CreateObjectReference();
        var font = TrueTypeFont.Parse(fontRef, CreateResourceName("F"), path);

        _documentResources.Add(font);

        return font;
    }

    /// <summary>
    /// Opens a drawing canvas on the current page. This method is only valid if a page is currently open,
    /// and no content stream has been directly opened yet.
    /// </summary>
    /// <returns>The drawing canvas.</returns>
    public async Task<DrawingCanvas> OpenCanvas()
    {
        if (_state != WriterState.Page)
            throw new PdfDocumentWriterException("Invalid state. A page must be opened before drawing on it.");
        if (HasContent())
            throw new PdfDocumentWriterException("Invalid state - cannot open a content stream if one already exists.");

        // Open a new contents object.
        await OpenObject(_currentPage!.ContentsReference);
        var length = CreateObjectReference();
        await _writer.WriteLineAsync($"\t/Length {length.ToString(PdfObjectNotation.Reference)}");

        await _writer.WriteLineAsync(">>");
        await _writer.WriteLineAsync("stream");
        _writer.Flush();

        _currentPage.StartContent(length, _stream.Position);

        // We are now in the Content stream
        _state = WriterState.Content;

        return _currentPage!.CreateCanvas(this);
    }
    */
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Adds a new page to the document.
    /// </summary>
    /// <param name="mediabox">The mediabox of the page.</param>
    /// <param name="display">The display orientation of the page.</param>
    /// <param name="closeObject">If true, the object will be closed after it is written.</param>
    private async Task AddPage(PdfRectangle mediabox, DisplayOrientation display, bool closeObject = true)
    {
        var pageRef = await OpenObject("Page");
        var resourcesRef = _objects.Next();
        var contentsRef = _objects.Next();

        // Write the page properties and close the object.
        await _writer.WriteLineAsync($"\t/Parent\t{_pagesReference!.ToString(PdfObjectNotation.Reference)}");
        await _writer.WriteLineAsync($"\t/MediaBox\t[{mediabox.Left} {mediabox.Bottom} {mediabox.Width} {mediabox.Height}]");
        await _writer.WriteLineAsync($"\t/Resources\t{resourcesRef.ToString(PdfObjectNotation.Reference)}");
        await _writer.WriteLineAsync($"\t/Contents\t{contentsRef.ToString(PdfObjectNotation.Reference)}");

        switch (display)
        {

            case DisplayOrientation.RotateClockwise:
                await _writer.WriteLineAsync($"\t/Rotate\t-90");
                break;

            case DisplayOrientation.RotateCounterClockwise:

                await _writer.WriteLineAsync($"\t/Rotate\t90");
                break;
            case DisplayOrientation.Rotate180:

                await _writer.WriteLineAsync($"\t/Rotate\t180");
                break;
        }

        if (closeObject) await CloseObject();

        _pages.Add(pageRef);
        _currentPage = new PageState(pageRef, resourcesRef, contentsRef, mediabox);
        _state = WriterState.Page;

    }

    /// <summary>
    /// Creates a name for a resource. This method creates a unique name for a new
    /// resource. The name is based on the prefix, the number of resources currently
    /// in the document, and a random postfix.
    /// </summary>
    /// <param name="prefix">The prefix to use.</param>
    private string CreateResourceName(string prefix)
    {
        var ms = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        var postfix = ms.ToString("X");
        return $"{prefix}{_documentResources.Count + 1}_{postfix}";
    }

    /// <summary>
    /// Retrieves the dictionary key for a resource. For a standard resource,
    /// this is a fixed name such as /Font or /XObject. For a custom resource,
    /// it is the custom resource's original type. 
    /// /// </summary>
    /// <param name="resource">The resource.</param>
    /// <returns>The dictionary name.</returns>
    private static string GetDictionaryName(PdfResourceReference resource)
    {
        /*
        if (PdfResourceType.Custom == resource.Type)
        {
            if (false == resource is CustomResource custom) throw new PdfDocumentWriterException("Custon resource must derive from CustomResource type.");

            return custom.OriginalType;
        }*/

        return PdfResourceReference.GetDictionaryName(resource.Type);
    }

    /// <summary>
    /// Determines if the current page already has a single content object,
    /// or multiple content objects, written to it.
    /// </summary>
    /// <returns>True if the content reference object has already been written
    /// or there is a non-empty array of content streams.</returns>
    private bool HasContent() => null != _currentPage!.ContentArray || HasSingleContent();

    /// <summary>
    /// Determines whether the current page has a single content object, already written.
    /// This would prevent writing single content again, or writing multiple content streams,
    /// to the current page.
    /// </summary>
    /// <returns>True if the content reference object has already been written.</returns>
    private bool HasSingleContent() => _xref.Exists(_currentPage!.ContentsReference);

    /// <summary>
    /// Tries to close the current page. This method returns a boolean that 
    /// indicates if a page was open, and could be closed. If no page is currently
    /// open, this method returns 'false'. Otherwise, the page is closed and this
    /// method returns 'true'.
    /// </summary>
    /// <returns>A boolean that indicates if a page could be closed.</returns>
    private async Task<bool> TryClosePage()
    {
        // If a page or content stream isn't open, we can't close the page.
        if (_state != WriterState.Page && _state != WriterState.Content) return false;

        switch (_state)
        {
            case WriterState.Content:
                await CloseContentStream();
                break;
            case WriterState.Page:
                if (!HasContent())
                {
                    await OpenContentStream();
                    await CloseContentStream();
                }
                break;
        }

        // If we had multiple content streams, we need to write the content object - which will
        // be an array of references to the content streams.
        if (null != _currentPage!.ContentArray)
        {
            await WriteObject(_currentPage.ContentsReference, $"[{string.Join(" ", _currentPage.ContentArray)}]");
        }

        // Write the page's resources.
        await WritePageResources();

        _state = WriterState.Free;

        return true;
    }

    /// <summary>
    /// Writes the cross reference table. This method asks the cross
    /// reference table to write itself to the stream, and returns
    /// the position of the cross reference table.
    /// </summary>
    /// <returns>The position of the cross reference table</returns>
    private async Task<long> WriteCrossReferenceTable()
    {
        _writer.Flush();
        var position = _stream.Position;

        await _xref.Write(_writer);

        return position;
    }

    /// <summary>
    /// Writes the document resources to the PDF stream.
    /// </summary>
    private async Task WriteDocumentResources()
    {
        var fonts = new List<Font>();
        var images = new List<Image>();
        //var customResources = new List<CustomResource>();
        //var forms = new List<Form>();

        // Copy fonts and images into their respective lists.
        // Custom resources require outside intervention.
        foreach (var resource in _documentResources)
        {
            switch (resource.Type)
            {
                case PdfResourceType.Font:
                    fonts.Add((Font)resource);
                    break;
                case PdfResourceType.Image:
                    images.Add((Image)resource);
                    break;
                case PdfResourceType.Form:
                    //forms.Add((Form)resource);
                    break;
                case PdfResourceType.Custom:
                    //customResources.Add((CustomResource)resource);
                    break;
            }
        }

        // if (customResources.Any()) await WriteCustomResources(customResources);
        if (fonts.Any()) await WriteFonts(fonts);
        if (images.Count != 0) await WriteImages(images);
        // if (forms.Any()) await WriteForms(forms);    
    }

    /// <summary>
    /// Writes the font definitions for each of the fonts used in the document.
    /// </summary>
    /// <param name="fonts">The fonts to write.</param>
    private async Task WriteFonts(IEnumerable<Font> fonts)
    {
        // We go through the fonts in the list and determine if they are standard fonts or
        // trueType ones. TrueType fonts can be references or embedded fonts.
        foreach (Font font in fonts)
        {
            if (FontUtilities.IsStandardFont(font.Typename))
                await WriteStandardFont(font);
            else
            {
                throw new NotImplementedException("TrueType fonts are not yet supported.");
            }
        }
    }

    /// <summary>
    /// Writes the footer of the PDF document. This method writes the footer
    /// object, which is a summary  of key data points that PDF readers
    /// user to construct the document representation.
    /// </summary>
    /// <param name="xrefPointer">The position of the xref table in the output stream.</param>
    private async Task WriteFooter(long xrefPointer)
    {
        await _writer.WriteLineAsync($"\r\nstartxref\r\n{xrefPointer}\r\n%%EOF");
    }

    /// <summary>
    /// Writes the PDF header.
    /// </summary>
    private async Task WriteHeader()
    {
        await _writer.WriteLineAsync("%PDF-1.7");
    }

    /// <summary>
    /// Writes an image resource. Currently this method supports writing a JPEG image,
    /// and no other formats. 
    /// </summary>
    /// <param name="image">The image resource.</param>
    /// <exception cref="NotSupportedException">The image type is not supported.</exception>
    /// <exception cref="PdfDocumentWriterException">There was an error writing the image.</exception>
    private async Task WriteImage(Image image)
    {
        var imageType = ImageUtilities.GetImageType(image.Filename);
        if (ImageType.Jpeg != imageType) throw
            new NotSupportedException("Only JPEG images are supported in this version of the library.");

        var file = new FileInfo(image.Filename);

        var info = JpegUtilities.Parse(image.Filename);

        await OpenObject(image.ObjectReference, "XObject");
        await _writer.WriteLineAsync($"\t/Subtype\t/Image");
        await _writer.WriteLineAsync($"\t/Width\t{info.Width}");
        await _writer.WriteLineAsync($"\t/Height\t{info.Height}");
        await _writer.WriteLineAsync($"\t/Length\t{file.Length}");

        await _writer.WriteLineAsync($"\t/ColorSpace\t/{info.ColourSpace}");

        if (ColourSpace.DeviceCMYK == info.ColourSpace && ImageType.Jpeg == imageType)
        { await _writer.WriteLineAsync($" /Decode\t[1 0 1 0 1 0 1 0]"); }

        await _writer.WriteLineAsync($"\t/BitsPerComponent\t8");
        await _writer.WriteLineAsync($"\t/Name\t/{image.Identifier}");
        await _writer.WriteLineAsync($"\t/Filter\t/DCTDecode");
        await _writer.WriteLineAsync(">>");
        await _writer.WriteLineAsync("stream");

        await WriteBuffer(File.ReadAllBytes(image.Filename), 0, (int)file.Length);

        await _writer.WriteLineAsync("\r\nendstream");
        await _writer.WriteLineAsync("endobj");
    }

    /// <summary>
    /// Writes the image definitions for each of the images used in the document.
    /// </summary>
    /// <param name="images">The images to write the definition of.</param>
    /// <exception cref="PdfDocumentWriterException">The image could not be found</exception>
    private async Task WriteImages(IEnumerable<Image> images)
    {
        // We go through each of the images in the list, and output them.
        foreach (var image in images)
        {
            if (false == File.Exists(image.Filename))
            {
                // The image couldn't be found so we throw an exception
                throw new PdfDocumentWriterException($"The image could not be found at path '{image.Filename}'.");
            }

            await _writer.WriteLineAsync();
            await WriteImage(image);
        } // for each image
    }

    /// <summary>
    /// Write a new object to the file, including the specified value,
    /// and stores its cross reference.
    /// This method returns the object reference so it can be used
    /// elsewhere. This method is used to quickly write some objects
    /// in the header/footer of the document.
    /// </summary>
    /// <param name="value">The string representation of the value of the object,</param>
    /// <returns>The object reference.</returns>
    private async Task<PdfObjectReference> WriteNewObject(string value)
    {
        var objRef = _objects.Next();

        _writer.Flush();
        var position = _stream.Position;

        await _writer.WriteLineAsync(objRef.ToString(PdfObjectNotation.Declaration));
        await _writer.WriteLineAsync(value);
        await _writer.WriteLineAsync("endobj");

        _xref.Add(position, objRef);

        return objRef;
    }

    /// <summary>
    /// Writes an object to the file stream. This method writes the object
    /// and stores its cross reference. It is used to quickly write some
    /// objects in the header and footer of the document.
    /// </summary>
    /// <param name="objRef">The object reference.</param>
    /// <param name="value">The value of the object.</param>
    /// <returns></returns>
    private async Task WriteObject(PdfObjectReference objRef, string value)
    {
        _writer.Flush();
        var position = _stream.Position;

        await _writer.WriteLineAsync(objRef.ToString(PdfObjectNotation.Declaration));
        await _writer.WriteLineAsync(value);
        await _writer.WriteLineAsync("endobj");

        _xref.Add(position, objRef);
    }

    /// <summary>
    /// Writes the resources used by the current page to the PDF stream.
    /// </summary>
    private async Task WritePageResources()
    {
        await WritePageOrFormResources(_currentPage!.ResourcesReference, _currentPage.Resources);
    }

    /// <summary>
    /// Writes the resources for a page or form. This method
    /// writes an object with the specified reference,
    /// and adds the required resource dictionaries.
    /// </summary>
    /// <param name="ref">The object reference for the resource dictionary.</param>
    /// <param name="resources">The resources to write.</param>
    private async Task WritePageOrFormResources(
        PdfObjectReference @ref,
        List<PdfResourceReference> resources)
    {
        // Open a new resources object.
        await OpenObject(@ref);

        await _writer.WriteLineAsync("\t/ProcSet\t[/PDF /Text /ImageC]");

        // We need to write each resource type separately so we need to maintain a dictionary of lists.
        var resourceDictionary = new Dictionary<string, List<PdfResourceReference>>();

        // Copy fonts, images etc. into their respective lists.
        foreach (var resource in resources)
        {
            // Find the resource type for this resource. If there is no list for this type yet, we add it.
            string resourceType = GetDictionaryName(resource);
            if (!resourceDictionary.ContainsKey(resourceType))
                resourceDictionary[resourceType] = [];

            // Add the resource to the appropriate list.
            resourceDictionary[resourceType].Add(resource);
        }

        // for each of the different resource types we create a dictionary entry with the resource references as its
        // value. The references are a dictionary in themselves, with the resource identifiers acting as keys.
        foreach (var pair in resourceDictionary)
        {
            if (pair.Value.Count > 0)
            {
                await _writer.WriteAsync($"\t/{pair.Key}\t<<");
                foreach (var resource in pair.Value)
                    await _writer.WriteAsync($"{resource}");
                await _writer.WriteLineAsync(">>");
            }
        }

        await CloseObject();
    }

    /// <summary>
    /// Writes the pages manifest to the output stream.
    /// </summary>
    private async Task WritePages()
    {
        await WriteObject(_pagesReference!, $"<< /Type /Pages /Count {_pages.Count} /Kids [ {string.Join(" ", _pages)} ] >>");
    }

    /// <summary>
    /// Writes a font definition for a standard font that is used in the document.
    /// </summary>
    /// <param name="font">The font to write.</param>
    private async Task WriteStandardFont(Font font)
    {
        await OpenObject(font.ObjectReference, "Font");
        await _writer.WriteLineAsync("\t/Subtype\t/Type1");
        await _writer.WriteLineAsync($"\t/BaseFont\t/{font.Typename.Replace(' ', '-')}");
        await _writer.WriteLineAsync($"\t/Name\t/{font.Identifier}");
        await _writer.WriteLineAsync("\t/Encoding\t/WinAnsiEncoding");
        await CloseObject();
    }

    /// <summary>
    /// /// Writes the trailer.
    /// </summary>
    private async Task WriteTrailer()
    {
        await _writer.WriteLineAsync("trailer");
        await _writer.WriteLineAsync($"<< /Size {this._xref.GetLength() + 1} /Root {this._catalogReference} >>");
    }

    /*
    /// <summary>
    /// Writes the custom resources to the PDF stream.
    /// This requires outside intervention because the Writer does not know how to write these
    /// resources.
    /// </summary>
    /// <param name="customResources">The custom resources</param>
    private async Task WriteCustomResources(IEnumerable<CustomResource> customResources)
    {
        await _writer.WriteLineAsync();

        foreach (var resource in customResources)
        {
            // Any resource without a callback needs to write itself and that's the user's
            // responsibility. For the ones with a callback, we invoke their callbacks.
            if (_customResourceCallbacks.TryGetValue(resource.ObjectReference, out var callback))
            {
                await _writer.FlushAsync();
                await callback(resource, this);
            }
        }
    }



/// <summary>
    /// Writes the form definitions and corresponding content,
    /// for each of the forms used in the document.
    /// </summary>
    /// <param name="forms">The forms to write.</param>
    private async Task WriteForms(IEnumerable<Form> forms)
    {
        // We go through each of the forms in the list, and output them.
        foreach (var form in forms)
        {
            await _writer.WriteLineAsync();

            var resourcesRef = CreateObjectReference();
            var contentLengthRef = CreateObjectReference();

            await OpenObject(form.ObjectReference, "XObject");
            await _writer.WriteLineAsync($"\t/Subtype\t/Form");
            await _writer.WriteLineAsync($"\t/Name\t/{form.Identifier}");
            await _writer.WriteLineAsync($"\t/Resources\t{resourcesRef.ToString(PdfObjectNotation.Reference)}");
            await _writer.WriteLineAsync($"\t/BBox\t[{form.BoundingBox.Left:F2} {form.BoundingBox.Bottom:F2} {form.BoundingBox.Right:F2} {form.BoundingBox.Top:F2}]");
            //await _writer.WriteLineAsync($"\t/Matrix\t[{form.Matrix.A} {form.Matrix.B} {form.Matrix.C} {form.Matrix.D} {form.Matrix.E} {form.Matrix.F}]");
            await _writer.WriteLineAsync($"\t/Length\t{contentLengthRef.ToString(PdfObjectNotation.Reference)}");

            await _writer.WriteLineAsync(">>");
            await _writer.WriteLineAsync("stream");

            var length = await form.WriteFormContent(this);
            await _writer.WriteLineAsync("\r\nendstream");
            await _writer.WriteLineAsync("endobj");

            await WriteObject(contentLengthRef, length.ToString());

            await WritePageOrFormResources(resourcesRef, form.Resources);
        }
    }

    /// <summary>
    /// Writes an image resource. Currently this method supports writing a JPEG image,
    /// and no other formats. 
    /// </summary>
    /// <param name="image">The image resource.</param>
    /// <exception cref="NotSupportedException">The image type is not supported.</exception>
    /// <exception cref="PdfDocumentWriterException">There was an error writing the image.</exception>
    private async Task WriteImage(Image image)
    {
        var imageType = ImageUtilities.GetImageType(image.Filename);
        if (ImageType.Jpeg != imageType) throw
            new NotSupportedException("Only JPEG images are supported in this version of the library.");

        var file = new FileInfo(image.Filename);

        try
        {
            var info = new Resources.Images.JpegImageInfo();
            info.Parse(image.Filename);

            await OpenObject(image.ObjectReference, "XObject");
            await _writer.WriteLineAsync($"\t/Subtype\t/Image");
            await _writer.WriteLineAsync($"\t/Width\t{info.Width}");
            await _writer.WriteLineAsync($"\t/Height\t{info.Height}");
            await _writer.WriteLineAsync($"\t/Length\t{file.Length}");

            await _writer.WriteLineAsync($"\t/ColorSpace\t/{info.ColourSpace}");

            if (PdfColourSpace.DeviceCMYK == info.ColourSpace && ImageType.Jpeg == imageType)
            { await _writer.WriteLineAsync($" /Decode\t[1 0 1 0 1 0 1 0]"); }

            await _writer.WriteLineAsync($"\t/BitsPerComponent\t8");
            await _writer.WriteLineAsync($"\t/Name\t/{image.Identifier}");
            await _writer.WriteLineAsync($"\t/Filter\t/DCTDecode");
            await _writer.WriteLineAsync(">>");
            await _writer.WriteLineAsync("stream");

            await WriteBuffer(File.ReadAllBytes(image.Filename), 0, (int)file.Length);

            await _writer.WriteLineAsync("\r\nendstream");
            await _writer.WriteLineAsync("endobj");

        }
        // If something went wrong during image parsing we throw an exception.
        catch (Resources.Images.ImageParseException ex)
        {
            throw new PdfDocumentWriterException($"Cannot parse image at {image.Filename}", ex);
        }
    }

    /// <summary>
    /// Writes the image definitions for each of the images used in the document.
    /// </summary>
    /// <param name="images">The images to write the definition of.</param>
    /// <exception cref="PdfDocumentWriterException">The image could not be found</exception>
    private async Task WriteImages(IEnumerable<Image> images)
    {
        // We go through each of the images in the list, and output them.
        foreach (var image in images)
        {
            if (false == File.Exists(image.Filename))
            {
                // The image couldn't be found so we throw an exception
                throw new PdfDocumentWriterException($"The image could not be found at path '{image.Filename}'.");
            }

            await _writer.WriteLineAsync();
            await WriteImage(image);
        } // for each image
    } // WriteImages()



    /// <summary>
    /// Writes a font definition for a TrueType font that is used in the document.
    /// </summary>
    /// <param name="font">The font to write.</param>
    private async Task WriteTrueTypeFont(TrueTypeFont font)
    {
        await OpenObject(font.ObjectReference, "Font");
        await _writer.WriteLineAsync("\t/Subtype\t/TrueType");
        await _writer.WriteLineAsync($"\t/BaseFont\t/{font.Typename.Replace(' ', '-')}");
        await _writer.WriteLineAsync($"\t/Name\t/{font.Identifier}");

        // If this font is not a symbolic font then output the Windows encoding.
        if (4 != font.Info.Flags)
            await _writer.WriteLineAsync("\t/Encoding\t/WinAnsiEncoding");

        // Create a new font descriptor reference and add that to the header.
        PdfObjectReference descriptorRef = CreateObjectReference();
        await _writer.WriteLineAsync($"\t/FontDescriptor\t{descriptorRef.ToString(PdfObjectNotation.Reference)}");

        // Write the widths of the glyphs that correspond to the characters in the range 32-255.
        const int firstChar = 32;
        const int lastChar = 255;
        await _writer.WriteLineAsync($"\t/FirstChar\t{firstChar}");
        await _writer.WriteLineAsync($"\t/LastChar\t{lastChar}");
        await _writer.WriteAsync("\t/Widths\t[");
        int charsPrinted = 0;

        // Fill a byte array with the numbers, and convert it to unicode numbers.
        // We then look up the character widths for them.
        byte[] bytes = new byte[lastChar - firstChar + 1];
        for (int c = firstChar; c <= lastChar; c++) bytes[c - firstChar] = (byte)c;
        char[] characters = System.Text.ASCIIEncoding.Default.GetChars(bytes);

        foreach (char ch in characters)
        {
            if (0 == charsPrinted % 16) await _writer.WriteAsync("\r\n\t\t");
            await _writer.WriteAsync($"{font.Info.GetCharacterWidth(ch) * 1000 / font.Info.UnitsPerEm} ");

            charsPrinted++;
        }
        await _writer.WriteAsync(" ]\r\n");
        await CloseObject();

        // We now open the font descriptor object.
        await OpenObject(descriptorRef, "FontDescriptor");
        await _writer.WriteLineAsync($"\t/Ascent\t{font.Info.Ascender * 1000 / font.Info.UnitsPerEm}");
        await _writer.WriteLineAsync($"\t/Descent\t{font.Info.Descender * 1000 / font.Info.UnitsPerEm}");
        await _writer.WriteLineAsync($"\t/CapHeight\t{font.Info.CapHeight * 1000 / font.Info.UnitsPerEm}");
        await _writer.WriteLineAsync($"\t/Flags\t{font.Info.Flags}");
        await _writer.WriteLineAsync($"\t/FontBBox\t[{font.Info.XMin * 1000 / font.Info.UnitsPerEm} {font.Info.YMin * 1000 / font.Info.UnitsPerEm} {font.Info.XMax * 1000 / font.Info.UnitsPerEm} {font.Info.YMax * 1000 / font.Info.UnitsPerEm}]");
        await _writer.WriteLineAsync($"\t/FontName\t/{font.Typename.Replace(' ', '-')}");
        await _writer.WriteLineAsync($"\t/ItalicAngle\t{font.Info.ItalicAngle}");
        await _writer.WriteLineAsync($"\t/StemV\t{font.Info.StemV * 1000 / font.Info.UnitsPerEm}");

        // If the font is embedded we will add the font file to the stream.
        PdfObjectReference fontProgramRef = PdfObjectReference.Empty;
        if (font.IsEmbedded)
        {
            fontProgramRef = CreateObjectReference();
            await _writer.WriteLineAsync($"\t/FontFile2\t{fontProgramRef.ToString(PdfObjectNotation.Reference)}");
        }
        await CloseObject();

        // We add the font program now (if neccessary)
        if (false == fontProgramRef.IsEmpty())
        {
            await WriteTrueTypeFontProgram(font, fontProgramRef);
        }
    }

    /// <summary>
    /// Writes a font definition for a TrueType font that is used in the document.
    /// </summary>
    /// <param name="font">The font to write.</param>
    /// <param name="ref">The object reference of the font program.</param>
    private async Task WriteTrueTypeFontProgram(TrueTypeFont font, PdfObjectReference @ref)
    {
        await OpenObject(@ref);
        await font.WriteFontProgram(_writer);
    }
    */
    #endregion



    // Private types
    // =============
    #region Private types
    /// <summary>
    /// The PageState class encapsulates the state for a page. It is used
    /// to keep track of references to content and resources.
    /// </summary>
    private class PageState
    {
        // Instance initialisers
        // =====================
        #region Instance initialisers
        /// <summary>
        /// Creates a new instance of the PageState class.
        /// </summary>
        /// <param name="pageReference">The object reference to the page.</param>
        /// <param name="resourcesReference">The object reference to the resources dictionary.</param>
        /// <param name="contentsReference">The object reference to contents.</param>
        /// <param name="mediabox">The page mediabox.</param>
        public PageState(
            PdfObjectReference pageReference,
            PdfObjectReference resourcesReference,
            PdfObjectReference contentsReference,
            PdfRectangle mediabox)
        {
            PageReference = pageReference;
            ResourcesReference = resourcesReference;
            ContentsReference = contentsReference;
            MediaBox = mediabox;
            Resources = [];
        }
        #endregion



        // Public properties
        // =================
        #region Public properties
        /// <summary>
        /// Reference to the page object.
        /// </summary>
        public PdfObjectReference PageReference { get; }
        /// <summary>
        /// Reference to the resources dictionary.
        /// </summary>
        public PdfObjectReference ResourcesReference { get; }
        /// <summary>
        /// Reference to the contents.
        /// </summary>
        public PdfObjectReference ContentsReference { get; }
        /// <summary>
        /// Reference to the media box.
        /// </summary>
        public PdfRectangle MediaBox { get; }

        /// <summary>
        /// The resources used by the page.
        /// </summary>
        public List<PdfResourceReference> Resources { get; }

        /// <summary>
        /// Determines if the page has already had content written.
        /// </summary>
        public bool HasContent => ContentStreamLengthReference != null;
        /// <summary>
        /// Reference to an object that contains the length of the content stream.
        /// </summary>
        public PdfObjectReference? ContentStreamLengthReference { get; private set; }

        /// <summary>
        /// The position in the stream at the start of the content.
        /// </summary>
        public long StreamMarker { get; private set; }

        /// <summary>
        /// If the page has multiple content streams, this is the array of content references.
        /// </summary>
        public List<PdfObjectReference>? ContentArray { get; private set; }
        #endregion



        // Public methods
        // ==============
        #region Public methods
        /// <summary>
        /// Starts content. This method updates the state to indicate that the page has content.
        /// </summary>
        /// <param name="contentStreamLengthReference">Reference to an object that contains the length of the content stream.</param>
        /// <param name="streamMarker">The position in the stream at the start of the content.</param>
        public void StartContent(PdfObjectReference contentStreamLengthReference, long streamMarker)
        {
            ContentStreamLengthReference = contentStreamLengthReference;
            StreamMarker = streamMarker;
        }

        /// <summary>
        /// Starts a new content stream. This method updates the state to indicate that the page has content.
        /// This method is used when the content is split across multiple streams.
        /// </summary>
        /// <param name="contentStreamLengthReference">Reference to an object that contains the length of the content stream.</param>
        /// <param name="streamMarker">The position in the stream at the start of the content.</param>
        /// <param name="contentReference">Reference to the content object.</param>
        public void NextContent(PdfObjectReference contentStreamLengthReference, long streamMarker, PdfObjectReference contentReference)
        {
            ContentStreamLengthReference = contentStreamLengthReference;
            StreamMarker = streamMarker;

            if (null == ContentArray) ContentArray = [];
            ContentArray.Add(contentReference);
        }
        #endregion
    }

    /// <summary>
    /// The WriterState enumeration lists the possible states for the PDF Writer.
    /// Initially, the writer is in the None state, and when the first page is
    /// opened, it moves to the Page state. When a page's content is opened, the
    /// state progresses to Content. When the file is closed, the state moves to
    /// Closed, at which point no further content or pages can be added.
    /// </summary>
    private enum WriterState
    {
        // The document has not yet been opened.
        None,
        // The document has ben opened. The writer is before the first page is opened, or in between pages.
        Free,
        // The writer has opened a page, but not yet started on its content.
        Page,
        // The writer is writing content for a page.
        Content,
        // The file is closed.
        Closed
    }
    #endregion

}
