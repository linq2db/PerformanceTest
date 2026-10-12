# linq2db PerformanceTest

Compares linq2db with plain ADO.NET and other data access libraries on SQL Server, and tracks
linq2db's own performance over time. linq2db is built from source: the solution expects a linq2db
checkout next to this repository (`../linq2db`).

Libraries: ADO.NET (baseline), linq2db (Sql, Linq, Linq Param, Compiled), EF Core (Sql, Linq,
Compiled), Dapper, PetaPoco; on .NET Framework 4.8 also EF6, LINQ to SQL and BLToolkit.
Target frameworks: `net10.0` and `net481`.

## Running

```
dotnet build ConsoleRunner/ConsoleRunner.csproj -c Release
dotnet ConsoleRunner/bin/Release/net10.0/ConsoleRunner.dll [--connection "<connection string>"] [perform]
```

`RunAll.cmd` builds the solution and runs both frameworks five times, interleaved.
`UI.cmd` starts the WPF viewer for `Result/Result.sqlite`.

The SQL Server connection string is taken from, in this order:

1. `--connection "<value>"` or `--connection=<value>`;
2. the `PERFTEST_CONNECTION_STRING` environment variable;
3. `Server=.;Database=PerformanceTest;Trusted_Connection=True;TrustServerCertificate=True;`.

The database named in it is created and filled on the first run (two tables of one million rows).
An existing database that this test did not create is never dropped.

## Method

- **Same work for everyone.** Every library runs the same query and reads every row it returns.
  List tests stream: rows are enumerated one by one and discarded, nobody builds a `List<T>`
  (Dapper uses `buffered: false`). Single-value tests read the value.
- **Idiomatic settings.** Change tracking is off unless a test asks for it (EF Core and EF6
  `NoTracking`, LINQ to SQL `ObjectTrackingEnabled = false`). EF Core contexts share one prebuilt
  `DbContextOptions`; context pooling is not used, because linq2db creates a plain `DataConnection`
  per context too. Compiled queries are measured separately where the library has them.
- **linq2db configuration.** The main linq2db providers run with `InlineParameters = true` and
  `ParameterizeTakeSkip = false`. "L2DB Linq Param" runs the LINQ tests with the library
  defaults (parameterized SQL).
- **Connection pools.** Every library uses its own `Application Name`, and so its own pool.
- **Warmup.** Before each timed run the same method runs with the same repeat and take counts until
  one second has passed (at most ten runs), so tiered JIT, caches and pools have settled.
- **Timing.** One timed run per test, provider and process, measured with `Stopwatch.Elapsed`.
  Providers run in random order within a process. Repeat the process (as `RunAll.cmd` does) to get
  several samples; the viewer shows a trimmed mean.

## Results

`Result/Result.sqlite` holds every sample (`TestRun` → `TestMethod` → `TestStopwatch`, times as
`TimeSpan` ticks) and, per run, `TestRunInfo`: runtime, OS, CPU, GC mode, build configuration,
library versions (linq2db with its commit), linq2db settings, warmup rule and SQL Server version.
`Result/<platform>.<group>.txt` holds the last run's table.
