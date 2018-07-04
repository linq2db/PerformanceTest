using System;

using LinqToDB.Mapping;

namespace TestRunner.DataModel
{
	[Table]
	public class TestResult
	{
		[PrimaryKey, Identity] public int      ID;
		[Column(Length=50)]    public string   Platform;
		[Column(Length=50)]    public string   GroupName;
		[Column(Length=50)]    public string   TestName;
		[Column]               public int      Repeat;
		[Column, Nullable]     public int?     Take;
		[Column(Length=50)]    public string   TestDescription;
		[Column(Length=50)]    public string   Provider;
		[Column]               public long     Ticks;
		[Column]               public TimeSpan Time;
	}
}
