using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tests.AdoNet
{
	using DataModel;

	class AdoNetTests : TestsBase
	{
		public AdoNetTests()
		{
			ConnectionString = ConnectionString.Replace("LinqToDB", "AdoNet");
		}

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

		public override bool SimpleLinqQuery(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = $@"
SELECT TOP ({takeCount})
	[t1].[ID]
FROM
	[Narrow] [t1]
WHERE
	[t1].[ID] < 100";

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
						{
							var item = new
							{
								ID = rd.GetInt32(0),
							};
						}
			}

			watch.Stop();

			return true;
		}

		public override bool ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var sql = $@"
SELECT
	[n1].[ID],
	Count(*) as [c1]
FROM
	[Narrow] [n1]
WHERE
	[n1].[ID] < 100 AND [n1].[Field1] <= 50
GROUP BY
	[n1].[ID]
ORDER BY
	[n1].[ID]
OFFSET 1 ROWS FETCH NEXT {takeCount} ROWS ONLY";

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
						{
							var item = new
							{
								Key   = rd.GetInt32(0),
								Count = rd.GetInt32(1),
							};
						}
			}

			watch.Stop();

			return true;
		}

		public override bool ComplicatedLinqSlow(Stopwatch watch, int repeatCount, int takeCount, int nRows)
		{
			var sql = $@"
SELECT
	[t1].[ID] as [ID1],
	[t1].[Field1] as [Field11]
FROM
	(
		SELECT
			[n].[ID],
			[w].[Field1]
		FROM
			[NarrowLong] [n]
				INNER JOIN [WideLong] [w] ON [n].[Field1] = [w].[Field1]
		WHERE
			[n].[ID] >= 0 AND [n].[ID] <= {nRows} AND [w].[Field1] NOT IN (0, 20, 50, 187635)
		UNION
		SELECT
			[n1].[ID],
			[w1].[Field1]
		FROM
			[NarrowLong] [n1]
				INNER JOIN [WideLong] [w1] ON [n1].[Field1] = [w1].[Field1]
		WHERE
			[n1].[ID] >= 0 AND [n1].[ID] <= {nRows} AND [w1].[Field1] NOT IN (0, 240, 500, 18635)
	) [t1]
ORDER BY
	[t1].[Field1] DESC
OFFSET 1000 ROWS FETCH NEXT {takeCount} ROWS ONLY
";

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
						{
							var item = new
							{
								Key   = rd.GetInt32(0),
								Count = rd.GetInt32(1),
							};
						}
			}

			watch.Stop();

			return true;
		}
	}
}
