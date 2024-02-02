using System;

using LinqToDB.Mapping;

namespace TestRunner.DataModel
{
	[Table]
	public class TestRun
	{
		[PrimaryKey, Identity] public int      ID        { get; set; }
		[Column(Length=50)]    public string   Platform  { get; set; } = default!;
		[Column(Length=50)]    public string   Name      { get; set; } = default!;
		[Column]               public DateTime CreatedOn { get; set; }
	}
}
