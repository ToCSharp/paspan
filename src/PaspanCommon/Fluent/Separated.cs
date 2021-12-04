namespace Paspan.Fluent;

public sealed class Separated<U, T> : Parser<List<T>>
{
    private readonly Parser<U> _separator;
    private readonly Parser<T> _parser;

    public Separated(Parser<U> separator, Parser<T> parser)
    {
        _separator = separator ?? throw new ArgumentNullException(nameof(separator));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<List<T>> result)
    {
        context.EnterParser(this);

        var results = new List<T>();

        var start = 0;
        var end = 0;

        var first = true;
        var parsed = new ParseResult<T>();
        var separatorResult = new ParseResult<U>();

        while (true)
        {
            if (!_parser.Parse(ref reader, context, ref parsed))
            {
                if (!first)
                {
                    break;
                }

                // A parser that returns false is reponsible for resetting the position.
                // Nothing to do here since the inner parser is already failing and resetting it.
                return false;
            }

            if (first)
            {
                start = parsed.Start;
            }

            end = parsed.End;
            results.Add(parsed.Value);

            if (!_separator.Parse(ref reader, context, ref separatorResult))
            {
                break;
            }
        }

        result = new ParseResult<List<T>>(start, end, results);
        return true;
    }
}
