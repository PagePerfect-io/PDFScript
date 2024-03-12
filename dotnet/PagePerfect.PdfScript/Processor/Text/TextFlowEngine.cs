using System.Text;
using PagePerfect.PdfScript.Writer;

namespace PagePerfect.PdfScript.Processor.Text;


/// <summary>
/// The TextFlowEngine class is responsible for laying out text within a text block.
/// It has a single method, <see cref="TextFlowEngine.FlowText"/>. 
/// </summary>
public class TextFlowEngine
{
    // Private fields
    // ==============
    #region Private fields
    private readonly TextAlignmentOptions _options;
    private double _left; // The position of the next word, if it fits on the current line.
    private double _bottom; // The bottom of the current line, if it fits on the current line.
    private Chunk? _currentChunk; // The chunk we're currently working on. This is relevant when a 'word' covers multiple spans.
    private PdfRectangle _rect; // The current working rectangle.
    private List<Line>? _lines; // The lines we will add to.
    private List<IWordMetric>? _currentLine; // The words on the current line.

    private readonly double _lineSpacing; // The line spacing.
    private readonly double _wordSpacing; // The word spacing.
    private readonly double _characterSpacing; // The character spacing.
    private readonly double _textRatio; // The text ratio.
    #endregion



    // Instance initialiser
    // ====================
    #region Instance initialiser
    /// <summary>
    /// Initialises a new instance of the TextFlowEngine class.
    /// </summary>
    /// <param name="options">The text alignment options.</param>
    public TextFlowEngine(
        TextAlignmentOptions options,
        double lineSpacing = 0,
        double wordSpacing = 0,
        double characterSpacing = 0,
        double textRatio = 1)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _lineSpacing = lineSpacing;

        if (HorizontalTextAlignment.FullyJustified == (_options.HorizontalAlignment & HorizontalTextAlignment.FullyJustified))
        {
            _wordSpacing = 0;
        }
        else
        {
            _wordSpacing = wordSpacing;
        }

        _characterSpacing = characterSpacing;
        _textRatio = textRatio;
    }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Flows the text within the specified spans. This method will flow the words
    /// contained within the spans into lines. Note that the TextFlowEngine will not
    /// automatically put spaces between spans. A single resulting word may use text
    /// content from multiple spans. 
    /// </summary>
    /// <param name="spans">The spans.</param>
    /// <param name="rect">The rectangle in which to lay out the text.</param> 
    /// <returns>The lines.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public IEnumerable<Line> FlowText(IEnumerable<Span> spans, PdfRectangle rect)
    {
        if (null == spans) throw new ArgumentNullException(nameof(spans));
        if (rect.Width <= 0) return new List<Line>();
        var lines = new List<Line>();

        // First, we need to flow the words into lines.
        Setup(lines, rect);
        foreach (var span in spans) FlowSpan(span);
        Finalise();

        // Then, we align the lines vertically within the rectangle.
        AlignLines(lines, rect);

        return lines;
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Aligns lines vertically within the specified rectangle. This method lays out the lines
    /// according to the vertical alignment option.
    /// </summary>
    /// <param name="lines">The lines.</param>
    /// <param name="rect">The rectangle.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value for vertical alignment is not one of the supported values.</exception>
    private void AlignLines(List<Line> lines, PdfRectangle rect)
    {
        if (lines.Count == 0) return;

        // Calculate the total height of the lines
        var totalHeight = lines.Sum(l => l.BoundingBox.Height) + (lines.Count - 1) * _lineSpacing;

        // Calculate the offset to the top of the rectangle
        var offset = _options.VerticalAlignment switch
        {
            VerticalTextAlignment.Top => rect.Top,
            VerticalTextAlignment.Middle => rect.Top - (rect.Height - totalHeight) / 2,
            VerticalTextAlignment.Bottom => rect.Bottom + totalHeight,
            _ => throw new ArgumentOutOfRangeException()
        };

        // Lay out the lines.
        foreach (var line in lines)
        {
            var descent = line.Spans.Max(s => s.Font.GetDescent(s.FontSize));
            line.BoundingBox = new PdfRectangle(line.BoundingBox.Left, offset - line.BoundingBox.Height - descent, line.BoundingBox.Width, line.BoundingBox.Height);
            offset -= line.BoundingBox.Height + _lineSpacing;
        }
    }

    /// <summary>
    /// Finalises the flowing of text. This method will add any pending chunk to the current line,
    /// or a new current line if required, and finalises the current line.
    /// </summary> 
    private void Finalise()
    {
        if (null != _currentChunk)
        {
            if (false == PlaceChunk(_currentChunk)) return;
        }

        FinaliseLastLine();
    }

    /// <summary>
    /// Finalise the current line. Because this method
    /// Finalises the current line. This method adds a new line to the list.
    /// Because we know this is not the final line, we can use the configued
    /// alignment even if this is fully justified.
    /// </summary> 
    private void FinaliseCurrentLine()
    {
        if (HorizontalTextAlignment.FullyJustified == (_options.HorizontalAlignment & HorizontalTextAlignment.FullyJustified))
        {
            JustifyAndFinalise();
        }
        else
        {
            var left = _options.HorizontalAlignment switch
            {
                HorizontalTextAlignment.Left => 0,
                HorizontalTextAlignment.Center => (_rect.Width - _left) / 2,
                HorizontalTextAlignment.Right => _rect.Width - _left,
                _ => throw new ArgumentOutOfRangeException()
            };

            PositionAndFinalise(_rect.Left + left);
        }
    }

    /// <summary>
    /// Finalises the last current line. This method adds a new line to the list.
    /// If the text is fully justified, we will actually pick the non-justified
    /// alignment, if specified, or Left alignment by default.
    /// </summary>
    private void FinaliseLastLine()
    {
        var left = (HorizontalTextAlignment)((int)_options.HorizontalAlignment & 0x03) switch
        {
            HorizontalTextAlignment.Left => 0,
            HorizontalTextAlignment.Center => (_rect.Width - _left) / 2,
            HorizontalTextAlignment.Right => _rect.Width - _left,
            _ => throw new ArgumentOutOfRangeException()
        };

        PositionAndFinalise(_rect.Left + left);
    }

    /// <summary>
    /// Flows the text in the specifie span onto one or more lines.
    /// This method extract all words from the span, and then flows them onto the current
    /// line and any subsequent lines as necessary. 
    /// </summary>
    /// <param name="span">The span of text to flow.</param>
    private bool FlowSpan(Span span)
    {
        // If this is the start of a new line, try to create space for it.
        if (0 == _left && null == _currentChunk) { if (false == TryFitNewLine(span.FontSize)) return false; }
        else { if (false == TryExpandExistingLine(span.FontSize)) return false; }

        // Measure the words in the span.
        var words = MeasureWords(span);

        // If there is an existing chunk, and there is no space before this new list of words,
        // we add the first word to the existing chunk. If it no longer fits on this line,
        // we create a new line for it.
        var start = 0;
        if (null != _currentChunk)
        {
            if (!words.Any()) return PlaceChunk(_currentChunk);
            if (null == words.First())
            {
                if (!PlaceChunk(_currentChunk)) return false;
            }
            else
            {
                _currentChunk.AddWord(words.First());
                if (words.Count > 1)
                {
                    if (!PlaceWord(_currentChunk)) return false;
                }
                else
                {
                    return true;
                }
            }

            ++start;
        }

        // Then, place each word on the line if possible. If a word does not fit, we create
        // a new line for it.
        // If the last entry is for a space, then we're done - otherwise, we need to create
        // a new chunk and remember it for the next span.
        for (var w = start; w < words.Count - 1; ++w)
        {
            if (false == PlaceWord(words[w])) return false;
        }

        if (null != words.LastOrDefault()) _currentChunk = new Chunk(words.Last());

        return true;
    }

    /// <summary>
    /// Finalises the current line after arranging the words across the width of the
    /// rectangle, creating fully-justified text. 
    /// </summary> 
    private void JustifyAndFinalise()
    {
        // We will create a line span for each word or chunk, and then arrange them
        // such that available space is distributed evenly between them.
        // We will add a space character before each word or chunk after the first one,
        // because we want the text to be output as words in the PDF document so the text
        // is readable.
        var equalSpace = _currentLine!.Count > 1 && _textRatio > 0
            ? ((_rect.Width - _left) / (_currentLine.Count - 1) - _characterSpacing) / _textRatio : 0;
        var left = _rect.Left;
        var spans = new List<LineSpan>();

        foreach (var metric in _currentLine)
        {
            if (spans.Any()) left += equalSpace;

            switch (metric)
            {
                case Chunk chunk:
                    foreach (var chunkWord in chunk.Words)
                    {
                        var chunkWidth = chunkWord.Width + (spans.Any() ? metric.Space + _wordSpacing : 0);
                        spans.Add(new LineSpan(
                            new PdfRectangle(left, _bottom, chunkWidth, chunkWord.Height),
                            chunkWord.Font,
                            chunkWord.FontSize,
                            spans.Any() ? $" {chunkWord.Text}" : chunkWord.Text));
                        left += chunkWidth;
                    }
                    break;

                case WordMetric word:
                    var width = metric.Width + (spans.Any() ? metric.Space + _wordSpacing : 0);
                    spans.Add(new LineSpan(
                        new PdfRectangle(left, _bottom, width, word.Height),
                        word.Font,
                        word.FontSize,
                        spans.Any() ? $" {word.Text}" : word.Text));
                    left += width;
                    break;
            }
        }

        if (spans.Any())
        {
            var first = spans.First();
            var last = spans.Last();
            var height = spans.Max(s => s.BoundingBox.Height);
            _lines!.Add(new Line(new PdfRectangle(first.BoundingBox.Left, _bottom, last.BoundingBox.Right - first.BoundingBox.Left, height), spans));
        }
    }

    /// <summary>
    /// Measures the words in the specified span. This method will return a list of words
    /// in the span, and their widths. This takes into account the character spacing and
    /// text ratio options.
    /// </summary>
    /// <returns>The words.</returns>
    private List<WordMetric> MeasureWords(Span span)
    {
        var space = span.Font.MeasureSpace(span.FontSize, _characterSpacing, _textRatio);

        var words = span.Text.Split(new[] { ' ', '\t', '\r', '\n', '\f' }, StringSplitOptions.RemoveEmptyEntries);

        return words.Select(w => new WordMetric(w, span.Font)
        {
            FontSize = span.FontSize,
            Space = space,
            Width = span.Font.MeasureString(w, span.FontSize, _characterSpacing, _textRatio)
        }).ToList();
    }

    /// <summary>
    /// Places the specfied chunk on the current line. This method will check if the
    /// chunk is unitary - if it is, it places the single word metric. Otherwise, it will place
    /// the entire chunk as a single "word".
    /// </summary>
    /// <returns>True if the chunk was placed, False otherwise.</returns>
    private bool PlaceChunk(Chunk chunk)
    {
        if (chunk.Words.Count == 1) return PlaceWord(chunk.Words[0]); else return PlaceWord(chunk);
    }

    /// <summary>
    /// Places the specfied word on the current line. If the word does not fit on the line,
    /// this method will attempt to create a new line for it. If that fails, this method
    /// returns False. Otherwise, it returns True and updates the current state. 
    /// </summary>
    /// <returns>True if the word was placed, False otherwise.</returns>
    private bool PlaceWord(IWordMetric word)
    {
        var width = word.Width + _wordSpacing;
        if (_currentLine!.Any()) width += word.Space;
        if (width + _left > _rect.Width)
        {
            if (_currentLine!.Any()) width -= word.Space;
            if (false == TryCreateNewLine(word, true)) return false;
        }
        _left += width;
        _currentLine!.Add(word);
        return true;
    }


    /// <summary>
    /// Finalises the current line after positioning the words according to the
    /// specified left coordinate. This method is called after determining the
    /// left-hand coordinate of the line, which is left-aligned, right-aligned or
    /// centered. As an optimisation, any words that use the same font and size
    /// are collapsed into a single line span. 
    /// </summary>
    /// <param name="left">The left coordinate.</param>
    private void PositionAndFinalise(double left)
    {
        var spans = new List<LineSpan>();
        var text = new StringBuilder();
        var width = 0d;
        var previous = default(WordMetric);
        for (var w = 0; w < _currentLine!.Count; w++)
        {
            switch (_currentLine[w])
            {
                case Chunk chunk:
                    if (width > 0)
                    {
                        spans.Add(new LineSpan(
                        new PdfRectangle(left, _bottom, width, previous!.Height),
                        previous.Font,
                        previous.FontSize,
                        text.ToString()));
                    }
                    left += width;
                    for (var cw = 0; cw < chunk.Words.Count; cw++)
                    {
                        var chunkWord = chunk.Words[cw];
                        width = chunkWord.Width;
                        if (spans.Any() && 0 == cw) { width += chunkWord.Space + _wordSpacing; }
                        spans.Add(new LineSpan(
                            new PdfRectangle(left, _bottom, width, chunkWord.Height),
                            chunkWord.Font,
                            chunkWord.FontSize,
                            chunkWord.Text));
                        left += width;
                    }
                    text.Clear();
                    width = 0;
                    previous = null;
                    break;

                case WordMetric word:
                    if (null != previous && (previous.Font != word.Font || previous.FontSize != word.FontSize))
                    {
                        spans.Add(new LineSpan(
                            new PdfRectangle(left, _bottom, width, previous.Height),
                            previous.Font,
                            previous.FontSize,
                            text.ToString()));
                        left += width;
                        text.Clear();
                        width = 0;
                        previous = null;
                    }
                    else
                    {
                        if (text.Length > 0 || spans.Any()) { text.Append(' '); width += word.Space + _wordSpacing; }
                        text.Append(word.Text);
                        width += word.Width;
                        previous = word;
                    }
                    break;
            }
        }

        if (width > 0)
        {
            spans.Add(new LineSpan(
                new PdfRectangle(left, _bottom, width, previous!.Height),
                previous.Font,
                previous.FontSize,
                text.ToString()));
        }

        if (spans.Any())
        {
            var first = spans.First();
            var last = spans.Last();
            var height = spans.Max(s => s.BoundingBox.Height);
            _lines!.Add(new Line(new PdfRectangle(first.BoundingBox.Left, _bottom, last.BoundingBox.Right - first.BoundingBox.Left, height), spans));
        }
    }

    /// <summary>
    /// Attempts to creates a new line with the specified word.
    /// The word is used to determine the line height. If the line
    /// does not fit within the remaining space of the rectangle,
    /// this method returns False. Otherwise, it returns True and updates
    /// the current state. 
    /// Optionally, this method first finalises the current line. 
    /// </summary>
    /// <param name="word">The word.</param>
    /// <param name="finaliseCurrentLine">Should the method finalise the current line?</param>
    private bool TryCreateNewLine(IWordMetric word, bool finaliseCurrentLine)
    {
        if (finaliseCurrentLine)
        {
            FinaliseCurrentLine();
        }

        // Can we fit another line in this rectangle?
        return TryFitNewLine(word.Height);
    }

    /// <summary>
    /// Attempts to find space to expand the current line.
    /// If the new space was found, this method returns True.
    /// Otherwise, it returns False.
    /// </summary>
    /// <param name="height">The new required line height.</param>
    /// <returns>True if the operation succeeded; False otherwise.</returns>
    private bool TryExpandExistingLine(double height)
    {
        var currentHeight = _currentLine!.Max(w => w.Height);
        _bottom -= Math.Max(height, currentHeight) - currentHeight;

        return _bottom >= _rect.Bottom;
    }

    /// <summary>
    /// Attempts to fit a new line into the space of the rectangle.
    /// This method returns True if there was space for a new line,
    /// otherwise it returns False. 
    /// /// </summary>
    /// <param name="height">The required line height.</param>
    /// <returns>True if the line fit; False otherwise.</returns>
    private bool TryFitNewLine(double height)
    {
        _left = 0;
        _currentLine!.Clear();
        _bottom -= height;

        return _bottom >= _rect.Bottom;
    }

    /// <summary>
    /// Sets up the text flow engine for a new rectangle.
    /// This method initialises the current line and the position within the
    /// rectangle. 
    /// </summary>
    /// <param name="lines">The lines we'll add to.</param> 
    /// <param name="rect">The rectangle.</param>
    private void Setup(List<Line> lines, PdfRectangle rect)
    {
        _lines = lines;
        _currentLine = new List<IWordMetric>();
        _rect = rect;
        _currentChunk = null;
        _left = 0;
        _bottom = rect.Top;
    }
    #endregion



    // Private types
    // =============
    #region Private types
    private interface IWordMetric
    {
        double Width { get; }
        double Height { get; }
        double Space { get; }
    }

    private class Chunk : IWordMetric
    {
        public Chunk(WordMetric word)
        {
            Words = new List<WordMetric>();
            AddWord(word);
        }

        public double Space => Words.First().Space;
        public double Width { get; private set; }
        public List<WordMetric> Words { get; set; } // ...        

        public double Height => Words.Max(w => w.Height);

        public void AddWord(WordMetric word)
        {
            Words.Add(word);
            Width += word.Width;
        }
    }

    private class WordMetric : IWordMetric
    {
        public WordMetric(string text, Font font)
        {
            Text = text;
            Font = font;
        }

        public double Space { get; set; }

        public double Width { get; set; }

        public Font Font { get; set; }

        public double FontSize { get; set; }

        public string Text { get; set; }

        public double Height { get => FontSize; }
    }
    #endregion
}