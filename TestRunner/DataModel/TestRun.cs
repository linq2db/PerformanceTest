using System;

using LinqToDB.Mapping;

namespace TestRunner.DataModel
{
	[Table]
	public class TestRun
	{
		[PrimaryKey, Identity] public int      ID;
		[Column(Length=50)]    public string   Platform;
		[Column(Length=50)]    public string   Name;
		[Column]               public DateTime CreatedOn;
	}
}
