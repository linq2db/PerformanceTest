using System;
using System.Diagnostics;
using System.Threading.Tasks;

using LinqToDB.Data;

namespace Tests.L2DB
{
	using DataModel;

	class L2DBSqlTests : TestsWithChangeTrackingBase
	{
		public override string Name => "L2DB Sql";

		public L2DBSqlTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.Execute<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					await db.ExecuteAsync<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					db.Execute<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					await db.ExecuteAsync<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.Execute<int>(GetParamSql,
						new DataParameter("@id", 1),
						new DataParameter("@p",  2));

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					await db.ExecuteAsync<int>(GetParamSql,
						new DataParameter("@id", 1),
						new DataParameter("@p",  2));

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					foreach (var item in db.Query<NarrowLong>(sql)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					foreach (var item in await db.QueryToListAsync<NarrowLong>(sql)) {}

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					foreach (var item in db.Query<WideLong>(sql)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					foreach (var item in await db.QueryToListAsync<WideLong>(sql)) {}

			watch.Stop();

			return true;
		}
	}
}
