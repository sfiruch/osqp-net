using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsqpNet.Native;

namespace OsqpNet.Tests;

[TestClass]
public unsafe class SpanTests
{
    [TestMethod]
    public void TestSpanSolution()
    {
        // Simple problem: min 0.5 * x^2 + x, s.t. x >= 2
        // Solution: x = 2
        var P = new CscMatrix(1, 1, new double[] { 1.0 }, new long[] { 0 }, new long[] { 0, 1 });
        var q = new double[] { 1.0 };
        var A = new CscMatrix(1, 1, new double[] { 1.0 }, new long[] { 0 }, new long[] { 0, 1 });
        var l = new double[] { 2.0 };
        var u = new double[] { 10.0 };

        OsqpSettings settings;
        NativeMethods.OsqpSetDefaultSettings(&settings);
        settings.EpsAbs = 1e-9;
        settings.EpsRel = 1e-9;

        using var solver = new OsqpSolver(P, q, A, l, u, settings);
        solver.Solve();

        var primal = solver.PrimalSolution;
        Assert.AreEqual(1, primal.Length);
        Assert.AreEqual(2.0, primal[0], 0.001);

        // Test copying to existing span
        Span<double> buffer = stackalloc double[1];
        solver.GetPrimalSolution(buffer);
        Assert.AreEqual(2.0, buffer[0], 0.001);
    }

    [TestMethod]
    public void TestSpanUpdate()
    {
        var P = new CscMatrix(1, 1, new double[] { 1.0 }, new long[] { 0 }, new long[] { 0, 1 });
        var q = new double[] { 1.0 };
        var A = new CscMatrix(1, 1, new double[] { 1.0 }, new long[] { 0 }, new long[] { 0, 1 });
        var l = new double[] { 2.0 };
        var u = new double[] { 10.0 };

        OsqpSettings settings;
        NativeMethods.OsqpSetDefaultSettings(&settings);
        settings.EpsAbs = 1e-9;
        settings.EpsRel = 1e-9;

        using var solver = new OsqpSolver(P, q, A, l, u, settings);
        solver.Solve();
        Assert.AreEqual(2.0, solver.PrimalSolution[0], 0.001);

        // Update bounds using Span
        ReadOnlySpan<double> newL = stackalloc double[] { 3.0 };
        solver.UpdateData(l: newL);
        solver.Solve();
        Assert.AreEqual(3.0, solver.PrimalSolution[0], 0.001);
        
        // Update matrix using Span
        ReadOnlySpan<double> newPx = stackalloc double[] { 2.0 }; // min x^2 + x, s.t. x >= 3 -> x = 3
        solver.UpdateMatrix(Px: newPx);
        solver.Solve();
        Assert.AreEqual(3.0, solver.PrimalSolution[0], 0.001);
    }
}
