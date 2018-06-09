using System;
using System.Diagnostics;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Tests.EFCore
{
	using Tests;

	class EFCoreSqlTests : TestsBase, IWithChangeTracking,
		ISingleColumnTests, ISingleColumnAsyncTests,
		IGetListTests, IGetListAsyncTests
	{
		public override string Name => "EF Core Sql";
		public          bool   TrackChanges { get; set; }

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					await db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					await db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
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

		public async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
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

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					foreach (var item in db.NarrowLong.FromSql(sql)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					await db.NarrowLong.FromSql(sql).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					foreach (var item in db.WideLong.FromSql(sql)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					await db.WideLong.FromSql(sql).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}
	}
}
