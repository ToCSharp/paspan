namespace Paspan.Fluent;

public sealed class Deferred<T> : Parser<T>
{

    public Parser<T>? Parser { get; set; }

    public Deferred()
    {
    }

    public Deferred(Func<Deferred<T>, Parser<T>> parser)
    {
        Parser = parser(this);
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(Parser);
        return Parser.Parse(ref reader, context, ref result);
    }

    //private bool _initialized = false;
    //private readonly Closure _closure = new();

    //private class Closure
    //{
    //    public object Func;
    //}
}
