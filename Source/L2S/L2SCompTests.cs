using System;
using System.Data.Linq;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.L2S
{
	class L2SCompTests : TestsWithChangeTrackingBase
	{
		public override string Name => "L2S Compiled";

		public L2SCompTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2SContext db) =>
				db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			using (var db = new L2SContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					query(db);

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2SContext db) =>
				db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					query(db);

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2SContext db, int id, int p) =>
				db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

			watch.Start();

			using (var db = new L2SContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					query(db, 1, 2);

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2SContext db, int top) =>
				db.NarrowLongs.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2SContext db, int top) =>
				db.WideLongs.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}


		public override bool SimpleLinqQuery(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2SContext db, int top) =>
				(
					from n1 in db.Narrows
					where n1.ID < 100
					select n1.ID
				)
				.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public override bool ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2SContext db, int top) =>
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

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public override bool ComplicatedLinqSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2SContext db, int top) =>
				(
					from n in db.NarrowLongs
					join w in db.WideLongs on n.Field1 equals w.Field1
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
					from n in db.NarrowLongs
					join w in db.WideLongs on n.Field1 equals w.Field1
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
				using (var db = new L2SContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}
	}
}
