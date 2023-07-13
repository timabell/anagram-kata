// https://github.com/timabell/anagram-kata
using NUnit.Framework;
using NUnitLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
// using FluentAssertions; // I'd use this but doesn't seem to be available in coderpad :'-(

class Solution
{
    // all the handling of CLI things and unit testing here, all actual logic delegated to unit testable class(es) below
    static int Main(string[] args)
    {
        if (args.Length > 0)
        { // allow user to specifiy file (the "production" code)
            WordGrouper.Run(args[0]);
        }
        else
        { // helpful output
            Console.WriteLine("No file provided, just running tests this time. Usage: program.exe <inputfile>");

            // hard code example files and run them for the sake of this exercise, I don't see a way to pass them in when running in coderpad UI
            WordGrouper.Run("example1.txt");
            //WordGrouper.Run("/home/coderpad/data/example2.txt");
            // This boilerplate is here to be able to run the unit tests in coderpad. There appears to be no buttons or ways to add test files and run them. https://coderpad.io/languages/csharp/
            return new AutoRun(Assembly.GetCallingAssembly()).Execute(new String[] { "--labels=All" });
        }
        return 0; // unix program exit code 0, meaning success - https://tldp.org/LDP/abs/html/exitcodes.html
    }
}

[TestFixture]
class GroupAnagramsTests
{
    // todo: maybe make some nice data driven parameterized tests
    [Test]
    public void GroupsMatchingAnagrams()
    {
        var input = new List<string> { "cba", "cab" };
        var actual = WordGrouper.GroupAnagrams(input).ToList();
        // todo: tidy this up a bit, not very DRY or clear. some kind of deep-equal might be nice
        Assert.AreEqual(1, actual.Count());

        Assert.AreEqual("abc", actual[0].Key);
        var actualGroup = actual[0].ToList();
        Assert.AreEqual(2, actualGroup.Count());
        Assert.AreEqual("cba", actualGroup[0]);
        Assert.AreEqual("cab", actualGroup[1]);
    }

    [Test]
    public void DoesntGroupNonMatchingAnagrams()
    {
        var input = new List<string> { "abc", "bcd" };
        var actual = WordGrouper.GroupAnagrams(input).ToList();
        Assert.AreEqual(2, actual.Count());

        Assert.AreEqual("abc", actual[0].Key);
        var actualGroup1 = actual[0].ToList();
        Assert.AreEqual(1, actualGroup1.Count());
        Assert.AreEqual("abc", actualGroup1[0]);

        Assert.AreEqual("bcd", actual[1].Key);
        var actualGroup2 = actual[1].ToList();
        Assert.AreEqual(1, actualGroup2.Count());
        Assert.AreEqual("bcd", actualGroup2[0]);
    }

    // todo: expand the suite some more, this was enough to TDD the first file processing

    //     [Test]
    //    public void IgnoresBlanks(){
    //        var input = new List<string>{"abc", " "};
    //        var actual = WordGrouper.GroupAnagrams(input).ToList();
    //        // todo
    //    }
}

[TestFixture]
class ProcessWordsTests
{
    [Test]
    public void GroupsAnagrams(){
        var input = new List<string>{
            "foo",
            "fff",
            "oof",
            "foof",
            "abcd",
            "ffoo",
            "dcba",
        };
        var outputWriter = new StringWriter();
        WordGrouper.ProcessWords(input, outputWriter);
        var expected = @"foo,oof
fff
foof,ffoo
abcd,dcba
";
        outputWriter.Flush();
        Assert.AreEqual(expected, outputWriter.ToString());
    }
}

/*
Let's have a think about the structure of the solution. We'll TDD this so we need to know the shape first.

We need to stream the file and process as we go to avoid the mentioned out of memory challenge.
Let's start with a simple load-everything and use linq and then iterate for perfomance and memory.
We can use the tests to ensure no regressions.

To take a set of words and find anagram matches
- slurp file
- split/loop on newline
- add to list
- group list by sorted letters (similar to https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/how-to-group-files-by-extension-linq )
- loop group and print

todo/questions
- case sensitivity? collation (i.e. do accented characters match?)
- nice gherkin/specflow for BAs to look at and provide examples (maybe)
- crlf / lf / cr handling
- trim for whitespace
- trailing newline
- empty lines
- lines that are "too long"
- run dotnet format to tidy up
- consider GC
- async for IO
- discuss policy on GPT things
- ask gpt how to make code better, more solid etc etc
- review with peers and see what we care about
- pair program on it in the first place
- consider sorting issues (and effect on tests)
- big O analysis
- what if one word-size has so many we get an out of memory, currently will just crash
*/

/// <summary>
/// A program that takes as an argument the path to a file containing one word per line,
/// groups the words that are anagrams to each other, and writes to the standard output of
/// each of these groups. The groups should be separated by newlines and the words inside
/// each group by commas.
/// </summary>
public class WordGrouper
{
    public static void Run(string inputFile)
    {
        Console.WriteLine("Processing " + inputFile);
        ProcessWords(File.ReadLines(inputFile), Console.Out);
    }

    public static void ProcessWords(IEnumerable<string> source, System.IO.TextWriter output)
    {
        // "You can make the following assumptions about the data in the files: The words in the input file are ordered by size"
        // because we know the words step up in size we can track the current size and when we've loaded all the words of one size in to memory (the "words" list) we can then process that group and pass the memorty back to the GC before moving to the next size
        int? wordLength = null;
        List<string> words = new List<string>();
        // todo: I'm sure this can be refactored to make the flow clearer. but this works for now. I'd add tests to this batching loop before refactoring but that'll take more time
        foreach (var line in source)
        {
            var input = line.Trim();
            if (input == "")
            { // todo: test coverage
                continue;
            }
            if (wordLength == null)
            { // first word in file, initialize length
                wordLength = input.Length;
            }
            if (input.Length != wordLength)
            {// Starting new batch. Process previous and release memory to GC
                ProcessBatch(words, output); // process previous batch
                words.Clear();
                wordLength = input.Length;
            }

            words.Add(input);
        }
        ProcessBatch(words, output); // process last batch
    }

    private static void ProcessBatch(List<string> words, System.IO.TextWriter output)
    {
        var grouped = GroupAnagrams(words);
        foreach (var group in grouped)
        {
            output.WriteLine(string.Join(',', group));
        }
    }

    public static IEnumerable<IGrouping<string, string>> GroupAnagrams(IList<string> input)
    {
        return input.GroupBy(i => SortString(i), i => i);
    }

    static string SortString(string input)
    {
        // https://stackoverflow.com/questions/6441583/is-there-a-simple-way-that-i-can-sort-characters-in-a-string-in-alphabetical-ord
        // allegedly this is fast.
        // todo: test if we care
        char[] characters = input.ToArray();
        Array.Sort(characters);
        return new string(characters);
    }
}
