namespace Paspan.Fluent;

interface ILabelled
{
    string Label { get; }
}
interface IHaveValueObject
{
    object ValueObject { get; }
}

public struct Labelled<T> : ILabelled, IHaveValueObject
{
    public T Value;
    public string Label { get; }
    public object ValueObject => Value;
    public Labelled(T value, string label)
    {
        Value = value;
        Label = label;
    }
    public override string ToString() => $"Labelled {typeof(T).Name}: '{Label}'={Value}";
}

public static partial class ParsersPlus
{
    public static Parser<Labelled<T>> Labelled<T>(this Parser<T> parser, string label)
        => new LabelledParser<T>(parser, label);

}

public sealed class LabelledParser<T> : Parser<Labelled<T>>
{
    public string Label;
    private readonly Parser<T> _parser;

    public LabelledParser(Parser<T> parser, string lable)
    {
        _parser = parser;
        Label = lable;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<Labelled<T>> result)
    {
        context.EnterParser(this);

        var parsed = new ParseResult<T>();

        if (_parser.Parse(ref reader, context, ref parsed))
        {
            result.Set(new Labelled<T>(parsed.Value, Label));

            return true;
        }

        return false;
    }

    public override string ToString() => $"LabelledParser '{Label}': {_parser}";
}
