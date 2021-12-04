﻿namespace Paspan.Fluent;

public static class BytesBeforeConfig
{
    public static int MaxLengthDefault = int.MaxValue;
}

public sealed class BytesBefore<T, D, S> : Parser<byte[]>
{
    private readonly Parser<D> _delimiter;
    private readonly Parser<S>? _stopOn;
    private readonly int _maxLength;
    private readonly bool _canBeEmpty;
    private readonly bool _failOnEof;
    private readonly bool _consumeDelimiter;

    public BytesBefore(Parser<D> delimiter, bool canBeEmpty = false, bool failOnEof = true, bool consumeDelimiter = false)
    {
        _delimiter = delimiter;
        _canBeEmpty = canBeEmpty;
        _failOnEof = failOnEof;
        _consumeDelimiter = consumeDelimiter;
        _maxLength = BytesBeforeConfig.MaxLengthDefault;
    }

    public BytesBefore(Parser<D> delimiter, Parser<S> stopOn, bool canBeEmpty = false, bool failOnEof = true, bool consumeDelimiter = false)
        : this(delimiter, canBeEmpty, failOnEof, consumeDelimiter)
    {
        _stopOn = stopOn;
        _maxLength = BytesBeforeConfig.MaxLengthDefault;
    }

    public BytesBefore(Parser<D> delimiter, int maxLength = -1, bool canBeEmpty = false, bool failOnEof = true, bool consumeDelimiter = false)
        : this(delimiter, canBeEmpty, failOnEof, consumeDelimiter)
    {
        _maxLength = maxLength == -1 ? BytesBeforeConfig.MaxLengthDefault : maxLength;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<byte[]> result)
    {
        context.EnterParser(this);

        var start = reader.CaptureState();

        var parsed = new ParseResult<D>();
        var parsed2 = new ParseResult<S>();
        var length = 0;

        while (true)
        {
            var previous = reader.CaptureState();
            var previous2 = reader.GetCurrentPosition();

            if (reader.Eof())
            {
                if (_failOnEof)
                {
                    reader.RollBackState(start);
                    return false;
                }

                if (length == 0 && !_canBeEmpty)
                {
                    reader.RollBackState(start);
                    return false;
                }

                if (length > 0)
                {
                    reader.SetValue(reader.GetPosition(start), previous2);
                    result.Set(reader.GetValueBytes());
                }
                return true;
            }

            var delimiterFound = _delimiter.Parse(ref reader, context, ref parsed);

            if (delimiterFound)
            {
                if (length == 0 && !_canBeEmpty)
                {
                    reader.RollBackState(start);
                    return false;
                }

                if (!_consumeDelimiter)
                {
                    reader.RollBackState(previous);
                }

                if (length > 0)
                {
                    reader.SetValue(reader.GetPosition(start), previous2);
                    result.Set(reader.GetValueBytes());
                }
                return true;
            }

            if (length >= _maxLength ||
                _stopOn?.Parse(ref reader, context, ref parsed2) == true)
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
