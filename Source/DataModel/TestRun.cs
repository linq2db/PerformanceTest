using System;

using LinqToDB.Mapping;

namespace Tests.DataModel
{
	[Table()]
	public class TestRun
	{
		[PrimaryKey, Identity] public int      ID;
		[Column(Length=20)]    public string   Platform;
		[Column(Length=20)]    public string   Name;
		[Column]               public DateTime CreatedOn;
	}
}
