using System;

namespace Tests.DataModel
{
#if NETCOREAPP2_0
#else
	[System.Data.Linq.Mapping.Table]
#endif
	[LinqToDB.Mapping.Table]
	public class NarrowLong
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
	}
}
