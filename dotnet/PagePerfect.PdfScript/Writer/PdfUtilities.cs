using System.Text;

namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The PdfUtilities class is a static library class that contains convenience methods that facilitate the
/// Writer and its assorted classes.
/// </summary>
public static class PdfUtilities
{
    private static readonly Encoding ISO88591 = Encoding.GetEncoding(28591);

    /// <summary>
    /// Escapes the specified string after encoding it to the ISO 8859-1 codepage.
    /// All characters above ASCII 127 are escaped, as well
    /// as the (, ) and \ characters.
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns></returns>
    public static string EscapeIso88591String(string text)
    {
        StringBuilder sb = new();

        // Encode the string to the default Window ANSI code page.
        byte[] bytes = ISO88591.GetBytes(text);

        int ch;
        for (int index = 0; index < bytes.Length; index++)
        {
            ch = bytes[index];
            if (ch >= 128)
            {
                sb.Append('\\').Append(Convert.ToString(ch, 8));
            }
            else if (ch == 40 || ch == 41) sb.Append("\\0").Append(Convert.ToString((int)ch, 8));
            else if (ch == 92) sb.Append("\\").Append(Convert.ToString((int)ch, 8));
            else sb.Append((char)ch);
        }
        return sb.ToString();
    }
}