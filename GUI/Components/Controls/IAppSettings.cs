using System;

namespace PerformanceTest.Components.Controls
{
	public interface IAppSettings
	{
		object? GetValue(string setting, string? defaultValue);
		object? GetValue(string setting);
		void    SetValue(string setting, object? value);

		void   Save();
	}
}
