using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Expressions;

namespace Tests
{
	using DataModel;
	using Tools;

	public static class TestRunner
	{
		const string DatabaseVersion = "1d";

		public static void Run(string platform)
		{
			Console.WriteLine($"Testing {platform}...");

			var serverName = ".";

			DataConnection.AddConfiguration(
				"Test",
				$"Server={serverName};Database=PerformanceTest;Trusted_Connection=True",
				SqlServerTools.GetDataProvider(SqlServerVersion.v2012));

			DataConnection.DefaultConfiguration = "Test";

			CreateDatabase(false, serverName);
			RunTests(platform);
		}

		static void RunTests(string platform)
		{
			var testProviders = new ITests[]
			{
				new AdoNet.AdoNetTests     (),
				new Dapper.DapperTests     (),
				new L2DB.L2DBSqlTests      (true),
				new L2DB.L2DBLinqTests     (true),
				new L2DB.L2DBCompTests     (true),
				new EFCore.EFCoreSqlTests  (true),
				new EFCore.EFCoreLinqTests (true),
				new EFCore.EFCoreCompTests (true),
#if NETCOREAPP2_0
#else
				new L2S.L2SSqlTests        (true),
				new L2S.L2SLinqTests       (true),
				new L2S.L2SCompTests       (true),
#endif
			};

			var testProvidersCT = new ITests[]
			{
				new AdoNet.AdoNetTests     (),
				new L2DB.L2DBLinqTests     (false),
				new L2DB.L2DBCompTests     (false),
				new EFCore.EFCoreSqlTests  (false),
				new EFCore.EFCoreLinqTests (false),
				new EFCore.EFCoreCompTests (false),
#if NETCOREAPP2_0
#else
				new L2S.L2SSqlTests        (false),
				new L2S.L2SLinqTests       (false),
				new L2S.L2SCompTests       (false),
#endif
			};

			RunTests(platform, "Single Column", testProviders, new[]
			{
				CreateTest<ITests>(t => t.GetSingleColumnFast,  10000),
				CreateTest<ITests>(t => t.GetSingleColumnSlow,  10000),
				CreateTest<ITests>(t => t.GetSingleColumnParam, 10000),
			});

			RunTests(platform, "Narrow List", testProviders, new[]
			{
//				CreateTest<ITests>(t => t.GetNarrowList,        10000, 100),
//				CreateTest<ITests>(t => t.GetNarrowList,        1000, 1000),
				CreateTest<ITests>(t => t.GetNarrowList,        100, 10000),
				CreateTest<ITests>(t => t.GetNarrowList,        10, 100000),
				CreateTest<ITests>(t => t.GetNarrowList,        1, 1000000),
			});

			RunTests(platform, "Wide List", testProviders, new[]
			{
//				CreateTest<ITests>(t => t.GetWideList,          10000, 100),
//				CreateTest<ITests>(t => t.GetWideList,          1000, 1000),
				CreateTest<ITests>(t => t.GetWideList,          100, 10000),
				CreateTest<ITests>(t => t.GetWideList,          10, 100000),
				CreateTest<ITests>(t => t.GetWideList,          1, 1000000),
			});

			RunTests(platform, "Single Column with Change Tracking", testProvidersCT, new[]
			{
				CreateTest<ITests>(t => t.GetSingleColumnFast,  10000),
				CreateTest<ITests>(t => t.GetSingleColumnSlow,  10000),
				CreateTest<ITests>(t => t.GetSingleColumnParam, 10000),
			});

			RunTests(platform, "Narrow List with Change Tracking", testProvidersCT, new[]
			{
//				CreateTest<ITests>(t => t.GetNarrowList,        10000, 100),
//				CreateTest<ITests>(t => t.GetNarrowList,        1000, 1000),
				CreateTest<ITests>(t => t.GetNarrowList,        100, 10000),
				CreateTest<ITests>(t => t.GetNarrowList,        10, 100000),
				CreateTest<ITests>(t => t.GetNarrowList,        1, 1000000),
			});

			RunTests(platform, "Wide List with Change Tracking", testProvidersCT, new[]
			{
//				CreateTest<ITests>(t => t.GetWideList,          10000, 100),
//				CreateTest<ITests>(t => t.GetWideList,          1000, 1000),
				CreateTest<ITests>(t => t.GetWideList,          100, 10000),
				CreateTest<ITests>(t => t.GetWideList,          10, 100000),
				CreateTest<ITests>(t => t.GetWideList,          1, 1000000),
			});
		}

		static readonly Random _random = new Random();

		static void RunTests(string platform, string testName, ITests[] testProviders, Test<ITests>[] testMethods)
		{
			testProviders =
			(
				from p in testProviders
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
					func(p)(stopwatch, m.Repeat, m.Take ?? -1);
					var time = new TimeSpan(stopwatch.ElapsedTicks);

					Console.Write('.');

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

					foreach (var watch in test.Stopwatch)
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
				AdoNet       = t.Stopwatch.SingleOrDefault(w => w?.p is AdoNet.AdoNetTests)?.time,
				Dapper       = t.Stopwatch.SingleOrDefault(w => w?.p is Dapper.DapperTests)?.time,

				L2DB_Sql     = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBSqlTests       p &&  p.NoTracking)?.time,
				L2DB_Linq    = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBLinqTests      p &&  p.NoTracking)?.time,
				L2DB_Comp    = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBCompTests      p &&  p.NoTracking)?.time,
				L2DB_CT_Linq = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBLinqTests      p && !p.NoTracking)?.time,
				L2DB_CT_Comp = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBCompTests      p && !p.NoTracking)?.time,
				EF_Sql       = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreSqlTests   p &&  p.NoTracking)?.time,
				EF_Linq      = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreLinqTests  p &&  p.NoTracking)?.time,
				EF_Comp      = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreCompTests  p &&  p.NoTracking)?.time,
				EF_CT_Query  = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreSqlTests   p && !p.NoTracking)?.time,
				EF_CT_Linq   = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreLinqTests  p && !p.NoTracking)?.time,
				EF_CT_Comp   = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreCompTests  p && !p.NoTracking)?.time,
#if NETCOREAPP2_0
#else
				L2S_Sql      = t.Stopwatch.SingleOrDefault(w => w?.p is L2S.L2SSqlTests  p &&  p.NoTracking)?.time,
				L2S_Linq     = t.Stopwatch.SingleOrDefault(w => w?.p is L2S.L2SLinqTests p &&  p.NoTracking)?.time,
				L2S_Comp     = t.Stopwatch.SingleOrDefault(w => w?.p is L2S.L2SCompTests p &&  p.NoTracking)?.time,
				L2S_CT_Sql   = t.Stopwatch.SingleOrDefault(w => w?.p is L2S.L2SSqlTests  p && !p.NoTracking)?.time,
				L2S_CT_Linq  = t.Stopwatch.SingleOrDefault(w => w?.p is L2S.L2SLinqTests p && !p.NoTracking)?.time,
				L2S_CT_Comp  = t.Stopwatch.SingleOrDefault(w => w?.p is L2S.L2SCompTests p && !p.NoTracking)?.time,
#endif
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
					ShortValue    = i % 2 == 0 ? null : (short?)   (i % Int16.MaxValue / 2),
					IntValue      = i % 2 == 1 ? null : (int?)     (i % Int32.MaxValue - 1),
					LongValue     = i % 2 == 0 ? null : (long?)    (i * 2),
					DateTimeValue = i % 2 == 1 ? null : (DateTime?)new DateTime(i),
					StringValue   = i % 2 == 0 ? null : new string(Enumerable.Range(0, 95).Select(n => (char)(n % 30 + (int)' ')).ToArray()),
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
