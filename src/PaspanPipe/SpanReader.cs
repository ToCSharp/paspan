using Paspan.Common;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using static Paspan.Common.Constants;

namespace Paspan;

/// <summary>
/// This class is used to return tokens extracted from the input buffer.
/// </summary>
public ref partial struct SpanReader
{
    StreamPipeReader? _reader;
    ReadOnlySpan<byte> _buffer;
    bool _isFinalBlock;
    int _consumed;
    long _prevSpansLength;
    bool _isLastSegment;
    bool _isMultiSegment;
    int _updateBufferIteration;
    SequencePosition _nextPosition;
    SequencePosition _currentPosition;
    ReadOnlySequence<byte> _sequence;
    long _toAdvance;
    int _segmentInd;
    public int _sleepTime;
    static int _readTimeOut = 3000;

    bool IsLastSpan => _isFinalBlock && (!_isMultiSegment || _isLastSegment);

    public ReadOnlySpan<byte> ValueSpan { get; private set; }
    public bool HasValueSequence { get; private set; }
    public ReadOnlySequence<byte> ValueSequence { get; private set; }

    public SpanReader(string text)
        : this(Encoding.UTF8.GetBytes(text).AsSpan())
    {
    }

    public SpanReader(byte[] data)
        : this(data.AsSpan())
    {
    }

    public SpanReader(Span<byte> data)
    {
        _reader = null;
        _consumed = 0;
        _prevSpansLength = 0;
        ValueSpan = ReadOnlySpan<byte>.Empty;
        HasValueSequence = false;
        ValueSequence = ReadOnlySequence<byte>.Empty;
        _buffer = data;
        _sequence = default;
        _isFinalBlock = true;
        _isLastSegment = true;
        _isMultiSegment = false;
        _nextPosition = default;
        _currentPosition = default;
        _updateBufferIteration = 0;
        _toAdvance = 0;
        _segmentInd = 0;
        _sleepTime = 0;
    }

    public SpanReader(StreamPipeReader Reader)
    {
        _reader = Reader;
        _consumed = 0;
        _prevSpansLength = 0;
        ValueSpan = ReadOnlySpan<byte>.Empty;
        HasValueSequence = false;
        ValueSequence = ReadOnlySequence<byte>.Empty;
        _buffer = default;
        _sequence = default;
        _isFinalBlock = default;
        _isLastSegment = default;
        _isMultiSegment = default;
        _nextPosition = default;
        _currentPosition = default;
        _updateBufferIteration = 0;
        _toAdvance = 0;
        _segmentInd = 0;
        _sleepTime = 0;
    }

    public bool ReaderReadMoreBytes()
    {
        ArgumentNullException.ThrowIfNull(_reader);
        var totalConsumed = (int)_prevSpansLength + _consumed;
        var notAdvancedButConsumed = totalConsumed - (int)_toAdvance;
        //var prevLength = (_isMultiSegment ? _sequence.Length : _buffer.Length) - _toAdvance;
        var prevLength = _sequence.Length - _toAdvance;
        if (_sequence.Length > 0)
        {
            _reader.AdvanceTo(_sequence.GetPosition(_toAdvance));
            _toAdvance = 0;
        }
        var res = ResetReader();
        // TODO: для коннектора надо бесконечно ждать, StreamPipeReader надо доработать, для чтения файла, который будет дописываться
        var timeOut = DateTime.UtcNow.AddMilliseconds(_readTimeOut);
        while (res)
        {
            var newLength = _isMultiSegment ? _sequence.Length : _buffer.Length;
            if (newLength <= prevLength)
            {
                if (_isLastSegment)
                    return false;
                if (DateTime.UtcNow > timeOut)
                    throw new Exception("Could not read from stream");
                Thread.Sleep(1);
                _sleepTime += 20;
                _reader.AdvanceTo(_sequence.GetPosition(0));
                res = ResetReader();
                _updateBufferIteration--;
            }
            else
            {
                if (notAdvancedButConsumed > 0)
                    MoveConsumedTo(notAdvancedButConsumed);
                break;
            }
        }
        return res;
    }

    void MoveConsumedTo(int totalConsumed)
    {
        if (!_isMultiSegment)
        {
            _consumed = totalConsumed;
        }
        else
        {
            ReadOnlyMemory<byte> memory = default;
            while (_prevSpansLength + _buffer.Length <= totalConsumed)
            {
                _currentPosition = _nextPosition;
                _prevSpansLength += _buffer.Length;
                _isLastSegment = !_sequence.TryGet(ref _nextPosition, out memory, advance: true) && _isFinalBlock;
                if (memory.Length != 0)
                {
                    _buffer = memory.Span;
                }
            }
            _consumed = totalConsumed - (int)_prevSpansLength;
        }

    }

    public bool ResetReader()
    {
        ArgumentNullException.ThrowIfNull(_reader);
        // TODO: _readTimeOut
        var timeOut = DateTime.UtcNow.AddMilliseconds(_readTimeOut);
        while (!_reader.Read(out _sequence, out _isFinalBlock))
        {
            if (DateTime.UtcNow > timeOut)
                return false;
            Thread.Sleep(20);
            _sleepTime += 20;
        }
        _segmentInd = 0;
        _updateBufferIteration++;
        _buffer = _sequence.First.Span;
        _consumed = 0;
        _prevSpansLength = 0;
        if (_sequence.IsSingleSegment)
        {
            _nextPosition = default;
            _currentPosition = _sequence.Start;
            _isLastSegment = _isFinalBlock;
            _isMultiSegment = false;
        }
        else
        {
            _currentPosition = _sequence.Start;
            _nextPosition = _currentPosition;
            bool firstSegmentIsEmpty = _buffer.Length == 0;
            if (firstSegmentIsEmpty)
            {
                // Once we find a non-empty segment, we need to set current position to it.
                // Therefore, track the next position in a copy before it gets advanced to the next segment.
                SequencePosition previousNextPosition = _nextPosition;
                while (_sequence.TryGet(ref _nextPosition, out ReadOnlyMemory<byte> memory, advance: true))
                {
                    // _currentPosition should point to the segment right befor the segment that _nextPosition points to.
                    _currentPosition = previousNextPosition;
                    if (memory.Length != 0)
                    {
                        _buffer = memory.Span;
                        break;
                    }
                    previousNextPosition = _nextPosition;
                }
            }
            // If firstSegmentIsEmpty is true,
            //    only check if we have reached the last segment but do not advance _nextPosition. The while loop above already advanced it.
            //    Otherwise, we would end up skipping a segment (i.e. advance = false).
            // If firstSegmentIsEmpty is false,
            //    make sure to advance _nextPosition so that it is no longer the same as _currentPosition (i.e. advance = true).
            _isLastSegment = !_sequence.TryGet(ref _nextPosition, out _, advance: !firstSegmentIsEmpty) && _isFinalBlock; // Don't re-order to avoid short-circuiting

            _isMultiSegment = true;
        }
        return true;
    }

    /// <summary>
    /// Readed till current point and can be advanced in StreamPipeReader
    /// </summary>
    public void Advance()
    {
        ArgumentNullException.ThrowIfNull(_reader);
        _toAdvance = (int)_prevSpansLength + _consumed;
        _reader.AdvanceTo(_sequence.GetPosition(_toAdvance));
        ResetReader();
        _toAdvance = 0;
        ValueSpan = ReadOnlySpan<byte>.Empty;
        HasValueSequence = false;
        ValueSequence = ReadOnlySequence<byte>.Empty;
    }

    public void SetValue(SequencePosition start, SequencePosition end)
    {
        if (_isMultiSegment)
        {
            HasValueSequence = true;
            ValueSequence = _sequence.Slice(start, end);
        }
        else
        {
            HasValueSequence = false;
            ValueSpan = _buffer[start.GetInteger()..end.GetInteger()];
        }
    }
    public ReadOnlySequence<byte> Slice(SequencePosition start, long length)
        => _sequence.Slice(start, length);
    public ReadOnlySequence<byte> Slice(SequencePosition start, SequencePosition end)
        => _isMultiSegment ? _sequence.Slice(start, end)
        : new ReadOnlySequence<byte>(_buffer[start.GetInteger()..end.GetInteger()].ToArray());
    public SequencePosition GetCurrentPosition()
    {
        if (_isMultiSegment)
            return new SequencePosition(_currentPosition.GetObject(), _currentPosition.GetInteger() + _consumed);
        return new SequencePosition(null, _consumed);
    }
    public SequencePosition GetCurrentPosition(int offset)
    {
        if (_isMultiSegment)
            // TODO:
            return new SequencePosition(_currentPosition.GetObject(), _currentPosition.GetInteger() + _consumed + offset);
        return new SequencePosition(null, _consumed + offset);
    }
    public SequencePosition GetPosition(PartialStateForRollback state, int offset = 0)
    {
        if (state._updateBufferIteration == _updateBufferIteration)
        {
            return new SequencePosition(state._prevCurrentPosition.GetObject(), state._prevCurrentPosition.GetInteger() + state._prevConsumed + offset);
        }
        else
        {
            if (_updateBufferIteration - state._updateBufferIteration > 1)
            {
                // _toAdvance could be wrong if multiple ReaderReadMoreBytes happened
                // TODO: Make StatesPool with SaveState, ReleaseState and recalculate with _toAdvance when 
                //       _reader.AdvanceTo(_sequence.GetPosition(_toAdvance));
                throw new NotImplementedException("Make StatesPool");
            }

            var pos = (int)state._prevSpansLength + state._prevConsumed - (int)state._toAdvance + offset;
            if (!_isMultiSegment)
            {
                return new SequencePosition(null, pos);
            }
            else
            {
                return _sequence.GetPosition(pos, _sequence.Start);
            }
        }
    }

    public bool Skip(byte b)
    {
        if (_isMultiSegment)
            return SkipMultiSegment(b);
        if (_consumed >= _buffer.Length)
        {
            if (IsLastSpan)
            {
                return false;
            }
            else
            {
                if (!ReaderReadMoreBytes())
                    return false;
                return Skip(b);
            }
        }

        if (b != _buffer[_consumed])
        {
            return false;
        }
        _consumed++;
        return true;
    }
    //public bool PrevByteIs(byte b)
    //{
    //    if (_isMultiSegment)
    //        return PrevByteIsMultiSegment(b);
    //    return _consumed > 0 && _buffer[_consumed] == b;
    //}
    public bool SkipOneOf(params byte[] bytes)
    {
        if (_isMultiSegment)
            return SkipOneOfMultiSegment(bytes);
        if (_consumed >= _buffer.Length)
        {
            if (IsLastSpan)
            {
                return false;
            }
            else
            {
                if (!ReaderReadMoreBytes())
                    return false;
                return SkipOneOf(bytes);
            }
        }

        var b = _buffer[_consumed];
        if (bytes.Contains(b))
        {
            _consumed++;
            return true;
        }
        return false;
    }
    public bool SkipWhile(Func<byte, bool> predicate, int maxSize)
    {
        if (_isMultiSegment)
        {
            var start = CaptureState();
            if (!SkipWhileInternalMultiSegment(predicate, maxSize))
            {
                return false;
            }
            var end = GetCurrentPosition();
            var v = _sequence.Slice(GetPosition(start), end);
            if (v.IsSingleSegment)
            {
                HasValueSequence = false;
                ValueSpan = v.First.Span;
            }
            else
            {
                HasValueSequence = true;
                ValueSequence = v;
            }
        }
        else
        {
            var start = _consumed;
            if (!SkipWhileInternal(predicate, maxSize))
            {
                return false;
            }
            HasValueSequence = false;
            ValueSpan = _buffer.Slice(start, _consumed - start);
        }
        return true;
    }
    bool SkipWhileInternal(Func<byte, bool> predicate, int maxSize)
    {
        if (_isMultiSegment)
            return SkipWhileInternalMultiSegment(predicate, maxSize);
        var length = 0;
        while (true)
        {
            if (_consumed >= _buffer.Length)
            {
                if (IsLastSpan)
                {
                    return true;
                }
                else
                {
                    if (!ReaderReadMoreBytes())
                        return false;
                    return SkipWhileInternal(predicate, maxSize - length);
                }
            }
            if (predicate(_buffer[_consumed]))
            {
                _consumed++;
                length++;
                if (length == maxSize)
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        return true;
    }

    public bool SkipZeroOrOne(byte b)
    {
        if (_isMultiSegment)
            return SkipZeroOrOneMultiSegment(b);
        if (_consumed >= _buffer.Length)
        {
            if (IsLastSpan)
            {
                return true;
            }
            else
            {
                if (!ReaderReadMoreBytes())
                    return false;
                return SkipZeroOrOne(b);
            }
        }

        if (b == _buffer[_consumed])
        {
            _consumed++;
        }
        return true;
    }
    public bool Skip(ReadOnlySpan<byte> bytes)
    {
        if (_isMultiSegment)
            return SkipBytesMultiSegment(bytes);
        if (_consumed + bytes.Length > _buffer.Length)
        {
            if (IsLastSpan)
            {
                return false;
            }
            else
            {
                if (!ReaderReadMoreBytes())
                    return false;
                return Skip(bytes);
            }
        }
        if (!_buffer.Slice(_consumed).StartsWith(bytes))
            return false;
        _consumed += bytes.Length;
        return true;
    }

    // TODO: remove allocation
    public bool Skip(string str) => Skip(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(str)));

    public bool Read(int count)
    {
        if (_isMultiSegment)
            return ReadMultiSegment(count);
        if (_consumed + count > _buffer.Length)
        {
            if (IsLastSpan)
            {
                return false;
            }
            else
            {
                if (!ReaderReadMoreBytes())
                    return false;
                return Read(count);
            }
        }
        HasValueSequence = false;
        ValueSpan = _buffer.Slice(_consumed, count);
        _consumed += count;
        return true;
    }

    public bool Peek(int count)
    {
        if (_isMultiSegment)
            return PeekMultiSegment(count);
        if (_consumed + count > _buffer.Length)
        {
            if (IsLastSpan)
            {
                return false;
            }
            else
            {
                if (!ReaderReadMoreBytes())
                    return false;
                return Read(count);
            }
        }
        HasValueSequence = false;
        ValueSpan = _buffer.Slice(_consumed, count);
        return true;
    }
    public bool PeekBack(int count)
    {
        if (_isMultiSegment)
            return PeekMultiSegment(count);
        if (_consumed < count)
        {
            if (IsLastSpan)
            {
                return false;
            }
        }
        HasValueSequence = false;
        ValueSpan = _buffer.Slice(_consumed - count, count);
        return true;
    }

    public void SkipWhiteSpace()
    {
        if (_isMultiSegment)
            SkipWhiteSpaceMultiSegment();
        // Create local copy to avoid bounds checks.
        ReadOnlySpan<byte> localBuffer = _buffer;
        for (; _consumed < localBuffer.Length; _consumed++)
        {
            byte val = localBuffer[_consumed];

            // JSON RFC 8259 section 2 says only these 4 characters count, not all of the Unicode definitions of whitespace.
            if (val != Constants.Space &&
                val != Constants.Tab)
            {
                break;
            }
        }
    }
    public void SkipWhiteSpaceOrNewLine()
    {
        if (_isMultiSegment)
            SkipWhiteSpaceOrNewLineMultiSegment();
        // Create local copy to avoid bounds checks.
        ReadOnlySpan<byte> localBuffer = _buffer;
        for (; _consumed < localBuffer.Length; _consumed++)
        {
            byte val = localBuffer[_consumed];

            // JSON RFC 8259 section 2 says only these 4 characters count, not all of the Unicode definitions of whitespace.
            if (val != Constants.Space &&
                val != Constants.CarriageReturn &&
                val != Constants.LineFeed &&
                val != Constants.Tab)
            {
                break;
            }
        }
    }

    public void ReadNonWhiteSpace()
    {
        throw new NotImplementedException(nameof(ReadNonWhiteSpace));
    }
    public void ReadNonWhiteSpaceOrNewLine()
    {
        throw new NotImplementedException(nameof(ReadNonWhiteSpaceOrNewLine));
    }

    public bool Eof()
    {
        if (_isMultiSegment)
            return EofMultiSegment();
        if (_consumed >= _buffer.Length)
        {
            if (IsLastSpan)
            {
                return true;
            }
            else
            {
                if (!ReaderReadMoreBytes())
                    return false;
                return Eof();
            }
        }
        return false;
    }

    public bool ReadToEof()
    {
        //var start = CaptureState();
        //var res = MoveToEof();
        //var end = GetPosition();
        if (_isMultiSegment)
        {
            var start = CaptureState();
            MoveToEof();
            var end = GetCurrentPosition();
            var v = _sequence.Slice(GetPosition(start), end);
            if (v.IsSingleSegment)
            {
                HasValueSequence = false;
                ValueSpan = v.First.Span;
            }
            else
            {
                HasValueSequence = true;
                ValueSequence = v;
            }
        }
        else
        {
            var start = _consumed;
            MoveToEof();
            HasValueSequence = false;
            ValueSpan = _buffer.Slice(start, _consumed - start);
        }
        return true;
    }

    public bool MoveToEof()
    {
        if (_isMultiSegment)
            return MoveToEofMultiSegment();
        if (IsLastSpan)
        {
            _consumed = _buffer.Length;
            return true;
        }
        else
        {
            if (!ReaderReadMoreBytes())
                return false;
            return MoveToEof();
        }
    }

    // TODO:
    //Not correct
    public bool ReadBefore(byte b, int maxLen)
    {
        var start = CaptureState();
        if (SkipBeforeInternal(b, maxLen))
        {
            var startNow = GetPosition(start);
            var end = GetCurrentPosition();
            if (_isMultiSegment)
            {
                var v = _sequence.Slice(startNow, end);
                if (v.IsSingleSegment)
                {
                    HasValueSequence = false;
                    ValueSpan = v.FirstSpan;
                    if (ValueSpan.Length == 0)
                    {

                    }
                    else
                    {
                        var ch = (char)ValueSpan[0];
                        if (ch == '-')
                        {

                        }

                    }
                }
                else
                {
                    HasValueSequence = true;
                    ValueSequence = v;
                }
            }
            else
            {
                if (start._updateBufferIteration == _updateBufferIteration)
                {
                    HasValueSequence = false;
                    ValueSpan = _buffer.Slice(start._prevConsumed, _consumed - start._prevConsumed);
                    var ch = (char)ValueSpan[0];
                    if (ch == '-')
                    {

                    }

                }
                else
                {
                    HasValueSequence = false;
                    ValueSpan = _buffer.Slice(startNow.GetInteger(), end.GetInteger());

                }
            }
            return true;
        }
        else
        {
            RollBackState(start);
            return false;
        }
    }

    public bool SkipBefore(byte b, int maxLen)
    {
        var start = CaptureState();
        if (!SkipBeforeInternal(b, maxLen))
        {
            RollBackState(start);
            return false;
        }
        return true;
    }

    bool SkipBeforeInternal(byte b, int maxLen)
    {
        if (_isMultiSegment)
            return SkipBeforeMultiSegment(b, maxLen);
        for (int i = 0; i < maxLen; i++)
        {
            if (_consumed >= _buffer.Length)
            {
                if (IsLastSpan)
                {
                    return false;
                }
                else
                {
                    if (!ReaderReadMoreBytes())
                        return false;
                    return SkipBeforeInternal(b, maxLen - i);
                }
            }
            if (_buffer[_consumed] == b)
            {
                _consumed++;
                return true;
            }
            _consumed++;
        }
        return false;
    }

    public bool ConsumeIntegerDigits()
    {
        if (_isMultiSegment)
            return ConsumeIntegerDigitsMultiSegment();

        byte nextByte = default;
        var i = _consumed;
        for (; i < _buffer.Length; i++)
        {
            nextByte = _buffer[i];
            if (!SpanReaderHelpers.IsDigit(nextByte))
            {
                break;
            }
        }
        if (i >= _buffer.Length)
        {
            if (IsLastSpan)
            {
                // A payload containing a single value of integers (e.g. "12") is valid
                // If we are dealing with multi-value JSON,
                // ConsumeNumber will validate that we have a delimiter following the integer.
            }
            else
            {
                if (!ReaderReadMoreBytes())
                    return false;
                return ConsumeIntegerDigits();
            }
        }
        if (i > _consumed)
        {
            var length = i - _consumed;
            ValueSpan = _buffer.Slice(_consumed, length);
            _consumed = i;
            return true;
        }
        return false;
    }


    public bool ConsumeDecimalDigits()
    {
        if (_isMultiSegment)
            throw new NotImplementedException(nameof(ReadQuotedString));
        if (!ConsumeIntegerDigits())
            return false;
        if (_consumed < _buffer.Length)
        {
            if (_buffer[_consumed] == '.' || _buffer[_consumed] == ',')
            {
                var c = _consumed;
                _consumed++;
                if (!ConsumeIntegerDigits())
                {
                    _consumed = c;
                }
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadSingleQuotedString()
    {
        return ReadQuotedString(SingleQuote);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadDoubleQuotedString()
    {
        return ReadQuotedString(Quote);
    }

    public bool ReadQuotedString()
    {
        if (_consumed >= _buffer.Length)
            return false;
        var startChar = _buffer[_consumed];

        if (startChar != SingleQuote && startChar != Quote)
        {
            return false;
        }

        return ReadQuotedString(startChar);
    }

    /// <summary>
    /// Reads a string token enclosed in single or double quotes.
    /// </summary>
    /// <remarks>
    /// This method doesn't escape the string, but only validates its content is syntactically correct.
    /// The resulting Span contains the original quotes.
    /// </remarks>
    private bool ReadQuotedString(byte quoteChar)
    {
        if (_isMultiSegment)
            throw new NotImplementedException(nameof(ReadQuotedString));

        // minimum ""
        if (_consumed + 1 >= _buffer.Length)
            return false;
        var startChar = _buffer[_consumed];

        if (startChar != quoteChar)
        {
            return false;
        }

        // Fast path if there aren't any escape char until next quote
        var startOffset = _consumed + 1;

        var nextQuote = _buffer[startOffset..].IndexOf(startChar);

        if (nextQuote == -1)
        {
            // There is no end quote, not a string
            return false;
        }

        var start = _consumed;

        _consumed++;

        var nextEscape = _buffer.Slice(startOffset, nextQuote - startOffset).IndexOf(BackSlash);

        // If the next escape if not before the next quote, we can return the string as-is
        if (nextEscape == -1 || nextEscape > nextQuote)
        {
            var length = nextQuote + 1 - startOffset;
            _consumed += length;

            ValueSpan = _buffer.Slice(start, length);
            return true;
        }

        while (_buffer[_consumed] != startChar)
        {
            // We can read Eof if there is an escaped quote sequence and no actual end quote, e.g. "'abc\'def"
            if (_consumed >= _buffer.Length)
            {
                return false;
            }

            if (_buffer[_consumed] == BackSlash)
            {
                _consumed++;

                switch (_buffer[_consumed])
                {
                    case (byte)'0':
                    case BackSlash:
                    case (byte)'b':
                    case (byte)'f':
                    case (byte)'n':
                    case (byte)'r':
                    case (byte)'t':
                    case (byte)'v':
                    case SingleQuote:
                    case Quote:
                        break;

                    case (byte)'u':

                        // https://stackoverflow.com/a/32175520/142772
                        // exactly 4 digits

                        var isValidUnicode = false;

                        _consumed++;

                        if (_consumed < _buffer.Length && Character.IsHexDigit(_buffer[_consumed]))
                        {
                            _consumed++;
                            if (_consumed < _buffer.Length && Character.IsHexDigit(_buffer[_consumed]))
                            {
                                _consumed++;
                                if (_consumed < _buffer.Length && Character.IsHexDigit(_buffer[_consumed]))
                                {
                                    _consumed++;
                                    if (_consumed < _buffer.Length && Character.IsHexDigit(_buffer[_consumed]))
                                    {
                                        isValidUnicode = true;
                                    }
                                }
                            }
                        }

                        if (!isValidUnicode)
                        {
                            _consumed = start;

                            return false;
                        }

                        break;
                    case (byte)'x':

                        // https://stackoverflow.com/a/32175520/142772
                        // exactly 4 digits

                        bool isValidHex = false;

                        _consumed++;

                        if (_consumed < _buffer.Length && Character.IsHexDigit(_buffer[_consumed]))
                        {
                            isValidHex = true;
                            _consumed++;
                            if (_consumed < _buffer.Length && Character.IsHexDigit(_buffer[_consumed]))
                            {
                                _consumed++;

                                if (_consumed < _buffer.Length && Character.IsHexDigit(_buffer[_consumed]))
                                {
                                    _consumed++;

                                    if (_consumed < _buffer.Length && Character.IsHexDigit(_buffer[_consumed]))
                                    {
                                        isValidUnicode = true;
                                    }
                                }
                            }
                        }

                        if (!isValidHex)
                        {
                            _consumed = start;

                            return false;
                        }

                        break;
                    default:
                        _consumed = start;

                        return false;
                }
            }

            _consumed++;
        }

        _consumed++;

        ValueSpan = _buffer.Slice(start, _consumed - start);

        return true;
    }

}
