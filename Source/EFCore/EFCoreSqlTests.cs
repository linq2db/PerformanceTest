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
		public override string Name         { get; set; } = "EF Core Sql";
		public          bool   TrackChanges { get; set; }

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
#if NET48
					db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).AsEnumerable().First();
#else
					db.Narrow.FromSqlRaw(GetSingleColumnSql).Select(t => t.ID).AsEnumerable().First();
#endif

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
#if NET48
					await db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).FirstAsync();
#else
					await db.Narrow.FromSqlRaw(GetSingleColumnSql).Select(t => t.ID).FirstAsync();
#endif

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
#if NET48
					db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).AsEnumerable().First();
#else
					db.Narrow.FromSqlRaw(GetSingleColumnSql).Select(t => t.ID).AsEnumerable().First();
#endif

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
#if NET48
					await db.Narrow.FromSql(GetSingleColumnSql).Select(t => t.ID).FirstAsync();
#else
					await db.Narrow.FromSqlRaw(GetSingleColumnSql).Select(t => t.ID).FirstAsync();
#endif

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
#if NET48
						.FromSql(GetParamSql,
							new SqlParameter("@id", 1),
							new SqlParameter("@p",  2))
#else
						.FromSqlInterpolated($"SELECT ID FROM Narrow WHERE ID = {1} AND Field1 = {2}")
#endif
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
#if NET48
						.FromSql(GetParamSql,
							new SqlParameter("@id", 1),
							new SqlParameter("@p",  2))
#else
						.FromSqlInterpolated($"SELECT ID FROM Narrow WHERE ID = {1} AND Field1 = {2}")
#endif
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
#if NET48
					foreach (var item in db.NarrowLong.FromSql(sql)) {}
#else
					foreach (var item in db.NarrowLong.FromSqlRaw(sql)) {}
#endif

			watch.Stop();

			return true;
		}

		public async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
#if NET48
					await db.NarrowLong.FromSql(sql).ForEachAsync(item => {});
#else
					await db.NarrowLong.FromSqlRaw(sql).ForEachAsync(item => {});
#endif

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
#if NET48
					foreach (var item in db.WideLong.FromSql(sql)) {}
#else
					foreach (var item in db.WideLong.FromSqlRaw(sql)) {}
#endif

			watch.Stop();

			return true;
		}

		public async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
#if NET48
					await db.WideLong.FromSql(sql).ForEachAsync(item => {});
#else
					await db.WideLong.FromSqlRaw(sql).ForEachAsync(item => {});
#endif

			watch.Stop();

			return true;
		}
	}
}
