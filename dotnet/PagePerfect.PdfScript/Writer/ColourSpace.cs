namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The ColourSpace enumeration lists the possible colour spaces supported by the Writer.
/// This enumeration is used by the ImageData class to specify the colour space of an image.
/// </summary>
public enum ColourSpace
{
    /// <summary>
    /// The DeviceRGB colour space defines colours using three components; red, green, and blue.
    /// These colours are used on TV and computer monitors.
    /// </summary>
    DeviceRGB,

    /// <summary>
    /// The DeviceCMYK colour space defines colours using four components; cyan, magenta, yellow, and black.
    /// These colours represent colours used in ink-based output, such as when printing a document.
    /// </summary>
    DeviceCMYK,

    /// <summary>
    /// The DeviceGray colour space defines colours using a single component; a shade of gray.
    /// </summary>
    DeviceGray
}