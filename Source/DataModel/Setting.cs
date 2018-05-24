using System;

using LinqToDB.Mapping;

namespace Tests.DataModel
{
	[Table]
	public class Setting
	{
		[Column(Length=20), PrimaryKey, NotNull] public string Name  { get; set; }
		[Column(Length=10)]                      public string Value { get; set; }
	}
}
