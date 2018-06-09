using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tests.Tests
{
	interface ISingleColumnAsyncTests : ITests
	{
		Task<bool> GetSingleColumnFastAsync (Stopwatch watch, int repeatCount, int takeCount);
		Task<bool> GetSingleColumnSlowAsync (Stopwatch watch, int repeatCount, int takeCount);
		Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount);
	}
}
