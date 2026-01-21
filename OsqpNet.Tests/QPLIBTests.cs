using Gurobi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OsqpNet.Modelling;
using OsqpNet.Native;
using System.Runtime;

namespace OsqpNet.Tests;

[TestClass]
public class QPLibTests
{
    public const string BaseDir = @"..\..\..\QPLIB";

    public static IEnumerable<object[]> EnumerateQPLibFiles() => Directory.EnumerateFiles(BaseDir, "*.lp")
        .Select(fn => new[] { Path.GetFileNameWithoutExtension(fn) });

    [TestMethod]
    [DynamicData(nameof(EnumerateQPLibFiles), DynamicDataSourceType.Method)]
    public unsafe void Instance(string filename)
    {
        using var env = new GRBEnv();
        using var m = new GRBModel(env, Path.Combine(BaseDir, filename + ".lp"));

        var model = new Model();
        model.Settings.EpsAbs = 0.05;
        model.Settings.EpsRel = 0;

        model.Settings.MaxIter = 100_000_000;

        var vars = m.GetVars().ToDictionary(v => v, v =>
        {
            var added = model.AddVariable();
            if (v.LB != -GRB.INFINITY)
                model.AddConstraint(added >= v.LB);
            if (v.UB != GRB.INFINITY)
                model.AddConstraint(added <= v.UB);
            return added;
        });
        foreach (var c in m.GetConstrs())
        {
            var le = new LinExpr();
            var row = m.GetRow(c);
            for (var i = 0; i < row.Size; i++)
                le.Add(vars[row.GetVar(i)], row.GetCoeff(i));
            switch (c.Sense)
            {
                case GRB.LESS_EQUAL:
                    model.AddConstraint(le <= c.RHS);
                    break;
                case GRB.EQUAL:
                    model.AddConstraint(le == c.RHS);
                    break;
                case GRB.GREATER_EQUAL:
                    model.AddConstraint(le >= c.RHS);
                    break;
            }
        }


        var obj = (GRBQuadExpr)m.GetObjective();
        var qe = new QuadExpr();
        for (var i = 0; i < obj.LinExpr.Size; i++)
            qe.Linear.Add(vars[obj.LinExpr.GetVar(i)], obj.LinExpr.GetCoeff(i));

        for (var i = 0; i < obj.Size; i++)
            qe.AddQuad(vars[obj.GetVar1(i)], vars[obj.GetVar2(i)], obj.GetCoeff(i));

        model.SetObjective(qe);

        var result = model.Solve();

        var sol = File.ReadAllLines(Path.Combine(BaseDir, filename + ".sol"))
            .Where(l => !l.StartsWith('#'))
            .Select(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToDictionary(t => t[0], t => double.Parse(t[1]));

        if (result.Status == OsqpStatus.NonCvx)
        {
            m.GetEnv().NonConvex = 0;
            m.Optimize();
            return;
        }

        if (!sol.Any())
            Assert.AreEqual(OsqpStatus.PrimalInfeasible, result.Status);
        else
        {
            Assert.IsTrue(result.Status == OsqpStatus.Solved || result.Status == OsqpStatus.SolvedInaccurate);
            Assert.AreEqual(sol["objvar"], result.ObjectiveValue, 0.1);
            //foreach (var kvp in sol)
            //    if (kvp.Key != "objvar")
            //        Assert.AreEqual(kvp.Value, result.Solution[vars[m.GetVarByName(kvp.Key)]], 0.2);
        }
    }
}
