namespace Paspan.Fluent;

public sealed class ElseError<T> : Parser<T>
{
    private readonly Parser<T> _parser;
    private readonly string _message;

    public ElseError(Parser<T> parser, string message)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _message = message;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<T> result)
    {
        context.EnterParser(this);

        if (!_parser.Parse(ref reader, context, ref result))
        {
            throw new ParseException(_message);
        }

        return true;
    }
}

public sealed class Error<T> : Parser<T>
{
    private readonly Parser<T> _parser;
    private readonly string _message;

    public Error(Parser<T> parser, string message)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _message = message;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<T> result)
    {
        context.EnterParser(this);

        if (_parser.Parse(ref reader, context, ref result))
        {
            throw new ParseException(_message);
        }

        return false;
    }
}

public sealed class Error<T, U> : Parser<U>
{
    private readonly Parser<T> _parser;
    private readonly string _message;

    public Error(Parser<T> parser, string message)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _message = message;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<U> result)
    {
        context.EnterParser(this);

        var parsed = new ParseResult<T>();

        if (_parser.Parse(ref reader, context, ref parsed))
        {
            throw new ParseException(_message);
        }

        return true;
    }
}
