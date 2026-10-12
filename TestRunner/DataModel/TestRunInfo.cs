using System;

using LinqToDB.Mapping;

namespace TestRunner.DataModel
{
	/// <summary>
	/// Environment and library versions a test run was measured with (one row per item).
	/// Created on demand, so result databases from older versions keep working.
	/// </summary>
	[Table]
	public class TestRunInfo
	{
		[PrimaryKey(1)]                               public int     TestRunID { get; set; }
		[PrimaryKey(2), Column(Length = 50), NotNull] public string  Name      { get; set; } = default!;
		[Column(Length = 500), Nullable]              public string? Value     { get; set; }
	}
}
