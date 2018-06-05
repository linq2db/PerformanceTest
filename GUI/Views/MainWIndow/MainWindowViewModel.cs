using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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
			Tests     = new ObservableCollection<TestViewModel>();
			Platforms = new ObservableCollection<PlatformViewModel>();
		}

		partial void AfterWindowStateChanged()
		{
			Settings.Default.Main_WindowState = WindowState.ToString();
		}
		bool Confirm()
		{
			var mbResult = MessageBoxResult.Cancel;

			Application.Current.Dispatcher.Invoke(() =>
				mbResult = MessageBox.Show("Delete?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question));

			return mbResult == MessageBoxResult.Yes;
		}

		#region RefreshCommand

		private AsyncRelayCommand _refreshCommand;
		public  AsyncRelayCommand  RefreshCommand =>
			_refreshCommand ?? (_refreshCommand = new AsyncRelayCommand(RefreshDataAsync));

		public void RefreshData()
		{
			Task.Run(RefreshDataAsync);
		}

		public static Dictionary<string,Brush> ProviderBrushes = new Dictionary<string,Brush>();

		public async Task RefreshDataAsync()
		{
			try
			{
				using (var db = new PerformanceTestDB())
				{
					Application.Current.Dispatcher.Invoke(() => Status = "Loading...");

					var runs    = await db.TestRuns.       ToListAsync();
					var methods = await db.TestMethods.    ToListAsync();
					var watches = await db.TestStopwatches.ToListAsync();

					var ws =
					(
						from w in watches
						group w by w.TestMethodID into g
						select new { g.Key, Items = g.ToList() }
					)
					.ToDictionary(t => t.Key, t => t.Items);

					var ms =
					(
						from m in methods
						group m by m.TestRunID into g
						select new
						{
							g.Key,
							Items = g.Select(mt => new
							{
								Method  = mt,
								Watches = ws.ContainsKey(mt.ID) ? ws[mt.ID] : new List<TestStopwatch>()
							}).ToList()
						}
					)
					.ToDictionary(t => t.Key, t => t.Items);

					var rs =
					(
						from r in runs
						group r by new { r.Platform, r.Name } into g
						//orderby g.Key.Platform, g.Key.Name
						select new { g.Key, Items = g.Select(rt => new { Run = rt, Methods = ms[rt.ID] }).ToList() }
					)
					.ToDictionary(t => t.Key, t => t.Items);

					var providers = ws.Values.SelectMany(w => w).Select(w => w.Provider).Distinct().OrderBy(p => p);

					var colors = new[]
					{
						Color.FromRgb(0xAA, 0x40, 0xFF),
						Color.FromRgb(0x63, 0x2F, 0x00),
						Color.FromRgb(0xFF, 0x76, 0xBC),

						Color.FromRgb(0x77, 0xB9, 0x00),
						Color.FromRgb(0x00, 0xC1, 0x3F),
						Color.FromRgb(0x19, 0x99, 0x00),

						Color.FromRgb(0xE1, 0xB7, 0x00),
						Color.FromRgb(0xF3, 0xB2, 0x00),
						Color.FromRgb(0xFF, 0x98, 0x1D),

						Color.FromRgb(0x1F, 0xAE, 0xFF),
						Color.FromRgb(0x56, 0xC5, 0xFF),
						Color.FromRgb(0x00, 0xD8, 0xCC),

						Color.FromRgb(0xAD, 0x10, 0x3C),
						Color.FromRgb(0xB0, 0x1E, 0x00),
						Color.FromRgb(0xC1, 0x00, 0x4F),

						Color.FromRgb(0x00, 0x82, 0x87),
						Color.FromRgb(0x00, 0xA3, 0xA3),
						Color.FromRgb(0x00, 0x6A, 0xC1),

						Color.FromRgb(0x25, 0x72, 0xEB),
						Color.FromRgb(0x46, 0x17, 0xB4),
						Color.FromRgb(0x72, 0x00, 0xAC),
						Color.FromRgb(0xFE, 0x7C, 0x22),
						Color.FromRgb(0xFF, 0x2E, 0x12),
						Color.FromRgb(0xFF, 0x1D, 0x77),
						Color.FromRgb(0x91, 0xD1, 0x00),
					};

					Application.Current.Dispatcher.Invoke(() =>
					{
						if (ProviderBrushes.Count == 0)
							foreach (var name in new[]
							{
								"AdoNet", "Dapper", "PetaPoco",
								"L2DB Sql", "L2DB Compiled", "L2DB Linq",
								"EF Core Sql", "EF Core Compiled", "EF Core Linq",
								"L2S Sql", "L2S Compiled", "L2S Linq",
								"EF6 Sql", "EF6 Compiled", "EF6 Linq",
								"BLT Sql", "BLT Compiled", "BLT Linq",
							})
							{
								ProviderBrushes[name] = new SolidColorBrush(colors[ProviderBrushes.Count]);
							}

						foreach (var item in providers)
							if (!ProviderBrushes.ContainsKey(item))
								ProviderBrushes[item] = new SolidColorBrush(colors[ProviderBrushes.Count]);
					});

					foreach (var platform in Platforms)
						foreach (var test in platform.Tests)
							test.Test = null;

					var tests = rs
						.Select(r => new TestViewModel
						{
							Platform = r.Key.Platform,
							Name     = r.Key.Name,
							Methods  = new ObservableCollection<MethodViewModel>(
								from rt in r.Value
								from m  in rt.Methods
								group new { rt, m } by new { m.Method.Name, m.Method.Repeat, m.Method.Take } into gr
								select new MethodViewModel(
									from t in gr
									from w in t.m.Watches
									group new { t, w } by w.Provider into g
									select new TimeViewModel(g.Select(tw => tw.w))
									{
										Name = g.Key,
									})
								{
									Name   = gr.Key.Name,
									Repeat = gr.Key.Repeat,
									Take   = gr.Key.Take,
								})
						})
						.ToList();

					for (var i = 0; i < tests.Count; i++)
					{
						var test = tests[i];
						var idx  = Tests
							.Select((t,n) => new { t, n} )
							.Where(t => t.t.Platform == test.Platform && t.t.Name == test.Name)
							.Select(t => (int?)t.n)
							.FirstOrDefault();

						var platform = Platforms.FirstOrDefault(p => p.Name == test.Platform);

						if (platform == null)
							Application.Current.Dispatcher.Invoke(() => Platforms.Add(platform = new PlatformViewModel { Name = test.Platform }));

						var platformTest = platform.Tests.FirstOrDefault(t => t.Name == test.Name);

						if (platformTest == null)
						{
							platformTest = new PlatformTestViewModel { Name = test.Name };
							Application.Current.Dispatcher.Invoke(() => platform.Tests.Add(platformTest));
						}

						if (idx == null)
						{
							platformTest.Test = test;
							Application.Current.Dispatcher.Invoke(() => Tests.Insert(i, test));
						}
						else
						{
							if (i != idx)
								Application.Current.Dispatcher.Invoke(() => Tests.Move(idx.Value, i));
							platformTest.Test = Tests[i];
							Tests[i].Merge(test);
						}
					}

					foreach (var platform in Platforms.ToList())
					{
						foreach (var test in platform.Tests.ToList())
							if (test.Test == null)
								Application.Current.Dispatcher.Invoke(() => platform.Tests.Remove(test));

						if (platform.Tests.Count == 0)
							Application.Current.Dispatcher.Invoke(() => Platforms.Remove(platform));
					}

					while (Tests.Count > tests.Count)
						Application.Current.Dispatcher.Invoke(() => Tests.RemoveAt(Tests.Count - 1));
				}
			}
			catch (Exception ex)
			{
				Application.Current.Dispatcher.Invoke(() =>
					MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error));
			}

			Application.Current.Dispatcher.Invoke(() => Status = "");
		}

		#endregion

		#region KillCommand

		private AsyncRelayCommand _killCommand;
		public  AsyncRelayCommand  KillCommand =>
			_killCommand ?? (_killCommand = new AsyncRelayCommand(KillDataAsync));

		async Task KillDataAsync()
		{
			if (!Confirm())
				return;

			try
			{
				using (var db = new PerformanceTestDB())
				{
					Application.Current.Dispatcher.Invoke(() => Status = "Loading...");

					await db.TestStopwatches.TruncateAsync();
					await db.TestMethods.    TruncateAsync();
					await db.TestRuns.       TruncateAsync();
				}

				await RefreshDataAsync();
			}
			catch (Exception ex)
			{
				Application.Current.Dispatcher.Invoke(() =>
					MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error));
			}

			Application.Current.Dispatcher.Invoke(() => Status = "");
		}

		#endregion
	}
}
