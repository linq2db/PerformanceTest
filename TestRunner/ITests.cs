using System;

namespace TestRunner
{
	public interface ITests
	{
		string Name { get; set; }

		void   SetUp   ();
		void   TearDown();
	}
}
