using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Threading.Tasks;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Expressions;

namespace Tests
{
	using DataModel;
	using Tests;
	using Tools;

	public static class TestRunner
	{
		const string DatabaseVersion = "1";

		public static void Run(string platform)
		{
			Console.WriteLine($"Testing {platform}...");

			var serverName = ".";

			DataConnection.AddConfiguration(
				"Test",
				$"Server={serverName};Database=PerformanceTest;Trusted_Connection=True;Application Name=LinqToDB Test;",
				SqlServerTools.GetDataProvider(SqlServerVersion.v2012));

			DataConnection.DefaultConfiguration = "Test";

			CreateDatabase(false, serverName);
			RunTests(platform);
		}

		static void RunTests(string platform)
		{
//			new L2DB.L2DBLinqTests().GetNarrowList(new Stopwatch(), 10000, 1);
//			return;

			var testProviders = new ITests[]
			{
				new AdoNet  .AdoNetTests    (),
				new Dapper  .DapperTests    (),
				new PetaPoco.PetaPocoTests  (),
				new L2DB    .L2DBSqlTests   (),
				new L2DB    .L2DBLinqTests  (),
				new L2DB    .L2DBCompTests  (),
				new EFCore  .EFCoreSqlTests (),
				new EFCore  .EFCoreLinqTests(),
				new EFCore  .EFCoreCompTests(),
				new BLT     .BLTSqlTests    (),
				new BLT     .BLTLinqTests   (),
				new BLT     .BLTCompTests   (),
				new EF6     .EF6SqlTests    (),
				new EF6     .EF6LinqTests   (),
				new L2S     .L2SSqlTests    (),
				new L2S     .L2SLinqTests   (),
				new L2S     .L2SCompTests   (),
			};

			var testProvidersWithChangeTracking = new ITests[]
			{
				new AdoNet.AdoNetTests     (),
				new L2DB.L2DBLinqTests     { TrackChanges = true },
				new L2DB.L2DBCompTests     { TrackChanges = true },
				new EFCore.EFCoreSqlTests  { TrackChanges = true },
				new EFCore.EFCoreLinqTests { TrackChanges = true },
				new EFCore.EFCoreCompTests { TrackChanges = true },
				new EF6.EF6SqlTests        { TrackChanges = true },
				new EF6.EF6LinqTests       { TrackChanges = true },
				new L2S.L2SSqlTests        { TrackChanges = true },
				new L2S.L2SLinqTests       { TrackChanges = true },
				new L2S.L2SCompTests       { TrackChanges = true },
			};

			RunTests(platform, "Single Column", testProviders.OfType<ISingleColumnTests>(), new[]
			{
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnFast,  10000),
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnSlow,  10000),
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnParam, 10000),
			});

			RunTests(platform, "Single Column with Change Tracking", testProvidersWithChangeTracking.OfType<ISingleColumnTests>(), new[]
			{
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnFast,  10000),
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnSlow,  10000),
				CreateTest<ISingleColumnTests>(t => t.GetSingleColumnParam, 10000),
			});

			RunTests(platform, "Single Column Async", testProviders.OfType<ISingleColumnAsyncTests>(), new[]
			{
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnFastAsync,  10000),
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnSlowAsync,  10000),
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnParamAsync, 10000),
			});

			RunTests(platform, "Single Column Async with Change Tracking", testProvidersWithChangeTracking.OfType<ISingleColumnAsyncTests>(), new[]
			{
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnFastAsync,  10000),
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnSlowAsync,  10000),
				CreateTest<ISingleColumnAsyncTests>(t => t.GetSingleColumnParamAsync, 10000),
			});

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

			RunTests(platform, "Narrow List with Change Tracking", testProvidersWithChangeTracking.OfType<IGetListTests>(), new[]
			{
				CreateTest<IGetListTests>(t => t.GetNarrowList,           10000,  1),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           10000, 10),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           1000, 100),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           100, 1000),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           10, 10000),
				CreateTest<IGetListTests>(t => t.GetNarrowList,           1, 100000),
//				CreateTest<IGetListTests>(t => t.GetNarrowList,           1, 1000000),
			});

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

			RunTests(platform, "Narrow List Async with Change Tracking", testProvidersWithChangeTracking.OfType<IGetListAsyncTests>(), new[]
			{
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 10000,  1),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 10000, 10),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 1000, 100),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 100, 1000),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 10, 10000),
				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 1, 100000),
//				CreateTest<IGetListAsyncTests>(t => t.GetNarrowListAsync, 1, 1000000),
			});

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

			RunTests(platform, "Wide List with Change Tracking", testProvidersWithChangeTracking.OfType<IGetListTests>(), new[]
			{
				CreateTest<IGetListTests>(t => t.GetWideList,             1000,   1),
				CreateTest<IGetListTests>(t => t.GetWideList,             1000,  10),
				CreateTest<IGetListTests>(t => t.GetWideList,             1000, 100),
				CreateTest<IGetListTests>(t => t.GetWideList,             100, 1000),
				CreateTest<IGetListTests>(t => t.GetWideList,             10, 10000),
				CreateTest<IGetListTests>(t => t.GetWideList,             1, 100000),
//				CreateTest<IGetListTests>(t => t.GetWideList,             1, 1000000),
			});

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

			RunTests(platform, "Wide List Async with Change Tracking", testProvidersWithChangeTracking.OfType<IGetListAsyncTests>(), new[]
			{
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1000,   1),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1000,  10),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1000, 100),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   100, 1000),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   10, 10000),
				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1, 100000),
//				CreateTest<IGetListAsyncTests>(t => t.GetWideListAsync,   1, 1000000),
			});

			RunTests(platform, "Linq Query", testProviders.OfType<ILinqQueryTests>(), new[]
			{
				CreateTest<ILinqQueryTests>(t => t.SimpleLinqQuery,     1000,  1),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqFast, 1000,  1),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,  100, 10, 100),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,  100, 10, 1000),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,   20, 10, 250000),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,   10, 10, 500000),
			});

#if !NETCOREAPP2_0
			var wcfTestProviders = new ITests[]
			{
				new AdoNet.AdoNetTests (),
				new L2DB.L2DBLinqTests (),
				new EF6.EF6LinqTests   { TrackChanges = true },
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
				CreateTest<ILinqQueryTests>(t => t.SimpleLinqQuery,      1000,  1),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqFast,  1000,  1),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,    20, 10, 250000),
				CreateTest<ILinqQueryTests>(t => t.ComplicatedLinqSlow,    10, 10, 500000),
			});
#endif

			RunTests("All", "LinqToDB Compare",
			new[]
			{
				new L2DB.L2DBAllTests(platform, L2DB.TestType.AdoNet),
				new L2DB.L2DBAllTests(platform, L2DB.TestType.Linq),
				new L2DB.L2DBAllTests(platform, L2DB.TestType.Async),
				new L2DB.L2DBAllTests(platform, L2DB.TestType.ChangeTracking),
				new L2DB.L2DBAllTests(platform, L2DB.TestType.ChangeTrackingAsync),
			},
			new[]
			{
				CreateTest<L2DB.L2DBAllTests>(t => t.GetList, 1000, 1000),
			});
		}

		static readonly Random _random = new Random();

		static void RunTests<T>(string platform, string testName, IEnumerable<T> testProviders, Test<T>[] testMethods)
			where T : class, ITests
		{
			RunTests<T>(platform, testName, testProviders, p => {}, testMethods);
		}

		static void RunTests<T>(string platform, string testName, IEnumerable<T> providers, Action<T> action, Test<T>[] testMethods)
			where T : class, ITests
		{
			Console.WriteLine($"Testing {testName}...");

			var testProviders =
			(
				from p in providers
				orderby _random.Next()
				select p
			)
			.ToArray();

			var tests = testMethods.Select(m =>
			{
				Console.Write($"{m.Name} / {m.Repeat} ");

				if (m.Take > 0)
					Console.Write($"/ {m.Take} ");

				var func  = m.Func;
				var watch = testProviders.Select(p =>
				{
					p.SetUp();
					action(p);

					// Warmup
					if (func(p)(new Stopwatch(), 1, 1) == false)
					{
						Console.Write(' ');
						return null;
					}

					GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
					GC.Collect();
					GC.WaitForPendingFinalizers();
					GC.Collect();

					// Test
					var stopwatch = new Stopwatch();
					if (func(p)(stopwatch, m.Repeat, m.Take ?? -1) == false)
						return null;
					var time = new TimeSpan(stopwatch.ElapsedTicks);

					Console.Write('.');

					p.TearDown();

					return new { time, stopwatch, p, m.Repeat };
				}).ToArray();

				Console.WriteLine();

				return new { Test = m.Name, m.Repeat, m.Take, Stopwatch = watch, };
			})
			.ToArray();

			Console.WriteLine("Storing results...");

			using (var db = new L2DB.L2DBContext())
			{
				var id = db.TestRuns
						.Value(t => t.Platform,  platform)
						.Value(t => t.Name,      testName)
						.Value(t => t.CreatedOn, () => Sql.CurrentTimestamp)
					.InsertWithIdentity();

				foreach (var test in tests)
				{
					var mid = db.TestMethods
							.Value(t => t.TestRunID, id)
							.Value(t => t.Name,      test.Test)
							.Value(t => t.Repeat,    test.Repeat)
							.Value(t => t.Take,      test.Take)
						.InsertWithIdentity();

					foreach (var watch in test.Stopwatch.Where(w => w != null))
					{
						db.TestStopwatches
								.Value(t => t.TestMethodID, mid)
								.Value(t => t.Time,         watch.time)
								.Value(t => t.Ticks,        watch.stopwatch.ElapsedTicks)
								.Value(t => t.Provider,     watch.p.Name)
							.Insert();
					}
				}

				var list =
				(
					from r in db.TestRuns
					join m in db.TestMethods on r.ID equals m.TestRunID
					join s in db.TestStopwatches on m.ID equals s.TestMethodID
					select new { r, m, s}
				)
				.AsEnumerable()
				.GroupBy(r => new
				{
					r.r.Platform, GroupName = r.r.Name, TestName = r.m.Name, r.m.Repeat, r.m.Take, r.s.Provider
				})
				.Select (g =>
				{
					var count = g.Count();
					var ts    = g.Select(w => w.s.Ticks).OrderBy(t => t).ToList();

					var ticks =
						count ==  1 ? ts[0] :
						count ==  2 ? (long)ts.Average(t => t) :
						count <=  5 ? (long)ts.Skip(1).Take(count - 2).Average(t => t) :
						count <= 10 ? (long)ts.Skip(2).Take(count - 4).Average(t => t) :
						              (long)ts.Skip(count / 5).Take(count - count / 5 * 2).Average(t => t);

					return new TestResult
					{
						Platform  = g.Key.Platform,
						GroupName = g.Key.GroupName,
						TestName  = g.Key.TestName,
						Repeat    = g.Key.Repeat,
						Take      = g.Key.Take,
						TestDescription = g.Key.Take == null ? $"{g.Key.TestName}({g.Key.Repeat})" : $"{g.Key.TestName}({g.Key.Repeat}/{g.Key.Take})",
						Provider  = g.Key.Provider,
						Ticks     = ticks,
						Time      = new TimeSpan(ticks)
					};
				})
				.ToList();

				db.TestResults.Truncate();
				db.TestResults.BulkCopy(list);
			}

			var res = tests.Select(t => new
			{
				t.Test,
				t.Repeat,
				t.Take,
				AdoNet       = t.Stopwatch.SingleOrDefault(w => w?.p is AdoNet.  AdoNetTests)  ?.time,
				Dapper       = t.Stopwatch.SingleOrDefault(w => w?.p is Dapper.  DapperTests)  ?.time,
				PetaPoco     = t.Stopwatch.SingleOrDefault(w => w?.p is PetaPoco.PetaPocoTests)?.time,

				L2DB_Sql     = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBSqlTests)     ?.time,
				L2DB_Linq    = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBLinqTests)    ?.time,
				L2DB_Comp    = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBCompTests)    ?.time,
				EF_Sql       = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreSqlTests) ?.time,
				EF_Linq      = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreLinqTests)?.time,
				EF_Comp      = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreCompTests)?.time,
				BLT_Sql      = t.Stopwatch.SingleOrDefault(w => w?.p is BLT.BLTLinqTests)      ?.time,
				BLT_Linq     = t.Stopwatch.SingleOrDefault(w => w?.p is BLT.BLTLinqTests)      ?.time,
				BLT_Comp     = t.Stopwatch.SingleOrDefault(w => w?.p is BLT.BLTCompTests)      ?.time,
				EF6_Sql      = t.Stopwatch.SingleOrDefault(w => w?.p is EF6.EF6SqlTests)       ?.time,
				EF6_Linq     = t.Stopwatch.SingleOrDefault(w => w?.p is EF6.EF6LinqTests)      ?.time,
				L2S_Sql      = t.Stopwatch.SingleOrDefault(w => w?.p is L2S.L2SSqlTests)       ?.time,
				L2S_Linq     = t.Stopwatch.SingleOrDefault(w => w?.p is L2S.L2SLinqTests)      ?.time,
				L2S_Comp     = t.Stopwatch.SingleOrDefault(w => w?.p is L2S.L2SCompTests)      ?.time,
				LoWCF_Linq   = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.LoWcfLinqTests)   ?.time,
			})
			.ToArray();

			var results = res.ToDiagnosticString();

			Console.WriteLine();
			Console.WriteLine(testName);
			Console.WriteLine(results);

			var basePath = Path.GetDirectoryName(typeof(TestRunner).Assembly.Location);

			while (!Directory.Exists(Path.Combine(basePath, "Result")))
				basePath = Path.GetDirectoryName(basePath);

			var filePath = Path.Combine(basePath, "Result", $"{platform}.{testName}.txt");

			File.WriteAllText(filePath, results);

			Console.WriteLine(filePath);
			Console.WriteLine();
		}

		class Test<T>
		{
			public Func<T,Func<Stopwatch,int,int,bool>> Func;
			public string Name;
			public int    Repeat;
			public int?   Take;
		}

		static Test<T> CreateTest<T>(Expression<Func<T,Func<Stopwatch,int,int,bool>>> func, int repeat, int take = -1)
		{
			return new Test<T>
			{
				Func   = func.Compile(),
				Name   = ((MethodInfo)((ConstantExpression)func.Body
					.Find(e => e is ConstantExpression c && c.Value is MethodInfo)).Value).Name,
				Repeat = repeat,
				Take   = take > 0 ? take : (int?)null
			};
		}

		static Test<T> CreateTest<T>(Expression<Func<T,Func<Stopwatch,int,int,int,bool>>> func, int repeat, int take, int parm)
		{
			var cfunc = func.Compile();
			var name  = ((MethodInfo)((ConstantExpression)func.Body
				.Find(e => e is ConstantExpression c && c.Value is MethodInfo)).Value).Name;

			return new Test<T>
			{
				Func   = p => (sw,r,t) => cfunc(p)(sw, r, t, parm),
				Name   = $"{name}({parm})",
				Repeat = repeat,
				Take   = take > 0 ? take : (int?)null
			};
		}

		static Test<T> CreateTest<T>(Expression<Func<T,Func<Stopwatch,int,int,Task<bool>>>> func, int repeat, int take = -1)
		{
			var cfunc = func.Compile();
			var name  = ((MethodInfo)((ConstantExpression)func.Body
				.Find(e => e is ConstantExpression c && c.Value is MethodInfo)).Value).Name;

			return new Test<T>
			{
				Func   = p => (sw,r,t) => cfunc(p)(sw, r, t).Result,
				Name   = name.Replace("Async", ""),
				Repeat = repeat,
				Take   = take > 0 ? take : (int?)null
			};
		}

		static void CreateDatabase(bool enforceCreate, string serverName)
		{
			Console.WriteLine("Creating database...");

			using (var db = SqlServerTools.CreateDataConnection($"Server={serverName};Database=master;Trusted_Connection=True"))
			{
				if (!enforceCreate)
					if (db.Execute<object>("SELECT db_id('PerformanceTest')") != null)
						if (db.Execute<object>("SELECT OBJECT_ID('PerformanceTest.dbo.Setting', N'U')") != null)
							if (db.GetTable<Setting>()
								.DatabaseName("PerformanceTest")
								.Any(s => s.Name == "DB Version" && s.Value == DatabaseVersion))
								return;

				db.Execute("DROP DATABASE IF EXISTS PerformanceTest");
				db.Execute("CREATE DATABASE PerformanceTest");
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
				db.CreateTable<TestRun>();
				db.CreateTable<TestMethod>();
				db.CreateTable<TestStopwatch>();
				db.CreateTable<TestResult>();
			}

			Console.WriteLine("Database created.");
		}

		static void CreateTable<T>(DataConnection db, IEnumerable<T> data)
		{
			var list = data.ToList();

			db.CreateTempTable(
				list,
				new BulkCopyOptions
				{
					NotifyAfter = 10000,
					RowsCopiedCallback = c => Console.Write($"\rCopying {typeof(T).Name} {c.RowsCopied} of {list.Count}...")
				});

			Console.WriteLine();
		}
	}
}
