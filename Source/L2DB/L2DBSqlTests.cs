using System;
using System.Diagnostics;
using System.Threading.Tasks;

using LinqToDB.Data;

namespace Tests.L2DB
{
	using DataModel;
	using Tests;

	class L2DBSqlTests : TestsBase, IWithChangeTracking,
		ISingleColumnTests, ISingleColumnAsyncTests,
		IGetListTests, IGetListAsyncTests
	{
		public override string Name         { get; set; } = "L2DB Sql";
		public          bool   TrackChanges { get; set; }

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					db.Execute<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					await db.ExecuteAsync<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(TrackChanges))
					db.Execute<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(TrackChanges))
					await db.ExecuteAsync<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					db.Execute<int>(GetParamSql,
						new DataParameter("@id", 1),
						new DataParameter("@p",  2));

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					await db.ExecuteAsync<int>(GetParamSql,
						new DataParameter("@id", 1),
						new DataParameter("@p",  2));

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(TrackChanges))
					foreach (var item in db.Query<NarrowLong>(sql)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(TrackChanges))
					foreach (var item in await db.QueryToListAsync<NarrowLong>(sql)) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(TrackChanges))
					foreach (var item in db.Query<WideLong>(sql)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(TrackChanges))
					foreach (var item in await db.QueryToListAsync<WideLong>(sql)) {}

			watch.Stop();

			return true;
		}
	}
}
