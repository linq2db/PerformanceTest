using System;

using Microsoft.EntityFrameworkCore;

namespace Tests.EFCore
{
	using DataModel;

	public class EFCoreContext : DbContext
	{
		public EFCoreContext(bool noTracking)
		{
			if (noTracking)
				ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer($"Server=.;Database=PerformanceTest;Trusted_Connection=True");
		}

		public DbSet<Narrow>     Narrow     { get; set; }
		public DbSet<NarrowLong> NarrowLong { get; set; }
		public DbSet<WideLong>   WideLong   { get; set; }
	}
}
