using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tests.AdoNet
{
	using DataModel;

	class AdoNetTests : TestsBase
	{
		public override string Name => "AdoNet";

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(ConnectionString))
			{
				con.Open();

				var cmd = con.CreateCommand();

				cmd.CommandText = GetSingleColumnSql;

				for (var i = 0; i < repeatCount; i++)
					cmd.ExecuteScalar();
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

				var cmd = con.CreateCommand();

				cmd.CommandText = GetSingleColumnSql;

				for (var i = 0; i < repeatCount; i++)
					await cmd.ExecuteScalarAsync();
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

					var cmd = con.CreateCommand();

					cmd.CommandText = GetSingleColumnSql;
					cmd.ExecuteScalar();
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

					var cmd = con.CreateCommand();

					cmd.CommandText = GetSingleColumnSql;
					await cmd.ExecuteScalarAsync();
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

				var cmd = con.CreateCommand();

				cmd.CommandText = GetParamSql;

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

		public override async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var con = new SqlConnection(ConnectionString))
			{
				await con.OpenAsync();

				var cmd = con.CreateCommand();

				cmd.CommandText = GetParamSql;

				for (var i = 0; i < repeatCount; i++)
				{
					cmd.Parameters.Clear();
					cmd.Parameters.Add(new SqlParameter("@id", 1));
					cmd.Parameters.Add(new SqlParameter("@p",  2));
					await cmd.ExecuteScalarAsync();
				}
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

				var cmd = con.CreateCommand();

				cmd.CommandText = sql;

				using (var rd = cmd.ExecuteReader())
					if (rd.HasRows)
						while (rd.Read())
							new NarrowLong
							{
								ID     = rd.GetInt32(0),
								Field1 = rd.GetInt32(1),
							};
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

				var cmd = con.CreateCommand();

				cmd.CommandText = sql;

				using (var rd = await cmd.ExecuteReaderAsync())
					if (rd.HasRows)
						while (await rd.ReadAsync())
							new NarrowLong
							{
								ID     = rd.GetInt32(0),
								Field1 = rd.GetInt32(1),
							};
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

				var cmd = con.CreateCommand();

				cmd.CommandText = sql;

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

		public override async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = GetWideListSql(takeCount);

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			using (var con = new SqlConnection(ConnectionString))
			{
				await con.OpenAsync();

				var cmd = con.CreateCommand();

				cmd.CommandText = sql;

				using (var rd = await cmd.ExecuteReaderAsync())
					if (rd.HasRows)
						while (await rd.ReadAsync())
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
