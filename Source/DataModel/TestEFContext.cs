using System;

using Microsoft.EntityFrameworkCore;

namespace Tests.DataModel
{
	public class TestEFContext : DbContext
	{
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer($"Server=.;Database=PerformanceTest;Trusted_Connection=True");
		}

		public DbSet<Narrow>     Narrow     { get; set; }
		public DbSet<NarrowLong> NarrowLong { get; set; }
	}
}
