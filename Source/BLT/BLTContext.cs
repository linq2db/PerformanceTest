using System;

using BLToolkit.Data;
using BLToolkit.Data.DataProvider;
using BLToolkit.Data.Linq;

namespace Tests.BLT
{
	using DataModel;

	public class BLTContext : DbManager
	{
		public BLTContext()
			: base( _dataProvider, LinqToDB.Data.DataConnection.GetConnectionString("Test").Replace("LinqToDB", "BLToolkit"))
		{
		}

		static readonly DataProviderBase _dataProvider = new Sql2012DataProvider();

		public Table<Narrow>        Narrows         => GetTable<Narrow>();
		public Table<NarrowLong>    NarrowLongs     => GetTable<NarrowLong>();
		public Table<WideLong>      WideLongs       => GetTable<WideLong>();

		public Table<Setting>       Settings        => GetTable<Setting>();
		public Table<TestRun>       TestRuns        => GetTable<TestRun>();
		public Table<TestMethod>    TestMethods     => GetTable<TestMethod>();
		public Table<TestStopwatch> TestStopwatches => GetTable<TestStopwatch>();
		public Table<TestResult>    TestResults     => GetTable<TestResult>();
	}
}
