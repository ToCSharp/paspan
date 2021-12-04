namespace Paspan.Fluent;

public sealed class Between<A, T, B> : Parser<T>
{
    private readonly Parser<T> _parser;
    private readonly Parser<A> _before;
    private readonly Parser<B> _after;

    public Between(Parser<A> before, Parser<T> parser, Parser<B> after)
    {
        _before = before ?? throw new ArgumentNullException(nameof(before));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _after = after ?? throw new ArgumentNullException(nameof(after));
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<T> result)
    {
        context.EnterParser(this);

        var start = reader.CaptureState();

        var parsedA = new ParseResult<A>();

        if (!_before.Parse(ref reader, context, ref parsedA))
        {
            // Don't reset position since _before should do it
            return false;
        }

        if (!_parser.Parse(ref reader, context, ref result))
        {
            reader.RollBackState(start);
            return false;
        }

        var parsedB = new ParseResult<B>();

        if (!_after.Parse(ref reader, context, ref parsedB))
        {
            reader.RollBackState(start);
            return false;
        }

        return true;
    }
}
