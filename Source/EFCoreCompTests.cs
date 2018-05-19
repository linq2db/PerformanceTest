using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace Tests
{
	using DataModel;

	class EFCoreCompTests : ITests
	{
		public readonly bool NoTracking;

		public EFCoreCompTests(bool noTracking)
		{
			NoTracking = noTracking;
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					query(db);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					query(db);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db, int id, int p) =>
				db.Narrow.Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

			watch.Start();

			using (var db = new EFCoreContext(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					query(db, 1, 2);

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db, int top) =>
				db.NarrowLong.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			var query = EF.CompileQuery((EFCoreContext db, int top) =>
				db.WideLong.Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EFCoreContext(NoTracking))
					foreach (var item in query(db, takeCount)) {}

			watch.Stop();

			return true;
		}
	}
}
