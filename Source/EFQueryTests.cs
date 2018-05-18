﻿using System;
using System.Diagnostics;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace Tests
{
	using DataModel;

	class EFQueryTests : ITests
	{
		public bool GetSingleColumnFast(Stopwatch watch, int count)
		{
			watch.Start();

			using (var db = new TestEFContext())
				for (var i = 0; i < count; i++)
					db.Narrow.FromSql("SELECT ID FROM Narrow WHERE ID = 1").Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}

		public bool GetSingleColumnSlow(Stopwatch watch, int count)
		{
			watch.Start();

			for (var i = 0; i < count; i++)
				using (var db = new TestEFContext())
					db.Narrow.FromSql("SELECT ID FROM Narrow WHERE ID = 1").Select(t => t.ID).AsEnumerable().First();

			watch.Stop();

			return true;
		}
	}
}
