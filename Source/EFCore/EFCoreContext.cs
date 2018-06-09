using System;

using Microsoft.EntityFrameworkCore;

namespace Tests.EFCore
{
	using DataModel;

	public class EFCoreContext : DbContext
	{
		public EFCoreContext(bool trackChanges)
		{
			ChangeTracker.QueryTrackingBehavior =
				trackChanges ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(GetConnectionString());
		}

		static string _connectionString;

		static string GetConnectionString()
		{
			if (_connectionString == null)
				_connectionString = LinqToDB.Data.DataConnection.GetConnectionString("Test").Replace("LinqToDB", "EFCore");
			return _connectionString;
		}

		public DbSet<Narrow>     Narrow     { get; set; }
		public DbSet<NarrowLong> NarrowLong { get; set; }
		public DbSet<WideLong>   WideLong   { get; set; }
	}
}
