using System;

using LinqToDB.Mapping;

namespace Tests.DataModel
{
	[Table()]
	public class TestMethod
	{
		[PrimaryKey, Identity] public int      ID;
		[Column]               public int      TestRunID;
		[Column(Length=50)]    public string   Name;
		[Column]               public int      Repeat;
		[Column, Nullable]     public int?     Take;
	}
}
