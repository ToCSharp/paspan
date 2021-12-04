using System.Text;

namespace Paspan;

public struct Region
{
    public int Start;
    public int End;
    public int Length;
    public Region(int start, int end)
    {
        Start = start;
        End = end;
        Length = end - start;
    }

    public string? ToString(ReadOnlySpan<byte> data) => Encoding.UTF8.GetString(data.Slice(Start, Length));

    public override string ToString()
    {
        return $"Region {Start}..{End}";
    }
}
