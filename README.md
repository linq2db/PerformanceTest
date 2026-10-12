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
dotnet ConsoleRunner/bin/Release/net10.0/ConsoleRunner.dll [--connection "<connection string>"] [--group "<group name>"] [perform]
```

`--group` runs one test group only, for example `--group "Single Column"`; an unknown name lists the
groups and exits with code 2. Option names are case-insensitive. Any other argument is an error
(exit code 2), and arguments are never printed, since the connection string may hold a password.
`RunAll.cmd` builds the solution and runs both frameworks five times, interleaved.
`UI.cmd` starts the WPF viewer for `Result/Result.sqlite`.

The SQL Server connection string is taken from, in this order:

1. `--connection "<value>"` or `--connection=<value>`;
2. the `PERFTEST_CONNECTION_STRING` environment variable;
3. `Server=.;Database=PerformanceTest;Trusted_Connection=True;TrustServerCertificate=True;`.

The database named in it is created and filled on the first run (two tables of one million rows).
The test marks a database it creates (table `PerformanceTestOwner`) and drops and recreates only a
marked database whose data is out of date. An existing unmarked database is used as is when it holds
the current test data (as databases from earlier versions of this test do); otherwise the run stops
without changing it.

## Method

- **Same work for everyone.** Every library runs the same query and reads every row it returns.
  List tests stream: rows are enumerated one by one and discarded, nobody builds a `List<T>`
  (Dapper uses `buffered: false`). The one exception is "BLToolkit Sql" (.NET Framework only), whose
  raw-SQL API fills a list (`ExecuteList`). Single-value tests read the value.
- **Same query shape.** The LINQ and compiled variants of every library run the same query.
  Single-value tests call `First()` (`FirstAsync()` in the async tests) on the query, so the
  library asks the server for one row (`TOP (1)`); list and LINQ query tests enumerate the query.
  Raw-SQL providers run the fixed SQL text (`SELECT ID FROM Narrow WHERE ID = 1`).
- **Idiomatic settings.** Change tracking is off unless a test asks for it (EF Core and EF6
  `NoTracking`, LINQ to SQL `ObjectTrackingEnabled = false`). EF Core contexts share one prebuilt
  `DbContextOptions`; context pooling is not used, because linq2db creates a plain `DataConnection`
  per context too. Compiled queries are measured separately where the library has them; they are
  held in static fields and compiled once per process, as an application keeps them, so their
  translation happens during warmup and never inside a timed run.
- **linq2db configuration.** The main linq2db providers run with `InlineParameters = true` and
  `ParameterizeTakeSkip = false`. "L2DB Linq Param" runs the LINQ tests with the library
  defaults (parameterized SQL).
- **Connection pools.** Every library uses its own `Application Name`, and so its own pool. The
  variants of one library share it: all linq2db providers use one pool, all EF Core providers another.
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

Tick units: runs with `TestRunInfo` rows store `TimeSpan` ticks (100 ns; the row "Tick unit" says
so, "Stopwatch frequency" records the timer). Older runs have no `TestRunInfo` rows and stored raw
`Stopwatch` ticks. All of them were recorded on Windows, where the `Stopwatch` frequency is 10 MHz,
so their ticks are `TimeSpan` ticks too and the viewer shows them as such. An old run made on Linux
(1 GHz timer, 100x larger values) cannot be told apart by the viewer: delete such runs.

`Result.sqlite` is never deleted or replaced by the test. If it cannot be opened or has another
version, the run stops and asks to move the file away; a new one is created on the next run.
