using System;
using System.Diagnostics;

namespace Tests.L2S
{
	class L2SSqlTests : TestsWithChangeTrackingBase
	{
		public override string Name => "L2S Sql";

		public L2SSqlTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2SContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.ExecuteQuery<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					db.ExecuteQuery<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2SContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.ExecuteQuery<int>("SELECT ID FROM Narrow WHERE ID = {0} AND Field1 = {1}", 1, 2);

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					foreach (var item in db.ExecuteQuery<NarrowLong>(sql)) {}

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					foreach (var item in db.ExecuteQuery<WideLong>(sql)) {}

			watch.Stop();

			return true;
		}
	}
}
