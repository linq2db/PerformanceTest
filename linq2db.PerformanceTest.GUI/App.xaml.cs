using System;
using System.Windows;

namespace PerformanceTest
{
	using Components.Controls;
	using Properties;
	using Views.MainWindow;

	public partial class App : Application
	{
		public App()
		{
			SettingValueExtension.AppSettings = new AppSettings();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			Settings.Default.TrySave();
			base.OnExit(e);
		}

		public static bool       IsInDesignMode;
		public static MainWindow Root;

		class AppSettings : IAppSettings
		{
			public object GetValue(string setting, string defaultValue)
			{
				return Settings.GetValue(setting, defaultValue);
			}

			public object GetValue(string setting)
			{
				var sp = Settings.Default.Properties[setting];

				if (sp == null)
					return null;

				return Settings.Default[setting];
			}

			public void SetValue(string setting, object value)
			{
				Settings.Default[setting] = value;
			}

			public void Save()
			{
				Settings.Default.DelaySave();
			}
		}
	}
}
