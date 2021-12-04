namespace Paspan.Fluent;


public static class TextBeforeConfig
{
    public static int MaxLengthDefault = int.MaxValue;
}
public sealed class TextBefore<T> : Parser<string>
{
    private readonly Parser<T> _delimiter;
    private readonly Parser<T>? _stopOn;
    private readonly int _maxLength;
    private readonly bool _canBeEmpty;
    private readonly bool _failOnEof;
    private readonly bool _consumeDelimiter;

    public TextBefore(Parser<T> delimiter, bool canBeEmpty = false, bool failOnEof = true, bool consumeDelimiter = false)
    {
        _delimiter = delimiter;
        _canBeEmpty = canBeEmpty;
        _failOnEof = failOnEof;
        _consumeDelimiter = consumeDelimiter;
        _maxLength = TextBeforeConfig.MaxLengthDefault;
    }

    public TextBefore(Parser<T> delimiter, Parser<T>? stopOn, bool canBeEmpty = false, bool failOnEof = true, bool consumeDelimiter = false)
        : this(delimiter, canBeEmpty, failOnEof, consumeDelimiter)
    {
        _stopOn = stopOn;
    }

    public TextBefore(Parser<T> delimiter, int maxLength, bool canBeEmpty = false, bool failOnEof = true, bool consumeDelimiter = false)
        : this(delimiter, canBeEmpty, failOnEof, consumeDelimiter)
    {
        _maxLength = maxLength == -1 ? TextBeforeConfig.MaxLengthDefault : maxLength;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<string> result)
    {
        context.EnterParser(this);

        var start = reader.CaptureState();

        var parsed = new ParseResult<T>();
        var length = 0;

        while (true)
        {
            var previous = reader.CaptureState();

            if (reader.Eof())
            {
                if (_failOnEof)
                {
                    reader.RollBackState(start);
                    return false;
                }

                if (length == 0 && !_canBeEmpty)
                {
                    return false;
                }

                reader.SetValue(reader.GetPosition(start), reader.GetCurrentPosition());
                //result.Set(PooledStringValue.GetString(reader.GetValue()));
                if (length > 0 && reader.TryGetString(reader.GetPosition(start), length, out string value))
                    result.Set(value);
                else
                    result.Set("");
                return true;
            }

            var delimiterFound = _delimiter.Parse(ref reader, context, ref parsed);

            if (delimiterFound)
            {

                if (!_consumeDelimiter)
                {
                    reader.RollBackState(previous);
                }

                if (length == 0 && !_canBeEmpty)
                {
                    return false;
                }

                reader.SetValue(reader.GetPosition(start), reader.GetCurrentPosition());
                //result.Set(PooledStringValue.GetString(reader.GetValue()));
                if (length > 0 && reader.TryGetString(reader.GetPosition(start), length, out string value))
                    result.Set(value);
                else
                    result.Set("");
                return true;
            }

            if (length >= _maxLength ||
                _stopOn?.Parse(ref reader, context, ref parsed) == true)
            {
                reader.RollBackState(start);
                return false;
            }

            length++;
            reader.Read(1);
        }
    }

    public override string ToString() => $"TextBefore {_delimiter}";
}
