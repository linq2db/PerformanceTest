using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LinqToDB;

namespace PerformanceTest.Views.MainWindow
{
	using Components.Controls;
	using DataModel;
	using Properties;
	using TestControl;

	partial class MainWindowViewModel
	{
		public MainWindowViewModel()
		{
			Tests = new ObservableCollection<TestViewModel>();
		}

		partial void AfterWindowStateChanged()
		{
			Settings.Default.Main_WindowState = WindowState.ToString();
		}

		private AsyncRelayCommand _refreshCommand;
		public  AsyncRelayCommand  RefreshCommand =>
			_refreshCommand ?? (_refreshCommand = new AsyncRelayCommand(RefreshDataAsync));

		public void RefreshData()
		{
			Task.Run(RefreshDataAsync);
		}

		async Task RefreshDataAsync()
		{
			using (var db = new PerformanceTestDB())
			{
				var results = await db.TestResults.ToListAsync();

				Application.Current.Dispatcher.Invoke(()=>
				{
					foreach (var item in results)
					{
						Tests.Add(new TestViewModel { Name = item.TestDescription });
					}
				});
			}
		}
	}
}
