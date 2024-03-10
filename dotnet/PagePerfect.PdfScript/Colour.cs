namespace PagePerfect.PdfScript;

/// <summary>
/// The Colour class rpresents a colour in a specific colour space.
/// </summary>
public class Colour(ColourSpace cs, params float[] components)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// The colour space for the colour.
    /// </summary>
    public ColourSpace ColourSpace { get; } = cs;

    /// <summary>
    /// The components of the colour.
    /// </summary>
    public float[] Components { get; } = components;
    #endregion



    // Base class overrides
    // ====================
    #region Object overrides
    /// <summary>
    /// Determines if this instance is equal to another instance.
    /// The two instances are equal if they are both Colour isntances,
    /// and the colour space and components are equal.
    /// </summary>
    /// <param name="obj">The other instance.</param>
    /// <returns>True if the instances are equal; false otherwise.</returns>
    public override bool Equals(object? obj)
    {
        if (false == obj is Colour other) return false;

        if (other.ColourSpace != ColourSpace || other.Components.Length != Components.Length) return false;

        for (int i = 0; i < Components.Length; i++)
        {
            if (other.Components[i] != Components[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(ColourSpace, Components);
    }

    /// <summary>
    /// Retrieves a string representation of this instance.
    /// </summary>
    /// <returns>The string.</returns>
    override public string ToString()
    {
        return $"{ColourSpace} ({string.Join(", ", Components)})";
    }
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Creates a new RGB colour.
    /// </summary>
    /// <param name="red">The red component.</param>
    /// <param name="green">The green component.</param>
    /// <param name="blue">The blue component.</param>
    /// <returns>The Colour instance.</returns>
    public static Colour RGB(float red, float green, float blue)
    {
        return new Colour(ColourSpace.DeviceRGB, red, green, blue);
    }

    /// <summary>
    /// Creates a new CMYK colour.
    /// </summary>
    /// <param name="cyan">The cyan component.</param>
    /// <param name="magenta">The magenta component.</param>
    /// <param name="yellow">The yellow component.</param>
    /// <param name="black">The black/key component.</param>
    /// <returns>The Colour instance.</returns>
    public static Colour CMYK(float cyan, float magenta, float yellow, float black)
    {
        return new Colour(ColourSpace.DeviceCMYK, cyan, magenta, yellow, black);
    }

    /// <summary>
    /// Creates a new colour in the DeviceGray colour space.
    /// </summary>
    /// <param name="gray">The shade of gray.</param>
    /// <returns>The Colour instance.</returns>
    public static Colour Gray(float gray)
    {
        return new Colour(ColourSpace.DeviceGray, gray);
    }
    #endregion
}
