namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The PdfObjectNotation enumeration lists the notations for PdfObject instances, representing the forms that
/// an object appears in a PDF document (either as an indirect reference or as a declaration).
/// It is used internally by the PDF writer.
/// </summary>
public enum PdfObjectNotation
{
    /// <summary>
    /// This notation is for values that reference PDF objects.
    /// </summary>
    Reference,

    /// <summary>
    /// This notation is for the declaration of PDF objects.
    /// </summary>
    Declaration
}
