﻿namespace Paspan.Fluent;

public sealed class Not<T> : Parser<T>
{
    private readonly Parser<T> _parser;

    public Not(Parser<T> parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<T> result)
    {
        context.EnterParser(this);

        var start = reader.CaptureState();

        if (!_parser.Parse(ref reader, context, ref result))
        {
            return true;
        }

        reader.RollBackState(start);
        return false;
    }
}
