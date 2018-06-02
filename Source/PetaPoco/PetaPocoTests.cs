using System;
using System.Data.SqlClient;
using System.Diagnostics;

using PetaPoco;

namespace Tests.PetaPoco
{
	using DataModel;

	class PetaPocoTests : TestsBase
	{
		public override string Name => "PetaPoco";

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(ConnectionString))
			{
				con.Open();
				using (var db = new Database(con))
					for (var i = 0; i < repeatCount; i++)
						db.Query<int>(GetSingleColumnSql);
			}

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var con = new SqlConnection(ConnectionString))
				{
					con.Open();

					using (var db = new Database(con))
						db.Query<int>(GetSingleColumnSql);
				}
			}

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(ConnectionString))
			{
				con.Open();
				using (var db = new Database(con))
					for (var i = 0; i < repeatCount; i++)
						db.Query<int>(GetParamSql, new {id = 1, p = 2});
			}

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var con = new SqlConnection(ConnectionString))
				{
					con.Open();
					using (var db = new Database(con))
						foreach (var item in db.Query<NarrowLong>(sql)) {}
				}
			}

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var con = new SqlConnection(ConnectionString))
				{
					con.Open();
					using (var db = new Database(con))
						foreach (var item in db.Query<WideLong>(sql)) {}
				}
			}

			watch.Stop();

			return true;
		}
	}
}
