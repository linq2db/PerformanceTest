using System;
using System.Diagnostics;
using System.Linq;

namespace Tests.EF6
{
	class EF6LinqTests : ITests
	{
		public string Name => "EF6 Linq" + (NoTracking ? "" : " CT");

		public readonly bool NoTracking;

		public EF6LinqTests(bool noTracking)
		{
			NoTracking = noTracking;
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EF6Context(NoTracking))
			{
				var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

				for (var i = 0; i < repeatCount; i++)
					q.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();
			}

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

					db.Narrow.AsNoTracking().Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();
				}
			}

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EF6Context(NoTracking))
			{
				var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					q.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).AsEnumerable().First();
				}
			}

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.NarrowLong.AsNoTracking() : db.NarrowLong;
					foreach (var item in q.Take(takeCount)) {}
				}
			}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.WideLong.AsNoTracking() : db.WideLong;
					foreach (var item in db.WideLong.Take(takeCount)) {}
				}
			}

			watch.Stop();

			return true;
		}
	}
}
