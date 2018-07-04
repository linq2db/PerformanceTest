using System;

using LinqToDB.Mapping;

namespace TestRunner.DataModel
{
	[Table]
	public class TestMethod
	{
		[PrimaryKey, Identity] public int      ID        { get; set; }
		[Column]               public int      TestRunID { get; set; }
		[Column(Length=50)]    public string   Name      { get; set; }
		[Column]               public int      Repeat    { get; set; }
		[Column, Nullable]     public int?     Take      { get; set; }
		[Column]               public DateTime CreatedOn { get; set; }
	}
}
