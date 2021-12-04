namespace Paspan.Fluent;

/// <summary>
/// OneOf the inner choices when all parsers return the same type.
/// We then return the actual result of each parser.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class OneOf<T> : Parser<T>
{
    private readonly Parser<T>[] _parsers;

    public OneOf(Parser<T>[] parsers)
    {
        _parsers = parsers ?? throw new ArgumentNullException(nameof(parsers));
    }

    public Parser<T>[] Parsers => _parsers;

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<T> result)
    {
        context.EnterParser(this);

        foreach (var parser in _parsers)
        {
            if (parser.Parse(ref reader, context, ref result))
            {
                return true;
            }
        }

        return false;
    }
}
