using System.Diagnostics;
using System.Linq;

namespace Tests.L2S
{
	class L2SLinqTests : TestsWithChangeTrackingBase
	{
		public override string Name => "L2S Linq";

		public L2SLinqTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2SContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First();

			watch.Stop();

			return true;
		}


		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new L2SContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					db.Narrows.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).AsEnumerable().First();
				}

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					foreach (var item in db.NarrowLongs.Take(takeCount)) {}

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new L2SContext(NoTracking))
					foreach (var item in db.WideLongs.Take(takeCount)) {}

			watch.Stop();

			return true;
		}
	}
}
