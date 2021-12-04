using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Text;

namespace Paspan;

/// <summary>
/// This class is used to return tokens extracted from the input buffer.
/// </summary>
public ref partial struct SpanReader
{
    public int ValueLength => HasValueSequence ? (int)ValueSequence.Length : ValueSpan.Length;
    public byte[] GetValueBytes() => HasValueSequence ? ValueSequence.ToArray() : ValueSpan.ToArray();
    public ReadOnlySpan<byte> GetValue() => HasValueSequence ? ValueSequence.ToArray() : ValueSpan;
    public ReadOnlySequence<byte> GetValueAsSequence() => HasValueSequence ? ValueSequence : new ReadOnlySequence<byte>(ValueSpan.ToArray());
    public bool TryGetBytes(out byte[] value)
    {
        value = HasValueSequence ? ValueSequence.ToArray() : ValueSpan.ToArray();
        return true;
    }
    public byte[] GetBytes() => HasValueSequence ? ValueSequence.ToArray() : ValueSpan.ToArray();

    public string? GetString()
    {
        if (TryGetString(out var v))
        {
            return v;
        }
        return null;
    }
    public long IndexOf(ReadOnlySequence<byte> sequence, byte b)
    {
        var pos = sequence.PositionOf(b);
        if (pos.HasValue)
        {
            return sequence.Slice(0, pos.Value).Length;
        }
        return -1;
    }
    //public long IndexOf(ReadOnlySequence<byte> sequence, ReadOnlySpan<byte> span)
    //{
    //    for (int i = 0; i < sequence.Length; i++) {
    //        if (sequence.Slice(i).StartsWith(span)) {
    //            return i;
    //        }
    //    }
    //    return -1;
    //}

    //public ReadOnlyMemory<byte> GetSequensSegment(int ind)
    //{
    //    return new System.Buffers.ReadOnlySequenceDebugView<byte>(this._sequence).BufferSegments.Segments[ind];
    //}
    public string GetString(ReadOnlySpan<byte> span) => Encoding.UTF8.GetString(span);
    public string GetString(ReadOnlySequence<byte> sequence) => GetString(sequence, 0, (int)sequence.Length);
    public string GetString(ReadOnlySequence<byte> sequence, int start, int length)
    {
        return Encoding.UTF8.GetString(sequence.Slice(start, length));
    }
    public string GetCurrentPosString(int length = 100) => GetString(_buffer.Slice(_consumed, Math.Min(length, _buffer.Length - _consumed)));
    public bool TryGetString(out string value)
    {
        value = HasValueSequence ? Encoding.UTF8.GetString(ValueSequence) : Encoding.UTF8.GetString(ValueSpan);
        return true;
    }
    public bool TryGetString(SequencePosition start, SequencePosition end, out string value)
    {
        if (HasValueSequence)
        {
            value = Encoding.UTF8.GetString(_sequence.Slice(start, end));
        }
        else
        {
            var s = start.GetInteger();
            var length = end.GetInteger() - s;
            value = length > 0 ? Encoding.UTF8.GetString(_buffer.Slice(s, length)) : "";
        }
        return true;
    }
    public bool TryGetString(SequencePosition start, int length, out string value)
    {
        if (HasValueSequence)
        {
            value = Encoding.UTF8.GetString(_sequence.Slice(start, length));
        }
        else
        {
            var s = start.GetInteger();
            value = length > 0 ? Encoding.UTF8.GetString(_buffer.Slice(s, length)) : "";
        }
        return true;
    }
    public bool TryGetInt32(out int value)
    {
        ReadOnlySpan<byte> span = HasValueSequence ? ValueSequence.ToArray() : ValueSpan;
        return TryGetInt32Core(out value, span);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetInt32Core(out int value, ReadOnlySpan<byte> span)
    {
        if (Utf8Parser.TryParse(span, out int tmp, out int bytesConsumed)
            && span.Length == bytesConsumed)
        {
            value = tmp;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetInt64(out long value)
    {
        ReadOnlySpan<byte> span = HasValueSequence ? ValueSequence.ToArray() : ValueSpan;
        return TryGetInt64Core(out value, span);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetInt64Core(out long value, ReadOnlySpan<byte> span)
    {
        if (Utf8Parser.TryParse(span, out long tmp, out int bytesConsumed)
            && span.Length == bytesConsumed)
        {
            value = tmp;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetHex(out ulong value)
    {
        ReadOnlySpan<byte> span = HasValueSequence ? ValueSequence.ToArray() : ValueSpan;
        return TryGetHexCore(out value, span);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetHexCore(out ulong value, ReadOnlySpan<byte> span)
    {
        if (Utf8Parser.TryParse(span, out ulong tmp, out int bytesConsumed, 'x')
            && span.Length == bytesConsumed)
        {
            value = tmp;
            return true;
        }

        value = 0;
        return false;
    }

    public bool TryGetDecimal(out decimal value) => TryGetDecimalCore(out value,
        _isMultiSegment ? ValueSequence.ToArray() : ValueSpan);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TryGetDecimalCore(out decimal value, ReadOnlySpan<byte> span)
    {
        if (Utf8Parser.TryParse(span, out decimal tmp, out int bytesConsumed)
            && span.Length == bytesConsumed)
        {
            value = tmp;
            return true;
        }

        value = 0;
        return false;
    }


    public bool TryGetChar(out char value)
    {
        if (TryGetString(out var str) && str.Length == 1)
        {
            value = str[0];
            return true;
        }
        value = default;
        return false;
    }

}
