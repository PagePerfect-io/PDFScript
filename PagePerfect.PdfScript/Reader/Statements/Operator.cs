namespace PagePerfect.PdfScript.Reader.Statements;

/// <summary>
/// Then Operator enumeration lists the possible operators for a graphics operation.
/// </summary>
public enum Operator
{
    // Special graphics state
    // ======================
    /// The q operator (PDF1-7, p.219) - Saves the current graphics state                
    q,

    /// The Q operator (PDF1-7, p.219) - Restores the graphics state.
    Q,

    /// The cm operator (PDF1-7, p.219) - Modifies the current
    /// transformation matrix.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    cm,



    // General graphics instructions
    // =============================
    /// The w operator - PDF1-7, p. 196 - set the line width.
    [GraphicsOperation(PdfsValueKind.Number)]
    w,

    /// The J operator - PDF1-7, p. 196 - set the line cap style.
    [GraphicsOperation(PdfsValueKind.Number)]
    J,

    /// The j operator - PDF1-7, p. 196 - set the line join style.
    [GraphicsOperation(PdfsValueKind.Number)]
    j,

    /// The M operator - PDF1-7, p. 196 - set the miter limit.
    [GraphicsOperation(PdfsValueKind.Number)]
    M,

    /// The d operator - PDF1-7, p. 196 - set the line dash pattern.
    [GraphicsOperation(PdfsValueKind.Array, PdfsValueKind.Number)]
    d,

    /// The ri operator - PDF1-7, p. 196 - set the rendering intent.
    [GraphicsOperation(PdfsValueKind.Name)]
    ri,

    /// The i operator - PDF1-7, p. 196 - set the flatness tolerance.
    [GraphicsOperation(PdfsValueKind.Number)]
    i,

    /// The gs operator (PDF1-7, p.219) - Injects the graphics state
    /// located by the resource indicated in the operand.
    [GraphicsOperation(PdfsValueKind.Name)]
    gs,



    // Path instructions
    // =================
    /// The m operator (PDF1-7, p.226) - starts a new path
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number)]
    m,
    /// The l operator (PDF1-7, p.226) - appends a line
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number)]
    l,
    /// The c operator (PDF1-7, p.226) - appends a bezier curve
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    c,

    /// The v operator (PDF1-7, p.226) - appends a bezier curve
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    v,
    /// The y operator (PDF1-7, p.226) - appends a bezier curve
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    y,
    /// The h operator (PDF1-7, p.226) - closes a path
    h,
    /// The re operator (PDF1-7, p.227) - adds a rectangle
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    re,


    // Path painting instructions
    // ==========================
    /// The S and s operators (PDF1-7, p230) - strokes a path
    s, S,

    /// The f, F, and f* operators (PDF1-7, p.230) - fills a path
    f,
    F,
    [GraphicsOperation(Operator = "f*")]
    fStar,

    /// The B and B* operators (PDF1-7, p.230) - fill and stroke a path
    B,
    [GraphicsOperation(Operator = "B*")]
    BStar,
    b,
    [GraphicsOperation(Operator = "b*")]
    bStar,

    /// The n operator (PDF1-7, p.230) - does nothing
    n,



    // Clipping path instructions
    // ==========================
    /// The W and W* operators (PDF1-7, p.235) - modifies the clipping path.
    W,
    [GraphicsOperation(Operator = "W*")]
    WStar,



    // Text object, state, positioning and movement instructions
    // =========================================================
    /// The Tc operator (PDF1-7, p.398) - Sets character spacing.
    [GraphicsOperation(PdfsValueKind.Number)]
    Tc,

    /// The Tw operator (PDF1-7, p.398) - Sets word spacing.
    [GraphicsOperation(PdfsValueKind.Number)]
    Tw,

    /// The Tz operator (PDF1-7, p.398) - Sets the horizontal scaling.
    [GraphicsOperation(PdfsValueKind.Number)]
    Tz,

    /// The TL operator (PDF1-7, p.398) - sets the text leading.
    [GraphicsOperation(PdfsValueKind.Number)]
    TL,

    /// The Tf operator (PDF1-7, p.398) - font name and font size.
    [GraphicsOperation(PdfsValueKind.Name, PdfsValueKind.Number)]
    Tf,

    /// The Tr operator (PDF1-7, p.398) - text rendering mode.
    [GraphicsOperation(PdfsValueKind.Number)]
    Tr,

    /// The Ts operator (PDF1-7, p.398) - text rise.
    [GraphicsOperation(PdfsValueKind.Number)]
    Ts,

    /// The BT operator (PDF1-7, p.405) - begins a text block.
    BT,

    /// The ET operator (PDF1-7, p.405) - ends a text block.
    ET,

    /// The Td operator (PDF1-7, p.406) - moves to the start of a new line.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number)]
    Td,

    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number)]
    TD,

    /// The Tm operator (PDF1-7, p.406) - sets the text matrix and text line matrix.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    Tm,

    /// The T* operator (PDF1-7, p.407) - moves to the next time.
    [GraphicsOperation(Operator = "T*")]
    TStar,

    /// The Tj operator (PDF1-7, p.407) - shows text.
    [GraphicsOperation(PdfsValueKind.String)]
    Tj,

    /// The TJ operator (PDF1-7, p.408) - shows text with glyph displacements.
    [GraphicsOperation(PdfsValueKind.Array)]
    TJ,

    /// The apostrophe operator (PDF1-7, p.407) - shows text.
    [GraphicsOperation(PdfsValueKind.String, Operator = "'")]
    Apos,

    /// The double-quote operator (PDF1-7, p.407) - shows text.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.String, Operator = "\"")]
    Quot,



    // Color instructions
    // ==================
    /// The CS operator - PDF1-7, p.196 - set the colour space.
    [GraphicsOperation(PdfsValueKind.Name)]
    CS,

    /// The cs operator - PDF1-7, p.196 - set the colour space.
    [GraphicsOperation(PdfsValueKind.Name)]
    cs,

    /// The SC operator - PDF1-7, p.196 - set the colour based on current colour space.
    [GraphicsOperation(PdfsValueKind.Number)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    SC,

    /// The sc operator - PDF1-7, p.196 - set the colour based on current colour space.
    [GraphicsOperation(PdfsValueKind.Number)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    sc,

    /// The SCN operator - PDF1-7, p.196 - set the colour based on current colour space.
    [GraphicsOperation(PdfsValueKind.Number)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Name)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Name)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Name)]
    SCN,

    /// The scn operator - PDF1-7, p.196 - set the colour based on current colour space.
    [GraphicsOperation(PdfsValueKind.Number)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Name)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Name)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Name)]
    scn,

    /// The G operator - PDF1-7, p.196 - set a gray colour.
    [GraphicsOperation(PdfsValueKind.Number)]
    G,

    /// The g operator - PDF1-7, p.196 - set a gray colour.
    [GraphicsOperation(PdfsValueKind.Number)]
    g,

    /// The RG operator - PDF1-7, p.196 - set an RGB colour.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    RG,

    /// The rg operator - PDF1-7, p.196 - set an RGB colour.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    rg,

    /// The K operator - PDF1-7, p.196 - set a CMYK colour.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    K,

    /// The k operator - PDF1-7, p.196 - set a CMYK colour.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number)]
    k,



    // Shading instructions
    // ====================
    /// The sh operator - PDF1-7, p.196 - paints a shading pattern.
    sh,



    /// The Do operator (PDF1-7, p. 332) - shows an object, such as an
    /// image or a form object.
    [GraphicsOperation(PdfsValueKind.Name)]
    Do,


    // Unknown
    // =======
    /// <summary>
    /// The Unknown operator is used when the operator is not known.
    /// </summary>
    Unknown,

}