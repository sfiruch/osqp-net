using System.Runtime.InteropServices;
using OsqpNet.Native;

namespace OsqpNet;

/// <summary>
/// Represents a matrix in Compressed Sparse Column (CSC) format.
/// </summary>
public sealed class CscMatrix : IDisposable
{
    internal unsafe OsqpCscMatrix* NativeMatrix { get; }
    private readonly double[] _x;
    private readonly long[] _i;
    private readonly long[] _p;
    private GCHandle _hX;
    private GCHandle _hI;
    private GCHandle _hP;
    private bool _disposed;

    internal ReadOnlySpan<double> X => _x;
    internal ReadOnlySpan<long> I => _i;
    internal ReadOnlySpan<long> P => _p;

    internal bool HasSameSparsity(CscMatrix other)
    {
        return I.SequenceEqual(other.I) && P.SequenceEqual(other.P);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CscMatrix"/> class.
    /// </summary>
    /// <param name="m">Number of rows.</param>
    /// <param name="n">Number of columns.</param>
    /// <param name="x">Non-zero values.</param>
    /// <param name="i">Row indices.</param>
    /// <param name="p">Column pointers.</param>
    public unsafe CscMatrix(int m, int n, ReadOnlySpan<double> x, ReadOnlySpan<long> i, ReadOnlySpan<long> p)
        : this(m, n, x.ToArray(), i.ToArray(), p.ToArray())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CscMatrix"/> class.
    /// </summary>
    /// <param name="m">Number of rows.</param>
    /// <param name="n">Number of columns.</param>
    /// <param name="x">Non-zero values.</param>
    /// <param name="i">Row indices.</param>
    /// <param name="p">Column pointers.</param>
    public unsafe CscMatrix(int m, int n, double[] x, long[] i, long[] p)
    {
        _x = x;
        _i = i;
        _p = p;

        _hX = GCHandle.Alloc(_x, GCHandleType.Pinned);
        _hI = GCHandle.Alloc(_i, GCHandleType.Pinned);
        _hP = GCHandle.Alloc(_p, GCHandleType.Pinned);

        NativeMatrix = (OsqpCscMatrix*)Marshal.AllocHGlobal(sizeof(OsqpCscMatrix));
        NativeMethods.OsqpCscMatrixSetData(NativeMatrix, m, n, x.Length, 
            (double*)_hX.AddrOfPinnedObject(), 
            (long*)_hI.AddrOfPinnedObject(), 
            (long*)_hP.AddrOfPinnedObject());
    }

    private unsafe void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (NativeMatrix != null)
            {
                Marshal.FreeHGlobal((IntPtr)NativeMatrix);
            }
            if (_hX.IsAllocated) _hX.Free();
            if (_hI.IsAllocated) _hI.Free();
            if (_hP.IsAllocated) _hP.Free();
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
    /// Finalizer for <see cref="CscMatrix"/>.
    /// </summary>
    ~CscMatrix() => Dispose(false);
}
