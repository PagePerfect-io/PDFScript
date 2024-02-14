using System.Text;
using PagePerfect.PdfScript.Reader;

namespace PagePerfect.PdfScript.Tests;

/// <summary>
/// The PdfsValueTests class contains tests for the PdfsValue class.
/// </summary>
public class PdfsValueTests
{
    // Public tests
    // ============
    #region Parsing arrays
    /// <summary>
    /// The PdfsValue class should return a Null reference
    /// when an array cannot be fully read. 
    /// </summary>
    [Fact]
    public async Task ShouldReturnNullIfArrayCannotBeRead()
    {
        using var stream = S("[ 10");
        var lexer = new PdfsLexer(stream);
        await lexer.Read();

        var value = await PdfsValue.ReadArray(lexer);
        Assert.Null(value);
    }

    /// <summary>
    /// The PDfsValue class should throw an exception when the array syntax is invalid.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenArraySyntaxInvalid()
    {
        // Keywords are not valid in arrays
        using (var stream = S("[ keyword ]"))
        {
            var lexer = new PdfsLexer(stream);
            await lexer.Read();
            await Assert.ThrowsAsync<PdfsReaderException>(async () => await PdfsValue.ReadArray(lexer));
        }

        // Prolog fragments are not valid in arrays
        using (var stream = S("[ # prolog ]"))
        {
            var lexer = new PdfsLexer(stream);
            await lexer.Read();
            await Assert.ThrowsAsync<PdfsReaderException>(async () => await PdfsValue.ReadArray(lexer));
        }

        // A dictionary-close token is not valid in an array.
        using (var stream = S("[ >> ]"))
        {
            var lexer = new PdfsLexer(stream);
            await lexer.Read();
            await Assert.ThrowsAsync<PdfsReaderException>(async () => await PdfsValue.ReadArray(lexer));
        }

    }

    /// <summary>
    /// The PdfsValue class should be able to parse an empty array from a lexer.
    /// </summary>
    [Fact]
    public async Task ShouldParseEmptyArray()
    {
        using var stream = S("[ ]");
        var lexer = new PdfsLexer(stream);
        await lexer.Read();

        var value = await PdfsValue.ReadArray(lexer);
        Assert.NotNull(value);
        var array = value.GetArray();
        Assert.NotNull(array);
        Assert.Empty(array);
    }

    /// <summary>
    /// The PdfsValue class should be able to parse an array from a lexer.
    /// </summary>
    [Fact]
    public async Task ShouldParseArray()
    {
        using var stream = S("[ 10 (Edwin) /TimesRoman $var ]");
        var lexer = new PdfsLexer(stream);
        await lexer.Read();

        var value = await PdfsValue.ReadArray(lexer);
        Assert.NotNull(value);
        var array = value.GetArray();
        Assert.NotNull(array);
        Assert.Equal(4, array.Length);

        Assert.IsType<PdfsValue>(array[0]);
        Assert.Equal(10, array[0].GetNumber());

        Assert.IsType<PdfsValue>(array[1]);
        Assert.Equal("Edwin", array[1].GetString());

        Assert.IsType<PdfsValue>(array[2]);
        Assert.Equal("/TimesRoman", array[2].GetString());

        Assert.IsType<PdfsValue>(array[3]);
        Assert.Equal("var", array[3].GetString());
    }

    /// <summary>
    /// The PdfsValue class should be able to parse an array with a nested array.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task ShouldParseArrayWithNestedArray()
    {
        using var stream = S("[ [ 10 [ 20 30 ] ] ]");
        var lexer = new PdfsLexer(stream);
        await lexer.Read();

        var value = await PdfsValue.ReadArray(lexer);
        Assert.NotNull(value);
        var array = value.GetArray();
        Assert.NotNull(array);
        Assert.Single(array);

        Assert.IsType<PdfsValue>(array[0]);
        var nested = array[0].GetArray();
        Assert.NotNull(nested);
        Assert.Equal(2, nested.Length);

        Assert.IsType<PdfsValue>(nested[0]);
        Assert.Equal(10, nested[0].GetNumber());

        Assert.IsType<PdfsValue>(nested[1]);
        var nested2 = nested[1].GetArray();
        Assert.NotNull(nested2);
        Assert.Equal(2, nested2.Length);

        Assert.IsType<PdfsValue>(nested2[0]);
        Assert.Equal(20, nested2[0].GetNumber());

        Assert.IsType<PdfsValue>(nested2[1]);
        Assert.Equal(30, nested2[1].GetNumber());

    }

    /// <summary>
    /// The PdfsValue class should be able to parse an array with a dictionary value.
    /// </summary>
    [Fact]
    public async Task ShouldParseArrayWithDictionaryValue()
    {
        using var stream = S("[ << /Key /Value >> ]");
        var lexer = new PdfsLexer(stream);
        await lexer.Read();

        var value = await PdfsValue.ReadArray(lexer);
        Assert.NotNull(value);
        var array = value.GetArray();
        Assert.NotNull(array);
        Assert.Single(array);

        Assert.IsType<PdfsValue>(array[0]);
        var dict = array[0].GetDictionary();
        Assert.NotNull(dict);
        Assert.Single(dict);

        Assert.IsType<PdfsValue>(dict["/Key"]);
        Assert.Equal("/Value", dict["/Key"].GetString());
    }
    #endregion

    #region Parsing dicctionaries
    /// <summary>
    /// The PdfsValue class should return a Null reference
    /// when a dictionary cannot be fully read. 
    /// </summary>
    [Fact]
    public async Task ShouldReturnNullIfDictionaryCannotBeRead()
    {
        using var stream = S("<< /Length 10");
        var lexer = new PdfsLexer(stream);
        await lexer.Read();

        var value = await PdfsValue.ReadDictionary(lexer);
        Assert.Null(value);
    }

    /// <summary>
    /// The PDfsValue class should throw an exception when the dictionary syntax is invalid.
    /// </summary>
    [Fact]
    public async Task ShouldThrowWhenDictionarySyntaxInvalid()
    {

        // Dictionar keys must be names
        using (var stream = S("<< (Name) /Value >>"))
        {
            var lexer = new PdfsLexer(stream);
            await lexer.Read();
            await Assert.ThrowsAsync<PdfsReaderException>(async () => await PdfsValue.ReadDictionary(lexer));
        }
        using (var stream = S("<< 13 /Value >>"))
        {
            var lexer = new PdfsLexer(stream);
            await lexer.Read();
            await Assert.ThrowsAsync<PdfsReaderException>(async () => await PdfsValue.ReadDictionary(lexer));
        }
        using (var stream = S("<< /Key 10 20 >>"))
        {
            var lexer = new PdfsLexer(stream);
            await lexer.Read();
            await Assert.ThrowsAsync<PdfsReaderException>(async () => await PdfsValue.ReadDictionary(lexer));
        }

        // Keywords are not valid in dictionaries
        using (var stream = S("<< /Key keyword >>"))
        {
            var lexer = new PdfsLexer(stream);
            await lexer.Read();
            await Assert.ThrowsAsync<PdfsReaderException>(async () => await PdfsValue.ReadDictionary(lexer));
        }

        // Prolog fragments are not valid in dictionaries
        using (var stream = S("<< /Key #prolog >>"))
        {
            var lexer = new PdfsLexer(stream);
            await lexer.Read();
            await Assert.ThrowsAsync<PdfsReaderException>(async () => await PdfsValue.ReadDictionary(lexer));
        }

        // An array-close token is not valid in a dictionary.
        using (var stream = S("<< ] >>"))
        {
            var lexer = new PdfsLexer(stream);
            await lexer.Read();
            await Assert.ThrowsAsync<PdfsReaderException>(async () => await PdfsValue.ReadDictionary(lexer));
        }
    }

    /// <summary>
    /// The PdfsValue class should be able to parse an empty dictionary from a lexer.
    /// </summary>
    [Fact]
    public async Task ShouldParseEmptyDictionary()
    {
        using var stream = S("<< >>");
        var lexer = new PdfsLexer(stream);
        await lexer.Read();

        var value = await PdfsValue.ReadDictionary(lexer);
        Assert.NotNull(value);
        var dict = value.GetDictionary();
        Assert.NotNull(dict);
        Assert.Empty(dict);
    }

    /// <summary>
    /// The PdfsValue class should be able to parse a dictionary from a lexer.
    /// </summary>
    [Fact]
    public async Task ShouldParseDictionary()
    {
        using var stream = S("<< /Num 10 /Str (Edwin) /Name /TimesRoman /Var $var >>");
        var lexer = new PdfsLexer(stream);
        await lexer.Read();

        var value = await PdfsValue.ReadDictionary(lexer);
        Assert.NotNull(value);
        var dict = value.GetDictionary();
        Assert.NotNull(dict);
        Assert.Equal(4, dict.Count);

        Assert.IsType<PdfsValue>(dict["/Num"]);
        Assert.Equal(10, dict["/Num"].GetNumber());

        Assert.IsType<PdfsValue>(dict["/Str"]);
        Assert.Equal("Edwin", dict["/Str"].GetString());

        Assert.IsType<PdfsValue>(dict["/Name"]);
        Assert.Equal("/TimesRoman", dict["/Name"].GetString());

        Assert.IsType<PdfsValue>(dict["/Var"]);
        Assert.Equal("var", dict["/Var"].GetString());
    }

    /// <summary>
    /// The PdfsValue class should be able to parse a dictionary with a nested dictionary.
    /// </summary>
    [Fact]
    public async Task ShouldParseNestedDictionaries()
    {
        using var stream = S("<< /Num 10 /Dict << /Str (Edwin) /Dict2 << /Name /TimesRoman /Var $var >> >> >>");
        var lexer = new PdfsLexer(stream);
        await lexer.Read();

        var value = await PdfsValue.ReadDictionary(lexer);
        Assert.NotNull(value);
        var dict = value.GetDictionary();
        Assert.NotNull(dict);
        Assert.Equal(2, dict.Count);

        Assert.IsType<PdfsValue>(dict["/Num"]);
        Assert.Equal(10, dict["/Num"].GetNumber());

        Assert.IsType<PdfsValue>(dict["/Dict"]);
        var nested = dict["/Dict"].GetDictionary();
        Assert.NotNull(nested);
        Assert.Equal(2, nested.Count);

        Assert.IsType<PdfsValue>(nested["/Str"]);
        Assert.Equal("Edwin", nested["/Str"].GetString());

        Assert.IsType<PdfsValue>(nested["/Dict2"]);
        var nested2 = nested["/Dict2"].GetDictionary();
        Assert.NotNull(nested2);
        Assert.Equal(2, nested2.Count);

        Assert.IsType<PdfsValue>(nested2["/Name"]);
        Assert.Equal("/TimesRoman", nested2["/Name"].GetString());

        Assert.IsType<PdfsValue>(nested2["/Var"]);
        Assert.Equal("var", nested2["/Var"].GetString());
    }

    /// <summary>
    /// The PdfsValue class should be able to parse a dictionary with an array value.
    /// </summary>
    [Fact]
    public async Task ShouldParseDictionaryWithArrayValue()
    {
        using var stream = S("<< /Arr [ 10 20 30 ] >>");
        var lexer = new PdfsLexer(stream);
        await lexer.Read();

        var value = await PdfsValue.ReadDictionary(lexer);
        Assert.NotNull(value);
        var dict = value.GetDictionary();
        Assert.NotNull(dict);
        Assert.Single(dict);

        Assert.IsType<PdfsValue>(dict["/Arr"]);
        var array = dict["/Arr"].GetArray();
        Assert.NotNull(array);
        Assert.Equal(3, array.Length);

        Assert.IsType<PdfsValue>(array[0]);
        Assert.Equal(10, array[0].GetNumber());

        Assert.IsType<PdfsValue>(array[1]);
        Assert.Equal(20, array[1].GetNumber());

        Assert.IsType<PdfsValue>(array[2]);
        Assert.Equal(30, array[2].GetNumber());
    }


    #endregion



    // Private implementation
    // ======================
    #region Private implementation
    /// <summary>
    /// Creates a memory stream out of a string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <returns>The memory stream.</returns>
    private static MemoryStream S(string source)
    {
        var bytes = Encoding.ASCII.GetBytes(source);
        return new MemoryStream(bytes);
    }
    #endregion




}