using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using LinqToDB;

namespace Tests.L2DB
{
	class L2DBLinqTests : TestsWithChangeTrackingBase
	{
		public override string Name => "L2DB Linq";

		public L2DBLinqTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					await db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					await db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).AsEnumerable().First();
				}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					await db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).FirstAsync();
				}

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					foreach (var item in db.NarrowLongs.Take(takeCount)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					await db.NarrowLongs.Take(takeCount).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					foreach (var item in db.WideLongs.Take(takeCount)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					await db.WideLongs.Take(takeCount).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}
	}
}
