using System.Linq.Expressions;

namespace ConsoleApp1;

internal class ETBuilder
{
    public static Delegate BuildExpression(IEnumerable<Token> tokens)
    {
        var tokensList = new List<Token>(tokens);

        // Helper: Return position for the first available operator.
        int? NextOpIndex() => tokensList
            .Select((x, i) => new { value = x, index = i })
            .FirstOrDefault(x => x.value.IsGeneralOperator())?
            .index;

        var varsList = tokensList.Where(x =>
            //x.Operator == FormulaOperator.DoubleConstant ||
            x.Operator == FormulaOperator.Variable)
            .ToArray();

        // Build Expressions for variables and constants.
        for (int i = 0; i < tokensList.Count(); i++)
            BuildParamExpression(tokensList[i]);

        int? opIndex;

        while ((opIndex = NextOpIndex()).HasValue
            && opIndex >= 0
            && tokensList.Count > 1)
        {
            BuildExpression(tokensList, opIndex.Value);
        }

        if (tokensList.Count != 1)
            throw new Exception("Unable to process the provided expression.");

        // Dynamically build the lambda.
        var parameterTypes = Enumerable.Repeat(typeof(double), varsList.Count()).ToArray();
        var delegateType = Expression.GetDelegateType(
            // This is the return type.
            parameterTypes.Concat(new[] { typeof(double) }).ToArray());

        ParameterExpression[] parameters = parameterTypes.Select(t => Expression.Parameter(t)).ToArray();

        var lambda = Expression.Lambda(
            delegateType,
            tokensList[0].ExpObject,
            varsList.Select(x => x.ExpObject).Cast<ParameterExpression>());

        Delegate compiledLambda = lambda.Compile();
        return compiledLambda;
    }


    static void BuildExpression(List<Token> tokensList, int operatorIndex)
    {
        Expression HandleLambda(Expression p1) =>
            p1.NodeType == ExpressionType.Lambda && p1 is Expression<Func<Double>> 
                ? (p1 as Expression<Func<double>>).Body 
                : p1;

        var opToken = tokensList[operatorIndex];
        Func<Expression, Expression, BinaryExpression>? opExpression = null;
        Expression? p1 = null;
        Expression? p2 = null;

        switch (opToken.ParametersCount)
        {
            case 0:
                switch (opToken.Operator)
                {
                    case FormulaOperator.Pi:
                        opToken.ExpObject = Expression.Constant(Math.PI);
                        opToken.Operator = FormulaOperator.None;
                        opToken.PreProcessed = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Parameters ({opToken.OperatorDisplay}) not supported.");
                }
                break;

            case 1:
                p1 = tokensList[operatorIndex - 1].ExpObject!;

                switch (opToken.Operator)
                {
                    case FormulaOperator.Sin:

                        var methodCall = Expression.Call(
                            typeof(Math).GetMethod("Sin", new[] { typeof(double) }),
                            p1!);

                        var lambda = Expression.Lambda<Func<double>>(methodCall);
                        opToken.ExpObject = lambda;
                        opToken.Operator = FormulaOperator.None;
                        opToken.PreProcessed = true;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Parameters ({opToken.OperatorDisplay}) not supported.");
                }
                break;

            case 2:
                p1 = tokensList[operatorIndex - 2].ExpObject!;
                p2 = tokensList[operatorIndex - 1].ExpObject!;

                switch (opToken.Operator)
                {
                    case FormulaOperator.Power: opExpression = Expression.Power; break;
                    case FormulaOperator.Division: opExpression = Expression.Divide; break;
                    case FormulaOperator.Multiplication: opExpression = Expression.Multiply; break;
                    case FormulaOperator.Sum: opExpression = Expression.Add; break;
                    case FormulaOperator.Substract: opExpression = Expression.Subtract; break;
                    case FormulaOperator.Max:

                        var methodCall = Expression.Call(
                            typeof(Math).GetMethod("Max", new[] { typeof(double), typeof(double) }),
                            p1!, p2!);

                        var lambda = Expression.Lambda<Func<double>>(methodCall);
                        opToken.ExpObject = lambda;
                        opToken.Operator = FormulaOperator.None;
                        opToken.PreProcessed = true;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Parameters ({opToken.OperatorDisplay}) not supported.");
                }
                break;
            default:
                throw new InvalidOperationException($"Parameters count ({opToken.ParametersCount}) for operator '{opToken.OperatorDisplay}' not supported.");
        }

        if (opExpression == null && !opToken.PreProcessed)
            throw new ArgumentException($"Parameters ({opToken.OperatorDisplay}) not supported.");

        // Remove the parameters and set the operator as the resulting expression.
        if (!opToken.PreProcessed)
        {
            opToken.ExpObject = opExpression(HandleLambda(p1), HandleLambda(p2));
            opToken.Operator = FormulaOperator.None;
        }

        for (int i = 0; i < opToken.ParametersCount; i++)
            tokensList.RemoveAt(operatorIndex - opToken.ParametersCount);
    }

    static void BuildParamExpression(Token token)
    {
        Expression? result = null;
        if (token.Operator == FormulaOperator.DoubleConstant && token.DoubleValue.HasValue)
            result = Expression.Constant(token.DoubleValue.Value);
        else if (token.Operator == FormulaOperator.Variable)
            result = Expression.Parameter(typeof(double), token.OperatorDisplay);

        if (result != null) token.ExpObject = result;
    }

}