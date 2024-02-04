using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CodeJam.Collections;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Interceptors;
using LinqToDB.Mapping;

namespace Tests.L2DB
{
	using DataModel;
	using static global::Dapper.SqlMapper;

	public class L2DBContext : DataConnection
	{
		static DataOptions _options = new DataOptions()
			.UseSqlServer(GetConnectionString("Test"), SqlServerVersion.v2022, SqlServerProvider.MicrosoftDataSqlClient)
			.WithOptions<LinqOptions>(o => o with { EnableContextSchemaEdit = false })
			.WithOptions<LinqOptions>(o => o with { ParameterizeTakeSkip    = false })
			;

		static L2DBContext()
		{
			LinqToDB.Common.Configuration.Linq.EnableContextSchemaEdit = false;
		}

		public L2DBContext(bool trackChanges = false) : base(_options)
		{
			InlineParameters = true;
			if (trackChanges)
				AddInterceptor(new ObjectIdentityTracker());
		}

		public ITable<Narrow>     Narrows     => this.GetTable<Narrow>();
		public ITable<NarrowLong> NarrowLongs => this.GetTable<NarrowLong>();
		public ITable<WideLong>   WideLongs   => this.GetTable<WideLong>();
	}

	class ObjectIdentityTracker : IEntityServiceInterceptor
	{
		interface IObjectStore
		{
			public object StoreEntity(object entity);
		}

		class ObjectStore<T> : IObjectStore
		{
			readonly Dictionary<T,T> _dic;

			static readonly IEqualityComparer<T> _comparer = ComparerBuilder<T>.GetEqualityComparer(
				ta => ta.Members.Where(m => m.MemberInfo.GetCustomAttribute<PrimaryKeyAttribute>() != null));

			public ObjectStore()
			{
				_dic = new Dictionary<T,T>(10000, _comparer);
			}

			public object StoreEntity(object entity)
			{
				if (_dic.TryGetValue((T)entity, out var e))
					return e;

				_dic.Add((T)entity, (T)entity);

				return entity;
			}
		}

		readonly Dictionary<Type,IObjectStore> _entityDic = new();

		public object EntityCreated(EntityCreatedEventData eventData, object entity)
		{
			var type = entity.GetType();

			if (!_entityDic.TryGetValue(type, out var store))
			{
				store = (IObjectStore)Activator.CreateInstance(typeof(ObjectStore<>).MakeGenericType(type))!;
				_entityDic.Add(type, store);
			}

			return store.StoreEntity(entity);
		}

	}
}
