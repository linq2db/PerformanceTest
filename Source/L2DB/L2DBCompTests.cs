using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LinqToDB;
using LinqToDB.Async;

namespace Tests.L2DB
{
	using DataModel;
	using Tests;

	// Compiled queries live in static fields, as an application keeps them: each is compiled once per
	// process, and its first translation happens during warmup rather than inside a timed run. Every
	// value a query uses is a parameter of the compiled delegate.
	class L2DBCompTests : TestsBase, IWithChangeTracking,
		ISingleColumnTests, ISingleColumnAsyncTests,
		IGetListTests, IGetListAsyncTests,
		ILinqQueryTests

	{
		public override string Name         { get; set; } = "L2DB Compiled";
		public          bool   TrackChanges { get; set; }

		protected virtual L2DBContext CreateContext()
		{
			return new L2DBContext(TrackChanges);
		}

		static readonly Func<L2DBContext,int> _singleColumnFast = CompiledQuery.Compile((L2DBContext db) =>
			db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = CreateContext())
				for (var i = 0; i < repeatCount; i++)
					_singleColumnFast(db);

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,Task<int>> _singleColumnFastAsync = CompiledQuery.Compile((L2DBContext db) =>
			db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).FirstAsync(default(CancellationToken)));

		public async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			await using (var db = CreateContext())
				for (var i = 0; i < repeatCount; i++)
					await _singleColumnFastAsync(db);

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,int> _singleColumnSlow = CompiledQuery.Compile((L2DBContext db) =>
			db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = CreateContext())
					_singleColumnSlow(db);

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,Task<int>> _singleColumnSlowAsync = CompiledQuery.Compile((L2DBContext db) =>
			db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).FirstAsync(default(CancellationToken)));

		public async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				await using (var db = CreateContext())
					await _singleColumnSlowAsync(db);

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,int,int,int> _singleColumnParam = CompiledQuery.Compile((L2DBContext db, int id, int p) =>
			db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = CreateContext())
				for (var i = 0; i < repeatCount; i++)
					_singleColumnParam(db, 1, 2);

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,int,int,Task<int>> _singleColumnParamAsync = CompiledQuery.Compile((L2DBContext db, int id, int p) =>
			db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).FirstAsync(default(CancellationToken)));

		public async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			await using (var db = CreateContext())
				for (var i = 0; i < repeatCount; i++)
					await _singleColumnParamAsync(db, 1, 2);

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,int,IQueryable<NarrowLong>> _narrowList = CompiledQuery.Compile((L2DBContext db, int top) =>
			db.NarrowLongs.Take(top));

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = CreateContext())
					foreach (var _ in _narrowList(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,int,IAsyncEnumerable<NarrowLong>> _narrowListAsync = CompiledQuery.Compile((L2DBContext db, int top) =>
			db.NarrowLongs.Take(top).AsAsyncEnumerable());

		public async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				await using (var db = CreateContext())
					await foreach (var item in _narrowListAsync(db, takeCount))
					{}

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,int,IQueryable<WideLong>> _wideList = CompiledQuery.Compile((L2DBContext db, int top) =>
			db.WideLongs.Take(top));

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = CreateContext())
					foreach (var item in _wideList(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,int,IAsyncEnumerable<WideLong>> _wideListAsync = CompiledQuery.Compile((L2DBContext db, int top) =>
			db.WideLongs.Take(top).AsAsyncEnumerable());

		public async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				await using (var db = CreateContext())
					await foreach (var item in _wideListAsync(db, takeCount))
					{}

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,IQueryable<int>> _simpleLinqQuery = CompiledQuery.Compile((L2DBContext db) =>
			(
				from n1 in db.Narrows
				where n1.ID < 100
				select n1.ID
			));

		public bool SimpleLinqQuery(Stopwatch watch, int repeatCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = CreateContext())
					foreach (var _ in _simpleLinqQuery(db))
						break;

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,int,IQueryable<int>> _simpleLinqQueryTop = CompiledQuery.Compile((L2DBContext db, int top) =>
			(
				from n1 in db.Narrows
				where n1.ID < 100
				select n1.ID
			)
			.Take(top));

		public bool SimpleLinqQueryTop(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = CreateContext())
					foreach (var _ in _simpleLinqQueryTop(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		// The result is an anonymous type; the field is typed through delegate covariance.
		static readonly Func<L2DBContext,int,IQueryable<object>> _complicatedLinqFast = CompiledQuery.Compile((L2DBContext db, int top) =>
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
			.Take(top));

		public bool ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = CreateContext())
					foreach (var _ in _complicatedLinqFast(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		static readonly Func<L2DBContext,int,int,IQueryable<object>> _complicatedLinqSlow = CompiledQuery.Compile((L2DBContext db, int top, int nRows) =>
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
			.OrderByDescending(n1 => n1.Field1)
			.Skip(1000)
			.Take(top));

		public bool ComplicatedLinqSlow(Stopwatch watch, int repeatCount, int takeCount, int nRows)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = CreateContext())
					foreach (var item in _complicatedLinqSlow(db, takeCount, nRows)) {}

			watch.Stop();

			return true;
		}
	}

	/// <summary>
	/// The compiled tests with linq2db's default settings: parameterized SQL, including Take/Skip.
	/// A compiled query keeps its translations per configuration, so it shares the compiled queries
	/// with "L2DB Compiled".
	/// </summary>
	class L2DBCompParamTests : L2DBCompTests
	{
		public override string Name { get; set; } = "L2DB Compiled Param";

		protected override L2DBContext CreateContext()
		{
			return new L2DBContext(TrackChanges, inlineParameters: false);
		}
	}
}
