using System;
using System.Text;

namespace Paspan.Fluent;

public sealed class TextLiteral : Parser<string>
{
    //private readonly StringComparer _comparer;

    public TextLiteral(string text/*, StringComparer comparer = null*/)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        TextBytes = Encoding.UTF8.GetBytes(Text);
        //_comparer = comparer;
    }

    public string Text { get; }
    public byte[] TextBytes { get; }

    public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<string> result)
    {
        context.EnterParser(this);

        if (reader.Skip(new ReadOnlySpan<byte>(TextBytes))) // ReadText(Text, _comparer))
        {
            result.Set(Text);
            return true;
        }

        return false;
    }
    public override string ToString() => $"TextLiteral '{Text}'";
}
