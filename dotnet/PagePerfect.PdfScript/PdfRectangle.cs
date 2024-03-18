namespace PagePerfect.PdfScript;

/// <summary>
/// The PdfRectangle structure represents a rectangle in PDFScript files as well as PDF documents.
/// </summary>
/// <remarks>
/// PDF documents use a coordinate space with an origin at the bottom left corner of the page. This means that
/// a rectangle's position is defined by its Left and Bottom properties. This is different from coordinate spaces used in
/// computer graphics libraries such as GDI+.
/// </remarks>
/// <remarks>
/// Initialises a new PdfRectangle instance.
/// </remarks>
/// <param name="left">The rectangle's left coordinate.</param>
/// <param name="bottom">The rectangle's bottom coordinate.</param>
/// <param name="width">The rectangle's width.</param>
/// <param name="height">The rectangle's height.</param>
public struct PdfRectangle(double left, double bottom, double width, double height)
{
    // Public properties
    // =================
    #region Public properties
    /// <summary>
    /// Sets or retrieves the rectangle's left coordinate.
    /// </summary>
    public double Left { get; set; } = left;

    /// <summary>
    /// Sets or retrieves the rectangle's bottom coordinate.
    /// </summary>
    public double Bottom { get; set; } = bottom;

    /// <summary>
    /// Sets or retrieves the rectangle's width.
    /// </summary>
    public double Width { get; set; } = width;

    /// <summary>
    /// Sets or retrieves the rectangle's height.
    /// </summary>
    public double Height { get; set; } = height;

    /// <summary>
    /// Returns the right-most coordinate of the rectangle.
    /// </summary>
    public readonly double Right => Left + Width;

    /// <summary>
    /// Returns the top-most coordinate of the rectangle.
    /// </summary>
    public readonly double Top => double.IsNaN(Height) ? Bottom : Bottom + Height;

    /// <summary>
    /// Returns a value that represents an empty object reference.
    /// </summary>
    public static PdfRectangle Empty => new();
    #endregion



    // Public methods
    // ==============
    #region Public methods
    /// <summary>
    /// Creates a rectangle from a bounding box.
    /// </summary>
    /// <param name="left">The left side of the bounding box.</param>
    /// <param name="bottom">The bottom of the bounding box.</param>
    /// <param name="right">The righ side of the bounding box.</param>
    /// <param name="top">The top of the bounding box.</param>
    /// <returns>The rectangle.</returns>
    public static PdfRectangle FromBBox(double left, double bottom, double right, double top)
    {
        return new PdfRectangle(left, bottom, right - left, top - bottom);
    }

    /// <summary>
    /// Creates a bounding rectangle out of a list of rectangles.
    /// This method creates the smallest rectangle that encompasses all
    /// of the rectangles in the specified list. The rectangles do not
    /// need to overlap.
    /// </summary>
    /// <param name="rectangles">The rectangles</param>
    /// <returns>The bounding rectangle.static</returns>
    public static PdfRectangle GetBoundingRectangle(IEnumerable<PdfRectangle> rectangles)
    {
        var left = rectangles.Min(rect => rect.Left);
        var bottom = rectangles.Min(rect => rect.Bottom);
        var top = rectangles.Max(rect => rect.Top);
        var right = rectangles.Max(rect => rect.Right);

        return PdfRectangle.FromBBox(left, bottom, right, top);
    }

    /// <summary>
    /// Does this rectangle intersect with the specified rectangle?
    /// </summary>
    /// <param name="other">The rectangle to test.</param>
    /// <returns>True if the rectangles intersect. False otherwise.</returns>
    public readonly bool Intersects(PdfRectangle other)
    {
        return !(Left > other.Right || Right < other.Left || Bottom > other.Top || Top < other.Bottom);
    }

    /// <summary>
    /// Moves the rectangle to the right, and upwards, by the amount specified.
    /// </summary>
    /// <param name="right">The amount to move the rectangle to the right.</param>
    /// <param name="up">The amount to move the rectangle up.</param>
    public void Move(double right, double up)
    {
        Left += right;
        Bottom += up;
    }

    /// <summary>
    /// Resizes the rectangle to the new width and height.
    /// </summary>
    /// <param name="width">The width</param>
    /// <param name="height">The height</param>
    public void Resize(double width, double height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Expands the rectangle by the specified offsets. The rectangle will expand in either direction
    /// on the axes that expand, in equal parts. So, expanding its width by 10 means that it will move 5 to the
    /// left and increase its width by 10 so that it's right-most coordinate is 5 further than original.
    /// </summary>
    /// <param name="width">The amount to expand the rectangle's width</param>
    /// <param name="height">The amount to expand the rectangle's height</param>
    public void Expand(double width, double height)
    {
        Left -= width / 2;
        Width += width;

        Bottom -= height / 2;
        Height += height;
    }
    #endregion



    // Object overrides
    // ================
    #region Object overrides
    /// <summary>
    /// Returns a hash code for this rectangle.
    /// </summary>
    /// <returns>The hash code</returns>
    public override readonly int GetHashCode()
    {
        return (int)(((uint)Left) ^ (((uint)Bottom) << 13 | ((uint)Bottom) >> 19) ^ (((uint)Width) << 26 | ((uint)Width) >> 6) ^ (((uint)Height) << 7 | ((uint)Height) >> 25));
    }

    /// <summary>
    /// Returns the string representation of this PdfRectangle.
    /// </summary>
    /// <returns>The string representation</returns>
    public override readonly string ToString()
    {
        return $"[{Left} {Bottom} W:{Width} H:{Height}]";
    }

    /// <summary>
    /// Indicates if the supplied instance is equal to the current instance.
    /// Two PdfRectangle instances are considered equal if their Left and Bottom coordinates and their widths
    /// and heights are all equal.
    /// </summary>
    /// <param name="obj">The instance to compare against.</param>
    /// <returns>A boolean that indicates if the two instances are equal.</returns>
    public override readonly bool Equals(object? obj)
    {
        if (null == obj) return false;
        if (obj is not PdfRectangle rect) return false;
        return
            rect.Bottom == Bottom && rect.Left == Left && rect.Width == Width && rect.Height == Height;
    }

    /// <summary>
    /// Indicates if the supplied instances are equal to each other.
    /// Two PdfRectangle instances are considered equal if their Left and Bottom coordinates and their widths
    /// and heights are all equal.
    /// </summary>
    /// <param name="left">An instance to compare against.</param>
    /// <param name="right">An instance to compare against.</param>
    /// <returns>A boolean that indicates if the two instances are equal.</returns>
    public static bool operator ==(PdfRectangle left, PdfRectangle right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Indicates if the supplied instances are not equal to each other.
    /// Two PdfRectangle instances are considered equal if their Left and Bottom coordinates and their widths
    /// and heights are all equal.
    /// </summary>
    /// <param name="left">An instance to compare against.</param>
    /// <param name="right">An instance to compare against.</param>
    /// <returns>A boolean that indicates if the two instances are equal.</returns>
    public static bool operator !=(PdfRectangle left, PdfRectangle right)
    {
        return !left.Equals(right);
    }
    #endregion
}
