using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tests
{
	abstract class TestsBase : ITests
	{
		protected const string GetSingleColumnSql = "SELECT ID FROM Narrow WHERE ID = 1";
		protected const string GetParamSql        = "SELECT ID FROM Narrow WHERE ID = @id AND Field1 = @p";

		protected string GetNarrowListSql(int takeCount) => $"SELECT TOP ({takeCount}) ID, Field1 FROM NarrowLong";
		protected string GetWideListSql  (int takeCount) => $@"
SELECT TOP ({takeCount})
	ID,
	Field1,
	ShortValue,
	IntValue,
	LongValue,
	StringValue,
	DateTimeValue
FROM WideLong";

		public abstract string Name { get; }

		protected readonly string ConnectionString = LinqToDB.Data.DataConnection.GetConnectionString("Test");

		public virtual bool GetSingleColumnFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}

		public virtual async Task<bool> GetSingleColumnFastAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			return await Task.FromResult(false);
		}

		public virtual bool GetSingleColumnSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}

		public virtual async Task<bool> GetSingleColumnSlowAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			return await Task.FromResult(false);
		}

		public virtual bool GetSingleColumnParam(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}

		public virtual async Task<bool> GetSingleColumnParamAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			return await Task.FromResult(false);
		}

		public virtual bool GetNarrowList(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}

		public virtual async Task<bool> GetNarrowListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			return await Task.FromResult(false);
		}

		public virtual bool GetWideList(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}
		public virtual async Task<bool> GetWideListAsync(Stopwatch watch, int repeatCount, int takeCount)
		{
			return await Task.FromResult(false);
		}

		public virtual bool SimpleLinqQuery(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}

		public virtual bool ComplicatedLinqFast(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}

		public virtual bool ComplicatedLinqSlow(Stopwatch watch, int repeatCount, int takeCount)
		{
			return false;
		}
	}
}
