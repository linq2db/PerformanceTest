using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Tests.EFCore
{
	class EFCoreCompTests : TestsWithChangeTrackingBase
	{
		public override string Name => "EF Core Compiled";

		public EFCoreCompTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					query(db);

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileAsyncQuery((EFCoreContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					await query(db);

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					query(db);

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileAsyncQuery((EFCoreContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					await query(db);

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db, int id, int p) =>
				db.Narrow.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					query(db, 1, 2);

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileAsyncQuery((EFCoreContext db, int id, int p) =>
				db.Narrow.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					await query(db, 1, 2);

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db, int top) =>
				db.NarrowLong.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileAsyncQuery((EFCoreContext db, int top) =>
				db.NarrowLong.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					await query(db, takeCount).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db, int top) =>
				db.WideLong.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileAsyncQuery((EFCoreContext db, int top) =>
				db.WideLong.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					await query(db, takeCount).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}

		public override bool SimpleLinqQuery(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db, int top) =>
				(
					from n1 in db.Narrow
					where n1.ID < 100
					select n1.ID
				)
				.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public override bool ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db, int top) =>
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

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public override bool ComplicatedLinqSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db, int top) =>
				(
					from n in db.NarrowLong
					join w in db.WideLong on n.Field1 equals w.Field1
					where
						n.ID >= 0 && n.ID <= 1000000 &&
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
						n.ID >= 0 && n.ID <= 1000000 &&
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

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}
	}
}
