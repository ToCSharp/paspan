using static Paspan.Common.Constants;

namespace Paspan.Fluent;

public sealed class DecimalLiteral : Parser<decimal>
{
    private readonly NumberOptions _numberOptions;

    public DecimalLiteral(NumberOptions numberOptions = NumberOptions.Default)
    {
        _numberOptions = numberOptions;
    }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<decimal> result)
    {
        context.EnterParser(this);

        var start = reader.CaptureState();

        var sign = 1;
        if ((_numberOptions & NumberOptions.AllowSign) == NumberOptions.AllowSign)
        {
            if (reader.Skip(Minus))
            {
                sign = -1;
            }
            else
            {
                reader.Skip(Plus);
            }
        }

        if (reader.ConsumeDecimalDigits() && reader.TryGetDecimal(out decimal value))
        {
            result.Set(sign * value);
            return true;
        }

        reader.RollBackState(start);

        return false;
    }
}
