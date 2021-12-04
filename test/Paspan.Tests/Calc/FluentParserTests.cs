using Paspan.Fluent;

namespace Paspan.Tests.Calc
{
    public class FluentParserTests : CalcTests
    {
        protected override decimal Evaluate(string text)
        {
            FluentParser.Expression.TryParse(text, out var expression);
            return expression.Evaluate();
        }
    }
}
