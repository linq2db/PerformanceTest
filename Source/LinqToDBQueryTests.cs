using System;
using System.Diagnostics;

using LinqToDB.Data;

namespace Tests
{
	using DataModel;

	class LinqToDBQueryTests : ITests
	{
		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new TestContext())
				for (var i = 0; i < repeatCount; i++)
					db.Execute<int>("SELECT ID FROM Narrow WHERE ID = 1");

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new TestContext())
					db.Execute<int>("SELECT ID FROM Narrow WHERE ID = 1");

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new TestContext())
				for (var i = 0; i < repeatCount; i++)
					db.Execute<int>("SELECT ID FROM Narrow WHERE ID = @id AND Field1 = @p",
						new DataParameter("@id", 1),
						new DataParameter("@p",  2));

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new TestContext())
				for (var i = 0; i < repeatCount; i++)
					foreach (var item in db.Query<NarrowLong>($"SELECT TOP {takeCount} ID, Field1 FROM NarrowLong")) {}

			watch.Stop();

			return true;
		}
	}
}
