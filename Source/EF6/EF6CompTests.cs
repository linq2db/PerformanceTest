using System;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;

namespace Tests.EF6
{
	class EF6CompTests : ITests
	{
		public string Name => "EF6 Compiled" + (NoTracking ? "" : " CT");

		public readonly bool NoTracking;

		public EF6CompTests(bool noTracking)
		{
			NoTracking = noTracking;
		}

		public bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;

			var query = CompiledQuery.Compile((ObjectContext db) =>
				db.CreateObjectSet<Narrow>().Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			using (var db = new EF6Context(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					query(((IObjectContextAdapter)db).ObjectContext);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;

			var query = CompiledQuery.Compile((ObjectContext db) =>
				db.CreateObjectSet<Narrow>().Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					query(((IObjectContextAdapter)db).ObjectContext);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;

			var query = CompiledQuery.Compile((ObjectContext db, int id, int p) =>
				db.CreateObjectSet<Narrow>().Where(t => t.ID == id && t.Field1 == p).Select(t => t.ID).First());

			watch.Start();

			using (var db = new EF6Context(NoTracking))
				for (var i = 0; i < repeatCount; i++)
					query(((IObjectContextAdapter)db).ObjectContext, 1, 2);

			watch.Stop();

			return true;
		}

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;

			var query = CompiledQuery.Compile((ObjectContext db, int top) =>
				db.CreateObjectSet<NarrowLong>().Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					foreach (var item in query(((IObjectContextAdapter)db).ObjectContext, takeCount)) {}

			watch.Stop();

			return true;
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;

			var query = CompiledQuery.Compile((ObjectContext db, int top) =>
				db.CreateObjectSet<WideLong>().Take(top));

			watch.Start();

			for (var i = 0; i < repeatCount; i++)
				using (var db = new EF6Context(NoTracking))
					foreach (var item in query(((IObjectContextAdapter)db).ObjectContext, takeCount)) {}

			watch.Stop();

			return true;
		}
	}
}
