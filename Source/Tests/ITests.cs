using System;
using System.Diagnostics;

namespace Tests.Tests
{
	interface ITests
	{
		string Name { get; set; }

		void   SetUp   ();
		void   TearDown();
	}
}
