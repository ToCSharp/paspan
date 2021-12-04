using System;
using Xunit;

namespace Paspan.Tests
{
    public class SpanReaderTests
    {
        [Theory]
        [InlineData("Lorem ipsum")]
        [InlineData("'Lorem ipsum")]
        [InlineData("Lorem ipsum'")]
        [InlineData("\"Lorem ipsum")]
        [InlineData("Lorem ipsum\"")]
        [InlineData("'Lorem ipsum\"")]
        [InlineData("\"Lorem ipsum'")]
        public void ShouldNotReadEscapedStringWithoutMatchingQuotes(string text)
        {
            SpanReader s = new(text);
            Assert.False(s.ReadQuotedString());
        }

        [Theory]
        [InlineData("'Lorem ipsum'", "Lorem ipsum")]
        [InlineData("\"Lorem ipsum\"", "Lorem ipsum")]
        public void ShouldReadEscapedStringWithMatchingQuotes(string text, string expected)
        {
            SpanReader s = new(text);
            var success = s.ReadQuotedString();
            Assert.True(success);
            Assert.Equal(expected, s.GetString());
        }

        [Theory]
        [InlineData("'Lorem \\n ipsum'", "Lorem \\n ipsum")]
        [InlineData("\"Lorem \\n ipsum\"", "Lorem \\n ipsum")]
        [InlineData("\"Lo\\trem \\n ipsum\"", "Lo\\trem \\n ipsum")]
        [InlineData("'Lorem \\u1234 ipsum'", "Lorem \\u1234 ipsum")]
        [InlineData("'Lorem \\xabcd ipsum'", "Lorem \\xabcd ipsum")]
        public void ShouldReadStringWithEscapes(string text, string expected)
        {
            SpanReader s = new(text);
            var success = s.ReadQuotedString();
            Assert.True(success);
            Assert.Equal(expected, s.GetString());
        }

        [Theory]
        [InlineData("'Lorem \\w ipsum'")]
        [InlineData("'Lorem \\u12 ipsum'")]
        [InlineData("'Lorem \\xg ipsum'")]
        public void ShouldNotReadStringWithInvalidEscapes(string text)
        {
            SpanReader s = new(text);
            Assert.False(s.ReadQuotedString());
        }

        [Fact]
        public void ReadSingleQuotedStringShouldReadSingleQuotedStrings()
        {
            SpanReader s = new("'abcd'");
            s.ReadSingleQuotedString();
            Assert.Equal("abcd", s.GetString());

            s = new("'a\\nb'");
            s.ReadSingleQuotedString();
            Assert.Equal("a\\nb", s.GetString());

            Assert.False(new SpanReader("'abcd").ReadSingleQuotedString());
            Assert.False(new SpanReader("abcd'").ReadSingleQuotedString());
            Assert.False(new SpanReader("ab\\'cd").ReadSingleQuotedString());
        }

        [Fact]
        public void ReadDoubleQuotedStringShouldReadDoubleQuotedStrings()
        {
            SpanReader s = new("\"abcd\"");
            s.ReadDoubleQuotedString();
            Assert.Equal("abcd", s.GetString());

            s = new("\"a\\nb\"");
            s.ReadDoubleQuotedString();
            Assert.Equal("a\\nb", s.GetString());

            Assert.False(new SpanReader("\"abcd").ReadDoubleQuotedString());
            Assert.False(new SpanReader("abcd\"").ReadDoubleQuotedString());
            Assert.False(new SpanReader("\"ab\\\"cd").ReadDoubleQuotedString());
        }

        [Theory]
        [InlineData("1", "1")]
        [InlineData("123", "123")]
        [InlineData("123a", "123")]
        [InlineData("123.0", "123.0")]
        [InlineData("123.0a", "123.0")]
        [InlineData("123.01", "123.01")]
        public void ShouldReadValidDecimal(string text, string expected)
        {
            SpanReader s = new(text);
            Assert.True(s.ConsumeDecimalDigits());
            Assert.Equal(expected, s.GetString());
        }

        [Theory]
        [InlineData(" 1")]
        [InlineData("123.")]
        public void ShouldNotReadInvalidDecimal(string text)
        {
            //Assert.False(new Scanner(text).ReadDecimal());
        }

        [Theory]
        [InlineData("'a\nb' ", "a\nb")]
        [InlineData("'a\r\nb' ", "a\r\nb")]
        public void ShouldReadStringsWithLineBreaks(string text, string expected)
        {
            SpanReader s = new(text);
            Assert.True(s.ReadSingleQuotedString());
            Assert.Equal(expected, s.GetString());
        }

        [Theory]
        [InlineData("'a\\bc'", "a\\bc")]
        [InlineData("'\\xa0'", "\\xa0")]
        [InlineData("'\\xfh'", "\\xfh")]
        [InlineData("'\\u1234'", "\\u1234")]
        [InlineData("' a\\bc ' ", " a\\bc ")]
        [InlineData("' \\xa0 ' ", " \\xa0 ")]
        [InlineData("' \\xfh ' ", " \\xfh ")]
        [InlineData("' \\u1234 ' ", " \\u1234 ")]

        public void ShouldReadUnicodeSequence(string text, string expected)
        {
            SpanReader s = new(text);
            s.ReadQuotedString();
            Assert.Equal(expected, s.GetString());
        }
    }
}
