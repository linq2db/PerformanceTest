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
	}
}
