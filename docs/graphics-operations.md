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

## Placing images

## Basic text operations

## Flowing text

# Further reading

- [Syntax and structure](docs/syntax-and-structure.md)
