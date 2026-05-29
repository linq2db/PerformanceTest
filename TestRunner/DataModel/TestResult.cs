using System;

using LinqToDB.Mapping;

namespace TestRunner.DataModel
{
	[Table]
	public class TestResult
	{
		[PrimaryKey, Identity] public int      ID              { get; set; }
		[Column(Length=50)]    public string   Platform        { get; set; } = default!;
		[Column(Length=50)]    public string   GroupName       { get; set; } = default!;
		[Column(Length=50)]    public string   TestName        { get; set; } = default!;
		[Column]               public int      Repeat          { get; set; }
		[Column, Nullable]     public int?     Take            { get; set; }
		[Column(Length=50)]    public string   TestDescription { get; set; } = default!;
		[Column(Length=50)]    public string   Provider        { get; set; } = default!;
		[Column]               public long     Ticks           { get; set; }
		[Column]               public TimeSpan Time            { get; set; }
		[Column]               public DateTime CreatedOn       { get; set; }
	}
}
