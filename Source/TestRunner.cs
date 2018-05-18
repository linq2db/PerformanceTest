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
		public static void Run(string[] args)
		{
			var serverName = ".";

			DataConnection.AddConfiguration(
				"Test",
				$"Server={serverName};Database=PerformanceTest;Trusted_Connection=True",
				SqlServerTools.GetDataProvider(SqlServerVersion.v2012));

			DataConnection.DefaultConfiguration = "Test";

			CreateDatabase(serverName);
			RunTests();
		}

		static void RunTests()
		{
			var testProviders = new ITests[]
			{
				new AdoNetTests       (),
				new DapperTests       (),
				new LinqToDBQueryTests(),
				new LinqToDBLinqTests (),
				new LinqToDBCompTests (),
#if !NET452
				new EFQueryTests      (),
				new EFLinqTests       (),
				new EFCompTests       (),
#endif
			};


			RunTests("Simple tests", testProviders, new[]
			{
				CreateTest<ITests>(t => t.GetSingleColumnFast,  100000),
				CreateTest<ITests>(t => t.GetSingleColumnSlow,  100000),
				CreateTest<ITests>(t => t.GetSingleColumnParam, 100000),
				CreateTest<ITests>(t => t.GetNarrowList,        1000,   10000),
				CreateTest<ITests>(t => t.GetNarrowList,        100,   100000),
				CreateTest<ITests>(t => t.GetNarrowList,        10,   1000000),
			});
		}

		static void RunTests(string description, ITests[] testProviders, Test<ITests>[] testMethods)
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
				AdoNet_ID  = t.Stopwatch.SingleOrDefault(w => w?.p is AdoNetTests)       ?.time,
				Dapper     = t.Stopwatch.SingleOrDefault(w => w?.p is DapperTests)       ?.time,
				L2DB_Query = t.Stopwatch.SingleOrDefault(w => w?.p is LinqToDBQueryTests)?.time,
				L2DB_Linq  = t.Stopwatch.SingleOrDefault(w => w?.p is LinqToDBLinqTests) ?.time,
				L2DB_Comp  = t.Stopwatch.SingleOrDefault(w => w?.p is LinqToDBCompTests) ?.time,
#if !NET452
				EF_Query   = t.Stopwatch.SingleOrDefault(w => w?.p is EFQueryTests)      ?.time,
				EF_Linq    = t.Stopwatch.SingleOrDefault(w => w?.p is EFLinqTests)       ?.time,
				EF_Comp    = t.Stopwatch.SingleOrDefault(w => w?.p is EFCompTests)       ?.time,
#endif
			})
			.ToArray();

			var results = tests.ToDiagnosticString();

			Console.WriteLine();
			Console.WriteLine(description);
			Console.WriteLine(results);

			var filePath = Path.Combine(Path.GetDirectoryName(typeof(TestRunner).Assembly.Location), @"..\..\..\Results.txt");

			File.WriteAllText(filePath, results);

			Console.WriteLine(filePath);
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

			using (var db = new TestContext())
			{
				CreateTable(db, new[] { new Narrow { ID = 1, Field1 = 2 } });
				CreateTable(db, Enumerable.Range(1, 1000000).Select(i => new NarrowLong { ID = i, Field1 = -i }));
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
