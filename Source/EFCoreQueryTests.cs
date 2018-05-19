using System;
using System.Diagnostics;
using System.Linq;
using System.Data.SqlClient;

using Microsoft.EntityFrameworkCore;

namespace Tests
{
	using DataModel;

	class EFCoreQueryTests : ITests
	{
		public readonly bool NoTracking;

		public EFCoreQueryTests(bool noTracking)
		{
			NoTracking = noTracking;
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.Narrow.FromSql("SELECT ID FROM Narrow WHERE ID = 1").Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					db.Narrow.FromSql("SELECT ID FROM Narrow WHERE ID = 1").Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
			{
				for (var i = 0; i < repeatCount; i++)
					db.Narrow
						.FromSql("SELECT ID FROM Narrow WHERE ID = @id AND Field1 = @p",
							new SqlParameter("@id", 1),
							new SqlParameter("@p",  2))
						.Select(t => t.ID).AsEnumerable().First();
			}

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in db.NarrowLong.FromSql($"SELECT TOP ({takeCount}) ID, Field1 FROM NarrowLong")) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in db.WideLong.FromSql($@"
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
