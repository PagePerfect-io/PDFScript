namespace PagePerfect.PdfScript.Utilities;

/// <summary>
/// The EnumExtensions method contains extension methods for the Enum type.
/// </summary>
public static class EnumExtensions
{
    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Retrieves the attribute for an enumerated value.
    /// This method finds the attribute of the specified type, as it is applied to the
    /// specified enumerated value, and returns it.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    /// <param name="value">The enumerated value.</param>
    /// <returns>The attribute instance.</returns>
    public static T? GetAttribute<T>(this Enum value) where T : Attribute
    {
        var type = value.GetType();
        var field = type.GetField(value.ToString());
        if (null == field) throw new ArgumentException($"The specified value '{value}' is not a valid value for the {type.Name} enumeration.");

        var attr = Attribute.GetCustomAttribute(field, typeof(T), false);
        return null == attr ? null : (T)attr;
    }
    #endregion
}