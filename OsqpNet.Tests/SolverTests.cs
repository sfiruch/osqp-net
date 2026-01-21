using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsqpNet;
using OsqpNet.Native;

namespace OsqpNet.Tests;

[TestClass]
public class SolverTests
{
    [TestMethod]
    public unsafe void SimpleQp_Solved()
    {
        // P = [4 1; 1 2] (upper triangular: [4 1; 0 2])
        // Column 0: rows [0], vals [4] -> p=[0, 1]
        // Column 1: rows [0, 1], vals [1, 2] -> p=[1, 3]
        // p = [0, 1, 3], i = [0, 0, 1], x = [4, 1, 2]

        using var P = new CscMatrix(2, 2, [4, 1, 2], [0, 0, 1], [0, 1, 3]);
        var q = new double[] { 1, 1 };
        using var A = new CscMatrix(3, 2, [1, 1, 1, 1], [0, 1, 0, 2], [0, 2, 4]);
        var l = new double[] { 1, 0, 0 };
        var u = new double[] { 1, 0.7, 0.7 };

        OsqpSettings settings;
        NativeMethods.OsqpSetDefaultSettings(&settings);
        settings.EpsAbs = 1e-6;
        settings.EpsRel = 1e-6;

        using var solver = new OsqpSolver(P, q, A, l, u, settings);
        var status = solver.Solve();

        Assert.AreEqual(OsqpStatus.Solved, status);
        var x = solver.GetPrimalSolution();
        
        // Expected solution approx: [0.3; 0.7]
        Assert.AreEqual(0.3, x[0], 0.001);
        Assert.AreEqual(0.7, x[1], 0.001);
    }
}