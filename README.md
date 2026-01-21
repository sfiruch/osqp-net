# OsqpNet

![.NET](https://github.com/sfiruch/osqp-net/actions/workflows/dotnet.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/osqp-net.svg)](https://www.nuget.org/packages/osqp-net/)

A modern .NET interface for the [OSQP (Operator Splitting Quadratic Program)](https://osqp.org/) solver. This library provides both a high-level modelling API and a low-level native wrapper using source-generated PInvoke.

## Installation

Install the package via NuGet:

```bash
dotnet add package osqp-net
```

The package includes native binaries for:
- **Windows** (x64)
- **Linux** (x64)

## Features

- **Modelling API**: Build optimization problems naturally using C# variables, expressions, and operators.
- **Efficient In-place Updates**: Uses new .NET 10 `void` operator overloads (`+=`, `-=`, `*=`) for zero-allocation expression building.
- **Incremental Solving**: Automatically detects when data can be updated in-place in the OSQP solver without re-initialization.
- **High-level Wrapper**: Clean, disposable `OsqpSolver` and `CscMatrix` classes.
- **Native Performance**: Uses .NET 10 `LibraryImport` for efficient C API calls.
- **Type Safety**: Strongly typed enums and structs reflecting the OSQP C API.

## Quick Start (Modelling API)

The modelling API allows you to define variables and constraints naturally:

```csharp
using OsqpNet.Modelling;
using OsqpNet.Native;

// Create a model
var model = new Model();

// Add variables
var x = model.AddVariable();
var y = model.AddVariable();

// Set objective: minimize 2*x^2 + y^2 + x*y + x + y
// You can build expressions incrementally
var obj = 2 * x * x + y * y;
obj += x * y; // Efficient in-place update (.NET 10)
obj += x + y;
model.SetObjective(obj);

// Add constraints
model.AddConstraint(x + y == 1);
model.AddConstraint(x >= 0);
model.AddConstraint(y >= 0);

// Solve
var result = model.Solve();

if (result.Status == OsqpStatus.Solved)
{
    Console.WriteLine($"x: {result.Solution[x]}");
    Console.WriteLine($"Objective: {result.ObjectiveValue}");
}

// Incremental Solve: update bounds without recreating the solver
// This is extremely efficient for repeated solves
model.AddConstraint(x <= 0.5); 
var result2 = model.Solve();
```

## Low-level API

If you already have your data in CSC (Compressed Sparse Column) format, you can use the `OsqpSolver` class directly:

```csharp
using OsqpNet;
using OsqpNet.Native;

// Define P, q, A, l, u in CSC format
using var P = new CscMatrix(2, 2, new double[] { 4, 1, 2 }, new long[] { 0, 0, 1 }, new long[] { 0, 1, 3 });
var q = new double[] { 1, 1 };
using var A = new CscMatrix(3, 2, new double[] { 1, 1, 1, 1 }, new long[] { 0, 1, 0, 2 }, new long[] { 0, 2, 4 });
var l = new double[] { 1, 0, 0 };
var u = new double[] { 1, 0.7, 0.7 };

using var solver = new OsqpSolver(P, q, A, l, u);
var status = solver.Solve();
var solution = solver.GetPrimalSolution();
```

## References

- **OSQP Project**: [https://osqp.org/](https://osqp.org/)
- **OSQP Documentation**: [https://osqp.org/docs/](https://osqp.org/docs/)
- **C API Reference**: [https://osqp.org/docs/interfaces/C.html](https://osqp.org/docs/interfaces/C.html)

## License

This project is licensed under the same terms as OSQP (Apache 2.0).
