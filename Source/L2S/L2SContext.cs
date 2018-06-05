using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Tests.L2S
{
	[Database(Name="PerformanceTest")]
	public class L2SContext : DataContext
	{
		private static MappingSource mappingSource = new AttributeMappingSource();

		public L2SContext(bool noTracking)
			: base(GetConnectionString(), mappingSource)
		{
			ObjectTrackingEnabled = !noTracking;
		}

		static string _connectionString;

		static string GetConnectionString()
		{
			if (_connectionString == null)
				_connectionString = LinqToDB.Data.DataConnection.GetConnectionString("Test").Replace("LinqToDB", "LinqToSql");
			return _connectionString;
		}

		public Table<Narrow>     Narrows     { get => GetTable<Narrow>(); }
		public Table<NarrowLong> NarrowLongs { get => GetTable<NarrowLong>(); }
		public Table<WideLong>   WideLongs   { get => GetTable<WideLong>(); }
	}

	[Table(Name="dbo.Narrow")]
	public class Narrow
	{
		[Column(DbType="Int NOT NULL", IsPrimaryKey=true)] public int ID     { get; set; }
		[Column(DbType="Int NOT NULL")]                    public int Field1 { get; set; }
	}

	[Table(Name="dbo.NarrowLong")]
	public class NarrowLong
	{
		[Column(DbType="Int NOT NULL", IsPrimaryKey=true)] public int ID     { get; set; }
		[Column(DbType="Int NOT NULL")]                    public int Field1 { get; set; }
	}

	[Table(Name="dbo.WideLong")]
	public class WideLong
	{
		[Column(DbType="Int NOT NULL", IsPrimaryKey=true)] public int       ID            { get; set; }
		[Column(DbType="Int NOT NULL")]                    public int       Field1        { get; set; }
		[Column(DbType="SmallInt",      CanBeNull=true)]   public short?    ShortValue    { get; set; }
		[Column(DbType="Int",           CanBeNull=true)]   public int?      IntValue      { get; set; }
		[Column(DbType="BigInt",        CanBeNull=true)]   public long?     LongValue     { get; set; }
		[Column(DbType="NVarChar(100)", CanBeNull=true)]   public string    StringValue   { get; set; }
		[Column(DbType="DateTime2",     CanBeNull=true)]   public DateTime? DateTimeValue { get; set; }
	}
}
