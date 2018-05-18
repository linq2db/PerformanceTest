using System;
using System.Diagnostics;
using System.Linq;

namespace Tests
{
	using DataModel;

	class EFLinqTests : ITests
	{
		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new TestEFContext())
				for (var i = 0; i < repeatCount; i++)
					db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new TestEFContext())
					db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new TestEFContext())
				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					db.Narrow.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).AsEnumerable().First();
				}

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new TestEFContext())
				for (var i = 0; i < repeatCount; i++)
					foreach (var item in db.NarrowLong.Take(takeCount)) {}

			watch.Stop();

			return true;
		}
	}
}
