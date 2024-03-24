# Graphics operations

This page describes the graphics operations available in PDFScript. The operations are mostly equal to the operations available in the PDF file format, but with some simplifications and additions.

This page covers:

- [Syntax](#syntax)
- [Graphics state](#graphics-state)
- [Coordinate system](#coordinate-system)
- [Creating paths](#creating-paths)
- [Stroking and filling paths](#stroking-and-filling-with-colors)
- [Clipping paths](#clipping-paths)
- [State and Coordinate space](#state-and-coordinate-space)
- [Using colors](#using-colors)
- [Placing images](#placing-images)
- [Basic text operations](#basic-text-operations)
- [Text state](#text-state)
- [Flowing text](#flowing-text)

## Syntax

Graphics operations consists of optionally one or more operands, followed by an operator. The operands are usually numbers or names, but can be arrays and dictionaries. The operator is a keyword.

```
operator
operand operator
operand operand (...) operator
```

### Graphics state

The PDF graphics system is a stateful system. This means
that separate operations set the state, such as the current fill color or the line width. A drawing operation uses the current state to determine how to draw the graphics.

The graphics state also includes the current drawing context, such as 'path' or 'text'. Not all operations are avaialble in all contexts. For example, while constructing a path, instructions to open a text block or set the line width, are not allowed.

### Coordinate system

The coordinate system in PDF puts (0,0) in the bottom-left hand corner. This means that on an A4 page, (0, 842) represents the top-left corner, and Y coordinates decrease as you move down the page.

## Creating paths

To draw shapes, you need to create a path, and then terminate it by stroking or filling it. A path is a sequence of lines, curves and rectangles, and can include sub-paths by moving the current point between operations.

### Moving and drawing lines

- `m` Move to
- `l` Line to
- `c`, `v`, `y` Curve to

#### `m` Move to

A PDF page starts with the current point at (0,0). The `m` operator moves the current point. It starts a path, and the current drawing context is set to 'path'.

```
50 50 m
```

#### `l` Line to

The `l` operator draws a line from the current point to the specified point. The current point is updated to the end of the line.

```
10 10 m
100 100 l
```

#### `c`, `v` and `y` Curve to

The `c`, `v` and `y` operators draw a Bezier curve from the current point to the specified point. The current point is updated to the end of the curve.

`x1 y1 x2 y2 x3 y3 c` draws a curve with two control points, from the current point to `x3 y3`.

`x2 y2 x3 y3 v` draws a curve with an end control point, where the start control point is equal to the current point.

`x1 y1 x3 y3 y` draws a curve with a start control point, where the end control point is equal to the end point of the curve.

### Rectangles and rounded rectangles

- `re` Rectangle
- `rr` Rounded rectangle

#### `re` Rectangle

`x y width height re` operator draws a rectangle from the specified point to a new point by adding the specified width and height. The current point is updated to the end of the rectangle.

A rectangle with 0 width or height will not be visible.

#### `rr` Rounded rectangle

```
10 10 100 100 10 rr
10 10 100 100 15 20 rr
```

`x y width height radius (radius) rr` operator draws a rectangle with rounded corners. If one value for `radius` is specified, the same value will be used for the X and Y radius.

### Closing a path

- `h` Close path

#### `h` Close path

The `h` operator closes the current sub-path by drawing a line from the current point to the start of the path. A new operation will start a new sub-path.

### The path context

A `m`, `re`, or `rr` operation starts a path context. Within a path context, only path drawing or path-terminating operations are allowed.

A path can be abandoned with the `n` operator, or filled/stroked with a number of operators (see Stroking and filling with colors, below).

The following PDFScript content will raise an error, because a text block is opened while the system is in a path context:

```
10 10 m
BT
/F1 12 Tf
(Hello, World!) Tj
ET
```

## Stroking and filling with colors

A path can be stroked or filled, or both, using one of many operations.

- `S` Stroke
- `s` Close and stroke
- `f` Fill
- `f*` Fill with even-odd rule
- `B` Fill and stroke
- `B*` Fill and stroke with even-odd rule
- `b` Close, fill and stroke
- `b*` Close, fill and stroke with even-odd rule

The operators do not take any operands. They use the current graphics state to determine which
color or pattern to use. The example below will stroke a rectangle with a red color, and fill it with a blue color:

```
1 0 0 RG
0 0 1 rg
10 10 100 100 re B
```

## Clipping paths

A path can be used to clip the drawing area. This means that any subsequent content will only be drawn within the path. This clipping area will be in effect for the duration of the page, or until the graphics state is restored (see 'State and Coordinate space', below).

- `W` Set the clip area
- `W*` Set the clip area, with even-odd rule

The `W` and `W*` operators do not take any operands. They use the current path to set the clipping area.

## State and Coordinate space

Initially the coordinate space has its origin at (0,0), with a 1:1 scale where each unit is an Adobe Point (1/72nd of an inch). PDF uses a single operation to update the current coordinate space, the `cm` operator.

The current state, including graphics state such as the current fill color, line width etc, and the current coordinate space, can be saved and restored using the `q` and `Q` operators.

- `cm` Update the current transformation matrix
- `q` Save the current state
- `Q` Restore the current state

### `cm` Update the current transformation matrix

`m1 m2 m3 m4 m5 m6 cm' updates the current transformation matrix by performing a matrix multiplication. It can be used to scale, rotate, translate or shear the current coordinate space. Each subsequent operation will be performed in the new coordinate space.

There is no operation to restore the coordinate space to the initial state. Insteadm use the `q` and `Q` operators to sage ane restore the graphics state.

#### Scaling

To scale the coordinate space, use the following matrix:

```
sx 0 0 sy 0 0 cm
```

Where `sx` and `sy` are the scaling factors in the X and Y directions.

#### Rotation

To rotate the coordinate space, use the following matrix:

```
<cos(a)> <sin(a)> <-sin(a)> <cos(a)> 0 0 cm
```

Where `a` is the angle in radians.

#### Translation

To translate the coordinate space, use the following matrix:

```
1 0 0 1 tx ty cm
```

Where `tx` and `ty` are the translation factors in the X and Y directions.

#### Order of operations

The order of operations is important. For example, the two examples below yield different results:

Scale by a factor of two, and the move (200, 200) in the new coordinate space, effectively moving the origin to (400, 400):

```
2 0 0 2 0 0 cm
1 0 0 1 200 200 cm
```

Move (200, 200) in the original coordinate space, and then scale by a factor of two:

```
1 0 0 1 200 200 cm
2 0 0 2 0 0 cm
```

#### Arbitrary transformations

Any combination of scaling, rotation and translation can be achieved by combining the above examples.

### `q` Save the current state

The `q` operator saves the current state, including the current transformation matrix and graphics state. The state can be restored using the `Q` operator. The `q` operator does not take any operands.

### `Q` Restore the current state

The `Q` operator restores the current state to the last saved state. The `Q` operator does not take any operands.
If a `Q` operator is used without a corresponding `q` operator, PDFScript will raise an error.

In the example below, the coordinate space is scaled and translated to draw 24pt text at (72, 200), and then restored afterwards:

```
q
2 0 0 2 72 200 cm
BT
/Helvetica 12 Tf
(Hello, World!) Tj
ET
Q
```

## Using colors

Colors in PDF are represented by operations that set the color space, and operations that set a stroking or non-stroking (filling) color. We will cover the most common operations in this section.

- `rg` and `RG` Set the filling and stroking color [RGB]
- `k` and `K` Set the filling and stroking color [CMYK]
- `g` and `G` Set the filling and stroking color [Gray]
- `scn` and `SCN` Set the filling and stroking color [Pattern]

We will also cover the `sc`, `SC`, `cs` and `CS` operations, which are also supported in PDFScript but effectively long-hand versions of the common operations.

PDFScript currently does not support creation of custom color spaces, such as Lab or ICCBased, and there is no plan to support these in the future.

### `rg` and `RG` Set the filling and stroking color [RGB]

The `rg` operator set the filling color to an RGB color, and also sets the current color space for filling to `/DeviceRGB`. The `RG` operator does the same for stroking.

```
<r> <g> <b> rg
<r> <g> <b> RG
```

Where `r`, `g` and `b` are numbers between 0 (off) and 1 (full).

To set the fill color to black, use `0 0 0 rg`. To set the stroke color to white, use `1 1 1 RG`.

### `k` and `K` Set the filling and stroking color [CMYK]

The `k` operator set the filling color to a CMYK color, and also sets the current color space for filling to `/DeviceCMYK`. The `K` operator does the same for stroking.

```
<c> <m> <y> <k> k
<c> <m> <y> <k> K
```

Where `c`, `m`, `y` and `k` are numbers between 0 (off) and 1 (full).

### `g` and `G` Set the filling and stroking color [Gray]

The `g` operator set the filling color to a gray color, and also sets the current color space for filling to `/DeviceGray`. The `G` operator does the same for stroking.

```
<gray> g
<gray> G
```

Where `gray` is a number between 0 (black) and 1 (white).

### `scn` and `SCN` Set the filling and stroking color [Pattern]

There are multiple overloads for the `scn` operation, with various operands. We will focus on the `<pattern> scn` overload, which is a managed version of the same operator in the PDF specification.

```
<pattern> scn
<pattern> SCN
```

Where `<pattern>` is a pattern object defined by the `# pattern` prologue statement, such as a linear gradient or radial gradient.

For example, to fill a rectangle with a linear gradient, use the following operations:

```
# pattern /BlueToRed /LinearGradient /DeviceRGB <<
    /Rect [10 10 200 150]
    /C0 [1.0 0.2 0.8]
    /C2 [0.7 0.2 1.0]
    /Stops [0 1]
>>
/BlueToRed scn
10 10 200 150 10 10 rr f
```

#### Other overloads of `scn` and `SCN`

```
<number> scn|SCN
<number> <number> <number> scn|SCN
<number> <number> <number> <number> scn|SCN
<number> <name> scn|SCN
<number> <number> <number> <name> scn|SCN
<number> <number> <number> <number> <name> scn|SCN
```

When PDFScript encounters these overloads, they will be passed on as-is to the PDF content. Is is up to the user to ensure that a correct color space is set before using these overloads.

#### The `sc` and `SC` operators

The `sc` and `SC` operators are earlier versions of the `scn` and `SCN` operators. They set a stroking or filling color in the current color space, which must be set by a previous operation such as `cs`, `CS`, `rg`, `RG`, `k`, `K`, `g`, or `G`.

They are effectively long-hand versions of the `rg`, `RG`, `k`, `K`, `g`, or `G` operations:

```
/DeviceRGB cs
1 0 0 sc
```

is equivalent to:

```
1 0 0 rg
```

#### The `cs` and `CS` operators

The `cs` and `CS` operators set the current color space for stroking or filling. The color space must be one of:

- `/DeviceRGB` sets the RGB color space
- `/DeviceCMYK` sets the CMYK color space
- `/DeviceGray` sets the Gray color space
- `/Pattern` sets the Pattern color space

Setting a color space with `cs` or `CS` allows using the `sc` and `SC` operators to set the color in the current color space. (See the previous section for more information on `sc` and `SC`.)

## Placing images

Images can be placed on a page using the `Do` operator. The image must be defined in the prologue using the `# resource` statement.

- `Do` Place an image

### `Do` Place an image

The `Do` operator places an image on the page. The image must be defined in the prologue using the `# resource` statement.

```
<name> Do
```

Where `<name>` is the name of the image resource.

Before placing an image, the current transformation matrix must be set to the correct position and scale. To place an image at (50, 50) and a 1in x 1in size:

```
q
72 0 0 72 50 50 cm
/Img1 Do
Q
```

Note that we use `q` and `Q` to save and restore the graphics state, to ensure that the transformation matrix is not applied to subsequent content.

The image size (72pt x 72pt) is independent of the image resolution. The image will be scaled to fit the specified size.

#### Other uses for `Do`

In a PDF document, the `Do` operation can also be used to place a 'Form', which is a reusable group of graphics operations (think of it as a component). Currently, Forms are not supported by PDFScript.

## Basic text operations

PDF support basic text operations that place text on a single line. All text positioning and text placement operations take place within a text object.

- `BT` and `ET` Begin/end a text object
- `Tf` Set the font and font size
- `Tm` Set the text matrix
- `Td` Move to the start of the next line
- `Tj` Show a text string
- Other operators

### `BT` and `ET` Begin/end a text object

Text positioning and text placement operations must be enclosed within a text object. The `BT` operator starts a text object, and the `ET` operator ends it.

Text objects cannot be nested.

```
BT
ET
```

The `BT` and `ET` operators do not take any operands.

### `Tf` Set the font and font size

The `Tf` operator sets the font and font size for the text object. The font must be defined in the prologue using the `# resource` statement.

```
<font> <size> Tf
```

Where `<font>` is the name of the font resource, and `<size>` is the font size in points.

#### Standard fonts

The `Tf` operation is slightly different from the PDF specification in its support for standard fonts. PDF supports a number of fonts out of the box, which can be used instead of the name of a font resource declared in a prologue statement.

For example, to use the Helvetica font at 12pt:

```
/Helvetica 12 Tf
```

The full list of standard fonts is:

- `/Courier`
- `/CourierBold`
- `/CourierOblique`
- `/CourierBoldOblique`
- `/Helvetica`
- `/HelveticaBold`
- `/HelveticaOblique`
- `/HelveticaBoldOblique`
- `/TimesRoman`
- `/TimesBold`
- `/TimesItalic`
- `/TimesBoldItalic`

### `Tm` Set the text matrix

Text is not placed at the current point (see 'Creating paths', above). Instead, a text matrix is used to position text. The initial value of the text matrix is the identity matrix, which means text is placed at (0,0).

The `Tm` operator sets the text matrix. The text matrix is a six-element matrix that determines the position and orientation of text.

```
<m1> <m2> <m3> <m4> <m5> <m6> Tm
```

Where `m1` to `m6` are numbers that define the text matrix.

To draw text at (72, 300), use the following text matrix:

```
BT
1 0 0 1 72 300 Tm
/Helvetica 12 Tf
(Hello, World!) Tj
ET
```

**Note** that the text matrix is reset to the identity matrix at the end of the text object.

**Note** Each `tm` operation will overwrite the previous text matrix. To move the text position, use the `Td` operation.

### `Td` Move to the start of the next line

The `Td` operation moves text to a new line, although it can effectively be used to move text in any direction.

```
<tx> <ty> Td
```

Where `tx` and `ty` are numbers that define the translation in the X and Y directions. Because the PDF coordinate system has (0,0) in the bottom-left corner, a negative `ty` value will move text down the page.

```
BT
1 0 0 1 72 300 Tm
/Helvetica 12 Tf
(The quick brown fox ) Tj
0 -12 Td
(jumps over the lazy dog.) Tj
ET
```

The `Td` operation also sets the leading (line height) state parameter. The leading is the distance between the baselines of two lines of text.

This means that after the first `Td` operation, the `T*` shortcut can be used to move the text down by the leading value.

```
BT
1 0 0 1 72 300 Tm
/Helvetica 12 Tf
(The quick brown fox ) Tj
0 -12 Td
(brown fox ) Tj
T*
(jumps over ) Tj
T*
(the lazy dog.) Tj
ET
```

Note that the text leading state paremeter can be set using the `TL` operation. See `Text state`, below.

## Text state

The appearance of text is controlled by the text state, which includes the current font, font size, leading, and other parameters.
_ `TL` Set the text leading
_ `Tc` Set the character spacing
_ `Tw` Set the word spacing
_ `Tz` Set the horizontal scaling \* `Ts` Set the text rise

### `TL` Set the text leading

The `TL` operator sets the text leading, which is the distance between the baselines of two lines of text.

```
<leading> TL
```

Where `<leading>` is a number that defines the text leading.

For a text with a font size of 12pt, a leading of 12pt will result in single-spaced text. A leading of 24pt will result in double-spaced text.

### `Tc` Set the character spacing

The `Tc` operator sets the character spacing, which is the additional space between characters. The default value is 0.

```
<char-spacing> Tc
```

Where `<char-spacing>` is a number that defines the character spacing.

### `Tw` Set the word spacing

The `Tw` operator sets the word spacing, which is the additional space between words. The default value is 0.

```
<word-spacing> Tw
```

Where `<word-spacing>` is a number that defines the word spacing.

### `Tz` Set the horizontal scaling

The `Tz` operator sets the horizontal scaling, which is the scaling factor to apply to text.

```
<horizontal-scaling> Tz
```

Where `<horizontal-scaling>` is a number that defines the horizontal scaling. The default value is 100, which means no scaling. 200 would double the width of the text, and 50 would halve it.

### `Ts` Set the text rise

The `Ts` operator sets the text rise, which is the distance to move text up or down from its default position.

```
<text-rise> Ts
```

Where `<text-rise>` is a number that defines the text rise. A positive value moves text up, and a negative value moves text down.

## Flowing text

PDF does not support flowing text. Text is placed on a single line, and it is up to the creator to figure out how to flow text across multiple lines.

PDFScript adds support for text-flowing.

- `Tfl` Flow text
- `Tb` Set the text box
- `Ta` Set the horizontal text alignment
- `TA` Set the vertical text alignment

### `Tfl` Flow text

The `Tfl` operation is a text-flow macro. It uses a 'text box' containing rectangle and some state parameters, and flows text within the container.

```

<text> Tfl
```

Where `<text>` is a string of text to flow within the text box.

For example, to flow and center a piece of text inside a box with a width of 300pt and a height of 200pt, use the following operations:

```
BT
/TimesRoman 24 Tf
1 Ta
1 TA
300 200 Tb
(The quick brown fox jumps over the lazy dog) Tfl
ET
```

The `Ta` and `TA` operations set the alignment of the text. The `Tb` operation sets the text box. You can use the special value `/Auto` for the width or height of the text box if you do not want a fixed width or height.

### `Tb` Set the text box

The `Tb` operation sets the text box. The text box is a rectangle that contains the text to flow. The box can have fixed or auto width and height.

```
<width> <height> Tb
```

Where `<width>` and `<height>` are numbers that define the width and height of the text box. The special value `/Auto` can be used for the width or height if you do not want a fixed width or height.

To flow a text on a single line, but center it vertically:

```
1 TA
/Auto 200 Tb
(The quick brown fox jumps over the lazy dog) Tfl
```

To flow a text along a fixed width of 300pt, but without bounding it to a fixed height:

```
300 /Auto Tb
(The quick brown fox jumps over the lazy dog) Tfl
```

### `Ta` Set the horizontal text alignment

The `Ta` operation sets the horizontal text alignment within the text box. The default value is 0, which means left-aligned text.

```
<alignment> Ta
```

Where `<alignment>` is a number that defines the horizontal text alignment. The values are:

- 0 - Left
- 1 - Center
- 2 - Right
- 4 - Justify, with last line left-aligned
- 5 - Justify, with last line center-aligned
- 6 - Justify, with last line right-aligned

**Note** The horizontal alignment has no effect on a `Tfl` operation if the text box width is set to `/Auto`.

### `TA` Set the vertical text alignment

The `TA` operation sets the vertical text alignment within the text box. The default value is 0, which means top-aligned text.

```
<alignment> TA
```

Where `<alignment>` is a number that defines the vertical text alignment. The values are:

- 0 - Top
- 1 - Center
- 2 - Bottom

**Note** The vertical alignment has no effect on a `Tfl` operation if the text box height is set to `/Auto`.

# Further reading

- [Syntax and structure](syntax-and-structure.md)
