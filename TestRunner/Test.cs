using System;
using System.Diagnostics;

namespace TestRunner
{
	public class Test<T>
	{
		public Func<T,Func<Stopwatch,int,int,bool>> Func;
		public string Name;
		public int    Repeat;
		public int?   Take;
	}
}
