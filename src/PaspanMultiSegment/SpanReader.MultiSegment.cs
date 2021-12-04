using Paspan.Common;

namespace Paspan;

/// <summary>
/// This class is used to return tokens extracted from the input buffer.
/// </summary>
public ref partial struct SpanReader
{
    private bool GetNextSpan()
    {
        ReadOnlyMemory<byte> memory = default;
        while (true)
        {
            //Debug.Assert(!_isMultiSegment || _currentPosition.GetObject() != null);
            SequencePosition copy = _currentPosition;
            _currentPosition = _nextPosition;
            _segmentInd++;
            bool noMoreData = !_sequence.TryGet(ref _nextPosition, out memory, advance: true);
            if (noMoreData)
            {
                _currentPosition = copy;
                _isLastSegment = true;
                return false;
            }
            if (memory.Length != 0)
            {
                break;
            }
            // _currentPosition needs to point to last non-empty segment
            // Since memory.Length == 0, we need to revert back to previous.
            _currentPosition = copy;
            //Debug.Assert(!_isMultiSegment || _currentPosition.GetObject() != null);
        }

        if (_isFinalBlock)
        {
            _isLastSegment = !_sequence.TryGet(ref _nextPosition, out _, advance: false);
        }

        _prevSpansLength += _buffer.Length;
        _buffer = memory.Span;
        _consumed = 0;

        return true;
    }

    bool SkipMultiSegment(byte b)
    {
        PartialStateForRollback rollBackState = CaptureState();
        if (_consumed >= _buffer.Length)
        {
            if (IsLastSpan || !GetNextSpan())
            {
                RollBackState(rollBackState);
                return false;
            }
        }
        if (b != _buffer[_consumed])
        {
            return false;
        }
        _consumed++;
        return true;
    }

    bool SkipOneOfMultiSegment(params byte[] bytes)
    {
        PartialStateForRollback rollBackState = CaptureState();
        if (_consumed >= _buffer.Length)
        {
            if (IsLastSpan || !GetNextSpan())
            {
                RollBackState(rollBackState);
                return false;
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

    bool SkipWhileInternalMultiSegment(Func<byte, bool> predicate, int maxSize)
    {
        var length = 0;
        while (true)
        {
            if (_consumed >= _buffer.Length)
            {
                if (IsLastSpan)
                {
                    return true;
                }
                else if (!GetNextSpan())
                {
                    return false;
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

    bool SkipBytesMultiSegment(ReadOnlySpan<byte> bytes)
    {
        PartialStateForRollback rollBackState = CaptureState();
        for (int i = 0; i < bytes.Length; i++)
        {
            if (_consumed >= _buffer.Length)
            {
                if (IsLastSpan || !GetNextSpan())
                {
                    RollBackState(rollBackState);
                    return false;
                }
            }
            if (bytes[i] != _buffer[_consumed])
            {
                RollBackState(rollBackState);
                return false;
            }
            _consumed++;
        }
        return true;
    }

    bool SkipZeroOrOneMultiSegment(byte b)
    {
        PartialStateForRollback rollBackState = CaptureState();
        if (_consumed >= _buffer.Length)
        {
            if (IsLastSpan || !GetNextSpan())
            {
                RollBackState(rollBackState);
                return true;
            }
        }
        if (b == _buffer[_consumed])
        {
            _consumed++;
        }
        return true;
    }


    public bool ReadMultiSegment(int count)
    {
        if (_prevSpansLength + _consumed + count >= _sequence.Length)
        {
            return false;
        }
        if (_consumed + count <= _buffer.Length)
        {
            HasValueSequence = false;
            ValueSpan = _buffer.Slice(_consumed, count);
            _consumed += count;
        }
        else
        {
            var start = new SequencePosition(_currentPosition.GetObject(), _currentPosition.GetInteger() + _consumed);
            var c = count - (_buffer.Length - _consumed);
            while (c > 0)
            {
                GetNextSpan();
                _consumed = c;
                c -= _buffer.Length;
            }
            //SequencePosition end = new SequencePosition(_currentPosition.GetObject(), _consumed);
            HasValueSequence = true;
            ValueSequence = _sequence.Slice(start, count);
        }
        return true;
    }

    public bool PeekMultiSegment(int count)
    {
        if (_prevSpansLength + _consumed + count >= _sequence.Length)
        {
            return false;
        }
        if (_consumed + count <= _buffer.Length)
        {
            HasValueSequence = false;
            ValueSpan = _buffer.Slice(_consumed, count);
        }
        else
        {
            HasValueSequence = true;
            ValueSequence = _sequence.Slice(_prevSpansLength + _consumed, count);
        }
        return true;
    }

    public void SkipWhiteSpaceMultiSegment()
    {
        while (true)
        {
            if (_consumed >= _buffer.Length)
            {
                if (IsLastSpan || !GetNextSpan())
                {
                    return;
                }
            }
            byte val = _buffer[_consumed];
            if (val != Constants.Space &&
                val != Constants.Tab)
            {
                break;
            }
            _consumed++;
        }
    }

    public void SkipWhiteSpaceOrNewLineMultiSegment()
    {
        while (true)
        {
            if (_consumed >= _buffer.Length)
            {
                if (IsLastSpan || !GetNextSpan())
                {
                    return;
                }
            }
            byte val = _buffer[_consumed];
            if (val != Constants.Space &&
                val != Constants.CarriageReturn &&
                val != Constants.LineFeed &&
                val != Constants.Tab)
            {
                break;
            }
            _consumed++;
        }
    }

    public bool EofMultiSegment()
    {
        if (_consumed >= _buffer.Length)
        {
            if (IsLastSpan || !GetNextSpan())
            {
                return true;
            }
        }
        return false;
    }
    public bool MoveToEofMultiSegment()
    {
        while (true)
        {
            while (GetNextSpan()) { }
            if (IsLastSpan)
            {
                _consumed = _buffer.Length;
                return true;
            }
            else
            {
                return false;
            }

        }
    }

    public bool SkipBeforeMultiSegment(byte b, int maxLen)
    {
        //SequencePosition startPosition = _currentPosition;
        //int startConsumed = _consumed;
        //PartialStateForRollback rollBackState = CaptureState();
        //var res = false;
        //HasValueSequence = false;
        for (int i = 0; i < maxLen; i++)
        {
            if (_consumed >= _buffer.Length)
            {
                if (IsLastSpan)
                {
                    //RollBackState(rollBackState);
                    return false;
                }

                if (!GetNextSpan())
                {
                    //RollBackState(rollBackState);
                    return false;
                }
                //HasValueSequence = true;
            }
            if (_buffer[_consumed] == b)
            {
                _consumed++;
                return true;
            }
            _consumed++;

            //if (_buffer[_consumed] == b) {
            //    if (HasValueSequence) {
            //        SequencePosition start = GetPosition(rollBackState);
            //        SequencePosition end = new SequencePosition(_currentPosition.GetObject(), _currentPosition.GetInteger() + _consumed);
            //        //HasValueSequence = true;
            //        ValueSequence = _sequence.Slice(start, end);
            //    } else {
            //        //HasValueSequence = false;
            //        ValueSpan = _buffer.Slice(startConsumed, i);
            //    }
            //    res = true;
            //    _consumed++;
            //    break;
            //} else {
            //    _consumed++;
            //}
        }
        return false;
        //if (!res)
        //    RollBackState(rollBackState);
        //return res;
    }

    public bool ConsumeIntegerDigitsMultiSegment()
    {
        int startConsumed = _consumed;
        PartialStateForRollback rollBackState = CaptureState();
        var res = false;
        HasValueSequence = false;
        while (true)
        {
            if (_consumed >= _buffer.Length)
            {
                if (IsLastSpan || !GetNextSpan())
                {
                    RollBackState(rollBackState);
                    return false;
                }
                HasValueSequence = true;
            }
            if (!SpanReaderHelpers.IsDigit(_buffer[_consumed]))
            {
                if (HasValueSequence)
                {
                    SequencePosition start = GetPosition(rollBackState);
                    SequencePosition end = new SequencePosition(_currentPosition.GetObject(), _currentPosition.GetInteger() + _consumed);
                    HasValueSequence = true;
                    ValueSequence = _sequence.Slice(start, end);
                    if (ValueSequence.Length > 2000)
                    {

                    }
                }
                else
                {
                    HasValueSequence = false;
                    ValueSpan = _buffer.Slice(startConsumed, _consumed - startConsumed);
                }
                res = true;
                break;
            }
            _consumed++;
        }
        if (!res)
            RollBackState(rollBackState);
        return res;
    }

    public void RollBackState(in PartialStateForRollback state/*, bool isError = false*/)
    {
        _prevSpansLength = state._prevSpansLength;

        //// Don't roll back byte position in line for invalid JSON since that is provided
        //// to the user within the exception.
        //if (!isError) {
        //    _bytePositionInLine = state._prevBytePositionInLine;
        //}

        _consumed = state._prevConsumed;
        _currentPosition = state._prevCurrentPosition;
    }
    public PartialStateForRollback CaptureState()
    {
        return new PartialStateForRollback(_prevSpansLength, _consumed, _currentPosition);
    }
    public readonly struct PartialStateForRollback
    {
        public readonly long _prevSpansLength;
        public readonly int _prevConsumed;
        public readonly SequencePosition _prevCurrentPosition;

        public PartialStateForRollback(long prevSpansLength, int consumed, SequencePosition currentPosition)
        {
            _prevSpansLength = prevSpansLength;
            _prevConsumed = consumed;
            _prevCurrentPosition = currentPosition;
        }

        // reader.GetPosition(state) must be used
        //public SequencePosition GetStartPosition(int offset = 0)
        //{
        //    return new SequencePosition(_prevCurrentPosition.GetObject(), _prevCurrentPosition.GetInteger() + _prevConsumed + offset);
        //}
    }

}
