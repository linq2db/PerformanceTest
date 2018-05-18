using System;

using LinqToDB.Mapping;

namespace Tests.DataModel
{
	[Table]
	public class Narrow
	{
		[PrimaryKey]      public int ID     { get; set; }
		[Column, NotNull] public int Field1 { get; set; }
	}
}
