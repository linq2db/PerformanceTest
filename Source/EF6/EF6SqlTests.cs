using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.EF6
{
	class EF6SqlTests : TestsWithChangeTrackingBase
	{
		public override string Name => "EF6 Sql";

		public EF6SqlTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (NoTracking)
				return false;

			watch.Start();

			using (var db = new EF6Context(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.Database.SqlQuery<int>(GetSingleColumnSql).First();

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (NoTracking)
				return false;

			watch.Start();

			using (var db = new EF6Context(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					await db.Database.SqlQuery<int>(GetSingleColumnSql).FirstAsync();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (NoTracking)
				return false;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					db.Database.SqlQuery<int>(GetSingleColumnSql).First();

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (NoTracking)
				return false;

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					await db.Database.SqlQuery<int>(GetSingleColumnSql).FirstAsync();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (NoTracking)
				return false;

			watch.Start();

			using (var db = new EF6Context(NoTracking))
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

		public override async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (NoTracking)
				return false;

			watch.Start();

			using (var db = new EF6Context(NoTracking))
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

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (NoTracking)
				return false;

			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					foreach (var item in db.NarrowLong.SqlQuery(sql)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (NoTracking)
				return false;

			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					await db.NarrowLong.SqlQuery(sql).ForEachAsync(item => {});

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (NoTracking)
				return false;

			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					foreach (var item in db.WideLong.SqlQuery(sql)) {}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			if (NoTracking)
				return false;

			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.WideLong.AsNoTracking() : db.WideLong;
					await db.WideLong.SqlQuery(sql).ForEachAsync(item => {});
				}

			watch.Stop();

			return true;
		}
	}
}
