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
					_ = db.Narrow.FromSqlRaw(GetSingleColumnSql).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					_ = await db.Narrow.FromSqlRaw(GetSingleColumnSql).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					_ = db.Narrow.FromSqlRaw(GetSingleColumnSql).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					_ = await db.Narrow.FromSqlRaw(GetSingleColumnSql).Select(t => t.ID).FirstAsync();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(TrackChanges))
			{
				for (var i = 0; i < repeatCount; i++)
					_ = db.Narrow
						.FromSqlInterpolated($"SELECT ID FROM Narrow WHERE ID = {1} AND Field1 = {2}")
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
					_ = await db.Narrow
						.FromSqlInterpolated($"SELECT ID FROM Narrow WHERE ID = {1} AND Field1 = {2}")
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
					foreach (var _ in db.NarrowLong.FromSqlRaw(sql)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					await db.NarrowLong.FromSqlRaw(sql).ForEachAsync(_ => {});

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					foreach (var _ in db.WideLong.FromSqlRaw(sql)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(TrackChanges))
					await db.WideLong.FromSqlRaw(sql).ForEachAsync(_ => {});

			watch.Stop();

			return true;
		}
	}
}
