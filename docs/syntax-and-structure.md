# Syntax and File Structure

This page describes the syntax of instructions in a `.pdfs` file, and explains the file structure.

PDFScript is based on the content streams inside a PDF document - the graphics instructions that place content on pages. The syntax of PDFScript is mostly identical to
the syntax of PDF graphics instructions, with some improvements.

Users familiar with PDF content streams will recognise the operators and basic data types.

This page covers:

- [Syntax](#syntax)
- [File structure](#file-structure)
  - [Prologue statements](#prologue-statements)
    - [Resource declarations](#resource-declarations)
    - [Variable declarations](#variable-declarations)
    - [Pattern declarations](#pattern-declarations)
    - [Color declarations](#color-declarations)
  - [Content](#content)
    - [Page structure](#page-structure)
    - [Specify page sizes](#specify-page-sizes)

## Syntax

### Graphics operations

PDFScript is a series of graphics operations with operands in a stack-based language, written in a UTF-8 text file. The format of an operation is in postfix notation:

```
operand1 operand2 operand3 operator
```

For example, to draw a line from (10, 10) to (100, 100):

```
10 10 m 100 100 l S
```

### Data types

PDFScript uses a subset of the data types used in PDF files, plus an additional data type for variables.

- **Numbers**: numbers are single-precision floating point numbers, such as `10` or `3.14`. Exponential notation is not supported.
- **Strings**: strings are enclosed in parentheses, such as `(Hello, World!)`. A PDFScript string is a series of UTF-8 encoded characters.
- **Names**: names are written with a leading slash, such as `/DeviceRGB`. Names are used to identify resources in a PDF file, and refer to enumerated values.
- **Arrays**: arrays are enclosed in square brackets, such as `[10 20 30]`. Arrays are used to group operands together. An array can contain any data type.
- **Dictionaries**: dictionaries are enclosed in double angle brackets, like this: `<< /Type /Catalog >>`. Dictionary keys are names. Values can by any data type.
- **Booleans**: the boolean values are `true` and `false`.
- **Keywords**: keywords are string values that are primarily used as operators, such as `BT` and `ET`.
- **Variables** are used to represent one of the above values. A variable starts with a `$`, for example `$myWidth`.

#### Numbers

All numbers are stored as single-precision floating point numbers. They can be written as integers or decimals. Leading zeroes are allowed. Exponential notation is not supported.

The following are valid numbers:

```
10 3.14 0.5 .6 -20 -0.5 -.3
```

Numbers can be separated by whitespace, but this is not required. Any non-numeric token will terminate a number.

`10-100.5.5` is equal to `10 -100.5 0.5`.

#### Strings

A string is one or more UTF-8 characters, enclosed in parentheses, for example `(Hello, World!)`.

Characters such as `(`, `)`, and `\` must be escaped with a backslash: `(This is a \( character)`.

Any character can be escaped with an octal code: `(\101)` is equal to `(A)`.

Currently, PDFScript supports ASCII characters and Latin-1 (ISO-8859-1) characters, so `£` and `€` are valid characters.

#### Names

A name is a word that starts with a `/` character. Names can include alpha-numeric characters, but cannot start with a number. Names are used to identify named resources (see 'resources', below) and enumerated values defined in the PDF specification.

An example of an enumerated value is `/DeviceRGB` (from the list of color spaces).

#### Arrays

A PDF array is a series of values, enclosed in square brackets and separated by whitespace. An array's items cannot be separated by commas.

The values in an array can be of any type, and can be of mixed types. For example, `[10 20 30 /DeviceRGB (Hello, World!)]` is a valid array.

In PDFScript, arrays are used almost exclusively in prologue statements. Only one PDF graphics operation uses an array as an operand. (The `d` operation sets a dash pattern)

#### Dictionaries

A PDF dictionary is a set of name-value pairs, enclosed in double angle (`<<`, `>>`) brackets. The keys are names, and the values can be any data type. The keys and values are separated by whitespace. The values can be of any type, and types can be mixed.

For example, the below dictionary can be used in the definition of a linear gradient pattern:

```
<<
    /Rect [0 0 595 842]
    /C0 [1 0 0]
    /C1 [0.7 0 1]
    /Stops [0 1]
>>
```

#### Booleans

Booleans are special reserved keywords: `true` and `false`. They are used to represent logical values.

#### Keywords

Keywords are reserved words that are used as operators in PDFScript. They are used to perform operations on the graphics state, such as `BT`, `m`, or `Tfl`. Keywords are case-sensitive.

#### Variables

Variables start with a `$` character, followed by a name that consists of alpha-numeric characters. Variable names cannot start with a number. Variables can store any data type, and can be used in place of a value in an operation. See 'Variable declarations' below.

### Whitespace

PDFScript treats all whitespace equally. A sequence of one or more whitespace characters is treated as a single whitespace token. There is no difference between:

```
10 10 m 100 100 l S
```

and

```
10 10 m
100 100 l
S
```

## File structure

A PDFScript file consists of two parts: an optional prologue, and a series of graphics operations that defines the content of the PDF file.

Prologue statements cannot appear once content has started.

The below is a minimal, valid PDFScript file:

```
BT /Helvetica 12 Tf 72 200 Td (Hello, World!) Tj ET
```

An example of a file with a prologue:

```
# resource /MyImage /Image (./images/frontpage.jpg)
595 0 0 842 0 0 cm /MyImage Do
```

### Prologue statements

A prologue statement (sometimes referred to as a 'prolog' statement) is a `#` character followed by a keyword that defines the type of the statement, and one or more operands that are specific to the prologue statement.

#### Resource declarations

Resource declarations define named resources that are used in the content of the PDF file.

```
# resource [Name] [Type] [Location]
```

The name of the resource must be a 'name' value, such as `/Image1`. The name must be unique. If another resource exists with the same name, PDFScript will raise an error.

The name must not be a reserved name, such as `/Type`.

The type of the resource is a 'name' value. Currently the following are supported:

- `/Image` The resource is an image. Currently, only JPEG images are supported.
- `/Font` The resource is a font. Currently, only TrueType fonts (`.ttf`) are supported.

The location of the resource is a string value that locates the resource. The location can be a URL or a local file system path. The following are valid locations:

- `(./images/frontpage.jpg)` A local file system path.
- `(https://coolimage.com)` A URL.

URLs must be absolute URLs, and include the schema (`http` or `https`).

#### Variable declarations

A variable declaration defines a variable using a name, type, and value. The value can be used instead of a constant expression in a graphics operation.

```
# var [Name] [Type] [Value]
```

The name of the variable must be a 'variable' value, such as `$myWidth`. The name must be unique. If another variable exists with the same name, PDFScript will raise an error.

The type must be a 'name' value, and the following are supported:

- `/Number` The variable is a number.
- `/String` The variable is a string.
- `/Name` The variable is a name.
- `/Boolean` The variable is a boolean.

Currently, dictionaries and arrays are not supported as variable types.

The value of the variable must match the type. For example, if the type is `/Number`, the value must be a number.

A variable can be used in content:

```
# var $myWidth /Number 144
# var $myHeight /Number 144

72 72 $myWidth $myHeight re f
```

#### Pattern declarations

A pattern declaration defines a named fill pattern for graphics operations.

```
# pattern [Name] [Type] [Color space] [Value]
```

The name of the pattern must be a 'name' value, such as `/MyPattern`. The name must be unique. If another resource exists with the same name, PDFScript will raise an error.

The type of the pattern is a 'name' value. Currently the following are supported:

- `/LinearGradient` The pattern is a linear gradient.
- `/RadialGradient` The pattern is a radial gradient.

The color space of the pattern is a 'name' value, and must be one of the below:

- `/DeviceRGB` The color space is RGB.
- `/DeviceCMYK` The color space is CMYK.
- `/DeviceGray` The color space is grayscale.

The value of the pattern is a dictionary that defines the pattern. The dictionary must contain the following keys:

- `/Rect` An array that defines the rectangle in which the pattern is painted.
- `/C0` The start color of the gradient.
- `/C1` The end color of the gradient.
- `/Stops` An array of numbers that define the gradient stops, which are numbers between 0 and 1.

An example of a linear gradient pattern:

```
# pattern /GreenYellow /LinearGradient /DeviceRGB <<
    /Rect [0 0 595 842]
    /C0 [0 0.7 0]
    /C1 [1 1 0]
    /Stops [0 1]
>>
```

To use a gradient pattern, use the `scn` or `SCN` operators:

```
/GreenYellow scn
0 0 595 842 0 0 re f
```

#### Color declarations

A color declaration defines a named resource that is a color in a specific color space.

```
# color [Name] [Color space] [Component]( [Component]...)
```

The name of the color must be a 'name' value, such as `/Orange`. The name must be unique. If another resource exists with the same name, PDFScript will raise an error.

The color space of the color is a 'name' value, and must be one of the below:

- `/DeviceRGB` The color space is RGB.
- `/DeviceCMYK` The color space is CMYK.
- `/DeviceGray` The color space is grayscale.

The components is a series of numbers that define the color. The number of components must match the color space:

- For `/DeviceRGB`, the components are three numbers [0..1] that define the red, green, and blue components.
- For `/DeviceCMYK`, the components are four numbers [0..1] that define the cyan, magenta, yellow, and black components.
- For `/DeviceGray`, the components is a single number [0..1] that defines the gray level.

An example of color declarations:

```
# color /Orange /DeviceRGB 1 0.5 0
# color /Print /DeviceCMYK 0.1 0.3 0.7 1
# color /LightGray /DeviceGray 0.8
```

### Content

The content of a PDFScript file is a series of graphics operations that define the content of the PDF file, and also control the document structure (i.e. they define the pages).

A minimal PDFScript file is a series of graphics operations:

```
50 50 200 150 re f
```

For more information, see the 'Graphics operations' page.

#### Page structure

The first page in a PDFScript file is implicitly defined. There is no need to 'open' a page. There is no need to terminate a page either, unless you need to start a new page.

To terminate the current page, and start a new one, use the `endpage` keyword and add more content:

```
50 50 200 150 re f
endpage
50 50 200 150 re f
```

#### Specify page sizes

By default, the page size is set to the DIN A4 standard (595 x 842 points). To change the page size, use the `page` keyword and specify a standard page, or a width and height.

The `page` keyword does not start a new page. It only sets the page size for any subsequent pages. Therefore, to change the page size of the first page, use the `page` keyword before any content.

```
/A3 page
BT /Helvetica 12 Tf 72 200 Td (Hello from page 1) Tj ET
endpage
300 500 page
BT /Helvetica 12 Tf 72 200 Td (Hello from page 2) Tj ET
```

The following names are valid page templates:

- `/A0` to `/A6` - the corresponding DIN A0 to A6 sizes.
- `/Letter` - the US Letter size (612 x 792 points).
- `/Legal` - the US Legal size (612 x 1008 points).
- `/Tabloid` - the US Tabloid size (792 x 1224 points).

To specify a custom page size, use the width and height in points:

```
595 842 page
```

# Further reading

- [Graphics operations](graphics-operations.md)
