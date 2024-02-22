using System.Text;

namespace PagePerfect.PdfScript.Writer.Resources.Fonts.TrueType;

/// <summary>
/// The TrueTypeFontInfo class contains information about a TrueType font.
/// </summary>
public class TrueTypeFontInfo
{
    // Private fields
    // ==============
    #region Private fields

    private readonly Dictionary<int, GlyphInfo> _mappedGlyphs = [];
    // The character-to-glyph mapping
    private GlyphInfo? _undefinedGlyph;
    // The glyph that represents an 'undefined' mapping
    #endregion



    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The revision of the TrueType file
    /// </summary>
    public decimal Revision { get; private set; }

    /// <summary>
    /// The creation date of the font
    /// </summary> 
    public DateTime DateCreated { get; private set; }
    /// <summary>
    /// The date it was modified
    /// </summary> 
    public DateTime DateModified { get; set; }
    /// <summary>
    /// The font's full name.
    /// </summary> 
    public string? FullName { get; set; }
    /// <summary>
    /// X min metric for the entire font
    /// </summary> 
    public int XMin { get; set; }
    /// <summary>
    /// Y min metric for the entire font
    /// </summary> 
    public int YMin { get; set; }
    /// <summary>
    /// X max metric for the entire font
    /// </summary> 
    public int XMax { get; set; }
    /// <summary>
    /// Y max metric for the entire font
    /// </summary> 
    public int YMax { get; set; }
    /// <summary>
    /// The number of units that make up one em-point
    /// </summary> 
    public int UnitsPerEm { get; set; }
    /// <summary>
    /// The ascender of the font
    /// </summary> 
    public int Ascender { get; set; }
    /// <summary>
    /// The descender of the font
    /// </summary> 
    public int Descender { get; set; }
    /// <summary>
    /// The font's italic angle
    /// </summary> 
    public decimal ItalicAngle { get; set; }
    /// <summary>
    /// The height of capitals in the font
    /// </summary> 
    public int CapHeight { get; set; }
    /// <summary>
    /// The width of vertical stems in the font
    /// </summary> 
    public int StemV { get; set; }
    /// <summary>
    /// Is this font a symbol font?
    /// </summary> 
    public bool IsSymbolFont { get; set; }
    /// <summary>
    /// The font's postscript name
    /// </summary> 
    public string? PostscriptName { get; set; }

    /// <summary>
    /// Retrieves the font's flags as demanded by the PDF specification.
    /// </summary>
    public int Flags => IsSymbolFont ? 4 : 32;
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Retrieves the width of the specified character.
    /// </summary>
    /// <param name="ch">The character</param>
    /// <returns>The width of the character</returns>
    public int GetCharacterWidth(char ch)
    {
        if (_mappedGlyphs.TryGetValue(ch, out var glyph)) return glyph.Metric.AdvanceWidth;
        return null == _undefinedGlyph ? 0 : _undefinedGlyph.Metric.AdvanceWidth;
    }

    /// <summary>
    /// Parses the TrueType font information from the specified file.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    public void Parse(string path)
    {
        Parse(File.OpenRead(path));
    }

    /// <summary>
    /// Parses the TrueType font information from the specified stream.
    /// </summary>
    /// <param name="stream">The stream that contains the font file.</param>
    public void Parse(Stream stream)
    {
        ParseFontStream(stream, false);
    }
    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Finds the table with the specified tag.
    /// </summary>
    /// <param name="tables">The list of tables to search</param>
    /// <param name="tag">The tag to look for</param>
    /// <returns>Reference to the table with the specified tag, if it exists, or a null reference if no
    /// table with that tag was found.</returns>
    private static Table? FindTable(Table[] tables, string tag)
    {
        return tables.FirstOrDefault(t => t.Tag.Equals(tag));
    }

    /// <summary>
    /// Retrieves the glyph for the specified character.
    /// </summary>
    /// <param name="character">The character</param>
    /// <returns>A reference to a GlyphInfo instance for the corresponding glyph, 
    /// or a null reference if no glyph exists for that character.</returns>
    private GlyphInfo? GetGlyph(char character)
    {
        return _mappedGlyphs.ContainsKey(character) ? _mappedGlyphs[character] : _undefinedGlyph;
    }

    /// <summary>
    /// Parses the data contained in the stream. 
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="headersOnly">True to read only the file headers.
    /// False to parse the entire file.</param>
    private void ParseFontStream(Stream stream, bool headersOnly)
    {
        try
        {
            // Rewind the stream.
            stream.Seek(0, SeekOrigin.Begin);

            // Read the offset table. This returns the number of tables.
            // We need at least one table.
            var tableCount = ReadOffsetTable(stream);
            if (0 == tableCount) throw
                new TrueTypeFontParseException("Could not read the offset table in this font file");

            // Read the tables off the directory.
            // If no tables could be read then the file is invalid.
            var tables = ReadTableDirectory(stream, tableCount);
            if (null == tables) throw
                new TrueTypeFontParseException("Could not read the tables in this font file");

            // Read the font headers to store global font properties.
            var headers = ReadFontHeaders(stream, tables);
            var horizontalMetricsCount = ReadHorizontalHeaders(stream, tables);
            ReadPostscriptData(stream, tables);
            ReadOs2Data(stream, tables);
            ReadNameData(stream, tables);

            if (false == headersOnly)
            {
                // Read character mappings. This dictionary will map ASCII character values to glyph indices.
                var mappings = ReadCharacterMappings(stream, tables);

                // We determine the number of glyphs in this file.
                var glyphCount = ReadMaximumProfile(stream, tables);

                // We now read the glyph data
                // Read the horizontal metrics for the glyphs.
                var metrics = ReadHorizontalMetrics(stream, tables, horizontalMetricsCount, glyphCount);
                var glyphs = ReadGlyphs(glyphCount, stream, tables, headers, metrics);
                if (null == glyphs || !glyphs.Any()) throw
                    new TrueTypeFontParseException("No glyph data could be read from the font file");

                _undefinedGlyph = glyphs[0];

                // Add the glyphs to the mapping.
                if (null != mappings)
                {
                    foreach (var mapping in mappings)
                    {
                        if (mapping.Item2 <= glyphs.Length && mapping.Item2 > 0)
                            _mappedGlyphs[mapping.Item1] = glyphs[mapping.Item2 - 1];
                    }
                }

                // Finally, if we have no capHeight or stemV set at the moment we create these values from
                // representative glyphs.
                if (0 == CapHeight)
                {
                    var capitalH = GetGlyph('H');
                    if (null != capitalH) CapHeight = capitalH.YMax;
                }

                if (0 == StemV)
                {
                    var smallCapsI = GetGlyph('i');
                    if (null != smallCapsI) StemV = smallCapsI.XMax - smallCapsI.XMin;
                }
            }

        }
        // If an I/O error occurred we rethrow as a TrueTypeFontParseException, and include the original
        // exception for reference.
        catch (IOException ex)
        {
            throw new TrueTypeFontParseException("An I/O error occurred while reading font data from the file", ex);
        }
    }

    /// <summary>
    /// Reads the character mappings from the stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="tables">The tables in the stream</param>
    /// <returns>The character mappings</returns>
    private Tuple<int, int>[]? ReadCharacterMappings(Stream stream, Table[] tables)
    {
        var cmapTable = FindTable(tables, "cmap");
        if (null == cmapTable) return null;

        // seek to the table, and then to the appropriate Windows Unicode or Windows Symbol subtable.
        stream.Seek(cmapTable.Offset, SeekOrigin.Begin);
        if (false == SeekToCmapSubtable(stream, cmapTable, 3, true == IsSymbolFont ? 0 : 1)) return null;

        return ReadCmapSubTable(stream);
    }

    /// <summary>
    /// Reads the Character mappings from the cmap sub-table that the specified stream currently points at.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    private Tuple<int, int>[] ReadCmapSubTable(Stream stream)
    {
        var mappings = new List<Tuple<int, int>>();

        // Read the subtable header.
        int format = ReadUShortValue(stream);
        int length = ReadUShortValue(stream);
        int version = ReadUShortValue(stream);
        int segments = ReadUShortValue(stream) / 2;

        // Read past stuff we're not interested in
        stream.Seek(6, SeekOrigin.Current);

        // the segments are in a block that is (4*count)+1 16-byte USHORTs long.
        // the glyph IDs are in a block next to that.
        var segmentsBuffer = new byte[((4 * segments) + 1) * 2];
        stream.Read(segmentsBuffer, 0, segmentsBuffer.Length);
        var glyphIdsBuffer = new byte[length];
        stream.Read(glyphIdsBuffer, 0, glyphIdsBuffer.Length);

        // read the character offsets now
        int endCounts = 0;
        int startCounts = (segments + 1) * 2;
        int idDeltas = startCounts + (segments * 2);
        int idRangeOffsets = idDeltas + (segments * 2);
        for (int segmentEnum = 0; segmentEnum < segments; segmentEnum++)
        {
            int startCharacter = ReadUShortValue(segmentsBuffer, startCounts + (segmentEnum * 2));
            int endCharacter = ReadUShortValue(segmentsBuffer, endCounts + (segmentEnum * 2));
            int idRangeOffset = ReadUShortValue(segmentsBuffer, idRangeOffsets + (segmentEnum * 2));
            int idDelta = ReadUShortValue(segmentsBuffer, idDeltas + (segmentEnum * 2));

            if (endCharacter != 0xffff)
            {
                for (int character = startCharacter; character <= endCharacter; character++)
                {
                    if (0 == idRangeOffset)
                        mappings.Add(new(character, (character + 1 + idDelta) % 0x10000));
                    else
                    {
                        int characterIndex = idRangeOffset + ((character - startCharacter) * 2) - ((segments - segmentEnum) * 2);
                        int glyphId = ReadUShortValue(glyphIdsBuffer, characterIndex) + 1;
                        if (glyphId != 0)
                            if (true == IsSymbolFont)
                                mappings.Add(new(character % 0xF000, (glyphId + idDelta) % 0x10000));
                            else
                                mappings.Add(new(character, (glyphId + idDelta) % 0x10000));
                    }

                }
            }
        }

        return mappings.ToArray();
    }

    /// <summary>
    /// Reads a 32-bits fixed-point from the stream.
    /// </summary>
    /// <param name="stream">The stream</param>
    /// <returns>The fixed-point value</returns>
    private static decimal ReadFixedValue(Stream stream)
    {
        var mantissa = (short)((short)(stream.ReadByte() << 8) + (short)stream.ReadByte());
        var fraction = stream.ReadByte() << 8 | stream.ReadByte();

        return mantissa + (fraction / 65536m);
    }

    /// <summary>
    /// Reads the global header data from the stream and stores it in instance properties.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="tables">The tables in the stream</param>
    private FontHeaders ReadFontHeaders(Stream stream, Table[] tables)
    {
        var headers = new FontHeaders();

        var headTable = FindTable(tables, "head");
        if (null == headTable) return headers;

        // seek to the 'head' table, and load the properties.
        stream.Seek(headTable.Offset, SeekOrigin.Begin);

        ReadFixedValue(stream); // table version (ignore)
        Revision = ReadFixedValue(stream); // revision
        ReadULongValue(stream); // checksum (ignore)
        ReadULongValue(stream); // magic (ignore)
        ReadUShortValue(stream); // flags (ignore)
        headers.UnitsPerEm = ReadUShortValue(stream); // unitsPerEm
        UnitsPerEm = headers.UnitsPerEm;
        DateCreated = ReadLongDateTimeValue(stream); // created
        DateModified = ReadLongDateTimeValue(stream); // modified
        XMin = ReadFWordValue(stream); // xmin
        YMin = ReadFWordValue(stream); // ymin
        XMax = ReadFWordValue(stream); // xmax
        YMax = ReadFWordValue(stream); // ymax
        ReadUShortValue(stream); // macStyle (ignore)
        ReadUShortValue(stream); // smallest pixel size (ignore)
        ReadShortValue(stream); // font direction hint (ignore)
        headers.IndexToLocFormat = ReadShortValue(stream); // indexToLocFormat

        return headers;
    }

    /// <summary>
    /// Reads a FWORD value from the stream.
    /// </summary>
    /// <param name="stream">The stream</param>
    /// <returns>The FWORD value</returns>
    private static int ReadFWordValue(Stream stream) => ReadShortValue(stream);

    /// <summary>
    /// Reads the glypth data from the stream.
    /// </summary>
    /// <param name="offsets">The offsets, in bytes, of each glyph in the file</param>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="tables">The tables in the stream</param>
    /// <param name="metrics">The horizontal metrics for the font.</param>
    /// <returns>The glyph elements</returns>
    private static GlyphInfo[] ReadGlyphData(long[] offsets, Stream stream, Table[] tables, HorizontalMetric[] metrics)
    {
        var glyfTable = FindTable(tables, "glyf");
        if (null == glyfTable) return Array.Empty<GlyphInfo>();

        var glyphs = new List<GlyphInfo>();

        // seek to the 'glyf' table, and read the glyph data.
        stream.Seek(glyfTable.Offset, SeekOrigin.Begin);

        // Read each glyph's bounding box.
        for (int glyphEnum = 0; glyphEnum < Math.Min(offsets.Length - 1, metrics.Length); glyphEnum++)
        {
            var glyph = new GlyphInfo(metrics[glyphEnum]);

            var glyphDataLength = offsets[glyphEnum + 1] - offsets[glyphEnum];
            if (glyphDataLength > 0)
            {
                ReadShortValue(stream); // ignore contours
                glyph.XMin = ReadShortValue(stream);
                glyph.YMin = ReadShortValue(stream);
                glyph.XMax = ReadShortValue(stream);
                glyph.YMax = ReadShortValue(stream);

                // .. skip the rest of the glyph's data
                stream.Seek(glyphDataLength - 10, SeekOrigin.Current);
            }

            // Add the glyph to the list.
            glyphs.Add(glyph);
        }

        return glyphs.ToArray();
    }

    /// <summary>
    /// Reads the glyph data from the stream.
    /// </summary>
    /// <param name="glyphCount">The number of glyphs to read</param>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="tables">The tables in the stream</param>
    /// <param name="headers">The font's headers.</param>
    /// <returns>The glyph offsets</returns>
    private static long[] ReadGlyphOffsets(int glyphCount, Stream stream, Table[] tables, FontHeaders headers)
    {
        var offsets = new long[glyphCount + 1];

        var locaTable = FindTable(tables, "loca");
        if (null == locaTable) return offsets;

        // seek to the 'loca' table to extract the glyph offsets.
        stream.Seek(locaTable.Offset, SeekOrigin.Begin);

        // If we use format #0, we read ushorts, otherwise we read ulongs.
        if (0 == headers.IndexToLocFormat)
        {
            for (int glyphEnum = 0; glyphEnum <= glyphCount; glyphEnum++)
                offsets[glyphEnum] = ReadUShortValue(stream) * 2;
        }
        else
        {
            for (int glyphEnum = 0; glyphEnum <= glyphCount; glyphEnum++)
                offsets[glyphEnum] = ReadULongValue(stream);
        }


        return offsets;
    }

    /// <summary>
    /// Reads the glyph data from the stream.
    /// </summary>
    /// <param name="glyphCount">The number of glyphs to read</param>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="tables">The tables in the stream</param>
    /// <param name="headers">The font's headers</param>
    /// <param name="metrics">The horizontal metrics for the font.</param>
    /// <returns>The glyph elements</returns>
    private static GlyphInfo[]? ReadGlyphs(int glyphCount, Stream stream, Table[] tables, FontHeaders headers, HorizontalMetric[] metrics)
    {
        if (null == metrics) return null;

        // First we read the glyph offsets.
        var offsets = ReadGlyphOffsets(glyphCount, stream, tables, headers);

        // Then, using the offsets we read the glyph data.
        return ReadGlyphData(offsets, stream, tables, metrics);
    }

    /// <summary>
    /// Reads the horizontal headers of this font.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="tables">The tables in the stream</param>
    /// <returns>The number of horizontal metrics.</returns>
    private int ReadHorizontalHeaders(Stream stream, Table[] tables)
    {
        var headTable = FindTable(tables, "hhea");
        if (null == headTable) return 0;

        int numberOfHMetrics;

        // seek to the 'head' table, and load the properties.
        stream.Seek(headTable.Offset, SeekOrigin.Begin);

        ReadFixedValue(stream); // table version (ignore)
        Ascender = ReadFWordValue(stream); // typographic ascender
        Descender = ReadFWordValue(stream); // typographic descender

        // skip the next 13 entries
        stream.Seek(26, SeekOrigin.Current);

        numberOfHMetrics = ReadUShortValue(stream); // number of H-Metrics

        return numberOfHMetrics;
    }

    /// <summary>
    /// Reads the horizontal metrics of this font.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="tables">The tables in the stream</param>
    /// <param name="horizontalMetricsCount">The number of horizontal metrics in the font.</param>
    /// <param name="glyphCount">The number of glyphs in the font.</param>
    /// <returns>The horizontal metrics.</returns>
    private HorizontalMetric[] ReadHorizontalMetrics(Stream stream, Table[] tables, int horizontalMetricsCount, int glyphCount)
    {
        List<HorizontalMetric> metrics = new List<HorizontalMetric>();

        var hmtxTable = FindTable(tables, "hmtx");
        if (null == hmtxTable) return Array.Empty<HorizontalMetric>();

        // seek to the 'hmtx' table, and load the properties.
        stream.Seek(hmtxTable.Offset, SeekOrigin.Begin);

        // Read all the metrics first.
        for (int metricEnum = 0; metricEnum < horizontalMetricsCount; metricEnum++)
        {
            var metric = new HorizontalMetric
            {
                AdvanceWidth = ReadUShortValue(stream),
                LeftSideBearing = ReadFWordValue(stream)
            };

            metrics.Add(metric);
        }

        // If the number of glyphs is larger than the number of metrics, we load the left-side bearings
        // for the remainder of the glyphs.
        if (glyphCount > horizontalMetricsCount)
        {
            HorizontalMetric baseMetric = metrics[horizontalMetricsCount - 1];
            //metrics.Remove(baseMetric);

            for (int remainderEnum = 0; remainderEnum < (glyphCount - horizontalMetricsCount); remainderEnum++)
            {
                var metric = new HorizontalMetric
                {
                    AdvanceWidth = baseMetric.AdvanceWidth,
                    LeftSideBearing = ReadFWordValue(stream)
                };

                metrics.Add(metric);
            }
        }

        return metrics.ToArray();
    }

    /// <summary>
    /// Reads the maximum profile for the font from the stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="tables">The tables in the stream</param>
    /// <returns>The number of glyphs</returns>
    private static int ReadMaximumProfile(Stream stream, Table[] tables)
    {

        var maxpTable = FindTable(tables, "maxp");
        if (null == maxpTable) return 0;

        // seek to the table, and read the properties.
        stream.Seek(maxpTable.Offset, SeekOrigin.Begin);

        /*decimal version = */
        ReadFixedValue(stream);
        return ReadUShortValue(stream);
    }

    /// <summary>
    /// Reads the name properties for this font.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="tables">The tables in the stream</param>
    private void ReadNameData(Stream stream, Table[] tables)
    {
        var nameTable = FindTable(tables, "name");
        if (null == nameTable) return;

        // seek to the 'head' table, and load the properties.
        stream.Seek(nameTable.Offset, SeekOrigin.Begin);

        ReadUShortValue(stream); // ignore this
        var recordCount = ReadUShortValue(stream); // number of name records
        var stringTableOffset = ReadUShortValue(stream) + nameTable.Offset;

        // Read through the records until we find a postscript name.
        // This will be a record with platform ID 3, Encoding ID 1 (0 for symbol fonts) and name ID 6.
        /*int ourEncoding = _isSymbolFont ? 0 : 1;*/
        int recordEnum = 0;
        while (recordEnum < recordCount && (null == PostscriptName || null == FullName))
        {
            // Read the data
            var platformId = ReadUShortValue(stream);
            var encodingId = ReadUShortValue(stream);
            /*int languageId =*/
            ReadUShortValue(stream);
            var nameId = ReadUShortValue(stream);
            var stringLength = ReadUShortValue(stream);
            var stringOffset = ReadUShortValue(stream);

            // If we have found the right record, we read the data.
            if (platformId == 3 && (0 == encodingId || 1 == encodingId))
            {
                if (6 == nameId)
                    PostscriptName = ReadNameString(stream, stringTableOffset + stringOffset, stringLength);

                if (4 == nameId)
                    FullName = ReadNameString(stream, stringTableOffset + stringOffset, stringLength); ;

            }

            recordEnum++;
        }
    }

    /// <summary>
    /// Reads a name string off the specified stream, as indicated by the offset and string length.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="offset">The offset of the string in the stream</param>
    /// <param name="stringLength">The length of the string</param>
    /// <returns>The string</returns>
    private static string ReadNameString(Stream stream, long offset, int stringLength)
    {
        long current = stream.Position;
        stream.Seek(offset, SeekOrigin.Begin);
        byte[] stringBuffer = new byte[stringLength];
        stream.Read(stringBuffer, 0, stringLength);
        stream.Seek(current, SeekOrigin.Begin);

        return System.Text.Encoding.BigEndianUnicode.GetString(stringBuffer);
    }

    /// <summary>
    /// Reads the OS/2 properties for this font.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="tables">The tables in the stream</param>
    private void ReadOs2Data(Stream stream, Table[] tables)
    {
        var os2Table = FindTable(tables, "OS/2");
        if (null == os2Table) return;

        // seek to the 'head' table, and load the properties.
        stream.Seek(os2Table.Offset, SeekOrigin.Begin);

        stream.Seek(68, SeekOrigin.Current); // skip the properties we are not interested in
        Ascender = ReadFWordValue(stream); // typographic ascender
        Descender = ReadFWordValue(stream); // typographic descender            

        // Read the codepage data to see if this is a symbol font or unicode font
        stream.Seek(6, SeekOrigin.Current); // skip the properties we are not interested in
        long codePage = ReadULongValue(stream); // Codepage data
        if ((codePage & 0x80000000) > 0) IsSymbolFont = true;
    }

    /// <summary>
    /// Reads the postscript properties for this font.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="tables">The tables in the stream</param>
    private void ReadPostscriptData(Stream stream, Table[] tables)
    {
        var postTable = FindTable(tables, "post");
        if (null == postTable) return;

        // seek to the 'post' table, and load the properties.
        stream.Seek(postTable.Offset, SeekOrigin.Begin);

        ReadFixedValue(stream); // format type
        ItalicAngle = ReadFixedValue(stream); // Italic angle
    }

    /// <summary>
    /// Reads a SHORT value from the stream.
    /// </summary>
    /// <param name="stream">The stream</param>
    /// <returns>The SHORT value</returns>
    private static int ReadShortValue(Stream stream)
    {
        var value = (short)(stream.ReadByte() << 8);
        value += (short)stream.ReadByte();
        return value;
    }

    /// <summary>
    /// Reads a DateTime value from the stream. This is understood to be a long (8-byte) number that encodes
    /// the number of seconds since 1904.
    /// </summary>
    /// <returns>The datetime value.</returns>
    private static DateTime ReadLongDateTimeValue(Stream stream)
    {
        ulong seconds = 0;
        for (int part = 0; part < 8; part++)
        {
            seconds <<= 8;
            seconds += (ulong)stream.ReadByte();
        }

        var result = new DateTime(1904, 1, 1);
        if (seconds < 10000000000)
            try { result = result.AddSeconds(seconds); }
            catch (ArgumentOutOfRangeException) { }

        return result;
    }

    /// <summary>
    /// Reads a fixed-length string from the supplied stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="length">The length of the stream to read.</param>
    /// <returns>The string value.</returns>
    private static string ReadStringValue(Stream stream, int length)
    {
        var sb = new StringBuilder();
        for (int n = 0; n < length; n++)
            sb.Append((char)stream.ReadByte());

        return sb.ToString();
    }


    /// <summary>
    /// Reads the table directory from the stream and returns the tables.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="tableCount">The number of tables to read</param>
    /// <returns>The tables</returns>
    private static Table[] ReadTableDirectory(Stream stream, int tableCount)
    {
        List<Table> tables = new List<Table>();

        for (int tableEnum = 0; tableEnum < tableCount; tableEnum++)
        {
            var tag = ReadStringValue(stream, 4);
            var checkSum = ReadULongValue(stream);
            var offset = ReadULongValue(stream);
            var length = ReadULongValue(stream);

            tables.Add(new Table(tag, checkSum, offset, length));
        }

        return tables.ToArray();
    }

    /// <summary>
    /// Reads a ULONG value from the stream.
    /// </summary>
    /// <param name="stream">The stream</param>
    /// <returns>The ULONG value</returns>
    private static long ReadULongValue(Stream stream)
    {
        long value = 0;
        value += stream.ReadByte() << 24;
        value += stream.ReadByte() << 16;
        value += stream.ReadByte() << 8;
        value += stream.ReadByte();

        return value;
    }

    /// <summary>
    /// Reads a USHORT value from the stream.
    /// </summary>
    /// <param name="stream">The stream</param>
    /// <returns>The USHORT value</returns>
    private static int ReadUShortValue(Stream stream)
    {
        return stream.ReadByte() << 8 | stream.ReadByte();
    }

    /// <summary>
    /// Reads a USHORT value from a buffer.
    /// </summary>
    /// <param name="buffer">The buffer</param>
    /// <param name="offset">The offset in the buffer to start reading at.</param>
    /// <returns>The USHORT value</returns>
    private static int ReadUShortValue(byte[] buffer, int offset)
    {
        return buffer[offset] << 8 | buffer[offset + 1];
    }

    /// <summary>
    /// Reads the table directory and returns its contents.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <returns>The number of tables in the font file.</returns>
    private static int ReadOffsetTable(Stream stream)
    {
        /*decimal version = */
        ReadFixedValue(stream);
        int numberOfTables = ReadUShortValue(stream);

        // Ignore the following three entries.
        ReadUShortValue(stream);
        ReadUShortValue(stream);
        ReadUShortValue(stream);

        return numberOfTables;
    }

    /// <summary>
    /// Seeks to the sub-table in the 'cmap' table that corresponds to the specified platform ID and
    /// encoding ID.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="cmapTable">The 'cmap' table</param>
    /// <param name="platformId">The platform ID to look for</param>
    /// <param name="encodingId">The encoding ID to look for</param>
    /// <returns>A boolean indicating if the operation was successful (true) or not</returns>
    private bool SeekToCmapSubtable(Stream stream, Table cmapTable, int platformId, int encodingId)
    {
        var isSuccess = false;

        ReadUShortValue(stream); // ignore table version
        int numberOfEncodingTables = ReadUShortValue(stream);

        // We go through each of the mapping tables until we find the one with the desired platform ID 
        // and Encoding ID.
        for (int encodingEnum = 0; encodingEnum < numberOfEncodingTables; encodingEnum++)
        {
            int thisPlatformId = ReadUShortValue(stream);
            int thisEncodingId = ReadUShortValue(stream);
            long subtableOffset = ReadULongValue(stream);

            // We have found the subtable we're looking for.
            if (platformId == thisPlatformId && encodingId == thisEncodingId)
            {
                // Seek to the subtable
                stream.Seek(subtableOffset + cmapTable.Offset, SeekOrigin.Begin);
                isSuccess = true;
                break;
            }
        }

        return isSuccess;
    }

    #endregion
}