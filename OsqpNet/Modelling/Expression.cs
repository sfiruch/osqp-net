#pragma warning disable CS0660, CS0661

namespace OsqpNet.Modelling;

/// <summary>
/// Represents a linear expression.
/// </summary>
public sealed class LinExpr
{
    internal Dictionary<Variable, double> Coefficients { get; } = new();

    /// <summary>
    /// The constant term of the expression.
    /// </summary>
    public double Constant { get; internal set; }

    /// <summary>Initializes a new instance of the LinExpr class.</summary>
    public LinExpr() { }

    internal LinExpr(Variable v, double coeff = 1.0)
    {
        Coefficients[v] = coeff;
    }

    /// <summary>Adds a variable to an expression.</summary>
    public static LinExpr operator +(LinExpr e, Variable v)
    {
        var result = e.Clone();
        result.Add(v, 1.0);
        return result;
    }

    /// <summary>Adds two linear expressions.</summary>
    public static LinExpr operator +(LinExpr e1, LinExpr e2)
    {
        var result = e1.Clone();
        result.Add(e2);
        return result;
    }

    /// <summary>Adds two linear expressions.</summary>
    public void operator +=(LinExpr e2) => this.Add(e2);

    /// <summary>Adds a variable to a linear expression.</summary>
    public void operator +=(Variable v) => this.Add(v, 1.0);

    /// <summary>Adds a constant to a linear expression.</summary>
    public void operator +=(double c) => this.Constant += c;

    /// <summary>Subtracts a linear expression.</summary>
    public void operator -=(LinExpr e2) => this.Subtract(e2);

    /// <summary>Subtracts a variable from a linear expression.</summary>
    public void operator -=(Variable v) => this.Add(v, -1.0);

    /// <summary>Subtracts a constant from a linear expression.</summary>
    public void operator -=(double c) => this.Constant -= c;

    /// <summary>Scales a linear expression.</summary>
    public void operator *=(double coeff) => this.Scale(coeff);

    /// <summary>Adds two linear expressions.</summary>
    public void Add(LinExpr e2)
    {
        foreach (var kv in e2.Coefficients) Add(kv.Key, kv.Value);
        Constant += e2.Constant;
    }

    /// <summary>Subtracts a linear expression.</summary>
    internal void Subtract(LinExpr e2)
    {
        foreach (var kv in e2.Coefficients) Add(kv.Key, -kv.Value);
        Constant -= e2.Constant;
    }

    /// <summary>Adds a constant to an expression.</summary>
    public static LinExpr operator +(LinExpr e, double c)
    {
        var result = e.Clone();
        result.Constant += c;
        return result;
    }

    /// <summary>Adds a constant to an expression.</summary>
    public static LinExpr operator +(double c, LinExpr e) => e + c;

    /// <summary>Subtracts a variable from an expression.</summary>
    public static LinExpr operator -(LinExpr e, Variable v)
    {
        var result = e.Clone();
        result.Add(v, -1.0);
        return result;
    }

    /// <summary>Subtracts an expression from a variable.</summary>
    public static LinExpr operator -(Variable v, LinExpr e)
    {
        var result = e * -1.0;
        result.Add(v, 1.0);
        return result;
    }

    /// <summary>Subtracts one expression from another.</summary>
    public static LinExpr operator -(LinExpr e1, LinExpr e2)
    {
        var result = e1.Clone();
        result.Subtract(e2);
        return result;
    }

    /// <summary>Subtracts a constant from an expression.</summary>
    public static LinExpr operator -(LinExpr e, double c)
    {
        var result = e.Clone();
        result.Constant -= c;
        return result;
    }

    /// <summary>Subtracts an expression from a constant.</summary>
    public static LinExpr operator -(double c, LinExpr e)
    {
        var result = e * -1.0;
        result.Constant += c;
        return result;
    }

    /// <summary>Multiplies an expression by a coefficient.</summary>
    public static LinExpr operator *(LinExpr e, double coeff)
    {
        var result = new LinExpr();
        foreach (var kv in e.Coefficients) result.Coefficients[kv.Key] = kv.Value * coeff;
        result.Constant = e.Constant * coeff;
        return result;
    }

    /// <summary>Multiplies an expression by a coefficient.</summary>
    public static LinExpr operator *(double coeff, LinExpr e) => e * coeff;

    /// <summary>Scales the expression by a coefficient.</summary>
    public void Scale(double coeff)
    {
        var keys = Coefficients.Keys.ToArray();
        foreach (var key in keys) Coefficients[key] *= coeff;
        Constant *= coeff;
    }

    /// <summary>Multiplies an expression by a variable to create a quadratic expression.</summary>
    public static QuadExpr operator *(LinExpr e, Variable v)
    {
        var result = new QuadExpr();
        foreach (var kv in e.Coefficients) result.AddQuad(kv.Key, v, kv.Value);
        result.Linear.Add(v, e.Constant);
        return result;
    }

    /// <summary>Multiplies a variable by an expression to create a quadratic expression.</summary>
    public static QuadExpr operator *(Variable v, LinExpr e) => e * v;

    /// <summary>Implicitly converts a linear expression to a quadratic expression.</summary>
    public static implicit operator QuadExpr(LinExpr l)
    {
        var result = new QuadExpr();
        result.Linear.Constant = l.Constant;
        foreach (var kv in l.Coefficients) result.Linear.Add(kv.Key, kv.Value);
        return result;
    }

    /// <summary>Multiplies two linear expressions to create a quadratic expression.</summary>
    public static QuadExpr operator *(LinExpr e1, LinExpr e2) => Multiply(e1, e2);

    /// <summary>Multiplies two linear expressions to create a quadratic expression.</summary>
    public static QuadExpr Multiply(LinExpr e1, LinExpr e2)
    {
        var result = new QuadExpr();
        foreach (var kv1 in e1.Coefficients)
        {
            foreach (var kv2 in e2.Coefficients)
            {
                result.AddQuad(kv1.Key, kv2.Key, kv1.Value * kv2.Value);
            }
            result.Linear.Add(kv1.Key, kv1.Value * e2.Constant);
        }
        foreach (var kv2 in e2.Coefficients)
        {
            result.Linear.Add(kv2.Key, kv2.Value * e1.Constant);
        }
        result.Linear.Constant = e1.Constant * e2.Constant;
        return result;
    }

    /// <summary>Creates a less-than-or-equal-to constraint.</summary>
    public static Constraint operator <=(LinExpr e, double bound) => new Constraint(e, -1e20, bound - e.Constant);
    /// <summary>Creates a greater-than-or-equal-to constraint.</summary>
    public static Constraint operator >=(LinExpr e, double bound) => new Constraint(e, bound - e.Constant, 1e20);
    /// <summary>Creates an equality constraint.</summary>
    public static Constraint operator ==(LinExpr e, double bound) => new Constraint(e, bound - e.Constant, bound - e.Constant);
    /// <summary>Not supported.</summary>
    public static Constraint operator !=(LinExpr e, double bound) => throw new NotSupportedException("Inequality constraints are not supported by OSQP.");

    /// <summary>Creates a less-than-or-equal-to constraint between expressions.</summary>
    public static Constraint operator <=(LinExpr e1, LinExpr e2) => (e1 - e2) <= 0;
    /// <summary>Creates a greater-than-or-equal-to constraint between expressions.</summary>
    public static Constraint operator >=(LinExpr e1, LinExpr e2) => (e1 - e2) >= 0;
    /// <summary>Creates an equality constraint between expressions.</summary>
    public static Constraint operator ==(LinExpr e1, LinExpr e2) => (e1 - e2) == 0;
    /// <summary>Not supported.</summary>
    public static Constraint operator !=(LinExpr e1, LinExpr e2) => throw new NotSupportedException("Inequality constraints are not supported by OSQP.");

    internal void Add(Variable v, double coeff)
    {
        if (Coefficients.TryGetValue(v, out double current))
            Coefficients[v] = current + coeff;
        else
            Coefficients[v] = coeff;
    }

    internal LinExpr Clone()
    {
        var clone = new LinExpr { Constant = this.Constant };
        foreach (var kv in Coefficients) clone.Coefficients[kv.Key] = kv.Value;
        return clone;
    }
}

/// <summary>
/// Represents a quadratic expression.
/// </summary>
public sealed class QuadExpr
{
    internal Dictionary<(Variable, Variable), double> QuadCoefficients { get; } = new();

    /// <summary>
    /// The linear part of the expression.
    /// </summary>
    public LinExpr Linear { get; } = new();

    /// <summary>Initializes a new instance of the QuadExpr class.</summary>
    public QuadExpr() { }

    internal QuadExpr(Variable v1, Variable v2, double coeff)
    {
        AddQuad(v1, v2, coeff);
    }

    /// <summary>Adds two quadratic expressions.</summary>
    public static QuadExpr operator +(QuadExpr q, QuadExpr other)
    {
        var result = q.Clone();
        result.Add(other);
        return result;
    }

    /// <summary>Adds two quadratic expressions.</summary>
    public void operator +=(QuadExpr q2) => this.Add(q2);

    /// <summary>Adds a linear expression to a quadratic one.</summary>
    public void operator +=(LinExpr l) => this.Add(l);

    /// <summary>Adds a variable to a quadratic expression.</summary>
    public void operator +=(Variable v) => this.Add(v);

    /// <summary>Adds a constant to a quadratic expression.</summary>
    public void operator +=(double c) => this.Linear.Constant += c;

    /// <summary>Subtracts a quadratic expression.</summary>
    public void operator -=(QuadExpr q2) => this.Subtract(q2);

    /// <summary>Subtracts a linear expression from a quadratic one.</summary>
    public void operator -=(LinExpr l) => this.Subtract(l);

    /// <summary>Subtracts a variable from a quadratic expression.</summary>
    public void operator -=(Variable v) => this.Add(v * -1.0);

    /// <summary>Subtracts a constant from a quadratic expression.</summary>
    public void operator -=(double c) => this.Linear.Constant -= c;

    /// <summary>Scales a quadratic expression.</summary>
    public void operator *=(double coeff) => this.Scale(coeff);

    /// <summary>Adds two quadratic expressions.</summary>
    public void Add(QuadExpr other)
    {
        foreach (var kv in other.QuadCoefficients) AddQuad(kv.Key.Item1, kv.Key.Item2, kv.Value);
        Linear.Constant += other.Linear.Constant;
        foreach (var kv in other.Linear.Coefficients) Linear.Add(kv.Key, kv.Value);
    }

    /// <summary>Adds a linear expression.</summary>
    public void Add(LinExpr l)
    {
        Linear.Constant += l.Constant;
        foreach (var kv in l.Coefficients) Linear.Add(kv.Key, kv.Value);
    }

    /// <summary>Adds a variable.</summary>
    public void Add(Variable v)
    {
        Linear.Add(v, 1.0);
    }

    /// <summary>Subtracts a quadratic expression.</summary>
    public void Subtract(QuadExpr other)
    {
        foreach (var kv in other.QuadCoefficients) AddQuad(kv.Key.Item1, kv.Key.Item2, -kv.Value);
        Linear.Constant -= other.Linear.Constant;
        foreach (var kv in other.Linear.Coefficients) Linear.Add(kv.Key, -kv.Value);
    }

    /// <summary>Subtracts a linear expression.</summary>
    public void Subtract(LinExpr l)
    {
        Linear.Constant -= l.Constant;
        foreach (var kv in l.Coefficients) Linear.Add(kv.Key, -kv.Value);
    }

    /// <summary>Adds a quadratic and a linear expression.</summary>
    public static QuadExpr operator +(QuadExpr q, LinExpr l)
    {
        var result = q.Clone();
        result.Add(l);
        return result;
    }

    /// <summary>Adds a linear and a quadratic expression.</summary>
    public static QuadExpr operator +(LinExpr l, QuadExpr q) => q + l;

    /// <summary>Adds a quadratic expression and a variable.</summary>
    public static QuadExpr operator +(QuadExpr q, Variable v) => q + new LinExpr(v);
    /// <summary>Adds a variable and a quadratic expression.</summary>
    public static QuadExpr operator +(Variable v, QuadExpr q) => q + v;

    /// <summary>Adds a constant to a quadratic expression.</summary>
    public static QuadExpr operator +(QuadExpr q, double c)
    {
        var result = q.Clone();
        result.Linear.Constant += c;
        return result;
    }

    /// <summary>Adds a constant to a quadratic expression.</summary>
    public static QuadExpr operator +(double c, QuadExpr q) => q + c;

    /// <summary>Subtracts one quadratic expression from another.</summary>
    public static QuadExpr operator -(QuadExpr q1, QuadExpr q2)
    {
        var result = q1.Clone();
        result.Subtract(q2);
        return result;
    }

    /// <summary>Subtracts a linear expression from a quadratic one.</summary>
    public static QuadExpr operator -(QuadExpr q, LinExpr l)
    {
        var result = q.Clone();
        result.Subtract(l);
        return result;
    }

    /// <summary>Subtracts a quadratic expression from a linear one.</summary>
    public static QuadExpr operator -(LinExpr l, QuadExpr q)
    {
        var result = q * -1.0;
        result.Add(l);
        return result;
    }

    /// <summary>Subtracts a constant from a quadratic expression.</summary>
    public static QuadExpr operator -(QuadExpr q, double c)
    {
        var result = q.Clone();
        result.Linear.Constant -= c;
        return result;
    }

    /// <summary>Multiplies a quadratic expression by a coefficient.</summary>
    public static QuadExpr operator *(QuadExpr q, double coeff)
    {
        var result = new QuadExpr();
        foreach (var kv in q.QuadCoefficients) result.QuadCoefficients[kv.Key] = kv.Value * coeff;
        foreach (var kv in q.Linear.Coefficients) result.Linear.Coefficients[kv.Key] = kv.Value * coeff;
        result.Linear.Constant = q.Linear.Constant * coeff;
        return result;
    }

    /// <summary>Multiplies a quadratic expression by a coefficient.</summary>
    public static QuadExpr operator *(double coeff, QuadExpr q) => q * coeff;

    /// <summary>Scales the expression by a coefficient.</summary>
    public void Scale(double coeff)
    {
        var keys = QuadCoefficients.Keys.ToArray();
        foreach (var key in keys) QuadCoefficients[key] *= coeff;
        Linear.Scale(coeff);
    }

    internal void AddQuad(Variable v1, Variable v2, double coeff)
    {
        var (first, second) = v1.Index <= v2.Index ? (v1, v2) : (v2, v1);
        var key = (first, second);
        if (QuadCoefficients.TryGetValue(key, out double current))
            QuadCoefficients[key] = current + coeff;
        else
            QuadCoefficients[key] = coeff;
    }

    internal QuadExpr Clone()
    {
        var clone = new QuadExpr();
        foreach (var kv in QuadCoefficients) clone.QuadCoefficients[kv.Key] = kv.Value;
        foreach (var kv in Linear.Coefficients) clone.Linear.Coefficients[kv.Key] = kv.Value;
        clone.Linear.Constant = Linear.Constant;
        return clone;
    }
}