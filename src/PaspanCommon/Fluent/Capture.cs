namespace Paspan.Fluent;

public sealed class Capture<T> : Parser<Region>
{
    private readonly Parser<T> _parser;

    public Capture(Parser<T> parser)
    {
        _parser = parser;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<Region> result)
    {
        context.EnterParser(this);

        var start = reader.CaptureState();

        ParseResult<T> _ = new();

        // Did parser succeed.
        if (_parser.Parse(ref reader, context, ref _))
        {
            var s = reader.GetPosition(start);
            var end = reader.GetCurrentPosition();
            reader.SetValue(s, end);
            result.Set(new Region(s, end));
            return true;
        }

        reader.RollBackState(start);

        return false;
    }
}
