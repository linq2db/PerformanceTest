using System;

namespace ConsoleRunner
{
	class Program
	{
		static int Main(string[] args)
		{
#if NET481
			return Tests.LinqTestRunner.Run("NET 4.8.2", true, args);
#elif NET10_0
			return Tests.LinqTestRunner.Run("NET 10.0", false, args);
#else
#error Unknown platform.
#endif
		}
	}
}
