using System;
using System.Diagnostics;
using System.Linq;

using BLToolkit.Data.Linq;

namespace Tests.BLToolkit
{
	using DataModel;
	using Tests;

	// Compiled queries live in static fields, as an application keeps them: each is compiled once per
	// process, and its first translation happens during warmup rather than inside a timed run. Every
	// value a query uses is a parameter of the compiled delegate.
	class BLToolkitCompTests : TestsBase, ISingleColumnTests, IGetListTests, ILinqQueryTests
	{
		public override string Name { get; set; } = "BLToolkit Compiled";

		static readonly Func<BLTContext,int> _singleColumnFast = CompiledQuery.Compile((BLTContext db) =>
			db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = _singleColumnFast;

			watch.Start();

			using (var db = new BLTContext())
				for (var i = 0; i < repeatCount; i++)
					query(db);

			watch.Stop();

			return true;
		}

		static readonly Func<BLTContext,int> _singleColumnSlow = CompiledQuery.Compile((BLTContext db) =>
			db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = _singleColumnSlow;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					query(db);

			watch.Stop();

			return true;
		}

		static readonly Func<BLTContext,int,int,int> _singleColumnParam = CompiledQuery.Compile((BLTContext db, int id, int p) =>
			db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = _singleColumnParam;

			watch.Start();

			using (var db = new BLTContext())
				for (var i = 0; i < repeatCount; i++)
					query(db, 1, 2);

			watch.Stop();

			return true;
		}

		static readonly Func<BLTContext,int,IQueryable<NarrowLong>> _narrowList = CompiledQuery.Compile((BLTContext db, int top) =>
			db.NarrowLongs.Take(top));

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = _narrowList;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		static readonly Func<BLTContext,int,IQueryable<WideLong>> _wideList = CompiledQuery.Compile((BLTContext db, int top) =>
			db.WideLongs.Take(top));

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = _wideList;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		static readonly Func<BLTContext,IQueryable<int>> _simpleLinqQuery = CompiledQuery.Compile((BLTContext db) =>
			(
				from n1 in db.Narrows
				where n1.ID < 100
				select n1.ID
			));

		public bool SimpleLinqQuery(Stopwatch watch, int repeatCount)
		{
			var query = _simpleLinqQuery;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					foreach (var item in query(db)) {}

			watch.Stop();

			return true;
		}

		static readonly Func<BLTContext,int,IQueryable<int>> _simpleLinqQueryTop = CompiledQuery.Compile((BLTContext db, int top) =>
			(
				from n1 in db.Narrows
				where n1.ID < 100
				select n1.ID
			)
			.Take(top));

		public bool SimpleLinqQueryTop(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = _simpleLinqQueryTop;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		// The result is an anonymous type; the field is typed through delegate covariance.
		static readonly Func<BLTContext,int,IQueryable<object>> _complicatedLinqFast = CompiledQuery.Compile((BLTContext db, int top) =>
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
			var query = _complicatedLinqFast;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		static readonly Func<BLTContext,int,int,IQueryable<object>> _complicatedLinqSlow = CompiledQuery.Compile((BLTContext db, int top, int nRows) =>
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
			var query = _complicatedLinqSlow;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					foreach (var item in query(db, takeCount, nRows)) {}

			watch.Stop();

			return true;
		}
	}
}
