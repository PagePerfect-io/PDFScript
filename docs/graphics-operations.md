# Graphics operations

This page describes the graphics operations available in PDFScript. The operations are mostly equal to the operations available in the PDF file format, but with some simplifications and additions.

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

## Clipping paths

## State and Coordinate space

## Placing images

## Basic text operations

## Flowing text
