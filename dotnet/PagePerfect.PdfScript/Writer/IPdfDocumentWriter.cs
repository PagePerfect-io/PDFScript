using PagePerfect.PdfScript.Reader;

namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The IPdfDocumentWriter interface defines the methods that a PDF document writer
/// implements. The PDF document writer is used to write a PDF document to a stream.
/// </summary>
public interface IPdfDocumentWriter
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// Indicates if the document is currently open.
    /// </summary>
    public bool IsOpen { get; }
    #endregion



    // Public methods
    // ==============
    #region Public methods
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
    public void AddResourceToPage(PdfResourceReference resource);

    /// <summary>
    /// Finalises the document. This method wraps up any pending
    /// operations, closes the current page if applicable, and any
    /// other state, and closes the file stream. This instance can
    /// no longer be used after calling this method.
    /// </summary>
    public Task Close();

    /// <summary>
    /// Closes the content stream, if one is open. It is not an error to attempt to
    /// close a stream if one isn't open - the call will be ignored instead.
    /// </summary>
    public Task CloseContentStream();

    /// <summary>
    /// Finalises the document. This method wraps up any pending
    /// operations, closes the current page if applicable, and any
    /// other state, and closes the file stream. This instance can
    /// no longer be used after calling this method.
    /// This method will do nothing if the document isn't yet open,
    /// or was already closed. To throw an exception if the document
    /// is in an invalid state, use the Close() method instead.
    /// /// </summary>
    public Task CloseIfNeeded();

    /// <summary>
    /// Closes the currently opened object. This call is only valid in the Free or Page state.
    /// </summary>
    public Task CloseObject();

    /// <summary>
    /// Closes the page. This method closes a currently open page, if one is open.
    /// If no page is open, this methid throws an error.
    /// Closing a page also closes any other state, such as a Text Frame state.
    /// </summary>
    public Task ClosePage();

    /// <summary>
    /// Creates a new Image resource and returns a reference that identifies the image.
    /// </summary>
    /// <param name="filename">The image filename</param>
    /// <param name="tag">An optional tag that can be used to identify the image.</param>
    /// <returns>Reference to the newly created image.</returns>
    /// <exception cref="ArgumentNullException">The filename argument is null</exception>
    public Image CreateImage(string filename, object? tag = null);

    /// <summary>
    /// Creates a new object reference that is valid within this document.
    /// The object reference must be used to write an object to the PDF stream at some point,
    /// otherwise an exception is thrown when the document is finalised.
    /// </summary>
    /// <returns>The new object reference.</returns>
    public PdfObjectReference CreateObjectReference();

    /// <summary>
    /// Flushes the PDF stream.
    /// </summary>
    public void Flush();

    /// <summary>
    /// Opens the next content stream. This method is an alternative to OpenContentStream(),
    /// and is used to support multip[le content streams in one page. It is not permitted to
    /// combine OpenContentStream() and NextContentStream().
    /// </summary>
    public Task<PdfObjectReference> NextContentStream();

    /// <summary>
    /// Opens the document. This means the document is ready to accept
    /// content and resources.
    /// </summary>
    public Task Open();


    /// <summary>
    /// Opens a new content stream. This operation is only valid in the Page state. It is an error
    /// to attempt to open a content stream in the Free or Content mode. It is also an error to
    /// attempt to open a content stream if a page has already had content written.
    /// </summary>
    public Task<PdfObjectReference> OpenContentStream();

    /// <summary>
    /// Opens a new object and returns its object reference. This operation only works in the Free or Page state.
    /// This method records the object reference in the cross reference table.
    /// </summary>
    /// <param name="type">Optionally, the type of the object to add as a dictionary entry.</param>
    /// <returns>The object reference</returns>
    /// <exception cref="PdfWriterException">This operation is not valid in the current state.</exception>
    public Task<PdfObjectReference> OpenObject(string? type = null);

    /// <summary>
    /// Opens a new object with the specified object reference as its identifier.
    /// This operation only works in the Free or Page states.
    /// This method records the object reference in the cross reference table.
    /// </summary>
    /// <param name="reference">The object's reference</param>
    /// <param name="type">Optionally, the type of the object to add as a dictionary entry.</param>
    /// <exception cref="PdfWriterException">This operation is not valid in the current state.</exception>
    public Task OpenObject(PdfObjectReference reference, string? type = null);

    /// <summary>
    /// Opens a new page in the PDF document. If a page is currently open, this method will close
    /// that page, and close any other state (such as a Text Frame state).
    /// </summary>
    /// <param name="width">The width of the page, in points.</param>
    /// <param name="height">The height of the page, in points.</param>
    /// <param name="display">The display orientation of the page. Defaults to a regular orientation.</param>
    public Task OpenPage(double width, double height, DisplayOrientation display);

    /// <summary>
    /// Outputs the contents of the specified buffer to the current contents stream.
    /// This operation is valid in the Free, Page and Content state. The buffer is written
    /// into the output stream as-is, so it is the responsibility of the caller to ensure
    /// that the buffer is valid PDF content.
    /// </summary>
    /// <param name="buffer">The buffer</param>
    /// <param name="start">The index to start writing from.</param>
    /// <param name="length">The number of bytes to write.</param>
    public Task WriteBuffer(byte[] buffer, int start, int length);

    /// <summary>
    /// Outputs the specified string directly to the current contents stream. No checking is done on this
    /// string so if it doesn't match the PDF specification, the document will not load.
    /// This method can be called regardless of the current state of the Writer.
    /// </summary>
    /// <param name="content">The content</param>
    public Task WriteRawContent(string content);

    /// <summary>
    /// Writes the specified PdfsValue to the current content stream.
    /// This is used by the processor to write graphics instructions to the output stream.
    /// </summary>
    /// <param name="value">The value.</param>
    public Task WriteValue(PdfsValue value);
    #endregion
}
