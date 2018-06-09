﻿using System;
using System.Diagnostics;

namespace Tests.Tests
{
	interface ILinqQueryTests : ITests
	{
		bool   SimpleLinqQuery    (Stopwatch watch, int repeatCount, int takeCount);
		bool   ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount);
		bool   ComplicatedLinqSlow(Stopwatch watch, int repeatCount, int takeCount, int nRows);
	}
}
