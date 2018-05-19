using System;

namespace Tests.DataModel
{
#if NETCOREAPP2_0
#else
	[System.Data.Linq.Mapping.Table]
#endif
	[LinqToDB.Mapping.Table]
	public class WideLong
	{
#if NETCOREAPP2_0
#else
		[System.Data.Linq.Mapping.Column(IsPrimaryKey=true)]
#endif
		[LinqToDB.Mapping.PrimaryKey]
		public int ID { get; set; }

#if NETCOREAPP2_0
#else
		[System.Data.Linq.Mapping.Column]
#endif
		[LinqToDB.Mapping.Column, LinqToDB.Mapping.NotNull]
		public int Field1 { get; set; }

#if NETCOREAPP2_0
#else
		[System.Data.Linq.Mapping.Column(CanBeNull=true)]
#endif
		[LinqToDB.Mapping.Column, LinqToDB.Mapping.Nullable]
		public short? ShortValue { get; set; }

#if NETCOREAPP2_0
#else
		[System.Data.Linq.Mapping.Column(CanBeNull=true)]
#endif
		[LinqToDB.Mapping.Column, LinqToDB.Mapping.Nullable]
		public int? IntValue { get; set; }

#if NETCOREAPP2_0
#else
		[System.Data.Linq.Mapping.Column(CanBeNull=true)]
#endif
		[LinqToDB.Mapping.Column,LinqToDB.Mapping. Nullable]
		public long? LongValue { get; set; }

#if NETCOREAPP2_0
#else
		[System.Data.Linq.Mapping.Column(CanBeNull=true)]
#endif
		[LinqToDB.Mapping.Column(Length=100), LinqToDB.Mapping.Nullable]
		public string StringValue { get; set; }

#if NETCOREAPP2_0
#else
		[System.Data.Linq.Mapping.Column(CanBeNull=true)]
#endif
		[LinqToDB.Mapping.Column, LinqToDB.Mapping.Nullable]
		public DateTime? DateTimeValue { get; set; }
	}
}
