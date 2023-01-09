using System;

namespace ConsoleRunner
{
	class Program
	{
		static void Main(string[] args)
		{
#if NET48
			Tests.LinqTestRunner.Run("NET 4.8", true, args);
#elif NET6_0
			Tests.LinqTestRunner.Run("NET 6.0", false, args);
#elif NET7_0
			Tests.LinqTestRunner.Run("NET 7.0", false, args);
#else
#error Unknown platform.
#endif
		}
	}
}
