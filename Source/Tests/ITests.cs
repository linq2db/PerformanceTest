using System;
using System.Diagnostics;

namespace Tests.Tests
{
	interface ITests
	{
		string Name { get; }

		void   SetUp   ();
		void   TearDown();
	}
}
