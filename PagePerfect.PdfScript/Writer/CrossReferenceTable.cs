using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PagePerfect.PdfScript.Tests")]

namespace PagePerfect.PdfScript.Writer;

/// <summary>
/// The CrossReferenceTable maintains a list of objects and their byte offsets
/// within the PDF document.
/// </summary>
public class CrossReferenceTable
{
    // Private fields
    // ==============
    #region Private fields
    private readonly List<CrossReferenceTableEntry> _entries;
    #endregion



    // Instance initialiser
    // ====================
    #region Instance initialiser
    /// <summary>
    /// Initialises a new CrossReferenceTable instance.
    /// </summary>
    public CrossReferenceTable()
    {
        _entries = [];
    }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Adds a new entry to the table.
    /// </summary>
    /// <param name="offset">The byte offset.</param>
    /// <param name="reference">The object reference to add.</param>
    public void Add(long offset, PdfObjectReference reference)
    {
        _entries.Add(new CrossReferenceTableEntry(reference, offset));
    }

    /// <summary>
    /// Does the specified object reference exist in the table?
    /// </summary>
    /// <param name="reference">The object reference.</param>
    /// <returns>True if the reference exists; false otherwise.</returns>
    public bool Exists(PdfObjectReference reference)
    {
        return _entries.Any(e => e.Object.Id == reference.Id && e.Object.Secondary == reference.Secondary);
    }

    /// <summary>
    /// Retrieves the number of entries in the table.
    /// </summary>
    /// <returns></returns>
    public int GetLength() => _entries.Count;

    /// <summary>
    /// Writes the cross reference table to the specified stream writer.
    /// </summary>
    /// <param name="writer">The stream writer.</param>
    public async Task Write(StreamWriter writer)
    {
        await writer.WriteLineAsync("xref");
        await writer.WriteLineAsync($"0 {_entries.Count + 1}");
        await writer.WriteLineAsync("0000000000 65535 f");
        foreach (var entry in _entries.OrderBy(e => e.Object.Id))
        {
            await writer.WriteLineAsync($"{entry.Offset:D10} 00000 n");
        }
    }
    #endregion



    // Private classes
    // ===============
    #region Private classes
    /// <summary>
    /// The CrossReferenceTableEntry class represents an entry in the cross-reference table.
    /// </summary>
    /// <remarks>
    /// Initialises a new CrossReferenceTableEntry instance.
    /// </remarks>
    /// <param name="object">The object reference.</param>
    /// <param name="offset">The byte offset in the output.</param>
    private class CrossReferenceTableEntry(PdfObjectReference @object, long offset)
    {
        // Public properties
        // =================
        #region Public properties
        public long Offset { get; set; } = offset;
        public PdfObjectReference Object { get; set; } = @object;
        #endregion



        // Public methods
        // ==============
        #region Public methods
        /// <summary>
        /// Returns a string representation of the entry.
        /// </summary>
        /// <returns>The string representation</returns>
        public override string ToString()
        {
            return $"{Object.Id} {Object.Secondary} {Offset}";
        }
        #endregion
    }
    #endregion
}
