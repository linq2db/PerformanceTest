using System;
using System.Data.SqlClient;
using System.Diagnostics;

using PetaPoco;

namespace Tests.PetaPoco
{
	using DataModel;

	class PetaPocoTests : ITests
	{
		public string Name => "PetaPoco";

		readonly string _connectionString = LinqToDB.Data.DataConnection.GetConnectionString("Test");

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(_connectionString))
			{
				con.Open();

				using (var db = new Database(con))
					for (var i = 0; i < repeatCount; i++)
						db.Query<int>("SELECT ID FROM Narrow WHERE ID = 1");
			}

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var con = new SqlConnection(_connectionString))
				{
					con.Open();

					using (var db = new Database(con))
						db.Query<int>("SELECT ID FROM Narrow WHERE ID = 1");
				}
			}

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(_connectionString))
			{
				con.Open();

				using (var db = new Database(con))
					for (var i = 0; i < repeatCount; i++)
						db.Query<int>("SELECT ID FROM Narrow WHERE ID = @id AND Field1 = @p", new {id = 1, p = 2});
			}

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var con = new SqlConnection(_connectionString))
				{
					con.Open();

					using (var db = new Database(con))
						foreach (var item in db.Query<NarrowLong>($"SELECT TOP {takeCount} ID, Field1 FROM NarrowLong")) { }
				}
			}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var con = new SqlConnection(_connectionString))
				{
					con.Open();

					using (var db = new Database(con))
					{
						foreach (var item in db.Query<WideLong>($@"
SELECT TOP {takeCount}
	ID,
	Field1,
	ShortValue,
	IntValue,
	LongValue,
	StringValue,
	DateTimeValue
FROM WideLong")) { }
					}
				}
			}

			watch.Stop();

			return true;
		}
	}
}
