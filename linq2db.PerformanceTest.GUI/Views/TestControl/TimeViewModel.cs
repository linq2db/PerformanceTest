using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PerformanceTest.Views.MainWindow;

namespace PerformanceTest.Views.TestControl
{
	public partial class TimeViewModel
	{
		public TimeViewModel(IEnumerable<TimeSpan> times)
		{
			var ts    = times.Select(t => t.Ticks).OrderBy(t => t).ToList();
			var count = ts.Count;

			var ticks =
				count ==  1 ? ts[0] :
				count ==  2 ? (long)ts.Average(t => t) :
				count <=  5 ? (long)ts.Skip(1).Take(count - 2).Average(t => t) :
				count <= 10 ? (long)ts.Skip(2).Take(count - 4).Average(t => t) :
				              (long)ts.Skip(count / 5).Take(count - count / 5 * 2).Average(t => t);

			Time = new TimeSpan(ticks);
		}

		partial void AfterMaxTimeChanged()
		{
			_width = (int)(300L * Time.Ticks / _maxTime.Ticks);
		}

		partial void AfterNameChanged()
		{
			Application.Current.Dispatcher.Invoke(() => Color = MainWindowViewModel.ProviderBrushes[_name]);
		}
	}
}
