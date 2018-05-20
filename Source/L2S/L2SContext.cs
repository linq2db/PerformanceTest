using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace Tests.L2S
{
	[Database(Name="PerformanceTest")]
	public class L2SContext : DataContext
	{
		private static MappingSource mappingSource = new AttributeMappingSource();

		public L2SContext(bool noTracking)
			: base($"Server=.;Database=PerformanceTest;Trusted_Connection=True", mappingSource)
		{
			ObjectTrackingEnabled = !noTracking;
		}

		public Table<Narrow>     Narrows     { get => GetTable<Narrow>(); }
		public Table<NarrowLong> NarrowLongs { get => GetTable<NarrowLong>(); }
		public Table<WideLong>   WideLongs   { get => GetTable<WideLong>(); }
	}

	[Table(Name="dbo.Narrow")]
	public class Narrow : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);

		private int _ID;
		private int _Field1;

		[Column(Storage=nameof(_ID), DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int ID
		{
			get
			{
				return _ID;
			}
			set
			{
				if (_ID != value)
				{
					SendPropertyChanging();
					_ID = value;
					SendPropertyChanged(nameof(ID));
				}
			}
		}

		[Column(Storage=nameof(_Field1), DbType="Int NOT NULL")]
		public int Field1
		{
			get
			{
				return _Field1;
			}
			set
			{
				if (_Field1 != value)
				{
					SendPropertyChanging();
					_Field1 = value;
					SendPropertyChanged(nameof(Field1));
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;
		public event PropertyChangedEventHandler  PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name="dbo.NarrowLong")]
	public class NarrowLong : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);

		private int _ID;
		private int _Field1;

		[Column(Storage=nameof(_ID), DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int ID
		{
			get
			{
				return _ID;
			}
			set
			{
				if (_ID != value)
				{
					SendPropertyChanging();
					_ID = value;
					SendPropertyChanged(nameof(ID));
				}
			}
		}

		[Column(Storage=nameof(_Field1), DbType="Int NOT NULL")]
		public int Field1
		{
			get
			{
				return _Field1;
			}
			set
			{
				if (_Field1 != value)
				{
					SendPropertyChanging();
					_Field1 = value;
					SendPropertyChanged(nameof(Field1));
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;
		public event PropertyChangedEventHandler  PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Table(Name="dbo.WideLong")]
	public class WideLong : INotifyPropertyChanging, INotifyPropertyChanged
	{
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);

		private int       _ID;
		private int       _Field1;
		private short?    _ShortValue;
		private int?      _IntValue;
		private long?     _LongValue;
		private string    _StringValue;
		private DateTime? _DateTimeValue;

		[Column(Storage=nameof(_ID), DbType="Int NOT NULL", IsPrimaryKey=true)]
		public int ID
		{
			get
			{
				return _ID;
			}
			set
			{
				if (_ID != value)
				{
					SendPropertyChanging();
					_ID = value;
					SendPropertyChanged(nameof(ID));
				}
			}
		}

		[Column(Storage=nameof(_Field1), DbType="Int NOT NULL")]
		public int Field1
		{
			get
			{
				return _Field1;
			}
			set
			{
				if (_Field1 != value)
				{
					SendPropertyChanging();
					_Field1 = value;
					SendPropertyChanged(nameof(Field1));
				}
			}
		}

		[Column(Storage=nameof(_ShortValue), DbType="SmallInt", CanBeNull=true)]
		public short? ShortValue
		{
			get
			{
				return _ShortValue;
			}
			set
			{
				if (_ShortValue != value)
				{
					SendPropertyChanging();
					_ShortValue = value;
					SendPropertyChanged(nameof(ShortValue));
				}
			}
		}

		[Column(Storage=nameof(_IntValue), DbType="Int", CanBeNull=true)]
		public int? IntValue
		{
			get
			{
				return _IntValue;
			}
			set
			{
				if (_IntValue != value)
				{
					SendPropertyChanging();
					_IntValue = value;
					SendPropertyChanged(nameof(IntValue));
				}
			}
		}

		[Column(Storage=nameof(_LongValue), DbType="BigInt", CanBeNull=true)]
		public long? LongValue
		{
			get
			{
				return _LongValue;
			}
			set
			{
				if (_LongValue != value)
				{
					SendPropertyChanging();
					_LongValue = value;
					SendPropertyChanged(nameof(LongValue));
				}
			}
		}

		[Column(Storage=nameof(_StringValue), DbType="NVarChar(100)", CanBeNull=true)]
		public string StringValue
		{
			get
			{
				return _StringValue;
			}
			set
			{
				if (_StringValue != value)
				{
					SendPropertyChanging();
					_StringValue = value;
					SendPropertyChanged(nameof(StringValue));
				}
			}
		}

		[Column(Storage=nameof(_DateTimeValue), DbType="DateTime2", CanBeNull=true)]
		public DateTime? DateTimeValue
		{
			get
			{
				return _DateTimeValue;
			}
			set
			{
				if (_DateTimeValue != value)
				{
					SendPropertyChanging();
					_DateTimeValue = value;
					SendPropertyChanged(nameof(DateTimeValue));
				}
			}
		}

		public event PropertyChangingEventHandler PropertyChanging;
		public event PropertyChangedEventHandler  PropertyChanged;

		protected virtual void SendPropertyChanging()
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(this, emptyChangingEventArgs);
			}
		}

		protected virtual void SendPropertyChanged(String propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591
