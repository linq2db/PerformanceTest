using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tests
{
	interface ITests
	{
		string Name { get; }
		bool   GetSingleColumnFast (Stopwatch watch, int repeatCount, int takeCount);
		bool   GetSingleColumnSlow (Stopwatch watch, int repeatCount, int takeCount);
		bool   GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount);
		bool   GetNarrowList       (Stopwatch watch, int repeatCount, int takeCount);
		bool   GetWideList         (Stopwatch watch, int repeatCount, int takeCount);
		bool   SimpleLinqQuery     (Stopwatch watch, int repeatCount, int takeCount);
		bool   ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount);
		bool   ComplicatedLinqSlow (Stopwatch watch, int repeatCount, int takeCount, int nRows);

		Task<bool> GetSingleColumnFastAsync (Stopwatch watch, int repeatCount, int takeCount);
		Task<bool> GetSingleColumnSlowAsync (Stopwatch watch, int repeatCount, int takeCount);
		Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount);
		Task<bool> GetNarrowListAsync       (Stopwatch watch, int repeatCount, int takeCount);
		Task<bool> GetWideListAsync         (Stopwatch watch, int repeatCount, int takeCount);
	}
}
