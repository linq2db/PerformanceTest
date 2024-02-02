﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CodeJam.Collections;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Mapping;

namespace Tests.L2DB
{
	using DataModel;

	public class L2DBContext : DataConnection
	{
		static DataOptions _options = new DataOptions()
			.UseSqlServer(GetConnectionString("Test"), SqlServerVersion.v2019, SqlServerProvider.MicrosoftDataSqlClient)
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
//			if (trackChanges)
//				AddInterceptor(OnEntityCreated = new ObjectIdentityTracker().EntityCreated);
		}

		public ITable<Narrow>     Narrows     => this.GetTable<Narrow>();
		public ITable<NarrowLong> NarrowLongs => this.GetTable<NarrowLong>();
		public ITable<WideLong>   WideLongs   => this.GetTable<WideLong>();
	}

	class ObjectIdentityTracker
	{
		interface IObjectStore
		{
			void StoreEntity(EntityCreatedEventArgs args);
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

			public void StoreEntity(EntityCreatedEventArgs args)
			{
				var entity = (T)args.Entity;

				if (_dic.TryGetValue(entity, out var e))
					args.Entity = e;
				else
					_dic.Add(entity, entity);
			}
		}

		readonly Dictionary<Type,IObjectStore> _entityDic = new Dictionary<Type,IObjectStore>();

		public void EntityCreated(EntityCreatedEventArgs args)
		{
			var entity = args.Entity;
			var type   = entity.GetType();

			if (!_entityDic.TryGetValue(type, out var store))
			{
				store = (IObjectStore)Activator.CreateInstance(typeof(ObjectStore<>).MakeGenericType(type));
				_entityDic.Add(type, store);
			}

			store.StoreEntity(args);
		}

		/*
		static readonly IEqualityComparer<NarrowLong> _comparer = ComparerBuilder<NarrowLong>.GetEqualityComparer(
			ta => ta.Members.Where(m => m.MemberInfo.GetCustomAttribute<PrimaryKeyAttribute>() != null));

		readonly Dictionary<NarrowLong,NarrowLong> _nlDic = new Dictionary<NarrowLong,NarrowLong>(10000, _comparer);

		public void EntityCreated1(EntityCreatedEventArgs args)
		{
			if (args.Entity is NarrowLong nl)
			{
				if (!_nlDic.TryGetValue(nl, out var n))
					_nlDic.Add(nl, nl);
				else
					args.Entity = n;
			}
		}
		*/
	}

	internal class EntityCreatedEventArgs
	{
		public object Entity;
	}
}
