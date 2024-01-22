namespace PagePerfect.PdfScript.Reader.Statements.Prolog;

/// <summary>
/// The ResourceDeclaration class represents a  '# resource' prolog statement in a .pdfs file.
/// </summary>
/// <remarks>
/// Initialises a new ResourceDeclaration instance.
/// </remarks>
public class ResourceDeclaration(string name, ResourceType resourceType, string location)
: PrologStatement(PrologStatementType.ResourceDeclaration)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The resource's name.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The type of resource - image or font.
    /// </summary>
    public ResourceType ResourceType { get; } = resourceType;

    /// <summary>
    /// The location of this resource.
    /// </summary>
    public string Location { get; } = location;
    #endregion
}