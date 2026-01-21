using System.Runtime.InteropServices;

namespace OsqpNet.Native;

/// <summary>
/// Solver capabilities.
/// </summary>
public enum OsqpCapability : int
{
    /// <summary>Direct solver available.</summary>
    DirectSolver = 0x01,
    /// <summary>Indirect solver available.</summary>
    IndirectSolver = 0x02,
    /// <summary>Code generation available.</summary>
    Codegen = 0x04,
    /// <summary>Matrix updates available.</summary>
    UpdateMatrices = 0x08,
    /// <summary>Derivatives available.</summary>
    Derivatives = 0x10
}

/// <summary>
/// OSQP Solver status.
/// </summary>
public enum OsqpStatus : int
{
    /// <summary>Solved.</summary>
    Solved = 1,
    /// <summary>Solved inaccurate.</summary>
    SolvedInaccurate = 2,
    /// <summary>Primal infeasible.</summary>
    PrimalInfeasible = 3,
    /// <summary>Primal infeasible inaccurate.</summary>
    PrimalInfeasibleInaccurate = 4,
    /// <summary>Dual infeasible.</summary>
    DualInfeasible = 5,
    /// <summary>Dual infeasible inaccurate.</summary>
    DualInfeasibleInaccurate = 6,
    /// <summary>Maximum iterations reached.</summary>
    MaxIterReached = 7,
    /// <summary>Time limit reached.</summary>
    TimeLimitReached = 8,
    /// <summary>Non-convex problem.</summary>
    NonCvx = 9,
    /// <summary>Interrupted by user.</summary>
    SigInt = 10,
    /// <summary>Unsolved.</summary>
    Unsolved = 11
}

/// <summary>
/// OSQP Polish status.
/// </summary>
public enum OsqpPolishStatus : int
{
    /// <summary>Linear system error.</summary>
    LinsysError = -2,
    /// <summary>Polishing failed.</summary>
    Failed = -1,
    /// <summary>Polishing not performed.</summary>
    NotPerformed = 0,
    /// <summary>Polishing successful.</summary>
    Success = 1,
    /// <summary>No active set found.</summary>
    NoActiveSetFound = 2
}

/// <summary>
/// Linear system solver type.
/// </summary>
public enum OsqpLinsysSolver : int
{
    /// <summary>Unknown solver.</summary>
    Unknown = 0,
    /// <summary>Direct solver.</summary>
    Direct = 1,
    /// <summary>Indirect solver.</summary>
    Indirect = 2
}

/// <summary>
/// Preconditioner type.
/// </summary>
public enum OsqpPreconditioner : int
{
    /// <summary>No preconditioner.</summary>
    None = 0,
    /// <summary>Diagonal preconditioner.</summary>
    Diagonal = 1
}

/// <summary>
/// OSQP Error codes.
/// </summary>
public enum OsqpError : int
{
    /// <summary>No error.</summary>
    NoError = 0,
    /// <summary>Data validation error.</summary>
    DataValidationError = 1,
    /// <summary>Settings validation error.</summary>
    SettingsValidationError = 2,
    /// <summary>Linear system solver initialization error.</summary>
    LinsysSolverInitError = 3,
    /// <summary>Non-convex error.</summary>
    NonCvxError = 4,
    /// <summary>Memory allocation error.</summary>
    MemAllocError = 5,
    /// <summary>Workspace not initialized error.</summary>
    WorkspaceNotInitError = 6,
    /// <summary>Algebra load error.</summary>
    AlgebraLoadError = 7,
    /// <summary>File open error.</summary>
    FopenError = 8,
    /// <summary>Codegen defines error.</summary>
    CodegenDefinesError = 9,
    /// <summary>Data not initialized error.</summary>
    DataNotInitialized = 10,
    /// <summary>Function not implemented error.</summary>
    FuncNotImplemented = 11
}

/// <summary>
/// Matrix in compressed-column form.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct OsqpCscMatrix
{
    /// <summary>Number of rows.</summary>
    public long m;
    /// <summary>Number of columns.</summary>
    public long n;
    /// <summary>Column pointers (size n+1).</summary>
    public long* p;
    /// <summary>Row indices.</summary>
    public long* i;
    /// <summary>Numerical values.</summary>
    public double* x;
    /// <summary>Maximum number of entries.</summary>
    public long nzmax;
    /// <summary>Number of entries in triplet matrix, -1 for csc.</summary>
    public long nz;
    /// <summary>1 if the pointers were allocated automatically, 0 if owned by user.</summary>
    public long owned;
}

/// <summary>
/// OSQP Solver settings.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct OsqpSettings
{
    /// <summary>Device identifier.</summary>
    public long Device;
    /// <summary>Linear system solver to use.</summary>
    public OsqpLinsysSolver LinsysSolver;
    /// <summary>Allocate solution in OSQPSolver during setup.</summary>
    public long AllocateSolution;
    /// <summary>Write out progress.</summary>
    public long Verbose;
    /// <summary>Level of detail for profiler annotations.</summary>
    public long ProfilerLevel;
    /// <summary>Warm start.</summary>
    public long WarmStarting;
    /// <summary>Data scaling iterations.</summary>
    public long Scaling;
    /// <summary>Polish ADMM solution.</summary>
    public long Polishing;

    /// <summary>ADMM penalty parameter.</summary>
    public double Rho;
    /// <summary>Is rho scalar or vector?</summary>
    public long RhoIsVec;
    /// <summary>ADMM penalty parameter.</summary>
    public double Sigma;
    /// <summary>ADMM relaxation parameter.</summary>
    public double Alpha;

    /// <summary>Maximum number of CG iterations per solve.</summary>
    public long CgMaxIter;
    /// <summary>Number of consecutive zero CG iterations before tolerance gets halved.</summary>
    public long CgTolReduction;
    /// <summary>CG tolerance (fraction of ADMM residuals).</summary>
    public double CgTolFraction;
    /// <summary>Preconditioner to use in the CG method.</summary>
    public OsqpPreconditioner CgPrecond;

    /// <summary>Rho stepsize adaption method.</summary>
    public long AdaptiveRho;
    /// <summary>Interval between rho adaptations.</summary>
    public long AdaptiveRhoInterval;
    /// <summary>Adaptation parameter controlling when non-fixed rho adaptations occur.</summary>
    public double AdaptiveRhoFraction;
    /// <summary>Tolerance applied when adapting rho.</summary>
    public double AdaptiveRhoTolerance;

    /// <summary>Maximum number of iterations.</summary>
    public long MaxIter;
    /// <summary>Absolute solution tolerance.</summary>
    public double EpsAbs;
    /// <summary>Relative solution tolerance.</summary>
    public double EpsRel;
    /// <summary>Primal infeasibility tolerance.</summary>
    public double EpsPrimInf;
    /// <summary>Dual infeasibility tolerance.</summary>
    public double EpsDualInf;
    /// <summary>Use scaled termination criteria.</summary>
    public long ScaledTermination;
    /// <summary>Check termination interval.</summary>
    public long CheckTermination;
    /// <summary>Use duality gap termination criteria.</summary>
    public long CheckDualgap;
    /// <summary>Maximum time to solve the problem (seconds).</summary>
    public double TimeLimit;

    /// <summary>Regularization parameter for polishing.</summary>
    public double Delta;
    /// <summary>Number of iterative refinement steps in polishing.</summary>
    public long PolishRefineIter;
}

/// <summary>
/// Information about the solution process.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct OsqpInfo
{
    /// <summary>Status string.</summary>
    public fixed byte Status[32];
    /// <summary>Status value.</summary>
    public long StatusVal;
    /// <summary>Polishing status.</summary>
    public long StatusPolish;

    /// <summary>Primal objective value.</summary>
    public double ObjVal;
    /// <summary>Dual objective value.</summary>
    public double DualObjVal;
    /// <summary>Norm of primal residual.</summary>
    public double PrimRes;
    /// <summary>Norm of dual residual.</summary>
    public double DualRes;
    /// <summary>Duality gap.</summary>
    public double DualityGap;

    /// <summary>Number of iterations taken.</summary>
    public long Iter;
    /// <summary>Number of rho updates performed.</summary>
    public long RhoUpdates;
    /// <summary>Best rho estimate so far.</summary>
    public double RhoEstimate;

    /// <summary>Setup phase time (seconds).</summary>
    public double SetupTime;
    /// <summary>Solve phase time (seconds).</summary>
    public double SolveTime;
    /// <summary>Update phase time (seconds).</summary>
    public double UpdateTime;
    /// <summary>Polish phase time (seconds).</summary>
    public double PolishTime;
    /// <summary>Total solve time (seconds).</summary>
    public double RunTime;

    /// <summary>Integral of duality gap over time.</summary>
    public double PrimDualInt;
    /// <summary>Relative KKT error.</summary>
    public double RelKktError;
}

/// <summary>
/// Structure to hold the computed solution.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct OsqpSolution
{
    /// <summary>Primal solution.</summary>
    public double* X;
    /// <summary>Lagrange multiplier.</summary>
    public double* Y;
    /// <summary>Primal infeasibility certificate.</summary>
    public double* PrimInfCert;
    /// <summary>Dual infeasibility certificate.</summary>
    public double* DualInfCert;
}

/// <summary>
/// Main OSQP solver structure that holds all information.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct OsqpSolverNative
{
    /// <summary>Problem settings.</summary>
    public OsqpSettings* Settings;
    /// <summary>Computed solution.</summary>
    public OsqpSolution* Solution;
    /// <summary>Solver information.</summary>
    public OsqpInfo* Info;
    /// <summary>Internal solver workspace.</summary>
    public void* Work;
}

/// <summary>
/// Native methods for the OSQP library.
/// </summary>
public static partial class NativeMethods
{
    private const string LibName = "libosqp";

    /// <summary>
    /// Gets the default settings.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_set_default_settings")]
    public static unsafe partial void OsqpSetDefaultSettings(OsqpSettings* settings);

    /// <summary>
    /// Initializes the OSQP solver.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_setup")]
    public static unsafe partial OsqpError OsqpSetup(OsqpSolverNative** solver, OsqpCscMatrix* P, double* q, OsqpCscMatrix* A, double* l, double* u, long m, long n, OsqpSettings* settings);

    /// <summary>
    /// Solves the quadratic program.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_solve")]
    public static unsafe partial OsqpError OsqpSolve(OsqpSolverNative* solver);

    /// <summary>
    /// Deallocates memory.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_cleanup")]
    public static unsafe partial OsqpError OsqpCleanup(OsqpSolverNative* solver);

    /// <summary>
    /// Warm starts the solver.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_warm_start")]
    public static unsafe partial OsqpError OsqpWarmStart(OsqpSolverNative* solver, double* x, double* y);

    /// <summary>
    /// Updates problem data vectors.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_update_data_vec")]
    public static unsafe partial OsqpError OsqpUpdateDataVec(OsqpSolverNative* solver, double* q_new, double* l_new, double* u_new);

    /// <summary>
    /// Updates problem data matrices.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_update_data_mat")]
    public static unsafe partial OsqpError OsqpUpdateDataMat(OsqpSolverNative* solver, double* Px_new, long* Px_new_idx, long P_new_n, double* Ax_new, long* Ax_new_idx, long A_new_n);

    /// <summary>
    /// Updates solver settings.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_update_settings")]
    public static unsafe partial OsqpError OsqpUpdateSettings(OsqpSolverNative* solver, OsqpSettings* new_settings);

    /// <summary>
    /// Updates the ADMM parameter rho.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_update_rho")]
    public static unsafe partial OsqpError OsqpUpdateRho(OsqpSolverNative* solver, double rho_new);

    /// <summary>
    /// Gets the OSQP version.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_version")]
    public static partial IntPtr OsqpVersion();

    /// <summary>
    /// Gets the solver capabilities.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "osqp_capabilities")]
    public static partial long OsqpCapabilities();

    /// <summary>
    /// Populates a CSC matrix.
    /// </summary>
    [LibraryImport(LibName, EntryPoint = "OSQPCscMatrix_set_data")]
    public static unsafe partial void OsqpCscMatrixSetData(OsqpCscMatrix* M, long m, long n, long nzmax, double* x, long* i, long* p);
}
