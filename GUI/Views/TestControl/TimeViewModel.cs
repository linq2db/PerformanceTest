using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using TestRunner.DataModel;

namespace PerformanceTest.Views.TestControl
{
	using MainWindow;

	public partial class TimeViewModel
	{
		public TimeViewModel(IEnumerable<TestStopwatch> times)
		{
			Watches   = times.OrderBy(t => t.Ticks).ToArray();
			var ts    = Watches.Select(t => t.Ticks).ToList();
			var count = ts.Count;

			foreach (var w in Watches)
				w.Time = new TimeSpan(w.Ticks);

			var ticks =
				count ==  1 ? ts[0] :
				count ==  2 ? (long)ts.Average(t => t) :
				count <=  5 ? (long)ts.Skip(1).Take(count - 2).Average(t => t) :
				count <= 10 ? (long)ts.Skip(2).Take(count - 4).Average(t => t) :
				              (long)ts.Skip(count / 5).Take(count - count / 5 * 2).Average(t => t);

			Time = new TimeSpan(ticks);
		}

		public TestStopwatch[] Watches { get; set; }

		partial void AfterMaxTimeChanged()
		{
			_width = Math.Max(10, (int)(300L * Time.Ticks / _maxTime.Ticks));
		}

		partial void AfterNameChanged()
		{
			Application.Current.Dispatcher.Invoke(() => Color = MainWindowViewModel.ProviderBrushes[_name]);
		}

		public IEnumerable<int> GetBestWorst()
		{
			var count = Watches.Length;

			if (count <= 2)
				return new int[0];

			var n = count <=  5 ? 1 : count <= 10 ? 2 : count / 5;

			return Watches.Take(n).Concat(Watches.Skip(count - n)).Select(w => (int)w.ID);
		}
	}
}
