using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace Tests
{
	using DataModel;

	class EFCompTests : ITests
	{
		public bool GetSingleColumnFast(Stopwatch watch, int count)
		{
			var query = EF.CompileQuery((TestEFContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			using (var db = new TestEFContext())
				for (var i = 0; i < count; i++)
					query(db);

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int count)
		{
			var query = EF.CompileQuery((TestEFContext db) =>
				db.Narrow.Where(t => t.ID == 1).Select(t => t.ID).First());

			watch.Start();

			for (var i = 0; i < count; i++)
				using (var db = new TestEFContext())
					query(db);

			watch.Stop();

			return true;
		}
	}
}
