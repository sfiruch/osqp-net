#pragma warning disable CS0660, CS0661

namespace OsqpNet.Modelling;

/// <summary>
/// Represents an optimization variable.
/// </summary>
public sealed class Variable
{
    internal int Index { get; set; } = -1;

    internal Variable()
    {
    }

    /// <summary>Multiplies a variable by a coefficient.</summary>
    public static LinExpr operator *(Variable v, double coeff) => new LinExpr(v, coeff);
    /// <summary>Multiplies a variable by a coefficient.</summary>
    public static LinExpr operator *(double coeff, Variable v) => new LinExpr(v, coeff);
    /// <summary>Adds two variables.</summary>
    public static LinExpr operator +(Variable v1, Variable v2) => new LinExpr(v1) + v2;
    /// <summary>Adds a variable and a linear expression.</summary>
    public static LinExpr operator +(Variable v, LinExpr e) => e + v;
    /// <summary>Subtracts a linear expression from a variable.</summary>
    public static LinExpr operator -(Variable v, LinExpr e) => v + (-1.0 * e);
    /// <summary>Adds a variable and a constant.</summary>
    public static LinExpr operator +(Variable v, double constant) => new LinExpr(v) + constant;
    /// <summary>Adds a constant and a variable.</summary>
    public static LinExpr operator +(double constant, Variable v) => new LinExpr(v) + constant;
    /// <summary>Subtracts one variable from another.</summary>
    public static LinExpr operator -(Variable v1, Variable v2) => new LinExpr(v1) - v2;
    /// <summary>Subtracts a constant from a variable.</summary>
    public static LinExpr operator -(Variable v, double constant) => new LinExpr(v) - constant;
    /// <summary>Subtracts a variable from a constant.</summary>
    public static LinExpr operator -(double constant, Variable v) => constant - new LinExpr(v);
    /// <summary>Negates a variable.</summary>
    public static LinExpr operator -(Variable v) => new LinExpr(v, -1.0);

    /// <summary>Multiplies two variables to create a quadratic term.</summary>
    public static QuadExpr operator *(Variable v1, Variable v2) => new QuadExpr(v1, v2, 1.0);

    /// <summary>Creates a less-than-or-equal-to constraint.</summary>
    public static Constraint operator <=(Variable v, double bound) => new LinExpr(v) <= bound;
    /// <summary>Creates a greater-than-or-equal-to constraint.</summary>
    public static Constraint operator >=(Variable v, double bound) => new LinExpr(v) >= bound;
    /// <summary>Creates an equality constraint.</summary>
    public static Constraint operator ==(Variable v, double bound) => new LinExpr(v) == bound;
    /// <summary>Not supported.</summary>
    public static Constraint operator !=(Variable v, double bound) => throw new NotSupportedException("Inequality constraints are not supported by OSQP.");

    /// <summary>Creates a less-than-or-equal-to constraint between two variables.</summary>
    public static Constraint operator <=(Variable v1, Variable v2) => new LinExpr(v1) <= new LinExpr(v2);
    /// <summary>Creates a greater-than-or-equal-to constraint between two variables.</summary>
    public static Constraint operator >=(Variable v1, Variable v2) => new LinExpr(v1) >= new LinExpr(v2);
    /// <summary>Creates an equality constraint between two variables.</summary>
    public static Constraint operator ==(Variable v1, Variable v2) => new LinExpr(v1) == new LinExpr(v2);
    /// <summary>Not supported.</summary>
    public static Constraint operator !=(Variable v1, Variable v2) => throw new NotSupportedException("Inequality constraints are not supported by OSQP.");
}