#define TEST_CHANGE_TRACKING1
#define TEST_ASYNC1
#define TEST_WCF1

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;

using Microsoft.Data.SqlClient;

using TestRunner;
using TestRunner.DataModel;
using TestRunner.Tools;

namespace Tests
{
	using DataModel;
	using Tests;

	public class LinqTestRunner : TestRunner.TestRunner
	{
		const string DatabaseVersion = "1";

		/// <summary>
		/// Used when neither <c>--connection</c> nor the PERFTEST_CONNECTION_STRING environment variable is set.
		/// </summary>
		const string DefaultConnectionString = "Server=.;Database=PerformanceTest;Trusted_Connection=True;TrustServerCertificate=True;";

		/// <summary>
		/// Runs the test plan. Returns the process exit code: 0 on success, 2 on bad arguments.
		/// </summary>
		public static int Run(string platform, bool asyncProcessing, string[] args)
		{
			Console.WriteLine($"Testing {platform}...");

			if (!ReadOptions(args, out var baseConnectionString, out var perform))
				return 2;

			if (GroupFilter != null)
			{
				// Walk the plan without running anything to check the group name before any database work.
				ListOnly = true;

				try
				{
					RunPlan(platform, perform);
				}
				finally
				{
					ListOnly = false;
				}

				if (!KnownGroups.Contains(GroupFilter, StringComparer.OrdinalIgnoreCase))
				{
					Console.Error.WriteLine($"Unknown test group '{GroupFilter}'. Groups{(perform ? " in perform mode" : "")}:");

					foreach (var group in KnownGroups)
						Console.Error.WriteLine($"  {group}");

					return 2;
				}
			}

#if DEBUG
			DataConnection.TurnTraceSwitchOn();
			DataConnection.WriteTraceLine = (s, s1, arg3) =>
			{
				Debug.WriteLine(s, s1);
			};
#endif

			var builder = new SqlConnectionStringBuilder(baseConnectionString);

			if (string.IsNullOrEmpty(builder.InitialCatalog))
				builder.InitialCatalog = "PerformanceTest";

			var databaseName = builder.InitialCatalog;

			TestsBase.BaseConnectionString = builder.ConnectionString;

			var connectionString = TestsBase.GetConnectionString("LinqToDB");

			// The server version is detected through master: the test database may not exist yet.
			var masterConnectionString = new SqlConnectionStringBuilder(builder.ConnectionString) { InitialCatalog = "master" }.ConnectionString;

			DataConnection.AddConfiguration(
				"Test",
				connectionString,
				SqlServerTools.GetDataProvider(connectionString : masterConnectionString));

			DataConnection.DefaultConfiguration = "Test";

			var basePath = Path.GetDirectoryName(typeof(LinqTestRunner).Assembly.Location);

			while (!Directory.Exists(Path.Combine(basePath, "Result")))
				basePath = Path.GetDirectoryName(basePath);

			var resultPath = Path.Combine(basePath, "Result");

			try
			{
				CreateResultDatabase(false, resultPath);
				CreateTestDatabase  (false, builder, databaseName);
			}
			catch (InvalidOperationException ex)
			{
				// Refusals to touch an existing results file or database: a message, not a crash.
				Console.Error.WriteLine(ex.Message);
				return 1;
			}

			RecordEnvironment();

			RunPlan(platform, perform);

			return 0;
		}

		static void RunPlan(string platform, bool perform)
		{
			if (perform)
				RunPerform(platform);
			else
				RunTests(platform);
		}

		/// <summary>
		/// The ad-hoc profiling plan selected by the "perform" argument.
		/// </summary>
		static void RunPerform(string platform)
		{
			var testProviders = new ITests[]
			{
				new AdoNet   .AdoNetTests       (),
				new L2DB     .L2DBSqlTests      (),
				new L2DB     .L2DBLinqTests     (),
				new L2DB     .L2DBLinqParamTests(),
				new L2DB     .L2DBCompTests     (),
				new EFCore   .EFCoreSqlTests    (),
				new EFCore   .EFCoreLinqTests   (),
				new EFCore   .EFCoreCompTests   (),
			};

			RunTests(platform, "Linq Query Test", testProviders.OfType<ILinqQueryTests>(), new[]
			{
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow, 1, 10, 500000),
			});

//				RunTests(platform, "Narrow List", testProviders.OfType<IGetListTests>(), new[]
//				{
//					CreateTest<IGetListTests>(t => t.GetNarrowList, 100,   10000),
//					CreateTest<IGetListTests>(t => t.GetNarrowList, 100,    100000),
//					CreateTest<IGetListTests>(t => t.GetNarrowList, 50000, 1),
//				});
//
//				RunTests(platform, "Single Column", testProviders.OfType<ISingleColumnTests>(), new[]
//				{
//					CreateTest<ISingleColumnTests>(t => t.GetSingleColumnFast,  100000),
//					CreateTest<ISingleColumnTests>(t => t.GetSingleColumnSlow,  10000),
//					CreateTest<ISingleColumnTests>(t => t.GetSingleColumnParam, 10000),
//				});
		}

		static void RunTests(string platform)
		{
//			new L2DB.L2DBLinqTests().GetNarrowList(new Stopwatch(), 10000, 1);
//			return;

			var testProviders = new ITests[]
			{
				new AdoNet   .AdoNetTests       (),
				new Dapper   .DapperTests       (),
				new PetaPoco .PetaPocoTests     (),
				new L2DB     .L2DBSqlTests      (),
				new L2DB     .L2DBLinqTests     (),
				new L2DB     .L2DBLinqParamTests(),
				new L2DB     .L2DBCompTests     (),
				new EFCore   .EFCoreSqlTests    (),
				new EFCore   .EFCoreLinqTests   (),
				new EFCore   .EFCoreCompTests   (),
				new BLToolkit.BLToolkitSqlTests (),
				new BLToolkit.BLToolkitLinqTests(),
				new BLToolkit.BLToolkitCompTests(),
				new EF6      .EF6SqlTests       (),
				new EF6      .EF6LinqTests      (),
				new L2S      .L2SSqlTests       (),
				new L2S      .L2SLinqTests      (),
				new L2S      .L2SCompTests      (),
			};

/*
RunTests("All", "LinqToDB Compare",
	new[]
	{
		new L2DB.L2DBAllTests(platform, L2DB.TestType.AdoNet),
		new L2DB.L2DBAllTests(platform, L2DB.TestType.Linq),
	},
	new[]
	{
		CreateTest<L2DB.L2DBAllTests>(t => t.GetList, 1000, 1000),
	});

RunTests(platform, "Narrow List", testProviders.OfType<IGetListTests>(), new[]
{
	CreateTest<IGetListTests>(t => t.GetNarrowList, 10000, 1),
	CreateTest<IGetListTests>(t => t.GetNarrowList, 10000, 10),
	CreateTest<IGetListTests>(t => t.GetNarrowList, 10000, 100),
	CreateTest<IGetListTests>(t => t.GetNarrowList, 1000,  1000),
	CreateTest<IGetListTests>(t => t.GetNarrowList, 100,   10000),
	CreateTest<IGetListTests>(t => t.GetNarrowList, 10,    100000),
	CreateTest<IGetListTests>(t => t.GetNarrowList, 1,     1000000),
});
return;
*/





			var testProvidersWithChangeTracking = new ITests[]
			{
				new AdoNet.AdoNetTests     (),
				new L2DB.  L2DBLinqTests   { TrackChanges = true },
				new L2DB.  L2DBCompTests   { TrackChanges = true },
				new EFCore.EFCoreSqlTests  { TrackChanges = true },
				new EFCore.EFCoreLinqTests { TrackChanges = true },
				new EFCore.EFCoreCompTests { TrackChanges = true },
				new EF6.   EF6SqlTests     { TrackChanges = true },
				new EF6.   EF6LinqTests    { TrackChanges = true },
				new L2S.   L2SSqlTests     { TrackChanges = true },
				new L2S.   L2SLinqTests    { TrackChanges = true },
				new L2S.   L2SCompTests    { TrackChanges = true },
			};

			RunTests("All", "LinqToDB Compare",
			new[]
			{
				new L2DB.L2DBAllTests(platform, L2DB.TestType.AdoNet),
				new L2DB.L2DBAllTests(platform, L2DB.TestType.Linq),
#if TEST_ASYNC
				new L2DB.L2DBAllTests(platform, L2DB.TestType.Async),
#endif
#if TEST_CHANGE_TRACKING
				new L2DB.L2DBAllTests(platform, L2DB.TestType.ChangeTracking),
#if TEST_ASYNC
				new L2DB.L2DBAllTests(platform, L2DB.TestType.ChangeTrackingAsync),
#endif
#endif
			},
			new[]
			{
				CreateTest<L2DB.L2DBAllTests>(t => t.GetList, 1000, 1000),
			});

			RunTests("All", "EF Core Compare",
			new[]
			{
				new EFCore.EFCoreAllTests(platform, EFCore.TestType.AdoNet),
				new EFCore.EFCoreAllTests(platform, EFCore.TestType.Linq),
#if TEST_ASYNC
				new EFCore.EFCoreAllTests(platform, EFCore.TestType.Async),
#endif
#if TEST_CHANGE_TRACKING
				new EFCore.EFCoreAllTests(platform, EFCore.TestType.ChangeTracking),
#if TEST_ASYNC
				new EFCore.EFCoreAllTests(platform, EFCore.TestType.ChangeTrackingAsync),
#endif
#endif
			},
			new[]
			{
				CreateTest<EFCore.EFCoreAllTests>(t => t.GetList, 1000, 1000),
			});

			RunTests(platform, "Single Column", testProviders.OfType<ISingleColumnTests>(), new[]
			{
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnFast,  10000),
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnSlow,  10000),
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnParam, 10000),
			});

#if TEST_CHANGE_TRACKING
			RunTests(platform, "Single Column with Change Tracking", testProvidersWithChangeTracking.OfType<ISingleColumnTests>(), new[]
			{
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnFast,  10000),
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnSlow,  10000),
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnParam, 10000),
			});
#endif

#if TEST_ASYNC
			RunTests(platform, "Single Column Async", testProviders.OfType<ISingleColumnAsyncTests>(), new[]
			{
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnFastAsync,  10000),
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnSlowAsync,  10000),
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnParamAsync, 10000),
			});
#endif

#if TEST_CHANGE_TRACKING
#if TEST_ASYNC
			RunTests(platform, "Single Column Async with Change Tracking", testProvidersWithChangeTracking.OfType<ISingleColumnAsyncTests>(), new[]
			{
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnFastAsync,  10000),
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnSlowAsync,  10000),
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnParamAsync, 10000),
			});
#endif
#endif

			RunTests(platform, "Narrow List", testProviders.OfType<IGetListTests>(), new[]
			{
				CreateTest<IGetListTests>(t => t.GetNarrowList,           10000,   1),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           10000,  10),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           10000, 100),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           1000, 1000),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           100, 10000),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           10, 100000),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           1, 1000000),
			});

#if TEST_CHANGE_TRACKING
			RunTests(platform, "Narrow List with Change Tracking", testProvidersWithChangeTracking.OfType<IGetListTests>(), new[]
			{
				CreateTest<IGetListTests>(t => t.GetNarrowList,           10000,  1),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           10000, 10),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           1000, 100),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           1000, 1000),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           100, 1000),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           10, 10000),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           1, 100000),
//				CreateTest<IGetListTests>(t => t.GetNarrowList,           1, 1000000),
			});
#endif

#if TEST_ASYNC
			RunTests(platform, "Narrow List Async", testProviders.OfType<IGetListAsyncTests>(), new[]
			{
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 10000,   1),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 10000,  10),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 10000, 100),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 1000, 1000),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 100, 10000),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 10, 100000),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 1, 1000000),
			});
#endif

#if TEST_CHANGE_TRACKING
#if TEST_ASYNC
			RunTests(platform, "Narrow List Async with Change Tracking", testProvidersWithChangeTracking.OfType<IGetListAsyncTests>(), new[]
			{
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 10000,  1),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 10000, 10),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 1000, 100),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 1000, 1000),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 100, 1000),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 10, 10000),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 1, 100000),
//				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 1, 1000000),
			});
#endif
#endif

			RunTests(platform, "Wide List", testProviders.OfType<IGetListTests>(), new[]
			{
				CreateTest<IGetListTests>(t => t.GetWideList,             10000,   1),
				CreateTest<IGetListTests>(t => t.GetWideList,             10000,  10),
				CreateTest<IGetListTests>(t => t.GetWideList,             10000, 100),
				CreateTest<IGetListTests>(t => t.GetWideList,             1000, 1000),
				CreateTest<IGetListTests>(t => t.GetWideList,             100, 10000),
				CreateTest<IGetListTests>(t => t.GetWideList,             10, 100000),
				CreateTest<IGetListTests>(t => t.GetWideList,             1, 1000000),
			});

#if TEST_CHANGE_TRACKING
			RunTests(platform, "Wide List with Change Tracking", testProvidersWithChangeTracking.OfType<IGetListTests>(), new[]
			{
				CreateTest<IGetListTests>(t => t.GetWideList,             1000,   1),
				CreateTest<IGetListTests>(t => t.GetWideList,             1000,  10),
				CreateTest<IGetListTests>(t => t.GetWideList,             1000, 100),
				CreateTest<IGetListTests>(t => t.GetWideList,             1000, 1000),
				CreateTest<IGetListTests>(t => t.GetWideList,             100, 1000),
				CreateTest<IGetListTests>(t => t.GetWideList,             10, 10000),
				CreateTest<IGetListTests>(t => t.GetWideList,             1, 100000),
//				CreateTest<IGetListTests>(t => t.GetWideList,             1, 1000000),
			});
#endif

#if TEST_ASYNC
			RunTests(platform, "Wide List Async", testProviders.OfType<IGetListAsyncTests>(), new[]
			{
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   10000,   1),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   10000,  10),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   10000, 100),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1000, 1000),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   100, 10000),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   10, 100000),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1, 1000000),
			});
#endif

#if TEST_CHANGE_TRACKING
#if TEST_ASYNC
			RunTests(platform, "Wide List Async with Change Tracking", testProvidersWithChangeTracking.OfType<IGetListAsyncTests>(), new[]
			{
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1000,   1),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1000,  10),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1000, 100),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1000, 1000),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   100, 1000),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   10, 10000),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1, 100000),
//				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1, 1000000),
			});
#endif
#endif

			RunTests(platform, "Linq Query", testProviders.OfType<ILinqQueryTests>(), new[]
			{
				CreateTest<ILinqQueryTests>(t => t.SimpleLinqQuery,     1000),
				CreateTest<ILinqQueryTests>(t => t.SimpleLinqQueryTop,  1000,  1),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqFast, 1000,  1),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,  100, 10, 100),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,  100, 10, 1000),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,   20, 10, 250000),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,   10, 10, 500000),
			});

#if NET481
#if TEST_WCF
			var wcfTestProviders = new ITests[]
			{
				new AdoNet.AdoNetTests (),
				new L2DB.L2DBLinqTests (),
				new L2DB.LoWcfLinqTests(),
			};

			RunTests(platform, "Linq over WCF Single Column", wcfTestProviders.OfType<ISingleColumnTests>(), new[]
			{
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnFast,  1000),
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnSlow,  1000),
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnParam, 1000),
			});

			RunTests(platform, "Linq over WCF Narrow List", wcfTestProviders.OfType<IGetListTests>(), new[]
			{
				CreateTest<IGetListTests>(t => t.GetNarrowList,        1000,   1),
				CreateTest<IGetListTests>(t => t.GetNarrowList,        1000,  10),
				CreateTest<IGetListTests>(t => t.GetNarrowList,        1000, 100),
				CreateTest<IGetListTests>(t => t.GetNarrowList,        100, 1000),
				CreateTest<IGetListTests>(t => t.GetNarrowList,        10, 10000),
				CreateTest<IGetListTests>(t => t.GetNarrowList,        1, 100000),
			});

			RunTests(platform, "Linq over WCF Wide List", wcfTestProviders.OfType<IGetListTests>(), new[]
			{
				CreateTest<IGetListTests>(t => t.GetWideList,          1000,   1),
				CreateTest<IGetListTests>(t => t.GetWideList,          1000,  10),
				CreateTest<IGetListTests>(t => t.GetWideList,          1000, 100),
				CreateTest<IGetListTests>(t => t.GetWideList,          100, 1000),
				CreateTest<IGetListTests>(t => t.GetWideList,          10, 10000),
			});

			RunTests(platform, "Linq over WCF Linq Query", wcfTestProviders.OfType<ILinqQueryTests>(), new[]
			{
				CreateTest<ILinqQueryTests>(t => t.SimpleLinqQuery,      1000),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqFast,  1000,  1),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,    20, 10, 250000),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,    10, 10, 500000),
			});
#endif
#endif
		}

		/// <summary>
		/// Reads the arguments: <c>--group &lt;name&gt;</c> (run only that test group), <c>perform</c>
		/// (the profiling plan) and the connection string: <c>--connection &lt;value&gt;</c> or
		/// <c>--connection=&lt;value&gt;</c>, then the PERFTEST_CONNECTION_STRING environment variable,
		/// then the default. Option names are case-insensitive. Any other argument is an error;
		/// arguments are never printed because one may hold a password.
		/// </summary>
		static bool ReadOptions(string[] args, out string connectionString, out bool perform)
		{
			string? fromArgs = null;

			perform          = false;
			connectionString = "";

			for (var i = 0; i < args.Length; i++)
			{
				var arg = args[i];

				if (string.Equals(arg, "--connection", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
					fromArgs = args[++i];
				else if (arg.StartsWith("--connection=", StringComparison.OrdinalIgnoreCase))
					fromArgs = arg.Substring("--connection=".Length);
				else if (string.Equals(arg, "--group", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
					GroupFilter = args[++i];
				else if (arg.StartsWith("--group=", StringComparison.OrdinalIgnoreCase))
					GroupFilter = arg.Substring("--group=".Length);
				else if (string.Equals(arg, "perform", StringComparison.OrdinalIgnoreCase))
					perform = true;
				else
				{
					Console.Error.WriteLine($"Unknown or incomplete argument #{i + 1} (not shown: it may hold a password).");
					Console.Error.WriteLine("Usage: ConsoleRunner [--connection <connection string>] [--group <group name>] [perform]");
					return false;
				}
			}

			var fromEnvironment = Environment.GetEnvironmentVariable("PERFTEST_CONNECTION_STRING");

			if (!string.IsNullOrEmpty(fromArgs))
			{
				Console.WriteLine("Connection string: --connection");
				connectionString = fromArgs!;
			}
			else if (!string.IsNullOrEmpty(fromEnvironment))
			{
				Console.WriteLine("Connection string: PERFTEST_CONNECTION_STRING");
				connectionString = fromEnvironment!;
			}
			else
			{
				Console.WriteLine("Connection string: default");
				connectionString = DefaultConnectionString;
			}

			return true;
		}

		static void RecordEnvironment()
		{
			RunEnvironment.AddVersion("linq2db",                  typeof(DataConnection));
			RunEnvironment.AddVersion("Microsoft.Data.SqlClient", typeof(SqlConnection));
			RunEnvironment.AddVersion("EF Core",                  typeof(Microsoft.EntityFrameworkCore.DbContext));
			RunEnvironment.AddVersion("Dapper",                   typeof(global::Dapper.SqlMapper));
			RunEnvironment.AddVersion("PetaPoco",                 typeof(global::PetaPoco.Database));
#if NET481
			RunEnvironment.AddVersion("EF6",                      typeof(System.Data.Entity.DbContext));
			RunEnvironment.AddVersion("BLToolkit",                typeof(global::BLToolkit.Data.DbManager));
			RunEnvironment.AddVersion("LINQ to SQL",              typeof(System.Data.Linq.DataContext));
#endif

			RunEnvironment.Items["linq2db settings"] = L2DB.L2DBContext.SettingsDescription;
			RunEnvironment.Items["Warmup"]           = $"full repeat x take, until {WarmupTime.TotalSeconds:0.###} s, at most {WarmupMaxRuns} runs";

			try
			{
				using var db = new L2DB.L2DBContext();

				RunEnvironment.Items["Database server"] = db.Execute<string>("SELECT CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(128)) + N' ' + CAST(SERVERPROPERTY('Edition') AS nvarchar(128))");
			}
			catch (Exception ex)
			{
				RunEnvironment.Items["Database server"] = ex.Message;
			}
		}

		const string OwnershipMarkerTable = "PerformanceTestOwner";
		const string OwnershipMarker      = "Created by linq2db PerformanceTest; the test may drop and recreate this database.";

		static readonly string[] TestTables = { "Setting", "Narrow", "NarrowLong", "WideLong" };

		static bool HasOwnershipMarker(DataConnection db, string quotedName)
		{
			if (db.Execute<object>("SELECT OBJECT_ID(@name, N'U')",
				new DataParameter("name", $"{quotedName}.dbo.{OwnershipMarkerTable}", DataType.NVarChar)) == null)
				return false;

			return db.Execute<int>(
				$"SELECT COUNT(*) FROM {quotedName}.dbo.{OwnershipMarkerTable} WHERE Marker = @marker",
				new DataParameter("marker", OwnershipMarker, DataType.NVarChar)) > 0;
		}

		/// <summary>
		/// True when the database holds all test tables and the current data version.
		/// </summary>
		static bool IsCurrent(DataConnection db, string quotedName, string databaseName)
		{
			foreach (var table in TestTables)
				if (db.Execute<object>("SELECT OBJECT_ID(@name, N'U')",
					new DataParameter("name", $"{quotedName}.dbo.{table}", DataType.NVarChar)) == null)
					return false;

			return db.GetTable<Setting>()
				.DatabaseName(databaseName)
				.Any(s => s.Name == "DB Version" && s.Value == DatabaseVersion);
		}

		static void CreateTestDatabase(bool enforceCreate, SqlConnectionStringBuilder builder, string databaseName)
		{
			Console.WriteLine("Creating database...");

			var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
			{
				InitialCatalog  = "master",
				ApplicationName = "PerformanceTest Setup",
			};

			using (var db = SqlServerTools.CreateDataConnection(masterBuilder.ConnectionString))
			{
				var quotedName = "[" + databaseName.Replace("]", "]]") + "]";

				if (db.Execute<object>("SELECT db_id(@name)", new DataParameter("name", databaseName, DataType.NVarChar)) != null)
				{
					var hasMarker = HasOwnershipMarker(db, quotedName);

					if (!enforceCreate && IsCurrent(db, quotedName, databaseName))
						return;

					// The database name is configurable: drop only a database that carries the marker
					// this harness writes when it creates one. Anything else is left alone.
					if (!hasMarker)
						throw new InvalidOperationException(
							$"Database '{databaseName}' exists, but it is not marked as created by this test " +
							$"(no {OwnershipMarkerTable} row) or its data is out of date. It was not changed. " +
							"Drop it yourself or use another database name.");

					db.Execute($"DROP DATABASE {quotedName}");
				}

				db.Execute($"CREATE DATABASE {quotedName}");

				// The first thing in a new database: the proof that this harness owns it.
				db.Execute($"CREATE TABLE {quotedName}.dbo.{OwnershipMarkerTable} (Marker nvarchar(200) NOT NULL)");
				db.Execute(
					$"INSERT INTO {quotedName}.dbo.{OwnershipMarkerTable} (Marker) VALUES (@marker)",
					new DataParameter("marker", OwnershipMarker, DataType.NVarChar));
			}

			using (var db = new L2DB.L2DBContext())
			{
				CreateTable(db, new[] { new Setting { Name = "DB Version", Value =  DatabaseVersion } });
				CreateTable(db, new[] { new Narrow { ID = 1, Field1 = 2 } });
				CreateTable(db, Enumerable.Range(1, 1000000).Select(i => new NarrowLong { ID = i, Field1 = -i }));
				CreateTable(db, Enumerable.Range(1, 1000000).Select(i => new WideLong
				{
					ID            = i,
					Field1        = -i,
					ByteValue     = i % 2 == 0 ? null : (byte?)    (i % Byte.MaxValue  / 2),
					ShortValue    = i % 2 == 0 ? null : (short?)   (i % Int16.MaxValue / 2),
					IntValue      = i % 2 == 1 ? null : (int?)     (i % Int32.MaxValue - 1),
					LongValue     = i % 2 == 0 ? null : (long?)    (i * 2),
					StringValue   = i % 2 == 0 ? null : new string(Enumerable.Range(0, 95).Select(n => (char)(n % 30 + (int)' ')).ToArray()),
					DateTimeValue = i % 2 == 1 ? null : (DateTime?)new DateTime(i),
					TimeValue     = i % 2 == 1 ? null : (TimeSpan?)new TimeSpan(i),
					DecimalValue  = i % 2 == 0 ? null : (decimal?)i,
					DoubleValue   = i % 2 == 1 ? null : (double?)i,
					FloatValue    = i % 2 == 0 ? null : (float?)i,
				}));
			}

			Console.WriteLine("Database created.");
		}
	}
}
