using System;
using System.Diagnostics;
using System.Linq;

namespace Tests
{
	using DataModel;

	class LinqToDBLinqTests : ITests
	{
		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new TestContext())
				for (var i = 0; i < repeatCount; i++)
					db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new TestContext())
					db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new TestContext())
				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).AsEnumerable().First();
				}

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new TestContext())
				for (var i = 0; i < repeatCount; i++)
					foreach (var item in db.NarrowLongs.Take(takeCount)) {}

			watch.Stop();

			return true;
		}
	}
}
