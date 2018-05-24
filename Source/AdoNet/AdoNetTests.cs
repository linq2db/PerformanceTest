using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Tests.AdoNet
{
	using DataModel;

	class AdoNetTests : ITests
	{
		public string Name => "AdoNet";

		readonly string _connectionString = LinqToDB.Data.DataConnection.GetConnectionString("Test");

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(_connectionString))
			{
				con.Open();

				var cmd = con.CreateCommand();

				cmd.CommandText = "SELECT ID FROM Narrow WHERE ID = 1";

				for (var i = 0; i < repeatCount; i++)
					cmd.ExecuteScalar();
			}

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var con = new SqlConnection(_connectionString))
				{
					con.Open();

					var cmd = con.CreateCommand();

					cmd.CommandText = "SELECT ID FROM Narrow WHERE ID = 1";
					cmd.ExecuteScalar();
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

				var cmd = con.CreateCommand();

				cmd.CommandText = "SELECT ID FROM Narrow WHERE ID = @id AND Field1 = @p";

				for (var i = 0; i < repeatCount; i++)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(new SqlParameter("@id", 1));
					cmd.Parameters.Add(new SqlParameter("@p",  2));
					cmd.ExecuteScalar();
				}
			}

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			using (var con = new SqlConnection(_connectionString))
			{
				con.Open();

				var cmd = con.CreateCommand();

				cmd.CommandText = $"SELECT TOP {takeCount} ID, Field1 FROM NarrowLong";

				using (var rd = cmd.ExecuteReader())
					if (rd.HasRows)
						while (rd.Read())
							new NarrowLong
							{
								ID = rd.GetInt32(0),
								Field1 = rd.GetInt32(1),
							};
			}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			using (var con = new SqlConnection(_connectionString))
			{
				con.Open();

				var cmd = con.CreateCommand();

				cmd.CommandText = $@"
SELECT TOP {takeCount}
	ID,
	Field1,
	ShortValue,
	IntValue,
	LongValue,
	StringValue,
	DateTimeValue
FROM WideLong";

				using (var rd = cmd.ExecuteReader())
					if (rd.HasRows)
						while (rd.Read())
							new WideLong
							{
								ID            = rd.GetInt32(0),
								Field1        = rd.GetInt32(1),
								ShortValue    = rd.IsDBNull(2) ? null : (short?)   rd.GetInt16   (2),
								IntValue      = rd.IsDBNull(3) ? null : (int?)     rd.GetInt32   (3),
								LongValue     = rd.IsDBNull(4) ? null : (long?)    rd.GetInt64   (4),
								StringValue   = rd.IsDBNull(5) ? null :            rd.GetString  (5),
								DateTimeValue =	rd.IsDBNull(6) ? null : (DateTime?)rd.GetDateTime(6)
							};
			}

			watch.Stop();

			return true;
		}
	}
}
