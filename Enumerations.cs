/// <summary>
/// All supported terms.
/// </summary>
internal enum FormulaOperator
{
    None = -1,
    // Basic Operators
    Power = 1,
    Division = 2,
    Multiplication = 4,
    Sum = 8,
    Substract = 16,
    BasicOperations = 31,
    // Functions
    Sin = 32,
    Max = 64,
    Pi = 128,
    Functions = 224,
    // Other
    DoubleConstant = 256,
    Variable = 512,
    OpenParenthesis = 1024,
    CloseParenthesis = 2048
}