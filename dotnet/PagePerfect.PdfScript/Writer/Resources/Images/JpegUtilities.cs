using System.Text;

namespace PagePerfect.PdfScript.Writer.Resources.Images;

/// <summary>
/// The JpegUtilities class is a static utility class that contains methods that
/// parse a JPEG image file or stream retrieve the necessary information.
/// It is used by the Writer to retrieve information about embedded JPEG images.
/// </summary>
public static class JpegUtilities
{
    #region Public methods
    /// <summary>
    /// Parses the image data found in the specified file.
    /// </summary>
    /// <param name="filename">The file to read the image data from</param>
    /// <exception cref="ImageParseException">An error occurred while reading the image data</exception>
    public static ImageInfo Parse(string filename) => Parse(File.OpenRead(filename));

    /// <summary>
    /// Parses the image data in the specified stream.
    /// </summary>
    /// <param name="stream">The stream to read the image data from</param>
    /// <exception cref="ImageParseException">An error occurred while reading the image data</exception>
    public static ImageInfo Parse(Stream stream) => ParseJpegStream(stream);
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Parses the image data contained within the specified JPEG stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    private static ImageInfo ParseJpegStream(Stream stream)
    {
        try
        {
            var cs = ColourSpace.DeviceRGB;
            int height = 0;
            int width = 0;

            // Rewind the stream.
            stream.Seek(0, SeekOrigin.Begin);

            // We open the file and look at the first two bytes. These need to match the JFIF file identifier.
            if (stream.ReadByte() == 0xff && stream.ReadByte() == 0xd8)
            {
                // Next up we read the JFIF headers. Each of these contains an identifier and the length
                // of the header. We are looking for the header with ID 192 (c0). This contains the width
                // and height of the image.
                while (0xff == stream.ReadByte())
                {
                    var identifier = stream.ReadByte();
                    var currentPosition = stream.Position;
                    var length = stream.ReadByte() << 8 | stream.ReadByte();

                    switch (identifier)
                    {
                        case 0xc0:
                        case 0xc2:
                            // This is the frame header. This will contain the width and height.
                            stream.Seek(1, SeekOrigin.Current);
                            height = stream.ReadByte() << 8 | stream.ReadByte();
                            width = stream.ReadByte() << 8 | stream.ReadByte();
                            break;

                        case 0xee:
                            // This is the Adobe APP14 header. This could contain an 'Adobe'
                            // value that indicates RGB or CMYK data.
                            var header = ReadString(stream);
                            if (string.Equals("Adobe", header, StringComparison.InvariantCultureIgnoreCase))
                            {
                                // 4 bytes into the 'ADOBE' marker we should find the color space.
                                stream.Seek(5, SeekOrigin.Current);
                                var colourSpace = stream.ReadByte();
                                switch (colourSpace)
                                {
                                    case 2:
                                        cs = ColourSpace.DeviceCMYK;
                                        break;
                                }
                            }
                            break;

                        default:
                            break;

                    }

                    // Find the next marker
                    stream.Seek(length + currentPosition - stream.Position, SeekOrigin.Current);

                }
            }

            return new ImageInfo
            {
                ColourSpace = cs,
                Width = width,
                Height = height
            };

        }
        // If an I/O error occurs we rethrow this as a JpegImageParseException and include the original exception for reference.
        catch (IOException ex)
        {
            throw new JpegImageParseException("An I/O error occurred while reading JPEG image information from the stream", ex);
        }
    }

    /// <summary>
    /// Reads a string from the specified stream.
    /// </summary>
    /// <param name="stream">The stream</param>
    /// <returns>The string</returns>
    private static string ReadString(Stream stream)
    {
        var sb = new StringBuilder();

        while (true)
        {
            int value = stream.ReadByte();
            if (value > 0)
                sb.Append((char)value);
            else
                break;
        }

        return sb.ToString();
    }
    #endregion

}

