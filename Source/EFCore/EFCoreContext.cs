using System;

using Microsoft.EntityFrameworkCore;

namespace Tests.EFCore
{
	using DataModel;

	public class EFCoreContext : DbContext
	{
		public EFCoreContext(bool trackChanges)
			: base(GetOptions(trackChanges))
		{
		}

		// Options are built once and shared, as EF Core recommends for applications that create many contexts;
		// configuring them in OnConfiguring would repeat that work for every context instance.
		static DbContextOptions<EFCoreContext>? _noTrackingOptions;
		static DbContextOptions<EFCoreContext>? _trackingOptions;

		static DbContextOptions<EFCoreContext> GetOptions(bool trackChanges)
		{
			if (trackChanges)
				return _trackingOptions ??= CreateOptions(QueryTrackingBehavior.TrackAll);

			return _noTrackingOptions ??= CreateOptions(QueryTrackingBehavior.NoTracking);
		}

		static DbContextOptions<EFCoreContext> CreateOptions(QueryTrackingBehavior trackingBehavior)
		{
			return new DbContextOptionsBuilder<EFCoreContext>()
				.UseSqlServer(Tests.TestsBase.GetConnectionString("EFCore"))
				.UseQueryTrackingBehavior(trackingBehavior)
				.Options;
		}

		public DbSet<Narrow>     Narrow     { get; set; } = default!;
		public DbSet<NarrowLong> NarrowLong { get; set; } = default!;
		public DbSet<WideLong>   WideLong   { get; set; } = default!;
	}
}
