using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace Tests.EF6
{
	//using DataModel;

	public class EF6Context : DbContext
	{
		public EF6Context(bool noTracking) : base(GetConnectionString())
		{
			_noTracking = noTracking;
			Configuration.AutoDetectChangesEnabled = _noTracking;
		}

		readonly bool _noTracking;

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
			Configuration.AutoDetectChangesEnabled = _noTracking;
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
		public int ID     { get; set; }

		public int Field1 { get; set; }

		public short? ShortValue { get; set; }

		public int? IntValue { get; set; }

		public long? LongValue { get; set; }

		[StringLength(100)]
		public string StringValue { get; set; }

		[Column(TypeName = "datetime2")]
		public DateTime? DateTimeValue { get; set; }
	}
}
