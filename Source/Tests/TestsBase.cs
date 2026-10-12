using System;

namespace Tests.Tests
{
	abstract class TestsBase : TestRunner.ITests
	{
		protected const string GetSingleColumnSql = "SELECT ID FROM Narrow WHERE ID = 1";
		protected const string GetParamSql        = "SELECT ID FROM Narrow WHERE ID = @id AND Field1 = @p";

		protected string GetNarrowListSql(int takeCount) => $"SELECT TOP ({takeCount}) ID, Field1 FROM NarrowLong";
		protected string GetWideListSql  (int takeCount) =>
			$"""
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
			FROM WideLong
			""";

		public abstract string Name { get; set; }

		public virtual void SetUp()
		{
		}

		public virtual void TearDown()
		{
		}

		/// <summary>
		/// The configured connection string without an application name.
		/// </summary>
		public static string BaseConnectionString { get; set; } = "";

		/// <summary>
		/// Returns the connection string for one library. Every library gets its own
		/// Application Name and therefore its own connection pool.
		/// </summary>
		public static string GetConnectionString(string libraryName)
		{
			// SqlClient uses the last occurrence of a keyword, so this overrides any application name in the base string.
			return BaseConnectionString.TrimEnd(';') + $";Application Name={libraryName} Test";
		}

		protected string ConnectionString = GetConnectionString("LinqToDB");
	}
}
