using System.Text;
using PagePerfect.PdfScript.Processor;
using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript;

/// <summary>
/// The Document class encapsulates the functionality for creating a PDF document
/// out of a PDFScript source or stream. Use the SaveAs method to save a PDF document,
/// or the ToStream method to write the document to an output stream.
/// </summary>
public class Document(Stream stream)
{
    // Private fields
    // ==============
    #region Private fields
    private readonly Stream _stream = stream;
    #endregion



    // Instance initialisers
    // =====================
    #region Instance initialisers
    /// <summary>
    /// Initialises a new Document instance based on the specified PDFScript content.
    /// </summary>
    /// <param name="source">The PDFScript content</param>
    public Document(string source) : this(new MemoryStream(Encoding.UTF8.GetBytes(source)))
    { }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Saves the PDF document to the specified path. This method processes the
    /// PDFScript content into a PDF document, and writes it to the specified path.
    /// </summary>
    /// <param name="path">The output path.</param>
    public async Task SaveAs(string path)
    {
        var writer = new PdfDocumentWriter(path);
        await PdfsProcessor.Process(_stream, writer, null);
    }

    /// <summary>
    /// Saves the PDF document to the specified path. This method processes the
    /// PDFScript content into a PDF document, and writes it to the specified path.
    /// </summary>
    /// <param name="path">The output path.</param>
    public async Task SaveAs(string path, dynamic variables)
    {
        var writer = new PdfDocumentWriter(path);
        await PdfsProcessor.Process(_stream, writer, variables);
    }

    /// <summary>
    /// Saves the PDF document to the specified stream. This method processes the
    /// PDFScript content into a PDF document, and writes it to the specified stream.
    /// </summary>
    /// <param name="output">The output stream.</param>
    public async Task ToStream(Stream output)
    {
        var writer = new PdfDocumentWriter(output);
        await PdfsProcessor.Process(_stream, writer, null);
    }

    /// <summary>
    /// Saves the PDF document to the specified stream. This method processes the
    /// PDFScript content into a PDF document, and writes it to the specified stream.
    /// </summary>
    /// <param name="output">The output stream.</param>
    public async Task ToStream(Stream output, dynamic variables)
    {
        var writer = new PdfDocumentWriter(output);
        await PdfsProcessor.Process(_stream, writer, variables);
    }
    #endregion
}
