using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Tests.EFCore
{
	using DataModel;
	using Tests;

	// Compiled queries are held in static instances, as an application keeps them: each is compiled once
	// per process, and its first translation happens during warmup rather than inside a timed run. Every
	// value a query uses is a parameter of the compiled delegate.
	class EFCoreCompTests : TestsBase, IWithChangeTracking,
		ISingleColumnTests, ISingleColumnAsyncTests,
		IGetListTests, IGetListAsyncTests,
		ILinqQueryTests
	{
		public override string Name         { get; set; } = "EF Core Compiled";
		public          bool   TrackChanges { get; set; }

		// EF Core fixes the tracking behaviour of a compiled query when it first runs it, so the
		// tracking and the no-tracking providers each get their own set.
		static readonly Queries _noTrackingQueries = new Queries();
		static readonly Queries _trackingQueries   = new Queries();

		Queries Q => TrackChanges ? _trackingQueries : _noTrackingQueries;

		sealed class Queries
		{
			public readonly Func<EFCoreContext,int> SingleColumnFast = EF.CompileQuery((EFCoreContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			public readonly Func<EFCoreContext,Task<int>> SingleColumnFastAsync = EF.CompileAsyncQuery((EFCoreContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			public readonly Func<EFCoreContext,int> SingleColumnSlow = EF.CompileQuery((EFCoreContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			public readonly Func<EFCoreContext,Task<int>> SingleColumnSlowAsync = EF.CompileAsyncQuery((EFCoreContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			public readonly Func<EFCoreContext,int,int,int> SingleColumnParam = EF.CompileQuery((EFCoreContext db, int id, int p) =>
				db.Narrow.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

			public readonly Func<EFCoreContext,int,int,Task<int>> SingleColumnParamAsync = EF.CompileAsyncQuery((EFCoreContext db, int id, int p) =>
				db.Narrow.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

			public readonly Func<EFCoreContext,int,IEnumerable<NarrowLong>> NarrowList = EF.CompileQuery((EFCoreContext db, int top) =>
				db.NarrowLong.Take(top));

			public readonly Func<EFCoreContext,int,IAsyncEnumerable<NarrowLong>> NarrowListAsync = EF.CompileAsyncQuery((EFCoreContext db, int top) =>
				db.NarrowLong.Take(top));

			public readonly Func<EFCoreContext,int,IEnumerable<WideLong>> WideList = EF.CompileQuery((EFCoreContext db, int top) =>
				db.WideLong.Take(top));

			public readonly Func<EFCoreContext,int,IAsyncEnumerable<WideLong>> WideListAsync = EF.CompileAsyncQuery((EFCoreContext db, int top) =>
				db.WideLong.Take(top));

			public readonly Func<EFCoreContext,IEnumerable<int>> SimpleLinqQuery = EF.CompileQuery((EFCoreContext db) =>
				(
					from n1 in db.Narrow
					where n1.ID < 100
					select n1.ID
				));

			public readonly Func<EFCoreContext,int,IEnumerable<int>> SimpleLinqQueryTop = EF.CompileQuery((EFCoreContext db, int top) =>
				(
					from n1 in db.Narrow
					where n1.ID < 100
					select n1.ID
				)
				.Take(top));

			// The result is an anonymous type; the field is typed through delegate covariance.
			public readonly Func<EFCoreContext,int,IEnumerable<object>> ComplicatedLinqFast = EF.CompileQuery((EFCoreContext db, int top) =>
				(
					from n1 in db.Narrow
					join n2 in db.Narrow on new { n1.ID, n1.Field1 } equals new { n2.ID, n2.Field1 }
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

			public readonly Func<EFCoreContext,int,int,IEnumerable<object>> ComplicatedLinqSlow = EF.CompileQuery((EFCoreContext db, int top, int nRows) =>
				(
					from n in db.NarrowLong
					join w in db.WideLong on n.Field1 equals w.Field1
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
					from n in db.NarrowLong
					join w in db.WideLong on n.Field1 equals w.Field1
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
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SingleColumnFast;

			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					query(db);

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SingleColumnFastAsync;

			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					await query(db);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SingleColumnSlow;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					query(db);

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SingleColumnSlowAsync;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					await query(db);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SingleColumnParam;

			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					query(db, 1, 2);

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SingleColumnParamAsync;

			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					await query(db, 1, 2);

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.NarrowList;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.NarrowListAsync;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using var db = new EFCoreContext(TrackChanges);
				await foreach (var item in query(db, takeCount)) { }
			}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.WideList;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.WideListAsync;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using var db = new EFCoreContext(TrackChanges);
				await foreach (var item in query(db, takeCount)) {}
			}

			watch.Stop();

			return true;
		}

		public bool SimpleLinqQuery(Stopwatch watch, int repeatCount)
		{
			var query = Q.SimpleLinqQuery;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					foreach (var item in query(db))
						break;

			watch.Stop();

			return true;
		}

		public bool SimpleLinqQueryTop(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SimpleLinqQueryTop;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public bool ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.ComplicatedLinqFast;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public bool ComplicatedLinqSlow(Stopwatch watch, int repeatCount, int takeCount, int nRows)
		{
			var query = Q.ComplicatedLinqSlow;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					foreach (var item in query(db, takeCount, nRows)) {}

			watch.Stop();

			return true;
		}
	}
}
