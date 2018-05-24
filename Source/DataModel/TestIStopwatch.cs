using System;

using LinqToDB.Mapping;

namespace Tests.DataModel
{
	[Table()]
	public class TestStopwatch
	{
		[PrimaryKey, Identity] public int      ID;
		[Column]               public int      TestMethodID;
		[Column]               public TimeSpan Time;
		[Column]               public long     Ticks;
		[Column(Length=50)]    public string   Provider;
	}
}
