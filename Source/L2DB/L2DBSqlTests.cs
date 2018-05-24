﻿using System;
using System.Diagnostics;

using LinqToDB.Data;

namespace Tests.L2DB
{
	using DataModel;

	class L2DBSqlTests : ITests
	{
		public string Name => "L2DB Sql" + (NoTracking ? "" : " CT");

		public readonly bool NoTracking;

		public L2DBSqlTests(bool noTracking)
		{
			NoTracking = noTracking;
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.Execute<int>("SELECT ID FROM Narrow WHERE ID = 1");

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					db.Execute<int>("SELECT ID FROM Narrow WHERE ID = 1");

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2DBContext(NoTracking))
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

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					foreach (var item in db.Query<NarrowLong>($"SELECT TOP {takeCount} ID, Field1 FROM NarrowLong")) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2DBContext(NoTracking))
					foreach (var item in db.Query<WideLong>($@"
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