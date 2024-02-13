namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The PdfResourceReference class represents a named reference to an object in the PDF stream. It is used
/// to include the object into a page as a resource.
/// </summary>
[Serializable]
public abstract class PdfResourceReference
{
    // Instance initialiser
    // ====================
    #region Instance initialiser
    /// <summary>
    /// Initialises a new PdfResourceReference instance.
    /// </summary>
    /// <param name="obj">The PDF object that this resource refers to.</param>
    /// <param name="identifier">The identifier that the object will be known as in the current page</param>
    /// <param name="type">The type of resource that this is.</param>
    /// <exception cref="ArgumentException">The obect reference and identifier cannot be Null or empty.</exception>
    public PdfResourceReference(PdfObjectReference obj, string identifier, PdfResourceType type)
    {
        if (null == obj) throw new ArgumentNullException(nameof(obj));
        if (obj.IsEmpty()) throw new ArgumentException("The object reference can not be empty", nameof(obj));
        if (string.IsNullOrEmpty(identifier)) throw new ArgumentException("The identifier cannot be null or empty", nameof(identifier));

        ObjectReference = obj;
        Identifier = identifier;
        Type = type;
    }
    #endregion


    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// Retrieves the identifier for this resource.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Retrieves the object reference for this resource.
    /// </summary>
    public PdfObjectReference ObjectReference { get; }

    /// <summary>
    /// Retrieves a value that indicates the resource's type - this string will be used in the page's
    /// resources dictionary.
    /// </summary>
    public PdfResourceType Type { get; }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Retrieves the name of a resource type, for use with
    /// resource dictionaries. 
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The type name.</returns>
    public static string GetDictionaryName(PdfResourceType type)
    {
        return type switch
        {
            PdfResourceType.Font => "Font",
            PdfResourceType.Image => "XObject",
            PdfResourceType.Form => "XObject",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    #endregion



    // Object overrides
    // ================
    #region Object overrides
    /// <summary>
    /// Retrieves a string representation of the resource reference.
    /// </summary>
    /// <returns>The string representation</returns>
    public override string ToString() => $"/{Identifier} {ObjectReference.ToString(PdfObjectNotation.Reference)}";
    #endregion

} // PdfResourceReference class
