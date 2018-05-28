using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PerformanceTest.Components.Controls
{
	public class RelayCommand : ICommand, INotifyPropertyChanged
	{
		readonly Action<object> _execute;

		public RelayCommand(Action execute, bool isEnabled = true)
		{
			_execute   = o => execute();
			_isEnabled = isEnabled;
		}

		public RelayCommand(Action<object> execute, bool isEnabled = true)
		{
			_execute   = execute;
			_isEnabled = isEnabled;
		}

		private bool _isEnabled;
		public  bool  IsEnabled
		{
			get => _isEnabled;
			set
			{
				if (value != _isEnabled)
				{
					_isEnabled = value;

					OnPropertyChanged("IsEnabled");

					if (CanExecuteChanged != null)
						CanExecuteChanged(this, EventArgs.Empty);
				}
			}
		}

		public bool CanExecute(object parameter)
		{
			return IsEnabled;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			try
			{
				_execute(parameter);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
			}
		}

		[field : NonSerialized]
		public virtual event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged(string propertyName)
		{
			var propertyChanged = PropertyChanged;

			if (propertyChanged != null)
			{
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	public class AsyncRelayCommand : RelayCommand
	{
		public AsyncRelayCommand(Func<Task> execute)
			: base(o => ExecuteAsyncTask(execute))
		{
		}

		public AsyncRelayCommand(Func<object,Task> execute)
			: base(o => ExecuteAsyncTask(execute, o))
		{
		}

		static async void ExecuteAsyncTask(Func<Task> execute)
		{
			try
			{
				await execute();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
			}
		}

		static async void ExecuteAsyncTask(Func<object,Task> execute, object o)
		{
			try
			{
				await execute(o);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
			}
		}
	}
}
