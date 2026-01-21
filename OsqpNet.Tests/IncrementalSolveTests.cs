using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsqpNet.Modelling;
using OsqpNet.Native;

namespace OsqpNet.Tests;

[TestClass]
public class IncrementalSolveTests
{
    [TestMethod]
    public void TestChangeLinearObjective()
    {
        using var model = new Model();
        model.Settings.EpsAbs = 1e-6;
        model.Settings.EpsRel = 1e-6;
        var x = model.AddVariable();
        
        // min 0.5 * x^2 + x, s.t. x >= 0  => x = 0 (Wait, vertex is at -1, so 0 if x >= 0)
        // Let's use min 0.5 * x^2 + 5x, s.t. x >= 0 => x = 0
        // Change to min 0.5 * x^2 - 5x, s.t. x >= 0 => x = 5
        
        model.SetObjective(0.5 * x * x + 5 * x);
        model.AddConstraint(x >= 0);
        
        var res1 = model.Solve();
        Assert.AreEqual(0.0, res1.Solution[x], 0.001);
        
        model.SetObjective(0.5 * x * x - 5 * x);
        var res2 = model.Solve();
        Assert.AreEqual(5.0, res2.Solution[x], 0.001);
    }

    [TestMethod]
    public void TestChangeQuadraticObjective()
    {
        using var model = new Model();
        model.Settings.EpsAbs = 1e-6;
        model.Settings.EpsRel = 1e-6;
        var x = model.AddVariable();
        
        // min 0.5 * x^2 - 2x => x = 2
        model.SetObjective(0.5 * x * x - 2 * x);
        var res1 = model.Solve();
        Assert.AreEqual(2.0, res1.Solution[x], 0.001);
        
        // min x^2 - 2x => x = 1
        model.SetObjective(x * x - 2 * x);
        var res2 = model.Solve();
        Assert.AreEqual(1.0, res2.Solution[x], 0.001);
    }

    [TestMethod]
    public void TestChangeConstraintBounds()
    {
        using var model = new Model();
        model.Settings.EpsAbs = 1e-6;
        model.Settings.EpsRel = 1e-6;
        var x = model.AddVariable();
        
        // min x^2, s.t. x >= 5 => x = 5
        model.SetObjective(x * x);
        var c = x >= 5;
        model.AddConstraint(c);
        
        var res1 = model.Solve();
        Assert.AreEqual(5.0, res1.Solution[x], 0.001);
        
        // Change to x >= 10 => x = 10
        c.LowerBound = 10;
        var res2 = model.Solve();
        Assert.AreEqual(10.0, res2.Solution[x], 0.001);
        
        // Change to 2 <= x <= 3 => x = 2 (since obj is x^2)
        c.LowerBound = 2;
        c.UpperBound = 3;
        var res3 = model.Solve();
        Assert.AreEqual(2.0, res3.Solution[x], 0.001);
    }

    [TestMethod]
    public void TestAddVariable()
    {
        using var model = new Model();
        model.Settings.EpsAbs = 1e-6;
        model.Settings.EpsRel = 1e-6;
        var x = model.AddVariable();
        
        // min x^2, x >= 1 => x = 1
        model.SetObjective(x * x);
        model.AddConstraint(x >= 1);
        var res1 = model.Solve();
        Assert.AreEqual(1.0, res1.Solution[x], 0.001);
        
        // Add y: min x^2 + y^2, x >= 1, y >= 2 => x=1, y=2
        var y = model.AddVariable();
        model.SetObjective(x * x + y * y);
        model.AddConstraint(y >= 2);
        
        var res2 = model.Solve();
        Assert.AreEqual(1.0, res2.Solution[x], 0.001);
        Assert.AreEqual(2.0, res2.Solution[y], 0.001);
    }

    [TestMethod]
    public void TestAddConstraint()
    {
        using var model = new Model();
        model.Settings.EpsAbs = 1e-6;
        model.Settings.EpsRel = 1e-6;
        var x = model.AddVariable();
        
        // min (x-10)^2 => x = 10
        model.SetObjective(x * x - 20 * x + 100);
        var res1 = model.Solve();
        Assert.AreEqual(10.0, res1.Solution[x], 0.001);
        
        // Add constraint x <= 5 => x = 5
        model.AddConstraint(x <= 5);
        var res2 = model.Solve();
        Assert.AreEqual(5.0, res2.Solution[x], 0.001);
    }

    [TestMethod]
    public void TestChangeObjectiveSparsity()
    {
        using var model = new Model();
        model.Settings.EpsAbs = 1e-6;
        model.Settings.EpsRel = 1e-6;
        var x = model.AddVariable();
        var y = model.AddVariable();
        
        // min x^2 + y, s.t. x >= 1, y >= 1 => x=1, y=1
        model.SetObjective(x * x + y);
        model.AddConstraint(x >= 1);
        model.AddConstraint(y >= 1);
        
        var res1 = model.Solve();
        Assert.AreEqual(1.0, res1.Solution[x], 0.001);
        
        // Add quadratic term for y: min x^2 + y^2 - 10y => x=1, y=5
        model.SetObjective(x * x + y * y - 10 * y);
        var res2 = model.Solve();
        Assert.AreEqual(1.0, res2.Solution[x], 0.001);
        Assert.AreEqual(5.0, res2.Solution[y], 0.001);
        
        // Add cross term: min x^2 + y^2 + xy - 10y - 10x
        model.SetObjective(x * x + y * y + x * y - 10 * x - 10 * y);
        // Gradient: [2x + y - 10; 2y + x - 10] = [0; 0]
        // 2x + y = 10
        // x + 2y = 10 => x = 10 - 2y => 2(10-2y) + y = 10 => 20 - 4y + y = 10 => 10 = 3y => y = 3.33, x = 3.33
        var res3 = model.Solve();
        Assert.AreEqual(3.333, res3.Solution[x], 0.01);
        Assert.AreEqual(3.333, res3.Solution[y], 0.01);
    }

    [TestMethod]
    public void TestComplexIncrementalChange()
    {
        using var model = new Model();
        model.Settings.EpsAbs = 1e-6;
        model.Settings.EpsRel = 1e-6;
        var x = model.AddVariable(0, 10);
        var y = model.AddVariable(0, 10);
        
        // min x + y, s.t. 0 <= x,y <= 10 => x=0, y=0
        model.SetObjective(x + y);
        var res1 = model.Solve();
        Assert.AreEqual(0.0, res1.Solution[x], 0.001);
        Assert.AreEqual(0.0, res1.Solution[y], 0.001);
        
        // Change to min (x-5)^2 + (y-5)^2, add constraint x + y <= 4
        // Unconstrained min is (5,5). Constraint x + y <= 4 is active.
        // Symmetry suggests x=2, y=2.
        model.SetObjective(x * x - 10 * x + y * y - 10 * y + 50);
        var c = x + y <= 4;
        model.AddConstraint(c);
        
        var res2 = model.Solve();
        Assert.AreEqual(2.0, res2.Solution[x], 0.001);
        Assert.AreEqual(2.0, res2.Solution[y], 0.001);
        
        // Change constraint to x + y <= 6 => x=3, y=3
        c.UpperBound = 6;
        var res3 = model.Solve();
        Assert.AreEqual(3.0, res3.Solution[x], 0.001);
        Assert.AreEqual(3.0, res3.Solution[y], 0.001);
        
        // Add third variable z, min (x-5)^2 + (y-5)^2 + (z-5)^2, s.t. x+y <= 6, z >= 0
        var z = model.AddVariable();
        model.SetObjective(x * x - 10 * x + y * y - 10 * y + z * z - 10 * z + 75);
        model.AddConstraint(z >= 0);
        
        var res4 = model.Solve();
        Assert.AreEqual(3.0, res4.Solution[x], 0.001);
        Assert.AreEqual(3.0, res4.Solution[y], 0.001);
        Assert.AreEqual(5.0, res4.Solution[z], 0.001);
    }
}
