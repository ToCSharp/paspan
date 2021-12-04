namespace Paspan.Tests.Calc
{
    public class ParserTests : CalcTests
    {
        protected override decimal Evaluate(string text)
        {
            return new Parser().Parse(text).Evaluate();
        }
    }
}
