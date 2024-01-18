namespace PagePerfect.PdfScript.Tests;

public class DocumentTests
{
    [Fact]
    public void ShouldThrowWhenSourceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new Document((string)null!));
    }
}