using System;
using System.Diagnostics;

namespace Tests.L2S
{
	using Tests;

	class L2SSqlTests : TestsBase, IWithChangeTracking, ISingleColumnTests, IGetListTests
	{
		public override string Name         { get; set; } = "L2S Sql";
		public          bool   TrackChanges { get; set; }

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2SContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					db.ExecuteQuery<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(TrackChanges))
					db.ExecuteQuery<int>(GetSingleColumnSql);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2SContext(TrackChanges))
				for (var i = 0; i < repeatCount; i++)
					db.ExecuteQuery<int>("SELECT ID FROM Narrow WHERE ID = {0} AND Field1 = {1}", 1, 2);

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(TrackChanges))
					foreach (var item in db.ExecuteQuery<NarrowLong>(sql)) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(TrackChanges))
					foreach (var item in db.ExecuteQuery<WideLong>(sql)) {}

			watch.Stop();

			return true;
		}
	}
}
