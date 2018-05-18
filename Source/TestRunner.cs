using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Expressions;

using TTest = System.Linq.Expressions.Expression<System.Func<Tests.ITests,System.Func<System.Diagnostics.Stopwatch,int,bool>>>;

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

			//CreateDatabase(serverName);
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

			var testMethods = new[]
			{
				new { Func = (TTest)(t => t.GetSingleColumnFast), Repeat =  1000 },
//				new { Func = (TTest)(t => t.GetSingleColumnFast), Repeat = 10000 },
				new { Func = (TTest)(t => t.GetSingleColumnSlow), Repeat =  1000 },
//				new { Func = (TTest)(t => t.GetSingleColumnSlow), Repeat = 10000 },
			};

			var tests = testMethods.Select(m =>
			{
				var func  = m.Func.Compile();
				var watch = testProviders.Select(p =>
				{
					// Warmup
					if (func(p)(new Stopwatch(), 1) == false)
						return null;

					// Test
					var stopwatch = new Stopwatch();
					func(p)(stopwatch, m.Repeat);
					var time = new TimeSpan(stopwatch.ElapsedTicks);

					return new { time, stopwatch, p, m.Repeat };
				}).ToArray();

				return new
				{
					Test = ((MethodInfo)((ConstantExpression)m.Func.Body
						.Find(e => e is ConstantExpression c && c.Value is MethodInfo)).Value).Name,
					m.Repeat,
					Stopwatch = watch,
				};
			})
			.ToArray()
			.Select(t => new
			{
				t.Test,
				t.Repeat,
				AdoNet     = t.Stopwatch.Single(w => w.p is AdoNetTests).       time,
				Dapper     = t.Stopwatch.Single(w => w.p is DapperTests).       time,
				L2DB_Query = t.Stopwatch.Single(w => w.p is LinqToDBQueryTests).time,
				L2DB_Linq  = t.Stopwatch.Single(w => w.p is LinqToDBLinqTests). time,
				L2DB_Comp  = t.Stopwatch.Single(w => w.p is LinqToDBCompTests). time,
#if !NET452
				EF_Query   = t.Stopwatch.Single(w => w.p is EFQueryTests).      time,
				EF_Linq    = t.Stopwatch.Single(w => w.p is EFLinqTests).       time,
				EF_Comp    = t.Stopwatch.Single(w => w.p is EFCompTests).       time,
#endif
			})
			.ToArray();

			Console.WriteLine(tests.ToDiagnosticString());
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
