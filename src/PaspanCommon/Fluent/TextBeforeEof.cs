namespace Paspan.Fluent;

public sealed class TextBeforeEof<T> : Parser<string>
{
    private readonly bool _canBeEmpty;
    private readonly bool _failOnEof;
    private readonly bool _consumeDelimiter;

    public TextBeforeEof(bool canBeEmpty = false, bool failOnEof = false, bool consumeDelimiter = false)
    {
        _canBeEmpty = canBeEmpty;
        _failOnEof = failOnEof;
        _consumeDelimiter = consumeDelimiter;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<string> result)
    {
        context.EnterParser(this);

        var start = reader.CaptureState();
        var res = reader.MoveToEof();
        var end = reader.GetCurrentPosition();
        if (reader.TryGetString(reader.GetPosition(start), end, out string value))
            result.Set(value);
        else
            result.Set("");

        return res;
    }
}
