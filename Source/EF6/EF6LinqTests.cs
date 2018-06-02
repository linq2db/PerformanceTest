using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.EF6
{
	class EF6LinqTests : TestsWithChangeTrackingBase
	{
		public override string Name => "EF6 Linq";

		public EF6LinqTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EF6Context(NoTracking))
			{
				var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

				for (var i = 0; i < repeatCount; i++)
					q.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();
			}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EF6Context(NoTracking))
			{
				var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

				for (var i = 0; i < repeatCount; i++)
					await q.Where(t => t.ID == 1).Select(t => t.ID).FirstAsync();
			}

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

					db.Narrow.AsNoTracking().Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();
				}
			}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

					await db.Narrow.AsNoTracking().Where(t => t.ID == 1).Select(t => t.ID).FirstAsync();
				}
			}

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EF6Context(NoTracking))
			{
				var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					q.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).AsEnumerable().First();
				}
			}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EF6Context(NoTracking))
			{
				var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					await q.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).FirstAsync();
				}
			}

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.NarrowLong.AsNoTracking() : db.NarrowLong;
					foreach (var item in q.Take(takeCount)) {}
				}
			}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.NarrowLong.AsNoTracking() : db.NarrowLong;
					await q.Take(takeCount).ForEachAsync(item => {});
				}
			}

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.WideLong.AsNoTracking() : db.WideLong;
					foreach (var item in db.WideLong.Take(takeCount)) {}
				}
			}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.WideLong.AsNoTracking() : db.WideLong;
					await db.WideLong.Take(takeCount).ForEachAsync(item => {});
				}
			}

			watch.Stop();

			return true;
		}
	}
}
