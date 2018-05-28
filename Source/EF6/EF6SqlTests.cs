using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace Tests.EF6
{
	class EF6SqlTests : ITests
	{
		public string Name => "EF6 Sql" + (NoTracking ? "" : " CT");

		public readonly bool NoTracking;

		public EF6SqlTests(bool noTracking)
		{
			NoTracking = noTracking;
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EF6Context(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.Database.SqlQuery<int>("SELECT ID FROM Narrow WHERE ID = 1").First();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					db.Database.SqlQuery<int>("SELECT ID FROM Narrow WHERE ID = 1").First();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EF6Context(NoTracking))
			{
				for (var i = 0; i < repeatCount; i++)
					db.Database
						.SqlQuery<int>("SELECT ID FROM Narrow WHERE ID = @id AND Field1 = @p",
							new SqlParameter("@id", 1),
							new SqlParameter("@p",  2))
						.First();
			}

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					foreach (var item in db.NarrowLong.SqlQuery($"SELECT TOP ({takeCount}) ID, Field1 FROM NarrowLong")) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					foreach (var item in db.WideLong.SqlQuery($@"
SELECT TOP ({takeCount})
	ID,
	Field1,
	ShortValue,
	IntValue,
	LongValue,
	StringValue,
	DateTimeValue
FROM WideLong")) {}

			watch.Stop();

			return true;
		}
	}
}
