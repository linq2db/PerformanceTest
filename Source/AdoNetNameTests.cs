using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Tests.DataModel;

namespace Tests
{
	class AdoNetNameTests : ITests
	{
		readonly string _connectionString = LinqToDB.Data.DataConnection.GetConnectionString("Test");

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}


		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(_connectionString))
			{
				con.Open();

				var cmd = con.CreateCommand();

				cmd.CommandText = $"SELECT TOP {takeCount} ID, Field1 FROM NarrowLong";

				for (var i = 0; i < repeatCount; i++)
				{
					using (var rd = cmd.ExecuteReader())
					{
						var idx1 = rd.GetOrdinal("ID");
						var idx2 = rd.GetOrdinal("Field1");

						if (rd.HasRows) while(rd.Read())
						{
							new Narrow
							{
								ID     = rd.GetInt32(idx1),
								Field1 = rd.GetInt32(idx2),
							};
						}
					}
				}
			}

			watch.Stop();

			return true;
		}
	}
}
