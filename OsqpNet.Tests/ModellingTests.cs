using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsqpNet.Modelling;
using OsqpNet.Native;

namespace OsqpNet.Tests;

[TestClass]
public class ModellingTests
{
    [TestMethod]
    public unsafe void SimpleQp_ModellingApi_Solved()
    {
        // Objective: minimize 2*x^2 + y^2 + x*y + x + y
        // Constraints: x + y == 1, x >= 0, y >= 0
        // (Wait, my previous test had P = [4 1; 1 2] which corresponds to 2*x^2 + y^2 + x*y)

        var model = new Model();
        var x = model.AddVariable();
        var y = model.AddVariable();

        model.SetObjective(2 * x * x + y * y + x * y + x + y);
        model.AddConstraint(x + y == 1);
        model.AddConstraint(x >= 0);
        model.AddConstraint(y >= 0);

        model.Settings.EpsAbs = 1e-6;
        model.Settings.EpsRel = 1e-6;

        var result = model.Solve();

        Assert.AreEqual(OsqpStatus.Solved, result.Status);
        
        // With x + y = 1 and higher precision
        // Objective f(x, y) = 2x^2 + (1-x)^2 + x(1-x) + x + (1-x)
        // f(x) = 2x^2 + 1 - 2x + x^2 + x - x^2 + 1 = 2x^2 - x + 2
        // f'(x) = 4x - 1 = 0 => x = 0.25, y = 0.75
        
        Assert.AreEqual(0.25, result.Solution[x], 0.001);
        Assert.AreEqual(0.75, result.Solution[y], 0.001);
    }

    [TestMethod]
    public void TestIncrementalSolve()
    {
        using var model = new Model();
        model.Settings.EpsAbs = 1e-9;
        model.Settings.EpsRel = 1e-9;
        
        var x = model.AddVariable();
        
        // min 0.5 * x^2 + x, s.t. x >= 2  => x = 2
        model.SetObjective(0.5 * x * x + x);
        var c = x >= 2;
        model.AddConstraint(c);

        var res1 = model.Solve();
        Assert.AreEqual(2.0, res1.Solution[x], 0.001);

        // Change bound: x >= 3 => x = 3
        c.LowerBound = 3;
        var res2 = model.Solve();
        Assert.AreEqual(3.0, res2.Solution[x], 0.001);

        // Change coefficient: min x^2 + x, s.t. x >= 3 => x = 3
        model.SetObjective(x * x + x);
        var res3 = model.Solve();
        Assert.AreEqual(3.0, res3.Solution[x], 0.001);
        
        // Change linear coefficient: min x^2 - 10x, s.t. x >= 3 => x = 5 (vertex of x^2 - 10x is 5)
        model.SetObjective(x * x - 10 * x);
        var res4 = model.Solve();
        Assert.AreEqual(5.0, res4.Solution[x], 0.001);
    }

    [TestMethod]
    public void TestAddVariableWithBounds()
    {
        using var model = new Model();
        var x = model.AddVariable(2.0, 5.0);
        
        // min x^2  s.t. 2 <= x <= 5  => x = 2
        model.SetObjective(x * x);
        var res = model.Solve();
        Assert.AreEqual(2.0, res.Solution[x], 0.001);
    }

    [TestMethod]
    public void TestObjectiveConstant()
    {
        using var model = new Model();
        var x = model.AddVariable();
        
        // min x^2 + 10  s.t. x == 0 => obj = 10
        model.SetObjective(x * x + 10);
        model.AddConstraint(x == 0);
        
        var res = model.Solve();
        Assert.AreEqual(0.0, res.Solution[x], 0.001);
        Assert.AreEqual(10.0, res.ObjectiveValue, 0.001);
    }
}
