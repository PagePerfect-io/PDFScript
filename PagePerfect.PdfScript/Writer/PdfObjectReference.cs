namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The PdfObjectReference class represents a reference to a PDF object. The reference consists of two identifiers, a primary and
/// secondary. The secondary identifier is not used by objects created by the Writer and only included for compatibility with
/// embedded PDF documents (which may use the secondary identifier).
/// Clients should never need to create a PdfObjectReference instance themselves, and any self-created instance is likely to invalidate
/// the PDF document created by the Writer. Instead, use the CreateObjectReference() method of the Writer to obtain a new
/// object reference that is valid within the PDF document.
/// </summary>
/// <remarks>
/// Constructs a new PdfObjectReference instance.
/// </remarks>
/// <param name="id">The ID to use</param>
/// <param name="secondary">The secondary ID</param>
public class PdfObjectReference(int id, int secondary)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// Sets or retrieves the object reference's identifier
    /// </summary>
    public int Id { get; set; } = id;

    /// <summary>
    /// Sets or retrieves the object reference's secondary identifier.
    /// </summary>
    public int Secondary { get; set; } = secondary;

    /// <summary>
    /// Returns a value that represents an empty object reference.
    /// </summary>
    public static PdfObjectReference Empty { get; } = new PdfObjectReference();
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Returns a value that indicates if the reference is empty.
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return Id == 0 && Secondary == 0;
    }
    #endregion



    // Object overrides
    // ================
    #region Object overrides
    /// <summary>
    /// Indicates if this instance equals the specified instance.
    /// </summary>
    /// <param name="obj">The instance to compare this instance with</param>
    /// <returns>A boolean that indicates that this instance equals the specified instance (true) or not.</returns>
    public override bool Equals(object? obj)
    {
        if (null == obj) return false;
        if (obj is not PdfObjectReference other) return false;

        return other.Id == Id && other.Secondary == Secondary;
    }

    /// <summary>
    /// Retrieves this instance's unique hash code.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        return Id | (Secondary << 24);
    }

    /// <summary>
    /// Returns the string representation of this PdfObjectReference.
    /// </summary>
    /// <returns>The string representation</returns>
    public override string ToString()
    {
        return ToString(PdfObjectNotation.Reference);
    }

    /// <summary>
    /// Returns the string representation of this PdfObjectReference.
    /// </summary>
    /// <returns>The string representation</returns>
    public string ToString(PdfObjectNotation notation) => notation switch
    {
        PdfObjectNotation.Reference => $"{Id} {Secondary} R",
        PdfObjectNotation.Declaration => $"{Id} {Secondary} obj",
        _ => throw new NotImplementedException(),
    };
    #endregion



    // Public constructor
    // ==================
    #region Public constructor
    /// <summary>
    /// Constructs a new, empty PdfObjectReference instance.
    /// </summary>
    private PdfObjectReference() : this(0, 0) { }

    /// <summary>
    /// Constructs a new PdfObjectReference instance.
    /// </summary>
    /// <param name="id">The ID to use</param>
    public PdfObjectReference(int id) : this(id, 0) { }
    #endregion

} // PdfObjectReference class
