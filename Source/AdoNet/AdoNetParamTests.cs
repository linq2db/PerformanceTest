using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tests.AdoNet
{
	using Tests;

	/// <summary>
	/// The ADO.NET baseline for the list tests with the row limit sent as a parameter (<c>TOP (@take)</c>),
	/// the way linq2db sends <c>Take</c> by default. "AdoNet" keeps the literal limit and is the floor
	/// for the raw-SQL providers, which run the same fixed SQL text.
	/// The other tests already pass the same parameters in "AdoNet", so only the list tests are repeated here.
	/// </summary>
	class AdoNetParamTests : TestsBase, IGetListTests, IGetListAsyncTests
	{
		readonly AdoNetTests _inner = new() { ParameterizeTake = true };

		public override string Name { get; set; } = "AdoNet Param";

		public bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			return _inner.GetNarrowList(watch, repeatCount, takeCount);
		}

		public Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			return _inner.GetNarrowListAsync(watch, repeatCount, takeCount);
		}

		public bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			return _inner.GetWideList(watch, repeatCount, takeCount);
		}

		public Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			return _inner.GetWideListAsync(watch, repeatCount, takeCount);
		}
	}
}
