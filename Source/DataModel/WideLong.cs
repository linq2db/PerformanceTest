using System;

using LinqToDB.Mapping;

namespace Tests.DataModel
{
	[Table]
	public class WideLong
	{
		[PrimaryKey]                   public int       ID            { get; set; }
		[Column, NotNull]              public int       Field1        { get; set; }
		[Column, Nullable]             public byte?     ByteValue     { get; set; }
		[Column, Nullable]             public short?    ShortValue    { get; set; }
		[Column, Nullable]             public int?      IntValue      { get; set; }
		[Column, Nullable]             public long?     LongValue     { get; set; }
		[Column(Length=100), Nullable] public string?   StringValue   { get; set; }
		[Column, Nullable]             public DateTime? DateTimeValue { get; set; }
		[Column, Nullable]             public TimeSpan? TimeValue     { get; set; }
		[Column, Nullable]             public decimal?  DecimalValue  { get; set; }
		[Column, Nullable]             public double?   DoubleValue   { get; set; }
		[Column, Nullable]             public float?    FloatValue    { get; set; }
	}
}
