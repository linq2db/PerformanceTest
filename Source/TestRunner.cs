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
		public static void Run(string platform)
		{
			Console.WriteLine($"Testing {platform}...");

			var serverName = ".";

			DataConnection.AddConfiguration(
				"Test",
				$"Server={serverName};Database=PerformanceTest;Trusted_Connection=True",
				SqlServerTools.GetDataProvider(SqlServerVersion.v2012));

			DataConnection.DefaultConfiguration = "Test";

			//CreateDatabase(serverName);
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
				new L2DB.L2DBLinqTests     (false),
				new L2DB.L2DBCompTests     (false),
#if NETCOREAPP2_0
				new EFCore.EFCoreSqlTests  (true),
				new EFCore.EFCoreLinqTests (true),
				new EFCore.EFCoreCompTests (true),
				new EFCore.EFCoreSqlTests  (false),
				new EFCore.EFCoreLinqTests (false),
				new EFCore.EFCoreCompTests (false),
#else
				new L2S.L2SSqlTests        (true),
				new L2S.L2SLinqTests       (true),
				new L2S.L2SCompTests       (true),
				new L2S.L2SSqlTests        (false),
				new L2S.L2SLinqTests       (false),
				new L2S.L2SCompTests       (false),
#endif
			};

//			RunTests(platform, "Single Column", testProviders, new[]
//			{
//				CreateTest<ITests>(t => t.GetSingleColumnFast,  10000),
//				CreateTest<ITests>(t => t.GetSingleColumnSlow,  10000),
//				CreateTest<ITests>(t => t.GetSingleColumnParam, 10000),
//			});

			RunTests(platform, "Narrow List", testProviders, new[]
			{
//				CreateTest<ITests>(t => t.GetNarrowList,        10000, 100),
//				CreateTest<ITests>(t => t.GetNarrowList,        1000, 1000),
//				CreateTest<ITests>(t => t.GetNarrowList,        100, 10000),
				CreateTest<ITests>(t => t.GetNarrowList,        10, 100000),
				CreateTest<ITests>(t => t.GetNarrowList,        1, 1000000),
			});

			RunTests(platform, "Wide List", testProviders, new[]
			{
//				CreateTest<ITests>(t => t.GetWideList,          10000, 100),
//				CreateTest<ITests>(t => t.GetWideList,          1000, 1000),
//				CreateTest<ITests>(t => t.GetWideList,          100, 10000),
				CreateTest<ITests>(t => t.GetWideList,          10, 100000),
				CreateTest<ITests>(t => t.GetWideList,          1, 1000000),
			});
		}

		static void RunTests(string platform, string testName, ITests[] testProviders, Test<ITests>[] testMethods)
		{
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
			.ToArray()
			.Select(t => new
			{
				t.Test,
				t.Repeat,
				t.Take,
				AdoNet_ID    = t.Stopwatch.SingleOrDefault(w => w?.p is AdoNet.AdoNetTests)?.time,
				Dapper       = t.Stopwatch.SingleOrDefault(w => w?.p is Dapper.DapperTests)?.time,

				L2DB_Sql     = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBSqlTests       p &&  p.NoTracking)?.time,
				L2DB_Linq    = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBLinqTests      p &&  p.NoTracking)?.time,
				L2DB_Comp    = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBCompTests      p &&  p.NoTracking)?.time,
				L2DB_CT_Linq = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBLinqTests      p && !p.NoTracking)?.time,
				L2DB_CT_Comp = t.Stopwatch.SingleOrDefault(w => w?.p is L2DB.L2DBCompTests      p && !p.NoTracking)?.time,
#if NETCOREAPP2_0
				EF_Sql       = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreSqlTests   p &&  p.NoTracking)?.time,
				EF_Linq      = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreLinqTests  p &&  p.NoTracking)?.time,
				EF_Comp      = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreCompTests  p &&  p.NoTracking)?.time,
				EF_CT_Query  = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreSqlTests   p && !p.NoTracking)?.time,
				EF_CT_Linq   = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreLinqTests  p && !p.NoTracking)?.time,
				EF_CT_Comp   = t.Stopwatch.SingleOrDefault(w => w?.p is EFCore.EFCoreCompTests  p && !p.NoTracking)?.time,
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

			var results = tests.ToDiagnosticString();

			Console.WriteLine();
			Console.WriteLine(testName);
			Console.WriteLine(results);

			var basePath = Path.GetDirectoryName(typeof(TestRunner).Assembly.Location);

			while (!Directory.Exists(Path.Combine(basePath, "Result")))
			{
				basePath = Path.GetDirectoryName(basePath);
			}

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

		static void CreateDatabase(string serverName = ".")
		{
			Console.WriteLine("Creating database...");

			using (var db = SqlServerTools.CreateDataConnection($"Server={serverName};Database=master;Trusted_Connection=True"))
			{
				db.Execute("DROP DATABASE IF EXISTS PerformanceTest");
				db.Execute("CREATE DATABASE PerformanceTest");
			}

			using (var db = new L2DB.L2DBContext())
			{
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
