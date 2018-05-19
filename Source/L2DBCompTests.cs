using System;
using System.Diagnostics;
using System.Linq;

using LinqToDB;

namespace Tests
{
	using DataModel;

	class L2DBCompTests : ITests
	{
		public readonly bool NoTracking;

		public L2DBCompTests(bool noTracking)
		{
			NoTracking = noTracking;
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2DBContext db) =>
				db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					query(db);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2DBContext db) =>
				db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					query(db);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2DBContext db, int id, int p) =>
				db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					query(db, 1, 2);

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2DBContext db, int top) =>
				db.NarrowLongs.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = CompiledQuery.Compile((L2DBContext db, int top) =>
				db.WideLongs.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}
	}
}
