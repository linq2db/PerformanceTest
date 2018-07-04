using System;

using LinqToDB;

namespace TestRunner.DataModel
{
	public class ResultDB : LinqToDB.Data.DataConnection
	{
		public ITable<Setting>       Settings        => GetTable<Setting>();
		public ITable<TestMethod>    TestMethods     => GetTable<TestMethod>();
		public ITable<TestRun>       TestRuns        => GetTable<TestRun>();
		public ITable<TestStopwatch> TestStopwatches => GetTable<TestStopwatch>();

		public ResultDB()
		{
		}

		public ResultDB(string configuration)
			: base(configuration)
		{
		}
	}
}
