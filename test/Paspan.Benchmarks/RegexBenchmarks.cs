using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System.Text;
using System.Text.RegularExpressions;

namespace ParlotNS
{
    using Parlot;
    using Parlot.Fluent;
    using static Parlot.Fluent.Parsers;
    public static class ParlotRegex
    {
        public static readonly Parser<char> Dot = Literals.Char('.');
        public static readonly Parser<char> Plus = Literals.Char('+');
        public static readonly Parser<char> Minus = Literals.Char('-');
        public static readonly Parser<char> At = Literals.Char('@');
        public static readonly Parser<TextSpan> WordChar = Literals.Pattern(char.IsLetterOrDigit);
        public static readonly Parser<List<char>> WordDotPlusMinus = OneOrMany(OneOf(WordChar.Then(x => 'w'), Dot, Plus, Minus));
        public static readonly Parser<List<char>> WordDotMinus = OneOrMany(OneOf(WordChar.Then(x => 'w'), Dot, Minus));
        public static readonly Parser<List<char>> WordMinus = OneOrMany(OneOf(WordChar.Then(x => 'w'), Minus));
        public static readonly Parser<TextSpan> Email = Capture(WordDotPlusMinus.And(At).And(WordMinus).And(Dot).And(WordDotMinus));

        public static readonly Parser<TextSpan> EmailCompiled = Email.Compile();
    }
}
namespace PaspanNS
{
    using Paspan;
    using Paspan.Fluent;
    using static Paspan.Fluent.Parsers;
    public static class PaspanRegex
    {
        public static readonly Parser<Unit> Dot = Literals.Char('.');
        public static readonly Parser<Unit> Plus = Literals.Char('+');
        public static readonly Parser<Unit> Minus = Literals.Char('-');
        public static readonly Parser<Unit> At = Literals.Char('@');
        public static readonly Parser<Unit> WordChar = Literals.Pattern(b => char.IsLetterOrDigit((char)b));
        public static readonly Parser<List<Unit>> WordDotPlusMinus = OneOrMany(OneOf(WordChar, Dot, Plus, Minus));
        public static readonly Parser<List<Unit>> WordDotMinus = OneOrMany(OneOf(WordChar, Dot, Minus));
        public static readonly Parser<List<Unit>> WordMinus = OneOrMany(OneOf(WordChar, Minus));
        public static readonly Parser<string> Email = Capture(WordDotPlusMinus.And(At).And(WordMinus).And(Dot).And(WordDotMinus)).AsString();
        public static readonly Parser<Region> EmailRegion = Capture(WordDotPlusMinus.And(At).And(WordMinus).And(Dot).And(WordDotMinus));

        public static bool RawIsEmail(ReadOnlySpan<byte> text)
        {
            var reader = new SpanReader(text);
            return reader.SkipWhile(static b => char.IsLetterOrDigit((char)b) || b == (byte)'.' || b == (byte)'+' || b == (byte)'-', 100)
                && reader.Skip((byte)'@')
                && reader.SkipWhile(static b => char.IsLetterOrDigit((char)b) || b == (byte)'-', 100)
                && reader.Skip((byte)'.')
                && reader.SkipWhile(static b => char.IsLetterOrDigit((char)b) || b == (byte)'.' || b == (byte)'+' || b == (byte)'-', 100);
        }
        public static string? RawEmailString(ReadOnlySpan<byte> text)
        {
            var reader = new SpanReader(text);
            var start = reader.GetCurrentPosition();
            if(reader.SkipWhile(static b => char.IsLetterOrDigit((char)b) || b == (byte)'.' || b == (byte)'+' || b == (byte)'-', 100)
                && reader.Skip((byte)'@')
                && reader.SkipWhile(static b => char.IsLetterOrDigit((char)b) || b == (byte)'-', 100)
                && reader.Skip((byte)'.')
                && reader.SkipWhile(static b => char.IsLetterOrDigit((char)b) || b == (byte)'.' || b == (byte)'+' || b == (byte)'-', 100))
                return reader.GetString(reader.GetPosition(start), reader.GetCurrentPosition());
            return null;

        }
    }
}

namespace Parlot.Benchmarks
{
    [MemoryDiagnoser, GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory), ShortRunJob]
    public class RegexBenchmarks
    {
        public static readonly Regex EmailRegex = new("[\\w\\.+-]+@[\\w-]+\\.[\\w\\.-]+");
        public static readonly Regex EmailRegexCompiled = new("[\\w\\.+-]+@[\\w-]+\\.[\\w\\.-]+", RegexOptions.Compiled);



        private static readonly string _email = "pa.span@unknown.com";
        private static readonly byte[] _emailBytes = Encoding.UTF8.GetBytes("pa.span@unknown.com");

        public RegexBenchmarks()
        {
            if (!RegexEmail()) throw new Exception(nameof(RegexEmail));
            if (!RegexEmailCompiled()) throw new Exception(nameof(RegexEmailCompiled));
            if (PaspanEmail() != _email) throw new Exception(nameof(PaspanEmail));
            if (PaspanEmailBytes() != _email) throw new Exception(nameof(PaspanEmailBytes));
            if (PaspanEmailRegion().ToString(_emailBytes) != _email) throw new Exception(nameof(PaspanEmailRegion));
            if (!PaspanEmailRaw() || PaspanNS.PaspanRegex.RawEmailString(_emailBytes) != _email) throw new Exception(nameof(PaspanEmailRaw));
            if (!ParlotEmail()) throw new Exception(nameof(ParlotEmail));
            if (!ParlotEmailCompiled()) throw new Exception(nameof(ParlotEmailCompiled));
        }

        [Benchmark]
        public bool RegexEmail()
        {
            return EmailRegex.Match(_email).Success;
        }

        [Benchmark]
        public bool RegexEmailCompiled()
        {
            return EmailRegexCompiled.Match(_email).Success;
        }

        [Benchmark]
        public string? PaspanEmail()
        {
            return PaspanNS.PaspanRegex.Email.Parse(_email);
        }

        [Benchmark]
        public string? PaspanEmailBytes()
        {
            return PaspanNS.PaspanRegex.Email.Parse(_emailBytes);
        }

        [Benchmark]
        public Paspan.Region PaspanEmailRegion()
        {
            return PaspanNS.PaspanRegex.EmailRegion.Parse(_emailBytes);
        }

        [Benchmark]
        public bool PaspanEmailRaw()
        {
            return PaspanNS.PaspanRegex.RawIsEmail(_emailBytes);
        }

        [Benchmark]
        public bool ParlotEmail()
        {
            return ParlotNS.ParlotRegex.Email.TryParse(_email, out _);
        }

        [Benchmark]
        public bool ParlotEmailCompiled()
        {
            return ParlotNS.ParlotRegex.EmailCompiled.Parse(_email).Length > 0;
        }
    }
}
