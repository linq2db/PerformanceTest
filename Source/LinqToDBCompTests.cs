using System;
using System.Diagnostics;
using System.Linq;

using LinqToDB;

namespace Tests
{
	using DataModel;

	class LinqToDBCompTests : ITests
	{
		public bool GetSingleColumnFast(Stopwatch watch, int count)
		{
			var query = CompiledQuery.Compile((TestContext db) =>
				db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			using (var db = new TestContext())
				for (var i = 0; i < count; i++)
					query(db);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int count)
		{
			var query = CompiledQuery.Compile((TestContext db) =>
				db.Narrows.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			for (var i = 0; i < count; i++)
				using (var db = new TestContext())
					query(db);

			watch.Stop();

			return true;
		}
	}
}
