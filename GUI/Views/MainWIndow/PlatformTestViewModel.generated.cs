﻿//---------------------------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated by T4Model template for T4 (https://github.com/linq2db/linq2db).
//    Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using PerformanceTest.Views.TestControl;

namespace PerformanceTest.Views.MainWindow
{
	public partial class PlatformTestViewModel : INotifyPropertyChanged
	{
		#region Name : string

		private string _name;
		public  string  Name
		{
			get { return _name; }
			set
			{
				if (_name != value)
				{
					BeforeNameChanged(value);
					_name = value;
					AfterNameChanged();

					OnNameChanged();
				}
			}
		}

		#region INotifyPropertyChanged support

		partial void BeforeNameChanged(string newValue);
		partial void AfterNameChanged ();

		public const string NameOfName = "Name";

		private static readonly PropertyChangedEventArgs _nameChangedEventArgs = new PropertyChangedEventArgs(NameOfName);

		private void OnNameChanged()
		{
			OnPropertyChanged(_nameChangedEventArgs);
		}

		#endregion

		#endregion

		#region INotifyPropertyChanged support

#if !SILVERLIGHT
		[field : NonSerialized]
#endif
		public virtual event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			var propertyChanged = PropertyChanged;

			if (propertyChanged != null)
			{
#if SILVERLIGHT
				if (System.Windows.Deployment.Current.Dispatcher.CheckAccess())
					propertyChanged(this, new PropertyChangedEventArgs(propertyName));
				else
					System.Windows.Deployment.Current.Dispatcher.BeginInvoke(
						() =>
						{
							var pc = PropertyChanged;
							if (pc != null)
								pc(this, new PropertyChangedEventArgs(propertyName));
						});
#else
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
#endif
			}
		}

		protected void OnPropertyChanged(PropertyChangedEventArgs arg)
		{
			var propertyChanged = PropertyChanged;

			if (propertyChanged != null)
			{
#if SILVERLIGHT
				if (System.Windows.Deployment.Current.Dispatcher.CheckAccess())
					propertyChanged(this, arg);
				else
					System.Windows.Deployment.Current.Dispatcher.BeginInvoke(
						() =>
						{
							var pc = PropertyChanged;
							if (pc != null)
								pc(this, arg);
						});
#else
				propertyChanged(this, arg);
#endif
			}
		}

		#endregion
	}
}
