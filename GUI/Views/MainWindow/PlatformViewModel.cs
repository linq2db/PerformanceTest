using System;
using System.Collections.ObjectModel;

namespace PerformanceTest.Views.MainWindow
{
	partial class PlatformViewModel
	{
		public PlatformViewModel()
		{
			Tests = new ObservableCollection<PlatformTestViewModel>();
		}
	}
}
