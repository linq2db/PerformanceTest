using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Tests.EFCore
{
	class EFCoreLinqTests : TestsWithChangeTrackingBase
	{
		public override string Name => "EF Core Linq";

		public EFCoreLinqTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					await db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					await db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					db.Narrow.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).AsEnumerable().First();
				}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					await db.Narrow.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).FirstAsync();
				}

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in db.NarrowLong.Take(takeCount)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					await db.NarrowLong.Take(takeCount).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in db.WideLong.Take(takeCount)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					await db.WideLong.Take(takeCount).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}
	}
}
