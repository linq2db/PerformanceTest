using System;
using System.Diagnostics;

namespace Tests.Tests
{
	interface ISingleColumnTests : TestRunner.ITests
	{
		bool   GetSingleColumnFast (Stopwatch watch, int repeatCount, int takeCount);
		bool   GetSingleColumnSlow (Stopwatch watch, int repeatCount, int takeCount);
		bool   GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount);
	}
}
