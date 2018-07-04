using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tests.Tests
{
	interface IGetListAsyncTests : TestRunner.ITests
	{
		Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount);
		Task<bool> GetWideListAsync  (Stopwatch watch, int repeatCount, int takeCount);
	}
}
