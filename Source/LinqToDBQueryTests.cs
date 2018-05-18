using System;
using System.Diagnostics;

using LinqToDB.Data;

namespace Tests
{
	using DataModel;

	class LinqToDBQueryTests : ITests
	{
		public bool GetSingleColumnFast(Stopwatch watch, int count)
		{
			watch.Start();

			using (var db = new TestContext())
				for (var i = 0; i < count; i++)
					db.Execute<int>("SELECT ID FROM Narrow WHERE ID = 1");

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int count)
		{
			watch.Start();

			for (var i = 0; i < count; i++)
				using (var db = new TestContext())
					db.Execute<int>("SELECT ID FROM Narrow WHERE ID = 1");

			watch.Stop();

			return true;
		}
	}
}
