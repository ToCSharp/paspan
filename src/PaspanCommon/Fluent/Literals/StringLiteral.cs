namespace Paspan.Fluent;

public enum StringLiteralQuotes
{
    Single,
    Double,
    SingleOrDouble
}

public sealed class StringLiteral : Parser<string>
{
    private readonly StringLiteralQuotes _quotes;

    public StringLiteral(StringLiteralQuotes quotes)
    {
        _quotes = quotes;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<string> result)
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

        if (success && reader.TryGetString(out var value))
        {
            result.Set(value);
            return true;
        }

        reader.RollBackState(start);

        return false;
    }
}
