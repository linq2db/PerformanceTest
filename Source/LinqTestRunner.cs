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

		public static void Run(string platform, bool asyncProcessing, string[] args)
		{
			Console.WriteLine($"Testing {platform}...");

#if DEBUG
			DataConnection.TurnTraceSwitchOn();
			DataConnection.WriteTraceLine = (s, s1, arg3) =>
			{
				Debug.WriteLine(s, s1);
			};
#endif

			args = ReadConnectionString(args, out var baseConnectionString);

			var builder = new SqlConnectionStringBuilder(baseConnectionString);

			if (string.IsNullOrEmpty(builder.InitialCatalog))
				builder.InitialCatalog = "PerformanceTest";

			var databaseName = builder.InitialCatalog;

			TestsBase.BaseConnectionString = builder.ConnectionString;

			var connectionString = TestsBase.GetConnectionString("LinqToDB");

			DataConnection.AddConfiguration(
				"Test",
				connectionString,
				SqlServerTools.GetDataProvider(connectionString : connectionString));

			DataConnection.DefaultConfiguration = "Test";

			var basePath = Path.GetDirectoryName(typeof(LinqTestRunner).Assembly.Location);

			while (!Directory.Exists(Path.Combine(basePath, "Result")))
				basePath = Path.GetDirectoryName(basePath);

			var resultPath = Path.Combine(basePath, "Result");

			CreateResultDatabase(false, resultPath);
			CreateTestDatabase  (false, builder, databaseName);

			RecordEnvironment();

			for (var index = 0; index < args.Length; index++)
				Console.WriteLine($"{index}: {args[index]}");

			if (args.Contains("perform"))
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

				return;
			}

			RunTests(platform);
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
		/// Takes the connection string from <c>--connection &lt;value&gt;</c> or <c>--connection=&lt;value&gt;</c>,
		/// then from the PERFTEST_CONNECTION_STRING environment variable, then the default,
		/// and returns the remaining arguments (the connection string may hold a password and is not printed).
		/// </summary>
		static string[] ReadConnectionString(string[] args, out string connectionString)
		{
			string? fromArgs = null;
			var     rest     = new List<string>();

			for (var i = 0; i < args.Length; i++)
			{
				if (args[i] == "--connection" && i + 1 < args.Length)
					fromArgs = args[++i];
				else if (args[i].StartsWith("--connection="))
					fromArgs = args[i].Substring("--connection=".Length);
				else
					rest.Add(args[i]);
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

			return rest.ToArray();
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
					// The database name is configurable now: never drop a database this harness did not create.
					if (db.Execute<object>("SELECT OBJECT_ID(@name, N'U')", new DataParameter("name", quotedName + ".dbo.Setting", DataType.NVarChar)) == null)
						throw new InvalidOperationException($"Database '{databaseName}' exists but was not created by this test; use another database name.");

					if (!enforceCreate && db.GetTable<Setting>()
						.DatabaseName(databaseName)
						.Any(s => s.Name == "DB Version" && s.Value == DatabaseVersion))
						return;
				}

				db.Execute($"DROP DATABASE IF EXISTS {quotedName}");
				db.Execute($"CREATE DATABASE {quotedName}");
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
