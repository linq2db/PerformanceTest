using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.Data.SqlClient;

using PetaPoco;

namespace Tests.PetaPoco
{
	using DataModel;
	using Tests;

	class PetaPocoTests : TestsBase, ISingleColumnTests, IGetListTests
	{
		public PetaPocoTests()
		{
			ConnectionString = ConnectionString.Replace("LinqToDB", "PetaPoco");
		}

		public override string Name { get; set; } = "PetaPoco";

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(ConnectionString))
			{
				con.Open();
				using (var db = new Database(con))
					for (var i = 0; i < repeatCount; i++)
						db.Query<int>(GetSingleColumnSql).First();
			}

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var con = new SqlConnection(ConnectionString))
				{
					con.Open();

					using (var db = new Database(con))
						db.Query<int>(GetSingleColumnSql).First();
				}
			}

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(ConnectionString))
			{
				con.Open();
				using (var db = new Database(con))
					for (var i = 0; i < repeatCount; i++)
						db.Query<int>(GetParamSql, new {id = 1, p = 2}).First();
			}

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
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

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
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
