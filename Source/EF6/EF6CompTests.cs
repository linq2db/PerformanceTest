namespace Tests.EF6
{
	class EF6CompTests : TestsWithChangeTrackingBase
	{
		public override string Name => "EF6 Compiled";

		public EF6CompTests(bool noTracking) : base(noTracking)
		{
		}
	}
}
