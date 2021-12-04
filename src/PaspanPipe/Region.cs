namespace Paspan;

public struct Region
{
    public SequencePosition Start;
    public SequencePosition End;
    public Region(SequencePosition start, SequencePosition end)
    {
        Start = start;
        End = end;
    }

    //public string? ToString(ReadOnlySpan<byte> data) => Encoding.UTF8.GetString(data.Slice(Start, Length));

    public override string ToString()
    {
        return $"Region {Start}..{End}";
    }
}
