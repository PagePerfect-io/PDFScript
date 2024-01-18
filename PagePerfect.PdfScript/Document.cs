using System.Text;

namespace PagePerfect.PdfScript;

public class Document
{
    private Stream _stream;

    public Document(string source)
    {
        _stream = new MemoryStream(Encoding.UTF8.GetBytes(source));
    }

    public Document(Stream stream)
    {
        _stream = stream;
    }

    public async Task SaveAs(string path)
    {

    }

    public async Task ToStream(Stream output)
    {

    }

}
