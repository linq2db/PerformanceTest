﻿using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.EF6
{
	class EF6LinqTests : TestsWithChangeTrackingBase
	{
		public override string Name => "EF6 Linq";

		public EF6LinqTests(bool noTracking) : base(noTracking)
		{
		}

		public override bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
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

		public override async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EF6Context(NoTracking))
			{
				var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

				for (var i = 0; i < repeatCount; i++)
					await q.Where(t => t.ID == 1).Select(t => t.ID).FirstAsync();
			}

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
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

		public override async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

					await db.Narrow.AsNoTracking().Where(t => t.ID == 1).Select(t => t.ID).FirstAsync();
				}
			}

			watch.Stop();

			return true;
		}

		public override bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
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

		public override async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			using (var db = new EF6Context(NoTracking))
			{
				var q = NoTracking ? db.Narrow.AsNoTracking() : db.Narrow;

				for (var i = 0; i < repeatCount; i++)
				{
					var id = 1;
					var p  = 2;
					await q.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).FirstAsync();
				}
			}

			watch.Stop();

			return true;
		}

		public override bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
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

		public override async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.NarrowLong.AsNoTracking() : db.NarrowLong;
					await q.Take(takeCount).ForEachAsync(item => {});
				}
			}

			watch.Stop();

			return true;
		}

		public override bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
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

		public override async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
			{
				using (var db = new EF6Context(NoTracking))
				{
					var q = NoTracking ? db.WideLong.AsNoTracking() : db.WideLong;
					await db.WideLong.Take(takeCount).ForEachAsync(item => {});
				}
			}

			watch.Stop();

			return true;
		}


		public override bool SimpleLinqQuery(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
				{
					var q =
					(
						from n1 in db.Narrow
						where n1.ID < 100
						select n1.ID
					)
					.Take(takeCount);

					foreach (var item in q) {}
				}

			watch.Stop();

			return true;
		}

		public override bool ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
				{
					var q =
					(
						from n1 in db.Narrow
						join n2 in db.Narrow on new { n1.ID, n1.Field1 } equals new { n2.ID, n2.Field1 }
						where n1.ID < 100 && n2.Field1 <= 50
						group n1 by n1.ID into gr
						select new
						{
							gr.Key,
							Count = gr.Count()
						}
					)
					.OrderBy(n1 => n1.Key)
					.Skip(1)
					.Take(takeCount);

					foreach (var item in q) {}
				}

			watch.Stop();

			return true;
		}

		public override bool ComplicatedLinqSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
				{
					var q =
					(
						from n in db.NarrowLong
						join w in db.WideLong on n.Field1 equals w.Field1
						where
							n.ID >= 0 && n.ID <= 1000000 &&
							!new[] { 0, 20, 50, 187635 }.Contains(w.Field1)
						select new
						{
							n.ID,
							w.Field1
						}
					)
					.Union
					(
						from n in db.NarrowLong
						join w in db.WideLong on n.Field1 equals w.Field1
						where
							n.ID >= 0 && n.ID <= 1000000 &&
							!new[] { 0, 240, 500, 18635 }.Contains(w.Field1)
						select new
						{
							n.ID,
							w.Field1
						}
					)
					.OrderByDescending(n1 => n1.Field1)
					.Skip(1000)
					.Take(takeCount);

					foreach (var item in q) {}
				}

			watch.Stop();

			return true;
		}
	}
}
