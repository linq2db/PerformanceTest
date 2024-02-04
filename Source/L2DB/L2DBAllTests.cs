using System;
using System.Diagnostics;

namespace Tests.L2DB
{
	enum TestType
	{
		AdoNet,
		Linq,
		Async,
		ChangeTracking,
		ChangeTrackingAsync,
	}

	class L2DBAllTests : Tests.TestsBase
	{
		readonly string   _platform;
		readonly TestType _testType;

		public L2DBAllTests(string platform, TestType testType)
		{
			_platform = platform;
			_testType = testType;
		}

		private         string? _name;
		public override string   Name
		{
			get
			{
				if (_name == null)
				{
					switch (_testType)
					{
						case TestType.AdoNet              : return $"{_platform} AdoNet";
						case TestType.Linq                : return $"{_platform} Linq";
						case TestType.Async               : return $"{_platform} Async";
						case TestType.ChangeTracking      : return $"{_platform} Change Tracking";
						case TestType.ChangeTrackingAsync : return $"{_platform} Change Tracking Async";
					}
				}

				return _name;
			}
			set => _name = value;
		}

		public bool GetList(Stopwatch watch, int repeatCount, int takeCount)
		{
			switch (_testType)
			{
				case TestType.AdoNet              : return new AdoNet.AdoNetTests().GetNarrowList(watch, repeatCount, takeCount);
				case TestType.Linq                : return new L2DBLinqTests().GetNarrowList     (watch, repeatCount, takeCount);
				case TestType.Async               : return new L2DBLinqTests().GetNarrowListAsync(watch, repeatCount, takeCount).Result;
				case TestType.ChangeTracking      : return new L2DBLinqTests{ TrackChanges = true }.GetNarrowList     (watch, repeatCount, takeCount);
				case TestType.ChangeTrackingAsync : return new L2DBLinqTests{ TrackChanges = true }.GetNarrowListAsync(watch, repeatCount, takeCount).Result;
			}

			throw new NotImplementedException();
		}
	}
}
