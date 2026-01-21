namespace OsqpNet.Modelling;

/// <summary>
/// Represents a linear constraint.
/// </summary>
public sealed class Constraint
{
    /// <summary>
    /// The linear expression of the constraint.
    /// </summary>
    public LinExpr Expression { get; }

    /// <summary>
    /// The lower bound.
    /// </summary>
    public double LowerBound { get; set; }

    /// <summary>
    /// The upper bound.
    /// </summary>
    public double UpperBound { get; set; }

    internal Constraint(LinExpr expression, double lb, double ub)
    {
        Expression = expression;
        LowerBound = lb;
        UpperBound = ub;
    }
}