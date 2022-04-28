using System;

using LinqToDB;

namespace TestRunner.DataModel
{
	public class ResultDB : LinqToDB.Data.DataConnection
	{
		public ITable<Setting>       Settings        => this.GetTable<Setting>();
		public ITable<TestMethod>    TestMethods     => this.GetTable<TestMethod>();
		public ITable<TestRun>       TestRuns        => this.GetTable<TestRun>();
		public ITable<TestStopwatch> TestStopwatches => this.GetTable<TestStopwatch>();

		public ResultDB()
		{
		}

		public ResultDB(string configuration)
			: base(configuration)
		{
		}
	}
}
