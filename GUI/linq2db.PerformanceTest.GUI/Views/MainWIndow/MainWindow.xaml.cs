using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

using MahApps.Metro.Controls;

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
//				LoadAgents();
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
	}
}
