using System;

namespace ConsoleRunner
{
	class Program
	{
		static void Main(string[] args)
		{
#if NET481
			Tests.LinqTestRunner.Run("NET 4.8.2", true, args);
#elif NET8_0
			Tests.LinqTestRunner.Run("NET 8.0", false, args);
#elif NET10_0
			Tests.LinqTestRunner.Run("NET 10.0", false, args);
#else
#error Unknown platform.
#endif
		}
	}
}
