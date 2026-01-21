using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsqpNet.Modelling;

namespace OsqpNet.Tests;

[TestClass]
public class ExpressionTests
{
    [TestMethod]
    public void TestVariableOperators()
    {
        var model = new Model();
        var x = model.AddVariable();
        var y = model.AddVariable();

        var e1 = x + y;
        Assert.AreEqual(1.0, e1.Coefficients[x]);
        Assert.AreEqual(1.0, e1.Coefficients[y]);

        var e2 = 2 * x - 3 * y + 5;
        Assert.AreEqual(2.0, e2.Coefficients[x]);
        Assert.AreEqual(-3.0, e2.Coefficients[y]);
        Assert.AreEqual(5.0, e2.Constant);

        var e3 = -x;
        Assert.AreEqual(-1.0, e3.Coefficients[x]);

        var e4 = 10 - x;
        Assert.AreEqual(-1.0, e4.Coefficients[x]);
        Assert.AreEqual(10.0, e4.Constant);
        
        var q1 = x * y;
        Assert.AreEqual(1.0, q1.QuadCoefficients[(x, y)]);
    }

    [TestMethod]
    public void TestLinExprOperators()
    {
        var model = new Model();
        var x = model.AddVariable();
        var y = model.AddVariable();

        var e1 = new LinExpr(x, 2.0);
        var e2 = new LinExpr(y, 3.0) + 1.0;

        var e3 = e1 + e2;
        Assert.AreEqual(2.0, e3.Coefficients[x]);
        Assert.AreEqual(3.0, e3.Coefficients[y]);
        Assert.AreEqual(1.0, e3.Constant);

        var e4 = e1 - e2;
        Assert.AreEqual(2.0, e4.Coefficients[x]);
        Assert.AreEqual(-3.0, e4.Coefficients[y]);
        Assert.AreEqual(-1.0, e4.Constant);

        var e5 = e1 * 2.0;
        Assert.AreEqual(4.0, e5.Coefficients[x]);

        var e6 = 3.0 * e1;
        Assert.AreEqual(6.0, e6.Coefficients[x]);
        
        var e7 = e1 + x;
        Assert.AreEqual(3.0, e7.Coefficients[x]);
    }

    [TestMethod]
    public void TestQuadExprOperators()
    {
        var model = new Model();
        var x = model.AddVariable();
        var y = model.AddVariable();

        var q1 = (x + 1) * (y + 2);
        // xy + 2x + y + 2
        Assert.AreEqual(1.0, q1.QuadCoefficients[(x, y)]);
        Assert.AreEqual(2.0, q1.Linear.Coefficients[x]);
        Assert.AreEqual(1.0, q1.Linear.Coefficients[y]);
        Assert.AreEqual(2.0, q1.Linear.Constant);

        var q2 = q1 + (x * x);
        Assert.AreEqual(1.0, q2.QuadCoefficients[(x, x)]);
        Assert.AreEqual(1.0, q2.QuadCoefficients[(x, y)]);

        var q3 = q1 - q2;
        // q1 - (q1 + x^2) = -x^2
        Assert.AreEqual(-1.0, q3.QuadCoefficients[(x, x)]);
        Assert.AreEqual(0.0, q3.QuadCoefficients.GetValueOrDefault((x, y)));

        var q4 = q1 * 2.0;
        Assert.AreEqual(2.0, q4.QuadCoefficients[(x, y)]);
        Assert.AreEqual(4.0, q4.Linear.Coefficients[x]);
        Assert.AreEqual(2.0, q4.Linear.Coefficients[y]);
        Assert.AreEqual(4.0, q4.Linear.Constant);
        
        var q5 = 0.5 * q1;
        Assert.AreEqual(0.5, q5.QuadCoefficients[(x, y)]);

        var q6 = q1 + 5.0;
        Assert.AreEqual(7.0, q6.Linear.Constant);

        var q7 = 10.0 + q1;
        Assert.AreEqual(12.0, q7.Linear.Constant);
        
        var q8 = q1 - 1.0;
        Assert.AreEqual(1.0, q8.Linear.Constant);

        var q9 = q1 + (x + y);
        Assert.AreEqual(3.0, q9.Linear.Coefficients[x]);
        Assert.AreEqual(2.0, q9.Linear.Coefficients[y]);

        var q10 = (x + y) + q1;
        Assert.AreEqual(3.0, q10.Linear.Coefficients[x]);
    }

    [TestMethod]
    public void TestLinExprMultiplication()
    {
        var model = new Model();
        var x = model.AddVariable();
        var y = model.AddVariable();

        var e = x + 1;
        var q = e * y; // x*y + y
        Assert.AreEqual(1.0, q.QuadCoefficients[(x, y)]);
        Assert.AreEqual(1.0, q.Linear.Coefficients[y]);

        var q2 = y * e;
        Assert.AreEqual(1.0, q2.QuadCoefficients[(x, y)]);
        Assert.AreEqual(1.0, q2.Linear.Coefficients[y]);
    }

    [TestMethod]
    public void TestClone()
    {
        var model = new Model();
        var x = model.AddVariable();
        
        var e = x + 1;
        var eClone = e.Clone();
        eClone.Constant = 5;
        Assert.AreEqual(1.0, e.Constant);
        Assert.AreEqual(5.0, eClone.Constant);

        var q = (x * x) + x + 1;
        var qClone = q.Clone();
        qClone.Linear.Constant = 10;
        Assert.AreEqual(1.0, q.Linear.Constant);
        Assert.AreEqual(10.0, qClone.Linear.Constant);
    }

    [TestMethod]
    public void TestQuadExprMutation()
    {
        var model = new Model();
        var x = model.AddVariable();
        var q1 = new QuadExpr(x, x, 1.0);
        var q2 = new QuadExpr(x, x, 2.0);
        
        var q1Before = q1;
        q1 += q2;
        Assert.AreSame(q1Before, q1);
        Assert.AreEqual(3.0, q1.QuadCoefficients[(x, x)]);

        var l = new LinExpr(x, 5.0);
        q1 += l;
        Assert.AreEqual(5.0, q1.Linear.Coefficients[x]);

        q1 -= l;
        Assert.AreEqual(0.0, q1.Linear.Coefficients[x]);
        
        q1 -= q2;
        Assert.AreEqual(1.0, q1.QuadCoefficients[(x, x)]);
    }
    
    [TestMethod]
    public void TestConstraints()
    {
        var model = new Model();
        var x = model.AddVariable();
        
        var c1 = x <= 5;
        Assert.AreEqual(-1e20, c1.LowerBound);
        Assert.AreEqual(5.0, c1.UpperBound);
        
        var c2 = x >= 2;
        Assert.AreEqual(2.0, c2.LowerBound);
        Assert.AreEqual(1e20, c2.UpperBound);
        
        var c3 = x == 3;
        Assert.AreEqual(3.0, c3.LowerBound);
        Assert.AreEqual(3.0, c3.UpperBound);
    }

    [TestMethod]
    public void TestMutation()
    {
        var model = new Model();
        var x = model.AddVariable();
        var e1 = new LinExpr(x, 1.0);
        var e2 = new LinExpr(x, 2.0);
        
        var e1Before = e1;
        e1.Add(e2);
        
        Assert.AreSame(e1Before, e1);
        Assert.AreEqual(3.0, e1.Coefficients[x]);

        e1.Subtract(e2);
        Assert.AreEqual(1.0, e1.Coefficients[x]);

        e1.Scale(2.0);
        Assert.AreEqual(2.0, e1.Coefficients[x]);
    }

    [TestMethod]
    public void TestQuadMutation()
    {
        var model = new Model();
        var x = model.AddVariable();
        var q1 = (x * x) + x + 1;
        
        q1.Scale(2.0);
        Assert.AreEqual(2.0, q1.QuadCoefficients[(x, x)]);
        Assert.AreEqual(2.0, q1.Linear.Coefficients[x]);
        Assert.AreEqual(2.0, q1.Linear.Constant);
    }

    [TestMethod]
    public void TestVariableVariableConstraints()
    {
        var model = new Model();
        var x = model.AddVariable();
        var y = model.AddVariable();

        var c1 = x <= y;
        // x - y <= 0
        Assert.AreEqual(1.0, c1.Expression.Coefficients[x]);
        Assert.AreEqual(-1.0, c1.Expression.Coefficients[y]);
        Assert.AreEqual(0.0, c1.UpperBound);

        var c2 = x >= y;
        Assert.AreEqual(0.0, c2.LowerBound);

        var c3 = x == y;
        Assert.AreEqual(0.0, c3.LowerBound);
        Assert.AreEqual(0.0, c3.UpperBound);

        Assert.ThrowsException<NotSupportedException>(() => x != y);
    }

    [TestMethod]
    public void TestQuadNormalization()
    {
        var model = new Model();
        var x = model.AddVariable(); // Index 0
        var y = model.AddVariable(); // Index 1
        
        var q1 = y * x;
        Assert.IsTrue(q1.QuadCoefficients.ContainsKey((x, y)));
        Assert.IsFalse(q1.QuadCoefficients.ContainsKey((y, x)));
        
        var q2 = new QuadExpr();
        q2.AddQuad(y, x, 1.0);
        Assert.IsTrue(q2.QuadCoefficients.ContainsKey((x, y)));
    }
}
