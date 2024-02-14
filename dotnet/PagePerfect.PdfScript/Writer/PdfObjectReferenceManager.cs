namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The PdfObjectReferenceManager class manages the allocation of object
/// references within a PDF document.
/// </summary>
public class PdfObjectReferenceManager
{
    // Private fields
    // ==============
    #region Private fields
    private int _generation = 0;
    private int _number = 0;
    #endregion


    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Creates a new generartion of object references.
    /// The next call to Next() will return the first object reference.
    /// </summary>
    public void NextGeneration()
    {
        _generation++;
        _number = 0;
    }

    /// <summary>
    /// Returns the next object reference.
    /// </summary>
    /// <returns></returns>
    public PdfObjectReference Next() => new(++_number, _generation);
    #endregion
}
