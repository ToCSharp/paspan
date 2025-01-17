﻿namespace Paspan.Fluent;

public class ParseContext
{
    /// <summary>
    /// Whether new lines are treated as normal chars or white spaces.
    /// </summary>
    /// <remarks>
    /// When <c>false</c>, new lines will be skipped like any other white space.
    /// Otherwise white spaces need to be read explicitely by a rule.
    /// </remarks>
    public bool UseNewLines { get; private set; }


    public ParseContext(bool useNewLines = false)
    {
        UseNewLines = useNewLines;
    }

    /// <summary>
    /// Delegate that is executed whenever a parser is invoked.
    /// </summary>
    public Action<object, ParseContext>? OnEnterParser { get; set; }

    /// <summary>
    /// The parser that is used to parse whitespaces and comments.
    /// </summary>
    public Parser<Memory<byte>>? WhiteSpaceParser { get; set; }

    public void SkipWhiteSpace(ref SpanReader reader)
    {
        if (WhiteSpaceParser is null)
        {
            if (UseNewLines)
            {
                reader.SkipWhiteSpace();
            }
            else
            {
                reader.SkipWhiteSpaceOrNewLine();
            }
        }
        else
        {
            ParseResult<Memory<byte>> _ = new();
            WhiteSpaceParser.Parse(ref reader, this, ref _);
        }
    }

    /// <summary>
    /// Called whenever a parser is invoked. Will be used to detect invalid states and infinite loops.
    /// </summary>
    public void EnterParser<T>(Parser<T> parser)
    {
        //OnEnterParser?.Invoke(parser, this);
    }
}
