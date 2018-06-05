using System;
using System.Diagnostics;

namespace Tests.BLT
{
	using DataModel;

	class BLTSqlTests : TestsBase
	{
		public override string Name => "BLT Sql";

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new BLTContext())
				for (var i = 0; i < repeatCount; i++)
					db.SetCommand(GetSingleColumnSql).ExecuteScalar<int>();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					db.SetCommand(GetSingleColumnSql).ExecuteScalar<int>();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new BLTContext())
				for (var i = 0; i < repeatCount; i++)
					db.SetCommand(GetParamSql,
						db.Parameter("@id", 1),
						db.Parameter("@p",  2))
						.ExecuteScalar<int>();

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					foreach (var item in db.SetCommand(sql).ExecuteList<NarrowLong>(sql)) {}

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					foreach (var item in db.SetCommand(sql).ExecuteList<WideLong>(sql)) {}

			watch.Stop();

			return true;
		}
	}
}
