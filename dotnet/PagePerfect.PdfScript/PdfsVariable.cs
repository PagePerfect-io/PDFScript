using PagePerfect.PdfScript.Processor;
using PagePerfect.PdfScript.Reader;

namespace PagePerfect.PdfScript;

public class PdfsVariable(string name, PdfsValue value)
{
    // Public properties
    // =================
    #region Public properties
    public string Name { get; } = name;

    public PdfsValue Value { get; private set; } = value;
    #endregion



    // Public methods
    // ==============
    #region Public methods
    public void Set(PdfsValue value)
    {
        if (value.Kind != Value.Kind)
        {
            throw new PdfsProcessorException($"Value of kind {value.Kind} cannot be assigned to variable of kind {Value.Kind}");
        }

        Value = value;
    }
    #endregion
}