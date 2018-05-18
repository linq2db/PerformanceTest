using System;
using System.Diagnostics;

namespace Tests
{
	interface ITests
	{
		bool GetSingleColumnFast (Stopwatch watch, int repeatCount, int takeCount);
		bool GetSingleColumnSlow (Stopwatch watch, int repeatCount, int takeCount);
		bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount);
		bool GetNarrowList       (Stopwatch watch, int repeatCount, int takeCount);
	}
}
