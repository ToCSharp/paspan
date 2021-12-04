namespace Paspan.Fluent;

/// <summary>
/// Successful when the cursor is at the end of the string.
/// </summary>
public sealed class Eof<T> : Parser<T>
{
    private readonly Parser<T> _parser;

    public Eof(Parser<T> parser)
    {
        _parser = parser;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<T> result)
    {
        context.EnterParser(this);

        if (_parser.Parse(ref reader, context, ref result) && reader.Eof())
        {
            return true;
        }

        return false;
    }
}
