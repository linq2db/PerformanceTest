using System;
using System.Diagnostics;

namespace Tests.Tests
{
	/// <summary>
	/// List tests stream: every implementation enumerates all rows one by one and discards them,
	/// without collecting them into a list first.
	/// </summary>
	interface IGetListTests : TestRunner.ITests
	{
		bool   GetNarrowList(Stopwatch watch, int repeatCount, int takeCount);
		bool   GetWideList  (Stopwatch watch, int repeatCount, int takeCount);
	}
}
