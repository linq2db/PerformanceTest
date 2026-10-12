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
		// The main linq2db configuration deliberately inlines parameter values and Take/Skip
		// into the SQL text. The "L2DB Linq Param" and "L2DB Compiled Param" providers run the same
		// tests with the library defaults (parameterized SQL) so the difference can be measured.
		static readonly DataOptions _options = new DataOptions()
			.UseConfiguration("Test")
			.WithOptions<LinqOptions>(o => o with
			{
//				EnableAutoFluentMapping = false,
				ParameterizeTakeSkip    = false
			})
			;

		static readonly DataOptions _defaultOptions = new DataOptions()
			.UseConfiguration("Test")
			;

		public const string SettingsDescription =
			"InlineParameters = true, ParameterizeTakeSkip = false; 'L2DB Linq Param', 'L2DB Compiled Param': library defaults";

		static L2DBContext()
		{
//			LinqToDB.Common.Configuration.Linq.EnableAutoFluentMapping = false;
		}

		public L2DBContext(bool trackChanges = false, bool inlineParameters = true)
			: base(inlineParameters ? _options : _defaultOptions)
		{
			InlineParameters = inlineParameters;
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
