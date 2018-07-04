using System;

namespace Tests.Tests
{
	abstract class TestsBase : TestRunner.ITests
	{
		protected const string GetSingleColumnSql = "SELECT ID FROM Narrow WHERE ID = 1";
		protected const string GetParamSql        = "SELECT ID FROM Narrow WHERE ID = @id AND Field1 = @p";

		protected string GetNarrowListSql(int takeCount) => $"SELECT TOP ({takeCount}) ID, Field1 FROM NarrowLong";
		protected string GetWideListSql  (int takeCount) => $@"
SELECT TOP ({takeCount})
	ID,
	Field1,
	ByteValue,
	ShortValue,
	IntValue,
	LongValue,
	StringValue,
	DateTimeValue,
	TimeValue,
	DecimalValue,
	DoubleValue,
	FloatValue
FROM WideLong";

		public abstract string Name { get; set; }

		public virtual void SetUp()
		{
		}

		public virtual void TearDown()
		{
		}

		protected string ConnectionString = LinqToDB.Data.DataConnection.GetConnectionString("Test");
	}
}
