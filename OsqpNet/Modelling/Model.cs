using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OsqpNet.Native;

namespace OsqpNet.Modelling;

/// <summary>
/// Represents an optimization model.
/// </summary>
public sealed class Model : IDisposable
{
    private readonly List<Variable> _variables = new();
    private readonly List<Constraint> _constraints = new();
    private QuadExpr _objective = new();

    private OsqpSolver? _solver;
    private CscMatrix? _currentP;
    private CscMatrix? _currentA;
    private int _lastN = -1;
    private int _lastM = -1;

    /// <summary>
    /// OSQP Solver settings for this model.
    /// </summary>
    public OsqpSettings Settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="Model"/> class.
    /// </summary>
    public unsafe Model()
    {
        OsqpSettings settings;
        NativeMethods.OsqpSetDefaultSettings(&settings);
        Settings = settings;
    }

    /// <summary>
    /// Adds a new variable to the model.
    /// </summary>
    public Variable AddVariable()
    {
        var v = new Variable() { Index = _variables.Count };
        _variables.Add(v);
        return v;
    }

    /// <summary>
    /// Adds a new variable to the model with lower and upper bounds.
    /// </summary>
    /// <param name="lb">Lower bound.</param>
    /// <param name="ub">Upper bound.</param>
    public Variable AddVariable(double lb, double ub)
    {
        var v = AddVariable();
        AddConstraint(v >= lb);
        AddConstraint(v <= ub);
        return v;
    }

    /// <summary>
    /// Sets the objective function.
    /// </summary>
    public void SetObjective(QuadExpr objective)
    {
        _objective = objective;
    }

    /// <summary>
    /// Adds a constraint to the model.
    /// </summary>
    public void AddConstraint(Constraint constraint)
    {
        _constraints.Add(constraint);
    }

    /// <summary>
    /// Solves the optimization problem.
    /// </summary>
    public ModelResult Solve()
    {
        int n = _variables.Count;
        int m = _constraints.Count;

        var pData = new List<(int row, int col, double val)>(_objective.QuadCoefficients.Count);
        foreach (var kv in _objective.QuadCoefficients)
        {
            var v1 = kv.Key.Item1;
            var v2 = kv.Key.Item2;
            int i = v1.Index;
            int j = v2.Index;
            if (i > j) (i, j) = (j, i);

            double val = kv.Value;
            if (i == j) val *= 2.0;
            
pData.Add((i, j, val));
        }
        var pCsc = BuildCsc(n, n, pData);

        double[] q = new double[n];
        foreach (var kv in _objective.Linear.Coefficients)
        {
            q[kv.Key.Index] = kv.Value;
        }

        var aData = new List<(int row, int col, double val)>();
        double[] l = new double[m];
        double[] u = new double[m];

        for (int i = 0; i < m; i++)
        {
            var c = _constraints[i];
            foreach (var kv in c.Expression.Coefficients)
            {
                aData.Add((i, kv.Key.Index, kv.Value));
            }
            l[i] = c.LowerBound;
            u[i] = c.UpperBound;
        }
        var aCsc = BuildCsc(m, n, aData);

        bool canUpdate = _solver != null &&
                         n == _lastN &&
                         m == _lastM &&
                         pCsc.HasSameSparsity(_currentP!) &&
                         aCsc.HasSameSparsity(_currentA!);

        if (canUpdate)
        {
            _solver!.UpdateData(q, l, u);
            _solver!.UpdateMatrix(Px: pCsc.X, Ax: aCsc.X);
            pCsc.Dispose();
            aCsc.Dispose();
        }
        else
        {
            _solver?.Dispose();
            _currentP?.Dispose();
            _currentA?.Dispose();

            _solver = new OsqpSolver(pCsc, q, aCsc, l, u, Settings);
            _currentP = pCsc;
            _currentA = aCsc;
            _lastN = n;
            _lastM = m;
        }

        var status = _solver.Solve();
        var info = _solver.GetInfo();
        var primal = _solver.PrimalSolution;

        var solution = new Dictionary<Variable, double>();
        for (int i = 0; i < n; i++) solution[_variables[i]] = primal[i];

        // Ensure we use the constant part of the objective which is stored in Linear.Constant
        return new ModelResult(status, solution, info.ObjVal + _objective.Linear.Constant);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _solver?.Dispose();
        _currentP?.Dispose();
        _currentA?.Dispose();
    }


    private CscMatrix BuildCsc(int rows, int cols, List<(int row, int col, double val)> data)
    {
        if (data.Count == 0)
        {
            return new CscMatrix(rows, cols, Array.Empty<double>(), Array.Empty<long>(), GC.AllocateArray<long>(cols + 1, pinned: true));
        }

        // Sort by column, then row
        data.Sort((a, b) =>
        {
            int c = a.col.CompareTo(b.col);
            if (c != 0) return c;
            return a.row.CompareTo(b.row);
        });

        // Sum duplicates in-place
        int j = 0;
        for (int i = 1; i < data.Count; i++)
        {
            if (data[i].col == data[j].col && data[i].row == data[j].row)
            {
                data[j] = (data[j].row, data[j].col, data[j].val + data[i].val);
            }
            else
            {
                j++;
                data[j] = data[i];
            }
        }
        int count = j + 1;

        double[] x = GC.AllocateArray<double>(count, pinned: true);
        long[] iRow = GC.AllocateArray<long>(count, pinned: true);
        long[] pCol = GC.AllocateArray<long>(cols + 1, pinned: true);

        int current = 0;

        for (int c = 0; c < cols; c++)
        {
            pCol[c] = current;
            while (current < count && data[current].col == c)
            {
                x[current] = data[current].val;
                iRow[current] = data[current].row;
                current++;
            }
        }
        pCol[cols] = current;

        return new CscMatrix(rows, cols, x, iRow, pCol);
    }
}

/// <summary>
/// Result of the model optimization.
/// </summary>
/// <param name="Status">Solver status.</param>
/// <param name="Solution">Variable values.</param>
/// <param name="ObjectiveValue">Computed objective value.</param>
public record ModelResult(OsqpStatus Status, Dictionary<Variable, double> Solution, double ObjectiveValue);