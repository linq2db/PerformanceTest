using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PerformanceTest.Views.TestControl
{
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
				Methods.SelectMany(m => m.Times).Select(t => t.Name).Distinct().Select(n => new ProviderViewModel
				{
					Name = n,
					Color = MainWindowViewModel.ProviderBrushes[n]
				}));
		}
	}
}
