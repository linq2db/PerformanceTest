using System;
using System.Data.Linq;

namespace Tests.DataModel
{
	class L2SContext : DataContext
	{
		public L2SContext()
			: base($"Server=.;Database=PerformanceTest;Trusted_Connection=True")
		{
			ObjectTrackingEnabled = false;
		}

		public ITable<Narrow>     Narrows     => GetTable<Narrow>();
		public ITable<NarrowLong> NarrowLongs => GetTable<NarrowLong>();
		public ITable<WideLong>   WideLongs   => GetTable<WideLong>();
	}
}
