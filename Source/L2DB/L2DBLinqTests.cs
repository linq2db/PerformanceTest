using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LinqToDB;
using LinqToDB.Async;

namespace Tests.L2DB
{
	using Tests;

	class L2DBLinqTests : TestsBase, IWithChangeTracking,
		ISingleColumnTests, ISingleColumnAsyncTests,
		IGetListTests, IGetListAsyncTests,
		ILinqQueryTests
	{
		public override string Name         { get; set; } = "L2DB Linq";
		public          bool   TrackChanges { get; set; }

		protected virtual L2DBContext CreateContext()
		{
			return new L2DBContext(TrackChanges);
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = CreateContext())
				for (var i = 0; i < repeatCount; i++)
					db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First();

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			await using (var db = CreateContext())
				for (var i = 0; i < repeatCount; i++)
					await db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = CreateContext())
					_ = db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First();

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				await using (var db = CreateContext())
					await db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = CreateContext())
				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					_ = db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First();
				}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			await using (var db = CreateContext())
				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					_ = await db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).FirstAsync();
				}

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = CreateContext())
					foreach (var _ in db.NarrowLongs.Take(takeCount)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			var token = default(CancellationToken);

			for (var i = 0; i < repeatCount; i++)
			{
				await using var db = CreateContext();

				var q = db.NarrowLongs.Take(takeCount).AsAsyncEnumerable();

				await foreach(var _ in q.WithCancellation(token))
				{
				}
			}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = CreateContext())
					foreach (var _ in db.WideLongs.Take(takeCount)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				await using (var db = CreateContext())
					await db.WideLongs.Take(takeCount).ForEachAsync(_ => {});

			watch.Stop();

			return true;
		}

		public bool SimpleLinqQuery(Stopwatch watch, int repeatCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using var db = CreateContext();

				var q =
					(
						from n1 in db.Narrows
						where n1.ID < 100
						select n1.ID
					);

				foreach (var _ in q)
					break;
			}

			watch.Stop();

			return true;
		}

		public bool SimpleLinqQueryTop(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using var db = CreateContext();

				var q =
					(
						from n1 in db.Narrows
						where n1.ID < 100
						select n1.ID
					)
					.Take(takeCount);

				foreach (var _ in q) {}
			}

			watch.Stop();

			return true;
		}

		public bool ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using var db = CreateContext();

				var q =
					(
						from n1 in db.Narrows
						join n2 in db.Narrows on new { n1.ID, n1.Field1 } equals new { n2.ID, n2.Field1 }
						where n1.ID < 100 && n2.Field1 <= 50
						group n1 by n1.ID into gr
						select new
						{
							gr.Key,
							Count = gr.Count()
						}
					)
					.OrderBy(n1 => n1.Key)
					.Skip(1)
					.Take(takeCount);

				foreach (var _ in q) {}
			}

			watch.Stop();

			return true;
		}

		public bool ComplicatedLinqSlow(Stopwatch watch, int repeatCount, int takeCount, int nRows)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using var db = CreateContext();

				var q =
					(
						from n in db.NarrowLongs
						join w in db.WideLongs on n.Field1 equals w.Field1
						where
							n.ID >= 0 && n.ID <= nRows &&
							!new[] { 0, 20, 50, 187635 }.Contains(w.Field1)
						select new
						{
							n.ID,
							w.Field1
						}
					)
					//.Concat
					.Union
					(
						from n in db.NarrowLongs
						join w in db.WideLongs on n.Field1 equals w.Field1
						where
							n.ID >= 0 && n.ID <= nRows &&
							!new[] { 0, 240, 500, 18635 }.Contains(w.Field1)
						select new
						{
							n.ID,
							w.Field1
						}
					)
					//.Distinct()
					//.OrderBy(n1 => Sql.Ext.RowNumber().Over().OrderByDesc(n1.Field1))
					.OrderByDescending(n1 => n1.Field1)
					.Skip(1000)
					.Take(takeCount);

				foreach (var _ in q) {}
			}

			watch.Stop();

			return true;
		}
	}

	/// <summary>
	/// The LINQ tests with InlineParameters = true and ParameterizeTakeSkip = false: values and
	/// Take/Skip are put into the SQL text instead of parameters.
	/// </summary>
	class L2DBLinqInlineTests : L2DBLinqTests
	{
		public override string Name { get; set; } = "L2DB Linq Inline";

		protected override L2DBContext CreateContext()
		{
			return new L2DBContext(TrackChanges, inlineParameters: true);
		}
	}
}
