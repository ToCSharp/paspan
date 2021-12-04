using System.Text;

namespace Paspan.Fluent;

public sealed class CharLiteral : Parser<Unit>
{
    public CharLiteral(char c)
    {
        //Char = c;
        CharBytes = Encoding.UTF8.GetBytes(new[] { c });
    }

    //public char Char;
    public byte[] CharBytes { get; }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<Unit> result)
    {
        context.EnterParser(this);

        var start = reader.CaptureState();

        if (reader.Skip(CharBytes))
        {
            reader.SetValue(reader.GetPosition(start), reader.GetCurrentPosition());
            result.Set(Unit.Value);
            return true;
        }

        return false;
    }

    public override string ToString() => $"CharLiteral '{Encoding.UTF8.GetString(CharBytes)}'";
}
