namespace PagePerfect.PdfScript.Utilities;

/// <summary>
/// The FileLocationType enumeration lists the types of file location that can be
/// used in a .pdfs document. The FileUtilities.NormalisePath method returns a normalised (absolute)
/// path and its type, which can be used to determine how to read the file. 
/// /// </summary>
public enum FileLocationType
{
    LocalFile,
    Internet,
    Unknown
}