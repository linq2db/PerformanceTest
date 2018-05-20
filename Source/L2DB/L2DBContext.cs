using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeJam.Collections;

using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace Tests.L2DB
{
	using DataModel;

	public class L2DBContext : DataConnection
	{
		public readonly bool NoTracking;

		public L2DBContext(bool noTracking = true)
		{
			NoTracking = noTracking;

			if (!NoTracking)
			{
				OnEntityCreated = new ObjectIdentityTracker().EntityCreated;
			}
		}

		public ITable<Narrow>     Narrows     => GetTable<Narrow>();
		public ITable<NarrowLong> NarrowLongs => GetTable<NarrowLong>();
		public ITable<WideLong>   WideLongs   => GetTable<WideLong>();
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

			public ObjectStore()
			{
				var comparer = ComparerBuilder<T>.GetEqualityComparer(
					ta => ta.Members.Where(m => m.MemberInfo.GetCustomAttribute<PrimaryKeyAttribute>() != null));
				_dic = new Dictionary<T,T>(comparer);
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
	}
}
