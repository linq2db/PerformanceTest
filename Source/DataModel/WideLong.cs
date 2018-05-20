using System;

using LinqToDB.Mapping;

namespace Tests.DataModel
{
	[Table]
	public class WideLong
	{
		[PrimaryKey]
		public int ID { get; set; }

		[Column, NotNull]
		public int Field1 { get; set; }

		[Column, Nullable]
		public short? ShortValue { get; set; }

		[Column, Nullable]
		public int? IntValue { get; set; }

		[Column, Nullable]
		public long? LongValue { get; set; }

		[Column(Length=100), Nullable]
		public string StringValue { get; set; }

		[Column, Nullable]
		public DateTime? DateTimeValue { get; set; }
	}
}
