using System;
using System.Diagnostics;

namespace TestRunner
{
	public record Test<T>(Func<T,Func<Stopwatch,int,int,bool>> Func, string Name, int Repeat, int? Take);
}
