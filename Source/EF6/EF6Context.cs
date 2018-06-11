using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace Tests.EF6
{
	//using DataModel;

	public class EF6Context : DbContext
	{
		public EF6Context(bool TrackChanges) : base(GetConnectionString())
		{
			_trackChanges = TrackChanges;
			Configuration.AutoDetectChangesEnabled = _trackChanges;
		}

		readonly bool _trackChanges;

		static string _connectionString;

		static string GetConnectionString()
		{
			if (_connectionString == null)
				_connectionString = LinqToDB.Data.DataConnection.GetConnectionString("Test").Replace("LinqToDB", "EF6");
			return _connectionString;
		}

		public virtual DbSet<Narrow>     Narrow     { get; set; }
		public virtual DbSet<NarrowLong> NarrowLong { get; set; }
		public virtual DbSet<WideLong>   WideLong   { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			Configuration.AutoDetectChangesEnabled = _trackChanges;
		}
	}

	public class EF6ObjectContext1 : ObjectContext
	{
		public EF6ObjectContext1()
			//: base("name=Test")
			: base("Server=.;Database=PerformanceTest;Trusted_Connection=True")
		{
		}

		public virtual DbSet<Narrow>     Narrow     { get; set; }
		public virtual DbSet<NarrowLong> NarrowLong { get; set; }
		public virtual DbSet<WideLong>   WideLong   { get; set; }
	}

	[Table("Narrow")]
	public partial class Narrow
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ID { get; set; }

		public int Field1 { get; set; }
	}

	[Table("NarrowLong")]
	public partial class NarrowLong
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ID { get; set; }

		public int Field1 { get; set; }
	}

	[Table("WideLong")]
	public partial class WideLong
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int ID                  { get; set; }

		public int       Field1        { get; set; }
		public byte?     ByteValue     { get; set; }
		public short?    ShortValue    { get; set; }
		public int?      IntValue      { get; set; }
		public long?     LongValue     { get; set; }
		[StringLength(100)]
		public string    StringValue   { get; set; }
		[Column(TypeName = "datetime2")]
		public DateTime? DateTimeValue { get; set; }
		public TimeSpan? TimeValue     { get; set; }
		public decimal?  DecimalValue  { get; set; }
		public double?   DoubleValue   { get; set; }
		public float?    FloatValue    { get; set; }
	}
}
