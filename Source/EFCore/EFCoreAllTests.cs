using System;
using System.Diagnostics;

namespace Tests.EFCore
{
	enum TestType
	{
		AdoNet,
		Linq,
		Async,
		ChangeTracking,
		ChangeTrackingAsync,
	}

	class EFCoreAllTests : Tests.TestsBase
	{
		readonly string   _platform;
		readonly TestType _testType;

		public EFCoreAllTests(string platform, TestType testType)
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
				case TestType.Linq                : return new EFCoreLinqTests().GetNarrowList     (watch, repeatCount, takeCount);
				case TestType.Async               : return new EFCoreLinqTests().GetNarrowListAsync(watch, repeatCount, takeCount).Result;
				case TestType.ChangeTracking      : return new EFCoreLinqTests{ TrackChanges = true }.GetNarrowList     (watch, repeatCount, takeCount);
				case TestType.ChangeTrackingAsync : return new EFCoreLinqTests{ TrackChanges = true }.GetNarrowListAsync(watch, repeatCount, takeCount).Result;
			}

			throw new NotImplementedException();
		}
	}
}
