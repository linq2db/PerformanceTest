using System;
using System.Diagnostics;
using System.Linq;

namespace Tests
{
	using DataModel;

	class LinqToDBLinqTests : ITests
	{
		public bool GetSingleColumnFast(Stopwatch watch, int count)
		{
			watch.Start();

			using (var db = new TestContext())
				for (var i = 0; i < count; i++)
					db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int count)
		{
			watch.Start();

			for (var i = 0; i < count; i++)
				using (var db = new TestContext())
					db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}
	}
}
