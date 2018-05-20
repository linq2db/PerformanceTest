using System;
using System.Diagnostics;

namespace Tests.L2S
{
	using DataModel;

	class L2SSqlTests : ITests
	{
		public readonly bool NoTracking;

		public L2SSqlTests(bool noTracking)
		{
			NoTracking = noTracking;
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2SContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.ExecuteQuery<int>("SELECT ID FROM Narrow WHERE ID = 1");

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					db.ExecuteQuery<int>("SELECT ID FROM Narrow WHERE ID = 1");

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2SContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.ExecuteQuery<int>("SELECT ID FROM Narrow WHERE ID = {0} AND Field1 = {1}", 1, 2);

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					foreach (var item in db.ExecuteQuery<NarrowLong>($"SELECT TOP {takeCount} ID, Field1 FROM NarrowLong")) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					foreach (var item in db.ExecuteQuery<WideLong>($@"
SELECT TOP {takeCount}
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
