using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ConsoleApp1;

internal static class MathStringParser
{
    /// <summary>
    /// Expression for identifying every every term in a formula.
    /// </summary>
    static Regex ExpressionParseMatch = new Regex(
        @"\d+(\.\d+)?|[,+\-\*\(\)\/^]|[a-z_]+",
        RegexOptions.IgnoreCase);

    /// <summary>
    /// Expression for identifing variable and function names.
    /// </summary>
    static Regex ParseVariableName = new Regex(
        @"[a-z_]+",
        RegexOptions.IgnoreCase);

    /// <summary>
    /// Parses the formula string and return its terms in the Reverse Polish Notation.
    /// </summary>
    /// <param name="stringFormula"></param>
    /// <returns></returns>
    public static Token[] ParseToRPN(string stringFormula)
    {
        return MathStringParser.RPNOrganization(
            ParseMathString(stringFormula));
    }

    /// <summary>
    /// Uses Shunting Yard algorithm for sorting the elements of the expression.
    /// </summary>
    /// <param name="tokens"></param>
    /// <returns>The tokens in the Reverse Polish Notation</returns>
    static Token[] RPNOrganization(IEnumerable<Token> tokens)
    {
        var outputStack = new Stack<Token>();
        var operatorStack = new Stack<Token>();

        foreach (var token in tokens)
        {
            switch (token)
            {
                case var t when t.Operator == FormulaOperator.OpenParenthesis:
                    operatorStack.Push(token);
                    break;

                case var t when t.Operator == FormulaOperator.CloseParenthesis:
                    // Discard everything till next '('
                    Token popedToken;
                    while (true)
                    {
                        popedToken = operatorStack.Pop(); // Discard ')'
                        if (popedToken.Operator != FormulaOperator.OpenParenthesis)
                            outputStack.Push(popedToken);
                        else break;
                    }
                    // This removes the Operation.
                    outputStack.Push(operatorStack.Pop());
                    break;

                case var t when t.Operator == FormulaOperator.DoubleConstant
                    || t.Operator == FormulaOperator.Variable
                    || t.Operator == FormulaOperator.Pi:
                    outputStack.Push(token);
                    break;

                default:
                    while (operatorStack.Any()
                        && token.Precedence != -1 && operatorStack.Peek().Precedence != -1
                        && token.Precedence <= operatorStack.Peek().Precedence)
                        outputStack.Push(operatorStack.Pop());

                    operatorStack.Push(token);
                    break;
            }
        }

        while (operatorStack.TryPop(out Token reamining))
            outputStack.Push(reamining);

        return outputStack.Reverse().ToArray();
    }

    /// <summary>
    /// Tokenizes the terms in the formula.
    /// </summary>
    /// <param name="stringExp"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    static Token[] ParseMathString(string stringExp)
    {
        if (string.IsNullOrWhiteSpace(stringExp))
            throw new ArgumentException("Empty or blank expression.");

        var validityTest = ExpressionParseMatch.Replace(stringExp, " ");
        if (!string.IsNullOrWhiteSpace(validityTest))
        {
            var invalidChar = validityTest.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
            throw new ArgumentException($"Invalid characters '{invalidChar}' in expression.");
        }

        var rawTokens = ExpressionParseMatch.Matches(stringExp)
            .Select(x => x.Value);
        var result = new List<Token>();

        foreach (var token in rawTokens)
        {
            var newToken = new Token();
            var lowerToken = token.ToLower();
            switch (lowerToken)
            {
                case "^":
                    newToken.Operator = FormulaOperator.Power;
                    newToken.Precedence = 3;
                    newToken.OperatorDisplay = lowerToken;
                    newToken.ParametersCount = 2;
                    break;
                case "/":
                    newToken.Operator = FormulaOperator.Division;
                    newToken.Precedence = 2;
                    newToken.OperatorDisplay = lowerToken;
                    newToken.ParametersCount = 2;
                    break;
                case "*":
                    newToken.Operator = FormulaOperator.Multiplication;
                    newToken.Precedence = 2;
                    newToken.OperatorDisplay = lowerToken;
                    newToken.ParametersCount = 2;
                    break;
                case "+":
                    newToken.Operator = FormulaOperator.Sum;
                    newToken.Precedence = 1;
                    newToken.OperatorDisplay = lowerToken;
                    newToken.ParametersCount = 2;
                    break;
                case "-":
                    newToken.Operator = FormulaOperator.Substract;
                    newToken.Precedence = 1;
                    newToken.OperatorDisplay = lowerToken;
                    newToken.ParametersCount = 2;
                    break;
                case "max":
                    newToken.Operator = FormulaOperator.Max;
                    newToken.OperatorDisplay = "max";
                    newToken.ParametersCount = 2;
                    break;
                case "sin":
                    newToken.Operator = FormulaOperator.Sin;
                    newToken.OperatorDisplay = "sin";
                    newToken.ParametersCount = 1;
                    break;
                case "pi":
                    newToken.Operator = FormulaOperator.Pi;
                    newToken.OperatorDisplay = "pi";
                    newToken.ParametersCount = 0;
                    break;
                case var t when ParseVariableName.IsMatch(t):
                    newToken.Operator = FormulaOperator.Variable;
                    newToken.VariableName = t;
                    break;
                case var t when Double.TryParse(t, out double doubleValue):
                    newToken.Operator = FormulaOperator.DoubleConstant;
                    newToken.DoubleValue = doubleValue;
                    break;
                case "(":
                    newToken.Operator = FormulaOperator.OpenParenthesis;
                    newToken.OperatorDisplay = lowerToken;
                    break;
                case ")":
                    newToken.Operator = FormulaOperator.CloseParenthesis;
                    newToken.OperatorDisplay = lowerToken;
                    break;
                case ",": /* Ignore */ break;

                default: throw new ArgumentException($"Invalid character '{token}'");
            }

            if (newToken.Operator != FormulaOperator.None)
                result.Add(newToken);
        }

        return result.ToArray();
    }

    public static string PrintTokens(IEnumerable<Token> tokens)
    {
        var result = string.Join(" ",
            tokens.Select(x =>
            {
                if (x.DoubleValue.HasValue)
                    return x.DoubleValue.Value.ToString();
                else if (x.Operator == FormulaOperator.OpenParenthesis)
                    return x.OperatorDisplay;
                else if (x.VariableName != null)
                    return x.VariableName;
                else if (x.OperatorDisplay != null)
                    return x.OperatorDisplay;
                else
                    return string.Empty;
            })
        );
        return result;
    }
}