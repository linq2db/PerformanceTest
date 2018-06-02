using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
	abstract class TestsWithChangeTrackingBase : TestsBase
	{
		public readonly bool NoTracking;

		protected TestsWithChangeTrackingBase(bool noTracking)
		{
			NoTracking = noTracking;
		}
	}
}
