namespace Paspan.Fluent;

public sealed class StringRegion : Parser<Region>
{
    private readonly StringLiteralQuotes _quotes;

    public StringRegion(StringLiteralQuotes quotes)
    {
        _quotes = quotes;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<Region> result)
    {
        context.EnterParser(this);

        var start = reader.CaptureState();

        var success = _quotes switch
        {
            StringLiteralQuotes.Single => reader.ReadSingleQuotedString(),
            StringLiteralQuotes.Double => reader.ReadDoubleQuotedString(),
            StringLiteralQuotes.SingleOrDouble => reader.ReadQuotedString(),
            _ => false
        };

        if (success)
        {
            result.Set(new Region(reader.GetPosition(start, 1), reader.GetCurrentPosition(-1)));
            return true;
        }

        reader.RollBackState(start);

        return false;
    }
}
