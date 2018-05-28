using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PerformanceTest.Views.TestControl
{
	public partial class MethodViewModel
	{
		public MethodViewModel(IEnumerable<TimeViewModel> times)
		{
			var list = times.OrderBy(t => t.Time).ToList();
			var max  = list.Max(t => t.Time);

			foreach (var item in list)
				item.MaxTime = max;

			Times = new ObservableCollection<TimeViewModel>(list);
		}
	}
}
