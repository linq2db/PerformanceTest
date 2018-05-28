using System;
using System.Configuration;
using System.Threading;

namespace PerformanceTest.Properties
{
	partial class Settings
	{
		public static object GetValue(string setting, string defaultValue)
		{
			var sp = Default.Properties[setting];

			if (sp == null)
			{
				sp = new SettingsProperty(Default.Properties["Ver"]) { Name = setting, DefaultValue = defaultValue };
				Default.Properties.Add(new SettingsProperty(sp));
			}

			var value = Default[setting];

			if (value == null || value is string && value.ToString().Length == 0)
				value = defaultValue;

			return value;
		}

		static readonly object _saveSync = new object();

		public void TrySave()
		{
			new Thread(() =>
			{
				lock (_saveSync)
					try
					{
						Save();
					}
					catch
					{
					}
			}) { Priority = ThreadPriority.BelowNormal}.Start();
		}

		private  Timer    _timer;
		readonly object   _timerSync = new object();
		private  DateTime _lastDelaySave;

		public void DelaySave()
		{
			_lastDelaySave = DateTime.Now;

			if (_timer == null)
			{
				_timer = new Timer(s =>
				{
					if ((DateTime.Now - _lastDelaySave).TotalMilliseconds > 2000 && _timer != null)
					{
						lock (_timerSync)
						{
							if (_timer != null)
							{
								_timer.Dispose();
								_timer = null;
								TrySave();
							}
						}
					}
				},
				null, 2100, 2100);
			}
		}
	}
}
