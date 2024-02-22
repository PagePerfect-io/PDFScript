namespace PagePerfect.PdfScript.Writer.Resources.Metrics;

/// <summary>
/// The AfmToken represents a single token read by the AfmReader class.
/// </summary>
internal class AfmToken
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The token type.
    /// </summary>
    public AfmTokenType Type { get; }

    /// <summary>
    /// The token's string value.
    /// </summary>
    public string? Value { get; }

    /// <summary>
    /// The token's numeric value.
    /// </summary>
    public double NumericValue { get; }
    #endregion



    // Instance initialiser
    // ====================
    #region Instance initialiser
    /// <summary>
    /// Initialses a new token with the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    public AfmToken(AfmTokenType type)
    {
        Value = null;
        Type = type;
    }

    /// <summary>
    /// Initialses a new token with the specified value and type.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="type">The type.</param>
    public AfmToken(string value, AfmTokenType type)
    {
        Value = value;
        Type = type;
    }

    /// <summary>
    /// Initialises a new token with a numeric value.
    /// </summary>
    /// <param name="value">The numeric value.</param>
    public AfmToken(double value)
    {
        Value = null;
        NumericValue = value;
        Type = AfmTokenType.Number;
    }
    #endregion
}
