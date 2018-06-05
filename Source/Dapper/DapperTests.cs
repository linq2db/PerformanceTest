using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

using Dapper;

namespace Tests.Dapper
{
	using DataModel;

	class DapperTests : TestsBase
	{
		public DapperTests()
		{
			ConnectionString = ConnectionString.Replace("LinqToDB", "Dapper");
		}

		public override string Name => "Dapper";


		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(ConnectionString))
			{
				con.Open();
				for (var i = 0; i < repeatCount; i++)
					con.Query<int>(GetSingleColumnSql);
			}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(ConnectionString))
			{
				await con.OpenAsync();
				for (var i = 0; i < repeatCount; i++)
					await con.QueryAsync<int>(GetSingleColumnSql);
			}

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var con = new SqlConnection(ConnectionString))
				{
					con.Open();
					con.Query<int>(GetSingleColumnSql);
				}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var con = new SqlConnection(ConnectionString))
				{
					await con.OpenAsync();
					await con.QueryAsync<int>(GetSingleColumnSql);
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
				for (var i = 0; i < repeatCount; i++)
					con.Query<int>(GetParamSql, new { id = 1, p = 2 });
			}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(ConnectionString))
			{
				await con.OpenAsync();
				for (var i = 0; i < repeatCount; i++)
					await con.QueryAsync<int>(GetParamSql, new { id = 1, p = 2 });
			}

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			using (var con = new SqlConnection(ConnectionString))
			{
				con.Open();
				foreach (var item in con.Query<NarrowLong>(sql)) {}
			}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetNarrowListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			using (var con = new SqlConnection(ConnectionString))
			{
				await con.OpenAsync();
				foreach (var item in await con.QueryAsync<NarrowLong>(sql)) {}
			}

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			using (var con = new SqlConnection(ConnectionString))
			{
				con.Open();
				foreach (var item in con.Query<WideLong>(sql)) {}
			}

			watch.Stop();

			return true;
		}

		public override async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			using (var con = new SqlConnection(ConnectionString))
			{
				await con.OpenAsync();
				foreach (var item in await con.QueryAsync<WideLong>(sql)) {}
			}

			watch.Stop();

			return true;
		}
	}
}
