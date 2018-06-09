using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tests.BLT
{
	using DataModel;
	using Tests;

	class BLTSqlTests : TestsBase, ISingleColumnTests, IGetListTests
	{
		public override string Name => "BLT Sql";

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new BLTContext())
				for (var i = 0; i < repeatCount; i++)
					db.SetCommand(GetSingleColumnSql).ExecuteScalar<int>();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					db.SetCommand(GetSingleColumnSql).ExecuteScalar<int>();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
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

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					foreach (var item in db.SetCommand(sql).ExecuteList(new List<NarrowLong>(takeCount))) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new BLTContext())
					foreach (var item in db.SetCommand(sql).ExecuteList(new List<WideLong>(takeCount))) {}

			watch.Stop();

			return true;
		}
	}
}
