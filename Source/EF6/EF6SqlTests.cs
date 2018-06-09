using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.EF6
{
	using Tests;

	class EF6SqlTests : TestsBase, IWithChangeTracking,
		ISingleColumnTests, ISingleColumnAsyncTests,
		IGetListTests, IGetListAsyncTests
	{
		public override string Name => "EF6 Sql";
		public          bool   TrackChanges { get; set; }

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
//			if (!TrackChanges)
//				return false;

			watch.Start();

			using (var db = new EF6Context(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					db.Database.SqlQuery<int>(GetSingleColumnSql).First();

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
//			if (NoTracking)
//				return false;

			watch.Start();

			using (var db = new EF6Context(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					await db.Database.SqlQuery<int>(GetSingleColumnSql).FirstAsync();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
//			if (NoTracking)
//				return false;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(TrackChanges))
					db.Database.SqlQuery<int>(GetSingleColumnSql).First();

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
//			if (NoTracking)
//				return false;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(TrackChanges))
					await db.Database.SqlQuery<int>(GetSingleColumnSql).FirstAsync();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
//			if (NoTracking)
//				return false;

			watch.Start();

			using (var db = new EF6Context(TrackChanges))
			{
				for (var i = 0; i < repeatCount; i++)
					db.Database
						.SqlQuery<int>(GetParamSql,
							new SqlParameter("@id", 1),
							new SqlParameter("@p",  2))
						.First();
			}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
//			if (NoTracking)
//				return false;

			watch.Start();

			using (var db = new EF6Context(TrackChanges))
			{
				for (var i = 0; i < repeatCount; i++)
					await db.Database
						.SqlQuery<int>(GetParamSql,
							new SqlParameter("@id", 1),
							new SqlParameter("@p",  2))
						.FirstAsync();
			}

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (!TrackChanges && takeCount >= 1000)
				return false;

			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(TrackChanges))
					foreach (var item in db.NarrowLong.SqlQuery(sql)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (!TrackChanges && takeCount >= 1000)
				return false;

			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(TrackChanges))
					await db.NarrowLong.SqlQuery(sql).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (!TrackChanges && takeCount >= 1000)
				return false;

			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(TrackChanges))
					foreach (var item in db.WideLong.SqlQuery(sql)) {}

			watch.Stop();

			return true;
		}

		public async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (!TrackChanges && takeCount >= 1000)
				return false;

			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(TrackChanges))
					await db.WideLong.SqlQuery(sql).ForEachAsync(item => { });

			watch.Stop();

			return true;
		}
	}
}
