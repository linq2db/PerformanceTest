using System;

using LinqToDB.Mapping;

namespace TestRunner.DataModel
{
	[Table]
	public class TestStopwatch
	{
		[PrimaryKey, Identity] public int      ID           { get; set; }
		[Column]               public int      TestMethodID { get; set; }
		[Column]               public TimeSpan Time         { get; set; }
		[Column]               public long     Ticks        { get; set; }
		[Column(Length = 50)]  public string   Provider     { get; set; } = default!;
		[Column]               public DateTime CreatedOn    { get; set; }
	}
}
