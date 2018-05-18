using System;
using System.Data.SqlClient;
using System.Diagnostics;

using Dapper;

namespace Tests
{
	class DapperTests : ITests
	{
		readonly string _connectionString = LinqToDB.Data.DataConnection.GetConnectionString("Test");

		public bool GetSingleColumnFast(Stopwatch watch, int count)
		{
			watch.Start();

			using (var con = new SqlConnection(_connectionString))
			{
				con.Open();
				for (var i = 0; i < count; i++)
					con.Query<int>("SELECT ID FROM Narrow WHERE ID = 1");
			}

			watch.Stop();

			return true;
		}
		public bool GetSingleColumnSlow(Stopwatch watch, int count)
		{
			watch.Start();

			for (var i = 0; i < count; i++)
				using (var con = new SqlConnection(_connectionString))
				{
					con.Open();
					con.Query<int>("SELECT ID FROM Narrow WHERE ID = 1");
				}

			watch.Stop();

			return true;
		}
	}
}
