using PagePerfect.PdfScript.Reader;
using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript.Processor.Text;

/// <summary>
/// The TextUtilities class is a static utility class that provides convenience methods
/// for working with text.
/// </summary> 
public static class TextUtilities
{
    /// <summary>
    /// Writes the specified lines to the document. This method will emit TJ or Tj instructions
    /// for the spans on each line, via the PdfDocumentWriter instance.
    /// </summary>
    /// <param name="writer">The document writer instance.</param>
    /// <param name="lines">The text lines.</param>
    public static async Task WriteLines(this IPdfDocumentWriter writer, IEnumerable<Line> lines)
    {
        if (!lines.Any()) return;

        Line? previous = null;
        Font? previousFont = null;
        var previousFontSize = 0f;
        var previousLeading = 0f;

        // Write each line in the text block.
        // We use the bounding box of the line to determine its vertical separation from
        // the line before, and the horizontal offset compared to the line before.
        // For the first line, we emit a text matrix instruction.
        foreach (var line in lines.OrderByDescending(l => l.BoundingBox.Top))
        {
            float? previousRight = null;

            // Before the first line, we set the matrix. Between lines, we emit a line break instruction.            
            if (null != previous)
            {
                var leading = (float)(previous.BoundingBox.Bottom - line.BoundingBox.Bottom);
                var offset = line.BoundingBox.Left - previous.BoundingBox.Left;

                if (leading != previousLeading || offset != 0) await writer.WriteRawContent($"{Math.Round(offset, 3)} {-Math.Round(leading, 3)} TD\r\n");
                else await writer.WriteRawContent("T*\r\n");

                previousLeading = leading;
            }
            else { await writer.WriteRawContent($"1 0 0 1 {Math.Round(line.BoundingBox.Left, 3)} {Math.Round(line.BoundingBox.Bottom, 3)} Tm\r\n"); }

            // Write each span in the line. We do not automatically insert spaces between spans -
            // the text flow engine will have already done that.
            // If the span's font/size is different from the previous one, we emit an instruction
            // to change the font and size.
            // If there is a gap between this span and the previous one, we emit that as a gap
            // within a TJ instruction.
            foreach (var span in line.Spans)
            {
                if (previousFont != span.Font || previousFontSize != span.FontSize)
                {
                    await writer.WriteRawContent($"/{span.Font.Identifier} {Math.Round(span.FontSize, 3)} Tf\r\n");
                    previousFont = span.Font;
                    previousFontSize = span.FontSize;
                }

                if (null != previousRight && span.BoundingBox.Left > previousRight)
                {
                    var gap = (int)((span.BoundingBox.Left - previousRight) * 1000 / span.FontSize);
                    await writer.WriteRawContent($"[-{gap} ");
                    await writer.WriteValue(new PdfsValue(span.Text));
                    await writer.WriteRawContent($"] TJ\r\n");
                }
                else
                {
                    await writer.WriteValue(new PdfsValue(span.Text));
                    await writer.WriteRawContent($" Tj\r\n");
                }

                previousRight = (float)span.BoundingBox.Right;
            }

            previous = line;
        }

        writer.Flush();
    }

    /// <summary>
    /// Writes the specified lines to the document. This method will emit TJ or Tj instructions
    /// for the spans on each line.
    /// </summary>
    /// <param name="writer">The document writer instance.</param>
    /// <param name="lines">The text lines.</param>
    public static async Task WriteLines(this TextWriter writer, IEnumerable<Line> lines)
    {
        if (null == lines) throw new ArgumentNullException(nameof(lines));
        if (!lines.Any()) return;

        await writer.WriteAsync("BT ");

        Line? previous = null;
        Font? previousFont = null;
        double previousFontSize = 0;
        double previousLeading = 0;

        // Write each line in the text block.
        // We use the bounding box of the line to determine its vertical separation from
        // the line before, and the horizontal offset compared to the line before.
        // For the first line, we emit a text matrix instruction.
        foreach (var line in lines.OrderByDescending(l => l.BoundingBox.Top))
        {
            double? previousRight = null;

            // Before the first line, we set the matrix. Between lines, we emit a line break instruction.            
            if (null != previous)
            {
                var leading = previous.BoundingBox.Bottom - line.BoundingBox.Bottom;
                var offset = line.BoundingBox.Left - previous.BoundingBox.Left;

                if (leading != previousLeading || offset != 0) await writer.WriteAsync($"{Math.Round(offset, 3)} {-Math.Round(leading, 3)} TD ");
                else await writer.WriteAsync("T* ");

                previousLeading = leading;
            }
            else { await writer.WriteAsync($"1 0 0 1 {Math.Round(line.BoundingBox.Left, 3)} {Math.Round(line.BoundingBox.Bottom, 3)} Tm "); }

            // Write each span in the line. We do not automatically insert spaces between spans -
            // the text flow engine will have already done that.
            // If the span's font/size is different from the previous one, we emit an instruction
            // to change the font and size.
            // If there is a gap between this span and the previous one, we emit that as a gap
            // within a TJ instruction.
            foreach (var span in line.Spans)
            {
                if (previousFont != span.Font || previousFontSize != span.FontSize)
                {
                    await writer.WriteAsync($"/{span.Font.Identifier} {Math.Round(span.FontSize, 3)} Tf ");
                    previousFont = span.Font;
                    previousFontSize = span.FontSize;
                }

                if (null != previousRight && span.BoundingBox.Left > previousRight)
                {
                    var gap = (int)((span.BoundingBox.Left - previousRight) * 1000 / span.FontSize);
                    await writer.WriteAsync($"[-{gap} ({PdfUtilities.EscapeIso88591String(span.Text)})] TJ ");
                }
                else
                    await writer.WriteAsync($"({PdfUtilities.EscapeIso88591String(span.Text)}) Tj ");

                previousRight = span.BoundingBox.Right;
            }

            /* */
            previous = line;
        }

        await writer.WriteAsync("ET ");
        await writer.FlushAsync();
    }
}