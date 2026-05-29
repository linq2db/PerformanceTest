using System;

using LinqToDB.Mapping;

namespace TestRunner.DataModel
{
	[Table]
	public class Setting
	{
		[Column(Length=20), PrimaryKey, NotNull] public string Name  { get; set; } = default!;
		[Column(Length=10)]                      public string Value { get; set; } = default!;
	}
}
