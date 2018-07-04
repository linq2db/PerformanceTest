using System;
using System.Diagnostics;

namespace Tests.Tests
{
	interface IGetListTests : TestRunner.ITests
	{
		bool   GetNarrowList(Stopwatch watch, int repeatCount, int takeCount);
		bool   GetWideList  (Stopwatch watch, int repeatCount, int takeCount);
	}
}
