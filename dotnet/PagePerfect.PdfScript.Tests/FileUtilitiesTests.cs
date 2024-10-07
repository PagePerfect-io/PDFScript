using PagePerfect.PdfScript.Utilities;

namespace PagePerfect.PdfScript.Tests;

public class FileUtilitiesTests
{
    [Fact]
    public void ShouldReturnAbsolutePathForFilename()
    {
        var path = "test.txt";
        var expected = Path.Combine(Directory.GetCurrentDirectory(), path);
        var (actual, type) = FileUtilities.NormalisePath(path);
        Assert.Equal(expected, actual);
        Assert.Equal(FileLocationType.LocalFile, type);
    }

    [Fact]
    public void ShouldReturnAbsolutePathForFileInSubdirectory()
    {
        var path = "subdir/test.txt";
        var expected = Path.Combine(Directory.GetCurrentDirectory(), path);
        var (actual, type) = FileUtilities.NormalisePath(path);
        Assert.Equal(expected, actual);
        Assert.Equal(FileLocationType.LocalFile, type);
    }

    [Fact]
    public void ShouldReturnAbsolutePathForAbsolutePath()
    {
        var path = "/test.txt";
        var expected = path;
        var (actual, type) = FileUtilities.NormalisePath(path);
        Assert.Equal(expected, actual);
        Assert.Equal(FileLocationType.LocalFile, type);
    }

    [Fact]
    public void ShouldReturnInternetPathForInternetLocation()
    {
        var path = "http://www.example.com/test.txt";
        var (actual, type) = FileUtilities.NormalisePath(path);
        Assert.Equal(path, actual);
        Assert.Equal(FileLocationType.Internet, type);

        path = "https://www.example.com/test.txt";
        (actual, type) = FileUtilities.NormalisePath(path);
        Assert.Equal(path, actual);
        Assert.Equal(FileLocationType.Internet, type);
    }

}