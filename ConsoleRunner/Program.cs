using System;

namespace ConsoleRunner
{
	class Program
	{
		static void Main(string[] args)
		{
#if NET472
			Tests.LinqTestRunner.Run("NET 4.72", true);
#elif NETCOREAPP2_1
			Tests.LinqTestRunner.Run("Core 2.1", false);
#else
#error Unknown platform.
#endif
		}
	}
}
