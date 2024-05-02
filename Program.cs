using ConsoleApp1;
using System.Linq.Dynamic.Core;

//Expression<Func<int, int>> itExpression = DynamicExpressionParser
//    .ParseLambda<int, int>(new ParsingConfig(), true, "Math.Max(@0, @1)", 1, 2);

// Ask user for the expression.
Line();
var mathExpression = UserInput("Provide a Match expression:");

// Parse the expression.
Write();
var expressionTokens = MathStringParser.ParseToRPN(mathExpression);
Write("This is your expression in Reverse Polish Notation:");
Write(MathStringParser.PrintTokens(expressionTokens), ConsoleColor.Green);

// Compile the expression.
var compiledLambda = ETBuilder.BuildExpression(expressionTokens);

// Ask values for variables.
Write();
var variables = expressionTokens.Where(x => x.Operator == FormulaOperator.Variable);
var variablesValues = new List<double>();

foreach (var variable in variables)
{
    do
    {
        var value = UserInput($"Value for variable '{variable.VariableName}':");
        if (!double.TryParse(value, out double doubleValue))
        {
            Write("Number not recognized.", ConsoleColor.Red);
            continue;
        }
        variablesValues.Add(doubleValue);
        break;
    } while (true);
}

Write();
var result = (double?)compiledLambda.DynamicInvoke(variablesValues.ToArray());
Write($"Result: {result.Value.ToString("#,##0.00")}", ConsoleColor.Yellow);

// ==========================================================================

string UserInput(string promptText)
{
    Console.ForegroundColor = ConsoleColor.Gray;
    Write(promptText);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("> ");
    Console.ForegroundColor = ConsoleColor.Cyan;
    var result = Console.ReadLine() ?? string.Empty;
    Console.ForegroundColor = ConsoleColor.Gray;
    return result;
}

void Write(string? text = null, ConsoleColor? color = null)
{
    var currentColor = Console.ForegroundColor;
    if (color.HasValue) Console.ForegroundColor = color.Value;
    Console.WriteLine(text);
    if (color.HasValue) Console.ForegroundColor = currentColor;
}

void Line()
{
    var currentForegroundColor = Console.ForegroundColor;
    var currentBackgroundColor = Console.BackgroundColor;
    Console.ForegroundColor = ConsoleColor.Black;
    Console.BackgroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("<><><><><><><><><><><><><><><><><><><><>");
    Console.ForegroundColor = currentForegroundColor;
    Console.BackgroundColor = currentBackgroundColor;

}

//var rpnTokens = MathStringParser.ParseToRPN("a+5*13+b");
//rpnTokens = MathStringParser.ParseToRPN("2^3");
//rpnTokens = MathStringParser.ParseToRPN("a*(b+c)");
//rpnTokens = MathStringParser.ParseToRPN("a * (b + c * d) + e");
//rpnTokens = MathStringParser.ParseToRPN("sin(max(2,3)/3*pi)");
//rpnTokens = MathStringParser.ParseToRPN("3+max(2,3)");
//rpnTokens = MathStringParser.ParseToRPN("3+sin(2)");
//rpnTokens = MathStringParser.ParseToRPN("pi*2");

//Console.WriteLine(MathStringParser.PrintTokens(rpnTokens));
//var compiledLambda = ETBuilder.BuildExpression(rpnTokens);
//var result = (double?)compiledLambda.DynamicInvoke();

//Console.WriteLine($"Result: {result}");
