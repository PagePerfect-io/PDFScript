using PagePerfect.PdfScript.Reader;
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
}