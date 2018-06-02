using System;
using System.Diagnostics;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Tests.EFCore
{
	class EFCoreSqlTests : TestsWithChangeTrackingBase
	{
		public override string Name => "EF Core Sql";

		public EFCoreSqlTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					await db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					await db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
			{
				for (var i = 0; i < repeatCount; i++)
					db.Narrow
						.FromSql(GetParamSql,
							new SqlParameter("@id", 1),
							new SqlParameter("@p",  2))
						.Select(t => t.ID).AsEnumerable().First();
			}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
			{
				for (var i = 0; i < repeatCount; i++)
					await db.Narrow
						.FromSql(GetParamSql,
							new SqlParameter("@id", 1),
							new SqlParameter("@p",  2))
						.Select(t => t.ID).FirstAsync();
			}

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in db.NarrowLong.FromSql(sql)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					await db.NarrowLong.FromSql(sql).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in db.WideLong.FromSql(sql)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					await db.WideLong.FromSql(sql).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}
	}
}
