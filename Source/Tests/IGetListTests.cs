using System;
using System.Diagnostics;

namespace Tests.Tests
{
	interface IGetListTests : ITests
	{
		bool   GetNarrowList(Stopwatch watch, int repeatCount, int takeCount);
		bool   GetWideList  (Stopwatch watch, int repeatCount, int takeCount);
	}
}
