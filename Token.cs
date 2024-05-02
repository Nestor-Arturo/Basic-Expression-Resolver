using System.Diagnostics;
using System.Linq.Expressions;

namespace ConsoleApp1;

[DebuggerDisplay("{Operator} {DoubleValue} {VariableName}")]
internal class Token
{
    public Token() { }
    public FormulaOperator Operator = FormulaOperator.None;
    public string? OperatorDisplay;
    public int Precedence = -1;
    public double? DoubleValue;
    public string? VariableName;
    public int ParametersCount = 0;
    public Expression? ExpObject;
    public bool PreProcessed = false;
    public bool IsGeneralOperator() =>
        (Operator & FormulaOperator.BasicOperations) == Operator
        || (Operator & FormulaOperator.Functions) == Operator;
}