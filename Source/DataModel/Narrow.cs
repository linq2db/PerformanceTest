using System;

namespace Tests.DataModel
{
	[LinqToDB.Mapping.Table]
	public class Narrow
	{
		[LinqToDB.Mapping.PrimaryKey]
		public int ID { get; set; }

		[LinqToDB.Mapping.Column, LinqToDB.Mapping.NotNull]
		public int Field1 { get; set; }
	}
}
