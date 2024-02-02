﻿//---------------------------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated by T4Model template for T4 (https://github.com/linq2db/linq2db).
//    Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace PerformanceTest.Views.TestControl
{
	public partial class TimeViewModel : INotifyPropertyChanged
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

		#region Time : TimeSpan

		private TimeSpan _time;
		public  TimeSpan  Time
		{
			get { return _time; }
			set
			{
				if (_time != value)
				{
					BeforeTimeChanged(value);
					_time = value;
					AfterTimeChanged();

					OnWidthChanged();
				}
			}
		}

		#region INotifyPropertyChanged support

		partial void BeforeTimeChanged(TimeSpan newValue);
		partial void AfterTimeChanged ();

		public const string NameOfTime = "Time";

		private static readonly PropertyChangedEventArgs _timeChangedEventArgs = new PropertyChangedEventArgs(NameOfTime);

		private void OnTimeChanged()
		{
			OnPropertyChanged(_timeChangedEventArgs);
		}

		#endregion

		#endregion

		#region MaxTime : TimeSpan

		private TimeSpan _maxTime;
		public  TimeSpan  MaxTime
		{
			get { return _maxTime; }
			set
			{
				if (_maxTime != value)
				{
					BeforeMaxTimeChanged(value);
					_maxTime = value;
					AfterMaxTimeChanged();

					OnWidthChanged();
				}
			}
		}

		#region INotifyPropertyChanged support

		partial void BeforeMaxTimeChanged(TimeSpan newValue);
		partial void AfterMaxTimeChanged ();

		public const string NameOfMaxTime = "MaxTime";

		private static readonly PropertyChangedEventArgs _maxTimeChangedEventArgs = new PropertyChangedEventArgs(NameOfMaxTime);

		private void OnMaxTimeChanged()
		{
			OnPropertyChanged(_maxTimeChangedEventArgs);
		}

		#endregion

		#endregion

		#region Border : Border

		private Border _border;
		public  Border  Border
		{
			get { return _border; }
			set
			{
				if (_border != value)
				{
					BeforeBorderChanged(value);
					_border = value;
					AfterBorderChanged();

					OnBorderChanged();
				}
			}
		}

		#region INotifyPropertyChanged support

		partial void BeforeBorderChanged(Border newValue);
		partial void AfterBorderChanged ();

		public const string NameOfBorder = "Border";

		private static readonly PropertyChangedEventArgs _borderChangedEventArgs = new PropertyChangedEventArgs(NameOfBorder);

		private void OnBorderChanged()
		{
			OnPropertyChanged(_borderChangedEventArgs);
		}

		#endregion

		#endregion

		#region Width : int

		private int _width;
		public  int  Width
		{
			get { return _width; }
			set
			{
				if (_width != value)
				{
					BeforeWidthChanged(value);
					_width = value;
					AfterWidthChanged();

					OnWidthChanged();
				}
			}
		}

		#region INotifyPropertyChanged support

		partial void BeforeWidthChanged(int newValue);
		partial void AfterWidthChanged ();

		public const string NameOfWidth = "Width";

		private static readonly PropertyChangedEventArgs _widthChangedEventArgs = new PropertyChangedEventArgs(NameOfWidth);

		private void OnWidthChanged()
		{
			OnPropertyChanged(_widthChangedEventArgs);
		}

		#endregion

		#endregion

		#region Color : Brush

		private Brush _color;
		public  Brush  Color
		{
			get { return _color; }
			set
			{
				if (_color != value)
				{
					BeforeColorChanged(value);
					_color = value;
					AfterColorChanged();

					OnColorChanged();
				}
			}
		}

		#region INotifyPropertyChanged support

		partial void BeforeColorChanged(Brush newValue);
		partial void AfterColorChanged ();

		public const string NameOfColor = "Color";

		private static readonly PropertyChangedEventArgs _colorChangedEventArgs = new PropertyChangedEventArgs(NameOfColor);

		private void OnColorChanged()
		{
			OnPropertyChanged(_colorChangedEventArgs);
		}

		#endregion

		#endregion

		#region Count : int

		public int Count
		{
			get { return Watches.Length; }
		}

		#region INotifyPropertyChanged support

		public const string NameOfCount = "Count";

		private static readonly PropertyChangedEventArgs _countChangedEventArgs = new PropertyChangedEventArgs(NameOfCount);

		private void OnCountChanged()
		{
			OnPropertyChanged(_countChangedEventArgs);
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
