using ConsoleApp1;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

Expression<Func<int, int>> itExpression = DynamicExpressionParser
    .ParseLambda<int, int>(new ParsingConfig(), true, "Math.Max(@0, @1)", 1, 2);


var rpnTokens = MathStringParser.ParseToRPN("a+5*13+b");
rpnTokens = MathStringParser.ParseToRPN("2^3");
rpnTokens = MathStringParser.ParseToRPN("a*(b+c)");
rpnTokens = MathStringParser.ParseToRPN("a * (b + c * d) + e");
rpnTokens = MathStringParser.ParseToRPN("sin(max(2,3)/3*pi)");
rpnTokens = MathStringParser.ParseToRPN("3+max(2,3)");
rpnTokens = MathStringParser.ParseToRPN("3+sin(2)");
rpnTokens = MathStringParser.ParseToRPN("pi*2");

Console.WriteLine(MathStringParser.PrintTokens(rpnTokens));
var compiledLambda = ETBuilder.BuildExpression(rpnTokens);
var result = (double?)compiledLambda.DynamicInvoke();

Console.WriteLine($"Result: {result}");
