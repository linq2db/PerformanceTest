using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PerformanceTest.Views.MainWindow
{
	using Properties;

	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			App.Root = this;
		}

		protected override void OnInitialized(EventArgs arg)
		{
			base.OnInitialized(arg);

			App.IsInDesignMode = DesignerProperties.GetIsInDesignMode(this);

			if (!App.IsInDesignMode)
			{
				ViewModel.RefreshData();
			}
		}

		void Window_Loaded(object sender, RoutedEventArgs e)
		{
			switch (Settings.Default.Main_WindowState)
			{
				case "Maximized" : ViewModel.WindowState = WindowState.Maximized; break;
				case "Minimized" : ViewModel.WindowState = WindowState.Minimized; break;
				default          : ViewModel.WindowState = WindowState.Normal;    break;
			}

			SetBinding(WindowStateProperty, new Binding("WindowState") { Mode = BindingMode.TwoWay });
		}

		void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			for (var i = 0; i < Tests.Items.Count; i++)
			{
				if (Tests.ItemContainerGenerator.ContainerFromIndex(i) is ContentPresenter container)
				{
					var childrenCount = VisualTreeHelper.GetChildrenCount(container);

					for (var j = 0; j < childrenCount; j++)
					{
						if (VisualTreeHelper.GetChild(container, j) is TestControl.TestControl test)
						{
							test.SaveButton_Click(test.SaveButton, null);
							break;
						}
					}
				}
			}
		}
	}
}
