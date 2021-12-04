﻿namespace Paspan.Fluent;

public static partial class Parsers
{

    /// <summary>
    /// Builds a parser that ensure the specified parsers match consecutively. The last parser's result is then ignored.
    /// </summary>
    public static Parser<T2> SkipAnd<T1, T2>(this Parser<T1> parser, Parser<T2> and) => new SequenceSkipAnd<T1, T2>(parser, and);

    /// <summary>
    /// Builds a parser that ensure the specified parsers match consecutively. The last parser's result is then ignored.
    /// </summary>
    public static Parser<(T1, T3)> SkipAnd<T1, T2, T3>(this Parser<(T1, T2)> parser, Parser<T3> and) => new SequenceSkipAnd<T1, T2, T3>(parser, and);

    /// <summary>
    /// Builds a parser that ensure the specified parsers match consecutively. The last parser's result is then ignored.
    /// </summary>
    public static Parser<(T1, T2, T4)> SkipAnd<T1, T2, T3, T4>(this Parser<(T1, T2, T3)> parser, Parser<T4> and) => new SequenceSkipAnd<T1, T2, T3, T4>(parser, and);

    /// <summary>
    /// Builds a parser that ensure the specified parsers match consecutively. The last parser's result is then ignored.
    /// </summary>
    public static Parser<(T1, T2, T3, T5)> SkipAnd<T1, T2, T3, T4, T5>(this Parser<(T1, T2, T3, T4)> parser, Parser<T5> and) => new SequenceSkipAnd<T1, T2, T3, T4, T5>(parser, and);

    /// <summary>
    /// Builds a parser that ensure the specified parsers match consecutively. The last parser's result is then ignored.
    /// </summary>
    public static Parser<(T1, T2, T3, T4, T6)> SkipAnd<T1, T2, T3, T4, T5, T6>(this Parser<(T1, T2, T3, T4, T5)> parser, Parser<T6> and) => new SequenceSkipAnd<T1, T2, T3, T4, T5, T6>(parser, and);

    /// <summary>
    /// Builds a parser that ensure the specified parsers match consecutively. The last parser's result is then ignored.
    /// </summary>
    public static Parser<(T1, T2, T3, T4, T5, T7)> SkipAnd<T1, T2, T3, T4, T5, T6, T7>(this Parser<(T1, T2, T3, T4, T5, T6)> parser, Parser<T7> and) => new SequenceSkipAnd<T1, T2, T3, T4, T5, T6, T7>(parser, and);

    /// <summary>
    /// Builds a parser that ensure the specified parsers match consecutively. The last parser's result is then ignored.
    /// </summary>
    public static Parser<(T1, T2, T3, T4, T5, T6, T8)> SkipAnd<T1, T2, T3, T4, T5, T6, T7, T8>(this Parser<(T1, T2, T3, T4, T5, T6, T7)> parser, Parser<T8> and) => new SequenceSkipAnd<T1, T2, T3, T4, T5, T6, T7, T8>(parser, and);
}
