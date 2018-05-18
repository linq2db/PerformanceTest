using System;

using LinqToDB;
using LinqToDB.Data;

namespace Tests.DataModel
{
	public class TestContext : DataConnection
	{
		public ITable<Narrow>     Narrows     => GetTable<Narrow>();
		public ITable<NarrowLong> NarrowLongs => GetTable<NarrowLong>();
	}
}
