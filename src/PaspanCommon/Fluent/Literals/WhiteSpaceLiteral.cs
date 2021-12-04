﻿namespace Paspan.Fluent
{
    public sealed class WhiteSpaceLiteral : Parser<Unit>
    {
        private readonly bool _includeNewLines;

        public WhiteSpaceLiteral(bool includeNewLines = true)
        {
            _includeNewLines = includeNewLines;
        }

        public override bool Parse(ref SpanReader reader, ParseContext context, ref ParseResult<Unit> result)
        {
            if (reader.Eof())
            {
                return false;
            }

            var start = reader.CaptureState();

            if (_includeNewLines)
            {
                reader.SkipWhiteSpaceOrNewLine();
            }
            else
            {
                reader.SkipWhiteSpace();
            }

            var end = reader.GetCurrentPosition();

            if (reader.GetPosition(start).Equals(end))
            {
                return false;
            }

            reader.SetValue(reader.GetPosition(start), end);
            return true;
        }
    }
}
