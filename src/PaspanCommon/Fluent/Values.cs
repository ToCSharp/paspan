using Paspan.Common;
using System.Text;

namespace Paspan.Fluent;

public abstract partial class Parser<T>
{
    public Parser<ulong> AsHex() => new HexValue<T>(this);
    public Parser<char> AsChar() => new CharValue<T>(this);
    //public Parser<(T, string)> AsString() => new StringValue<T>(this);
    //public Parser<(T1, string)> AsString() => new StringValue<T>(this);
}
public static partial class Parsers
{
    public static Parser<object?> AsObject<T>(this Parser<T> parser) => new AsObject<T>(parser);

}
public sealed class HexValue<T> : Parser<ulong>
{
    private readonly Parser<T> _parser;
    public HexValue(Parser<T> parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }
    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<ulong> result)
    {
        context.EnterParser(this);

        var parsed = new ParseResult<T>();

        if (_parser.Parse(ref reader, context, ref parsed))
        {
            if (reader.TryGetHex(out var v))
            {
                result.Set(v);
                return true;
            }
        }
        return false;
    }
}

public sealed class CharValue<T> : Parser<char>
{
    private readonly Parser<T> _parser;
    public CharValue(Parser<T> parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }
    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<char> result)
    {
        context.EnterParser(this);

        var parsed = new ParseResult<T>();

        if (_parser.Parse(ref reader, context, ref parsed))
        {
            if (reader.TryGetChar(out var v))
            {
                result.Set(v);
                return true;
            }
        }
        return false;
    }
}
public sealed class StringValue : Parser<string>
{
    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<string> result)
    {
        context.EnterParser(this);

        if (reader.TryGetString(out var v))
        {
            result.Set(v);
            return true;
        }
        return false;
    }
}

public sealed class PooledStringValue : Parser<string?>
{
    //public static DictionaryBytes<string?> StringPool = new();
    public static Dictionary<byte[], string?> StringPool = new();
    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<string?> result)
    {
        context.EnterParser(this);

        var bytes = reader.GetValue();
        if (StringPool.TryGetValue(bytes.ToArray(), out var value))
        {
            result.Set(value);
        }
        else
        {
            result.Set(Encoding.UTF8.GetString(bytes));
            StringPool.Add(bytes.ToArray(), result.Value);
        }
        return true;
    }
    public static string GetString(ReadOnlySpan<byte> bytes)
    {
        if (StringPool.TryGetValue(bytes.ToArray(), out var value))
        {
            return value;
        }
        else
        {
            var valueAdded = Encoding.UTF8.GetString(bytes);
            StringPool.Add(bytes.ToArray(), valueAdded);
            return valueAdded;
        }
    }
}

public sealed class AsObject<T> : Parser<object?>
{
    private readonly Parser<T> _parser;
    public AsObject(Parser<T> parser)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    }
    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<object?> result)
    {
        context.EnterParser(this);

        var parsed = new ParseResult<T>();
        if (_parser.Parse(ref reader, context, ref parsed))
        {
            result.Set(parsed.Value);
            return true;
        }
        return false;
    }
}

//public sealed class StringValue<T> : Parser<(T, string)>
//{
//    private readonly Parser<T> _parser;
//    public StringValue(Parser<T> parser)
//    {
//        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
//    }
//    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<(T, string)> result)
//    {
//        context.EnterParser(this);

//        var parsed = new ParseResult<T>();

//        if (_parser.Parse(ref reader, context, ref parsed))
//        {
//            if (reader.TryGetString(out var v))
//            {
//                result.Set((parsed.Value, v));
//                return true;
//            }
//        }
//        return false;
//    }
//}

