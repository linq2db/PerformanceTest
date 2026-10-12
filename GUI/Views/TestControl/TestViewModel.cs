using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using LinqToDB;

using TestRunner.DataModel;

namespace PerformanceTest.Views.TestControl
{
	using Components.Controls;
	using MainWindow;

	public partial class TestViewModel
	{
		public void Merge(TestViewModel test)
		{
			Methods = test.Methods;
		}

		partial void AfterMethodsChanged()
		{
			Providers = new ObservableCollection<ProviderViewModel>(
				from m in Methods
				from t in m.Times
				group t by t.Name into gr
				orderby gr.Average(tt => tt.Time.Ticks)
				select new ProviderViewModel
				{
					Name  = gr.Key,
					Color = MainWindowViewModel.ProviderBrushes?[gr.Key]
				});
		}

		static bool Confirm()
		{
			var mbResult = MessageBoxResult.Cancel;

			Application.Current.Dispatcher.Invoke(() =>
				mbResult = MessageBox.Show("Delete?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question));

			return mbResult == MessageBoxResult.Yes;
		}

		#region DeleteCommand

		public  AsyncRelayCommand  DeleteCommand => field ??= new AsyncRelayCommand(DeleteDataAsync);

		async Task DeleteDataAsync()
		{
			if (!Confirm())
				return;

			App.Root!.ViewModel.Status = "Deleting...";

			try
			{
				await using var db = new ResultDB();

				var runs =
					from r in db.TestRuns
					where r.Platform == Platform && r.Name == Name
					select r;

				var methods =
					from m in db.TestMethods
					join r in runs on m.TestRunID equals r.ID
					select m;

				var watches =
					from w in db.TestStopwatches
					join m in methods on w.TestMethodID equals m.ID
					select w;

				var infos =
					from i in db.TestRunInfos
					join r in runs on i.TestRunID equals r.ID
					select i;

				// Result databases written before run info was recorded do not have the table yet.
				await db.CreateTableAsync<TestRunInfo>(tableOptions: TableOptions.CreateIfNotExists);

				await watches.DeleteAsync();
				await methods.DeleteAsync();
				await infos.  DeleteAsync();
				await runs.   DeleteAsync();

				await App.Root.ViewModel.RefreshDataAsync();
			}
			catch (Exception ex)
			{
				Application.Current.Dispatcher.Invoke(() =>
					MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK));
			}

			App.Root.ViewModel.Status = "";
		}

		#endregion

		#region DeleteBestWorstCommand

		public  AsyncRelayCommand  DeleteBestWorstCommand => field ??= new AsyncRelayCommand(DeleteBestWorstDataAsync);

		async Task DeleteBestWorstDataAsync()
		{
			if (!Confirm())
				return;

			App.Root!.ViewModel.Status = "Deleting...";

			var dataToDelete =
			(
				from m in Methods
				from t in m.Times
				from d in t.GetBestWorst()
				select new { ID = d }
			)
			.ToList();

			try
			{
				await using var db  = new ResultDB();
				await using var tmp = await db.CreateTempTableAsync("#tmp", dataToDelete);

				var q =
					from w in db.TestStopwatches
					join t in tmp on w.ID equals t.ID
					select w;

				await q.DeleteAsync();

				await App.Root.ViewModel.RefreshDataAsync();
			}
			catch (Exception ex)
			{
				Application.Current.Dispatcher.Invoke(() =>
					MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK));
			}

			App.Root.ViewModel.Status = "";
		}

		#endregion
	}
}
