using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

namespace PerformanceTest.Components.Controls
{
	public class SettingValueExtension : MarkupExtension
	{
		public static IAppSettings AppSettings { get; set; }

		public string Setting { get; set; }
		public string Default { get; set; }

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			if (AppSettings == null)
				return null;

			var providerValue = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

			if (providerValue == null)
				throw new NotSupportedException("The IProvideValueTarget is not supported");

			var target     = (DependencyObject)  providerValue.TargetObject;
			var property   = (DependencyProperty)providerValue.TargetProperty;
			var value      = AppSettings.GetValue(Setting, Default);
			var descriptor = DependencyPropertyDescriptor.FromProperty(property, target.GetType());

			descriptor.AddValueChanged(target, (_,__) =>
			{
				var newValue  = descriptor.GetValue(target);
				var convValue = descriptor.Converter.ConvertTo(newValue, value.GetType());

				if (!Equals(convValue, AppSettings.GetValue(Setting)))
				{
					AppSettings.SetValue(Setting, convValue);
					AppSettings.Save();
				}
			});

			try
			{
				return descriptor.Converter.ConvertFrom(value);
			}
			catch (Exception ex)
			{
				ex.ToString();
			}

			var str = (value ?? Default ?? "").ToString();

			if (property.PropertyType == typeof(GridLength)) return GetGridLength(str);
			if (property.PropertyType == typeof(double))     return GetDouble    (str);

			return null;
		}

		GridLength GetGridLength(string value)
		{
			if (value.Length > 0)
			{
				var    isStar = value[value.Length - 1] == '*';

				if (double.TryParse(value.Substring(0, value.Length - 1), out var number))
					return new GridLength(number, isStar ? GridUnitType.Star : GridUnitType.Pixel);
			}

			if (value != Default)
				return GetGridLength(Default);

			return GridLength.Auto;
		}

		double GetDouble(string value)
		{
			if (double.TryParse(value, out var number))
				return number;

			if (value != Default)
				return GetDouble(Default);

			return double.NaN;
		}
	}
}
