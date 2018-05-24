using System;

using LinqToDB.Mapping;

namespace Tests.DataModel
{
	[Table()]
	public class TestResult
	{
		[PrimaryKey, Identity] public int      ID;
		[Column(Length=20)]    public string   Platform;
		[Column(Length=20)]    public string   GroupName;
		[Column(Length=20)]    public string   TestName;
		[Column]               public int      Repeat;
		[Column, Nullable]     public int?     Take;
		[Column(Length=50)]    public string   TestDescription;
		[Column(Length=50)]    public string   Provider;
		[Column]               public long     Ticks;
		[Column]               public TimeSpan Time;
	}
}
