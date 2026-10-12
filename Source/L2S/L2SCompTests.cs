using System;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;

namespace Tests.L2S
{
	using Tests;

	// Compiled queries are held in static instances, as an application keeps them: each is compiled once
	// per process, and its first translation happens during warmup rather than inside a timed run. Every
	// value a query uses is a parameter of the compiled delegate.
	class L2SCompTests : TestsBase, IWithChangeTracking, ISingleColumnTests, IGetListTests, ILinqQueryTests
	{
		public override string Name         { get; set; } = "L2S Compiled";
		public          bool   TrackChanges { get; set; }

		// A compiled query is translated on its first run against the context it is given, so the
		// tracking and the no-tracking providers each get their own set.
		static readonly Queries _noTrackingQueries = new Queries();
		static readonly Queries _trackingQueries   = new Queries();

		Queries Q => TrackChanges ? _trackingQueries : _noTrackingQueries;

		sealed class Queries
		{
			public readonly Func<L2SContext,int> SingleColumnFast = CompiledQuery.Compile((L2SContext db) =>
				db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

			public readonly Func<L2SContext,int> SingleColumnSlow = CompiledQuery.Compile((L2SContext db) =>
				db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

			public readonly Func<L2SContext,int,int,int> SingleColumnParam = CompiledQuery.Compile((L2SContext db, int id, int p) =>
				db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

			public readonly Func<L2SContext,int,IQueryable<NarrowLong>> NarrowList = CompiledQuery.Compile((L2SContext db, int top) =>
				db.NarrowLongs.Take(top));

			public readonly Func<L2SContext,int,IQueryable<WideLong>> WideList = CompiledQuery.Compile((L2SContext db, int top) =>
				db.WideLongs.Take(top));

			public readonly Func<L2SContext,IQueryable<int>> SimpleLinqQuery = CompiledQuery.Compile((L2SContext db) =>
				(
					from n1 in db.Narrows
					where n1.ID < 100
					select n1.ID
				));

			public readonly Func<L2SContext,int,IQueryable<int>> SimpleLinqQueryTop = CompiledQuery.Compile((L2SContext db, int top) =>
				(
					from n1 in db.Narrows
					where n1.ID < 100
					select n1.ID
				)
				.Take(top));

			// The result is an anonymous type; the field is typed through delegate covariance.
			public readonly Func<L2SContext,int,IQueryable<object>> ComplicatedLinqFast = CompiledQuery.Compile((L2SContext db, int top) =>
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

			public readonly Func<L2SContext,int,int,IQueryable<object>> ComplicatedLinqSlow = CompiledQuery.Compile((L2SContext db, int top, int nRows) =>
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
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SingleColumnFast;

			watch.Start();

			using (var db = new L2SContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					query(db);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SingleColumnSlow;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(TrackChanges))
					query(db);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SingleColumnParam;

			watch.Start();

			using (var db = new L2SContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					query(db, 1, 2);

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.NarrowList;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(TrackChanges))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.WideList;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(TrackChanges))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public bool SimpleLinqQuery(Stopwatch watch, int repeatCount)
		{
			var query = Q.SimpleLinqQuery;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(TrackChanges))
					foreach (var item in query(db)) {}

			watch.Stop();

			return true;
		}

		public bool SimpleLinqQueryTop(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.SimpleLinqQueryTop;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(TrackChanges))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public bool ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = Q.ComplicatedLinqFast;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(TrackChanges))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public bool ComplicatedLinqSlow(Stopwatch watch, int repeatCount, int takeCount, int nRows)
		{
			var query = Q.ComplicatedLinqSlow;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(TrackChanges))
					foreach (var item in query(db, takeCount, nRows)) {}

			watch.Stop();

			return true;
		}
	}
}
