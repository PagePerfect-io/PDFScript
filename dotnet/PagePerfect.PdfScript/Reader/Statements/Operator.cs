namespace PagePerfect.PdfScript.Reader.Statements;

/// <summary>
/// Then Operator enumeration lists the possible operators for a graphics operation.
/// </summary>
public enum Operator
{
    // Special graphics state
    // ======================
    /// The q operator (PDF1-7, p.219) - Saves the current graphics state                
    [GraphicsOperation(AllowedIn = GraphicsObject.Page)]
    q,

    /// The Q operator (PDF1-7, p.219) - Restores the graphics state.
    [GraphicsOperation(AllowedIn = GraphicsObject.Page)]
    Q,

    /// The cm operator (PDF1-7, p.219) - Modifies the current
    /// transformation matrix.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    cm,



    // General graphics instructions
    // =============================
    /// The w operator - PDF1-7, p. 196 - set the line width.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    w,

    /// The J operator - PDF1-7, p. 196 - set the line cap style.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    J,

    /// The j operator - PDF1-7, p. 196 - set the line join style.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    j,

    /// The M operator - PDF1-7, p. 196 - set the miter limit.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    M,

    /// The d operator - PDF1-7, p. 196 - set the line dash pattern.
    [GraphicsOperation(PdfsValueKind.Array, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    d,

    /// The ri operator - PDF1-7, p. 196 - set the rendering intent.
    [GraphicsOperation(PdfsValueKind.Name, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    ri,

    /// The i operator - PDF1-7, p. 196 - set the flatness tolerance.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    i,

    /// The gs operator (PDF1-7, p.219) - Injects the graphics state
    /// located by the resource indicated in the operand.
    [GraphicsOperation(PdfsValueKind.Name, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    gs,



    // Path instructions
    // =================
    /// The m operator (PDF1-7, p.226) - starts a new path
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Path)]
    m,
    /// The l operator (PDF1-7, p.226) - appends a line
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Path)]
    l,
    /// The c operator (PDF1-7, p.226) - appends a bezier curve
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Path)]
    c,

    /// The v operator (PDF1-7, p.226) - appends a bezier curve
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Path)]
    v,
    /// The y operator (PDF1-7, p.226) - appends a bezier curve
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Path)]
    y,
    /// The h operator (PDF1-7, p.226) - closes a path
    [GraphicsOperation(AllowedIn = GraphicsObject.Path)]
    h,
    /// The re operator (PDF1-7, p.227) - adds a rectangle
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Path)]
    re,


    // Path painting instructions
    // ==========================
    /// The S and s operators (PDF1-7, p230) - strokes a path
    [GraphicsOperation(AllowedIn = GraphicsObject.Path)]

    s,

    [GraphicsOperation(AllowedIn = GraphicsObject.Path)]
    S,

    /// The f, F, and f* operators (PDF1-7, p.230) - fills a path
    [GraphicsOperation(AllowedIn = GraphicsObject.Path)]
    f,
    [GraphicsOperation(AllowedIn = GraphicsObject.Path)]
    F,
    [GraphicsOperation(Operator = "f*", AllowedIn = GraphicsObject.Path)]
    fStar,

    /// The B and B* operators (PDF1-7, p.230) - fill and stroke a path
    [GraphicsOperation(AllowedIn = GraphicsObject.Path)]
    B,
    [GraphicsOperation(Operator = "B*", AllowedIn = GraphicsObject.Path)]
    BStar,
    [GraphicsOperation(AllowedIn = GraphicsObject.Path)]
    b,
    [GraphicsOperation(Operator = "b*", AllowedIn = GraphicsObject.Path)]
    bStar,

    /// The n operator (PDF1-7, p.230) - does nothing
    [GraphicsOperation(AllowedIn = GraphicsObject.Path)]

    n,



    // Clipping path instructions
    // ==========================
    /// The W and W* operators (PDF1-7, p.235) - modifies the clipping path.
    [GraphicsOperation(AllowedIn = GraphicsObject.Path)]
    W,
    [GraphicsOperation(Operator = "W*", AllowedIn = GraphicsObject.Path)]
    WStar,



    // Text object, state, positioning and movement instructions
    // =========================================================
    /// The Tc operator (PDF1-7, p.398) - Sets character spacing.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    Tc,

    /// The Tw operator (PDF1-7, p.398) - Sets word spacing.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    Tw,

    /// The Tz operator (PDF1-7, p.398) - Sets the horizontal scaling.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    Tz,

    /// The TL operator (PDF1-7, p.398) - sets the text leading.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    TL,

    /// The Tf operator (PDF1-7, p.398) - font name and font size.
    [GraphicsOperation(PdfsValueKind.Name, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    Tf,

    /// The Tr operator (PDF1-7, p.398) - text rendering mode.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    Tr,

    /// The Ts operator (PDF1-7, p.398) - text rise.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Text)]
    Ts,

    /// The BT operator (PDF1-7, p.405) - begins a text block.
    [GraphicsOperation(AllowedIn = GraphicsObject.Page)]
    BT,

    /// The ET operator (PDF1-7, p.405) - ends a text block.
    [GraphicsOperation(AllowedIn = GraphicsObject.Text)]
    ET,

    /// The Td operator (PDF1-7, p.406) - moves to the start of a new line.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Text)]
    Td,

    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Text)]
    TD,

    /// The Tm operator (PDF1-7, p.406) - sets the text matrix and text line matrix.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Text)]
    Tm,

    /// The T* operator (PDF1-7, p.407) - moves to the next time.
    [GraphicsOperation(Operator = "T*", AllowedIn = GraphicsObject.Text)]
    TStar,

    /// The Tj operator (PDF1-7, p.407) - shows text.
    [GraphicsOperation(PdfsValueKind.String, AllowedIn = GraphicsObject.Text)]
    Tj,

    /// The TJ operator (PDF1-7, p.408) - shows text with glyph displacements.
    [GraphicsOperation(PdfsValueKind.Array, AllowedIn = GraphicsObject.Text)]
    TJ,

    /// The apostrophe operator (PDF1-7, p.407) - shows text.
    [GraphicsOperation(PdfsValueKind.String, Operator = "'", AllowedIn = GraphicsObject.Text)]
    Apos,

    /// The double-quote operator (PDF1-7, p.407) - shows text.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.String, Operator = "\"", AllowedIn = GraphicsObject.Text)]
    Quot,



    // Color instructions
    // ==================
    /// The CS operator - PDF1-7, p.196 - set the colour space.
    [GraphicsOperation(PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    CS,

    /// The cs operator - PDF1-7, p.196 - set the colour space.
    [GraphicsOperation(PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    cs,

    /// The SC operator - PDF1-7, p.196 - set the colour based on current colour space.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    SC,

    /// The sc operator - PDF1-7, p.196 - set the colour based on current colour space.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    sc,

    /// The SCN operator - PDF1-7, p.196 - set the colour based on current colour space.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    SCN,

    /// The scn operator - PDF1-7, p.196 - set the colour based on current colour space.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    scn,

    /// The G operator - PDF1-7, p.196 - set a gray colour.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    G,

    /// The g operator - PDF1-7, p.196 - set a gray colour.
    [GraphicsOperation(PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    g,

    /// The RG operator - PDF1-7, p.196 - set an RGB colour.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    RG,

    /// The rg operator - PDF1-7, p.196 - set an RGB colour.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    rg,

    /// The K operator - PDF1-7, p.196 - set a CMYK colour.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    K,

    /// The k operator - PDF1-7, p.196 - set a CMYK colour.
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page)]
    k,



    // Shading instructions
    // ====================
    /// The sh operator - PDF1-7, p.196 - paints a shading pattern.
    [GraphicsOperation(PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    sh,



    /// The Do operator (PDF1-7, p. 332) - shows an object, such as an
    /// image or a form object.
    [GraphicsOperation(PdfsValueKind.Name, AllowedIn = GraphicsObject.Page)]
    Do,


    // Unknown
    // =======
    /// <summary>
    /// The Unknown operator is used when the operator is not known.
    /// </summary>
    Unknown,



    // Additional instructions introduced in PDFScript
    // ===============================================
    /// <summary>
    /// The rr operator - constructs a rounded rectangle.
    /// </summary>
    /// The re operator (PDF1-7, p.227) - adds a rectangle
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Path)]
    [GraphicsOperation(PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, PdfsValueKind.Number, AllowedIn = GraphicsObject.Page | GraphicsObject.Path)]
    rr
}