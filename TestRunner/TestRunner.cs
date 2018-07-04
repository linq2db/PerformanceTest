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
using LinqToDB.DataProvider.SQLite;
using LinqToDB.Expressions;

namespace TestRunner
{
	using DataModel;
	using Tools;

	public class TestRunner
	{
		const string DatabaseVersion = "1";

		static readonly Random _random = new Random();

		public static void RunTests<T>(string platform, string testName, IEnumerable<T> testProviders, Test<T>[] testMethods)
			where T : class, ITests
		{
			RunTests(platform, testName, testProviders, p => {}, testMethods);
		}

		public static void RunTests<T>(string platform, string testName, IEnumerable<T> providers, Action<T> action, Test<T>[] testMethods)
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

			using (var db = new DataConnection("Result"))
			{
				var id = db.GetTable<TestRun>()
						.Value(t => t.Platform,  platform)
						.Value(t => t.Name,      testName)
						.Value(t => t.CreatedOn, () => Sql.CurrentTimestamp)
					.InsertWithIdentity();

				foreach (var test in tests)
				{
					var mid = db.GetTable<TestMethod>()
							.Value(t => t.TestRunID, id)
							.Value(t => t.Name,      test.Test)
							.Value(t => t.Repeat,    test.Repeat)
							.Value(t => t.Take,      test.Take)
							.Value(t => t.CreatedOn, () => Sql.CurrentTimestamp)
						.InsertWithIdentity();

					foreach (var watch in test.Stopwatch.Where(w => w != null))
					{
						db.GetTable<TestStopwatch>()
								.Value(t => t.TestMethodID, mid)
								.Value(t => t.Time,         watch.time)
								.Value(t => t.Ticks,        watch.stopwatch.ElapsedTicks)
								.Value(t => t.Provider,     watch.p.Name)
								.Value(t => t.CreatedOn,    () => Sql.CurrentTimestamp)
							.Insert();
					}
				}

				/*
				var list =
				(
					from r in db.GetTable<TestRun>()
					join m in db.GetTable<TestMethod>()    on r.ID equals m.TestRunID
					join s in db.GetTable<TestStopwatch>() on m.ID equals s.TestMethodID
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
						Time      = new TimeSpan(ticks),
						CreatedOn = DateTime.Now
					};
				})
				.ToList();

				db.GetTable<TestResult>().Truncate();
				db.GetTable<TestResult>().BulkCopy(list);
				*/
			}

			var res = tests.Select(t =>
			{
				var dic = new Dictionary<string,object>
				{
					["Test"]   = t.Test,
					["Repeat"] = t.Repeat,
					["Take"]   = t.Take,
				};

				foreach (var w in
					from w in t.Stopwatch
					where w != null
					orderby w.time.Ticks
					select w)
				{
					dic.Add(w.p.Name, w.time);
				}

				return dic;
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

		public static Test<T> CreateTest<T>(Expression<Func<T,Func<Stopwatch,int,int,bool>>> func, int repeat, int take = -1)
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

		public static Test<T> CreateTest<T>(Expression<Func<T,Func<Stopwatch,int,int,int,bool>>> func, int repeat, int take, int parm)
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

		public static Test<T> CreateTest<T>(Expression<Func<T,Func<Stopwatch,int,int,Task<bool>>>> func, int repeat, int take = -1)
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

		public static void CreateResultDatabase(bool enforceCreate, string resultFolder)
		{
			Console.WriteLine("Creating database...");

			var dbPath = Path.Combine(resultFolder, "Result");

			DataConnection.AddConfiguration("Result", $"Data Source={dbPath}.sqlite", SQLiteTools.GetDataProvider());

			using (var db = new DataConnection("Result"))
			{
				if (!enforceCreate)
				{
					try
					{
						if (db.GetTable<Setting>().Any(s => s.Name == "DB Version" && s.Value == DatabaseVersion))
							return;
					}
					catch
					{
					}
				}

				db.Close();

				File.Delete($"{dbPath}.sqlite");

				CreateTable(db, new[] { new Setting { Name = "DB Version", Value =  DatabaseVersion } });
				db.CreateTable<TestRun>();
				db.CreateTable<TestMethod>();
				db.CreateTable<TestStopwatch>();
				//db.CreateTable<TestResult>();
			}

			Console.WriteLine("Database created.");
		}

		public static void CreateTable<T>(DataConnection db, IEnumerable<T> data)
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
