using System;

using LinqToDB.Mapping;

namespace TestRunner.DataModel
{
	[Table]
	public class TestResult1
	{
		[PrimaryKey, Identity] public int      ID              { get; set; }
		[Column(Length=50)]    public string   Platform        { get; set; }
		[Column(Length=50)]    public string   GroupName       { get; set; }
		[Column(Length=50)]    public string   TestName        { get; set; }
		[Column]               public int      Repeat          { get; set; }
		[Column, Nullable]     public int?     Take            { get; set; }
		[Column(Length=50)]    public string   TestDescription { get; set; }
		[Column(Length=50)]    public string   Provider        { get; set; }
		[Column]               public long     Ticks           { get; set; }
		[Column]               public TimeSpan Time            { get; set; }
		[Column]               public DateTime CreatedOn       { get; set; }
	}
}
