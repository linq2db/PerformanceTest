using System;
using System.Diagnostics;

namespace Tests
{
	interface ITests
	{
		bool GetSingleColumnFast(Stopwatch watch, int count);
		bool GetSingleColumnSlow(Stopwatch watch, int count);
	}
}
