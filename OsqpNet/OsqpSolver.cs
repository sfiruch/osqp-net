using System.Runtime.InteropServices;
using OsqpNet.Native;

namespace OsqpNet;

/// <summary>
/// High-level interface for the OSQP solver.
/// </summary>
public sealed class OsqpSolver : IDisposable
{
    private unsafe OsqpSolverNative* _solver;
    private bool _disposed;
    private readonly int _m;
    private readonly int _n;

    /// <summary>
    /// Initializes a new instance of the <see cref="OsqpSolver"/> class and performs setup.
    /// </summary>
    /// <param name="P">Quadratic cost matrix (upper triangular).</param>
    /// <param name="q">Linear cost vector.</param>
    /// <param name="A">Constraint matrix.</param>
    /// <param name="l">Lower bound vector.</param>
    /// <param name="u">Upper bound vector.</param>
    /// <param name="settings">Optional solver settings.</param>
    public unsafe OsqpSolver(CscMatrix P, ReadOnlySpan<double> q, CscMatrix A, ReadOnlySpan<double> l, ReadOnlySpan<double> u, OsqpSettings? settings = null)
    {
        _m = (int)A.NativeMatrix->m;
        _n = (int)A.NativeMatrix->n;

        if (q.Length != _n) throw new ArgumentException("q length mismatch", nameof(q));
        if (l.Length != _m) throw new ArgumentException("l length mismatch", nameof(l));
        if (u.Length != _m) throw new ArgumentException("u length mismatch", nameof(u));

        fixed (double* pq = q, pl = l, pu = u)
        fixed (OsqpSolverNative** pSolver = &_solver)
        {
            OsqpSettings s;
            NativeMethods.OsqpSetDefaultSettings(&s);
            if (settings.HasValue)
            {
                // Copy settings field by field or just use the provided struct if it's supposed to be complete.
                // Given OsqpSettings is a large struct, if the user provides it, they probably want to override everything
                // but we should at least ensure it's valid.
                // Actually, the common pattern is to get defaults and then modify.
                // If we want to allow partial initialization, we'd need a different approach.
                // For now, let's just use the provided settings if they exist, but the tests failed because
                // the user-provided struct was empty (default initialized in C#).
                
                // Let's use a better approach: if user provides settings, we use them. 
                // In SpanTests I should have called OsqpSetDefaultSettings or used a helper.
                s = settings.Value;
            }

            var error = NativeMethods.OsqpSetup(pSolver, P.NativeMatrix, pq, A.NativeMatrix, pl, pu, _m, _n, &s);
            if (error != OsqpError.NoError)
            {
                throw new InvalidOperationException($"OSQP setup failed with error: {error}");
            }
        }
    }

    /// <summary>
    /// Solves the quadratic program.
    /// </summary>
    /// <returns>Solver status.</returns>
    public unsafe OsqpStatus Solve()
    {
        CheckDisposed();
        var error = NativeMethods.OsqpSolve(_solver);
        if (error != OsqpError.NoError)
        {
            throw new InvalidOperationException($"OSQP solve failed with error: {error}");
        }
        return (OsqpStatus)_solver->Info->StatusVal;
    }

    /// <summary>
    /// Gets the primal solution vector.
    /// </summary>
    /// <returns>The primal solution.</returns>
    public unsafe double[] GetPrimalSolution()
    {
        var solution = new double[_n];
        GetPrimalSolution(solution);
        return solution;
    }

    /// <summary>
    /// Copies the primal solution vector to the provided span.
    /// </summary>
    /// <param name="destination">The destination span.</param>
    public unsafe void GetPrimalSolution(Span<double> destination)
    {
        CheckDisposed();
        if (destination.Length < _n) throw new ArgumentException("Destination span is too short", nameof(destination));
        PrimalSolution.CopyTo(destination);
    }

    /// <summary>
    /// Gets a read-only span of the primal solution directly from native memory.
    /// </summary>
    public unsafe ReadOnlySpan<double> PrimalSolution
    {
        get
        {
            CheckDisposed();
            return new ReadOnlySpan<double>(_solver->Solution->X, _n);
        }
    }

    /// <summary>
    /// Gets the dual solution vector.
    /// </summary>
    /// <returns>The dual solution.</returns>
    public unsafe double[] GetDualSolution()
    {
        var solution = new double[_m];
        GetDualSolution(solution);
        return solution;
    }

    /// <summary>
    /// Copies the dual solution vector to the provided span.
    /// </summary>
    /// <param name="destination">The destination span.</param>
    public unsafe void GetDualSolution(Span<double> destination)
    {
        CheckDisposed();
        if (destination.Length < _m) throw new ArgumentException("Destination span is too short", nameof(destination));
        DualSolution.CopyTo(destination);
    }

    /// <summary>
    /// Gets a read-only span of the dual solution directly from native memory.
    /// </summary>
    public unsafe ReadOnlySpan<double> DualSolution
    {
        get
        {
            CheckDisposed();
            return new ReadOnlySpan<double>(_solver->Solution->Y, _m);
        }
    }

    /// <summary>
    /// Gets the solver information.
    /// </summary>
    /// <returns>Solver info.</returns>
    public unsafe OsqpInfo GetInfo()
    {
        CheckDisposed();
        return *_solver->Info;
    }

    /// <summary>
    /// Updates problem data vectors.
    /// </summary>
    /// <param name="q">New linear cost vector.</param>
    /// <param name="l">New lower bound vector.</param>
    /// <param name="u">New upper bound vector.</param>
    public unsafe void UpdateData(ReadOnlySpan<double> q = default, ReadOnlySpan<double> l = default, ReadOnlySpan<double> u = default)
    {
        CheckDisposed();
        fixed (double* pq = q, pl = l, pu = u)
        {
            var error = NativeMethods.OsqpUpdateDataVec(_solver, 
                q.IsEmpty ? null : pq, 
                l.IsEmpty ? null : pl, 
                u.IsEmpty ? null : pu);

            if (error != OsqpError.NoError)
            {
                throw new InvalidOperationException($"OSQP update data failed with error: {error}");
            }
        }
    }

    /// <summary>
    /// Updates problem data matrices.
    /// </summary>
    /// <param name="Px">New values for P.</param>
    /// <param name="PxIdx">Indices of values in P to update (optional).</param>
    /// <param name="Ax">New values for A.</param>
    /// <param name="AxIdx">Indices of values in A to update (optional).</param>
    public unsafe void UpdateMatrix(ReadOnlySpan<double> Px = default, ReadOnlySpan<long> PxIdx = default, ReadOnlySpan<double> Ax = default, ReadOnlySpan<long> AxIdx = default)
    {
        CheckDisposed();
        fixed (double* pPx = Px, pAx = Ax)
        fixed (long* pPxIdx = PxIdx, pAxIdx = AxIdx)
        {
            var error = NativeMethods.OsqpUpdateDataMat(_solver,
                Px.IsEmpty ? null : pPx, PxIdx.IsEmpty ? null : pPxIdx, Px.Length,
                Ax.IsEmpty ? null : pAx, AxIdx.IsEmpty ? null : pAxIdx, Ax.Length);

            if (error != OsqpError.NoError)
            {
                throw new InvalidOperationException($"OSQP update matrix failed with error: {error}");
            }
        }
    }

    /// <summary>
    /// Warm starts the solver with initial guesses for x and y.
    /// </summary>
    /// <param name="x">Initial guess for primal variables.</param>
    /// <param name="y">Initial guess for dual variables.</param>
    public unsafe void WarmStart(ReadOnlySpan<double> x = default, ReadOnlySpan<double> y = default)
    {
        CheckDisposed();
        fixed (double* px = x, py = y)
        {
            var error = NativeMethods.OsqpWarmStart(_solver, 
                x.IsEmpty ? null : px, 
                y.IsEmpty ? null : py);

            if (error != OsqpError.NoError)
            {
                throw new InvalidOperationException($"OSQP warm start failed with error: {error}");
            }
        }
    }

    private void CheckDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(OsqpSolver));
    }

    private unsafe void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (_solver != null)
            {
                NativeMethods.OsqpCleanup(_solver);
                _solver = null;
            }
            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer for <see cref="OsqpSolver"/>.
    /// </summary>
    ~OsqpSolver() => Dispose(false);
}
