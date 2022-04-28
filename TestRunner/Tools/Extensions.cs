using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToDB.Extensions;
using LinqToDB.Mapping;
using LinqToDB.Reflection;

namespace TestRunner.Tools
{
	public static class Extensions
	{
		class ValueHolder<T>
		{
#pragma warning disable 414
			public T Value;
#pragma warning restore 414
		}

		public static StringBuilder ToDiagnosticString<T>(this IEnumerable<T> source, StringBuilder stringBuilder)
		{
			if (MappingSchema.Default.IsScalarType(typeof(T)))
				return source.Select(value => new ValueHolder<T> { Value = value }).ToDiagnosticString(stringBuilder);

			var ta         = TypeAccessor.GetAccessor<T>();
			var itemValues = new List<string[]>();
			var members    = ta.Members.Where(m => source.Any(v => m.GetValue(v) != null)).ToList();

			foreach (var item in source)
			{
				var values = new string[members.Count];

				for (var i = 0; i < members.Count; i++)
				{
					var member = members[i];
					var value  = member.GetValue(item);
					var type   = members[i].Type.ToNullableUnderlying();

					if      (value == null)            values[i] = ""; //"<NULL>";
					else if (type == typeof(decimal))  values[i] = ((decimal) value).ToString("G");
					else if (type == typeof(DateTime)) values[i] = ((DateTime)value).ToString("yyy-MM-dd hh:mm:ss");
					else if (type == typeof(TimeSpan))
					{
						values[i] = new string(((TimeSpan)value).ToString().SkipWhile(c => c == '0' || c == ':').ToArray());
						if (values[i].Length > 0 && values[i][0] == '.')
							values[i] = "0" + values[i];
					}
					else values[i] = value.ToString();
				}

				itemValues.Add(values);
			}

			stringBuilder
				.Append("Count : ").Append(itemValues.Count).AppendLine()
				;

			var lens = members.Select(m => m.Name.Length).ToArray();

			foreach (var values in itemValues)
				for (var i = 0; i < lens.Length; i++)
					lens[i] = Math.Max(lens[i], values[i].Length);

			void PrintDivider()
			{
				foreach (var len in lens)
					stringBuilder.Append("+-").Append('-', len).Append("-");
				stringBuilder.Append("+").AppendLine();
			}

			PrintDivider();

			for (var i = 0; i < lens.Length; i++)
			{
				var member = members[i];
				stringBuilder.Append("| ").Append(member.Name).Append(' ', lens[i] - member.Name.Length).Append(" ");
			}

			stringBuilder.Append("|").AppendLine();

			PrintDivider();

			foreach (var values in itemValues)
			{
				for (var i = 0; i < lens.Length; i++)
				{
					stringBuilder.Append("| ");

					var type  = members[i].Type.ToNullableUnderlying();
					var right = false;

					switch (Type.GetTypeCode(type))
					{
						case TypeCode.Byte:
						case TypeCode.DateTime:
						case TypeCode.Decimal:
						case TypeCode.Double:
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.SByte:
						case TypeCode.Single:
						case TypeCode.UInt16:
						case TypeCode.UInt32:
						case TypeCode.UInt64:
							right = true;
							break;
						default:
							if (type == typeof(TimeSpan))
								right = true;
							break;
					}

					if (right)
						stringBuilder.Append(' ', lens[i] - values[i].Length).Append(values[i]);
					else
						stringBuilder.Append(values[i]).Append(' ', lens[i] - values[i].Length);

					stringBuilder.Append(" ");
				}

				stringBuilder.Append("|").AppendLine();
			}

			PrintDivider();

			return stringBuilder;
		}

		public static StringBuilder ToDiagnosticString(this IEnumerable<IDictionary<string,object>> source, StringBuilder stringBuilder)
		{
			var itemValues = new Dictionary<string,(Type type,List<string> list)>();

			foreach (var dic in source)
			{
				var max = itemValues.Max(d => (int?)d.Value.list.Count) ?? 0;

				foreach (var item in dic)
				{
					if (item.Value != null)
					{
						var value  = item.Value;
						var type   = item.Value.GetType();

						string str;

						if (!itemValues.TryGetValue(item.Key, out var items))
						{
							itemValues.Add(item.Key, items = (type, new List<string>()));

							for (var i = 0; i < max; i++)
								items.list.Add(null);
						}

						if      (value == null)            str = ""; //"<NULL>";
						else if (type == typeof(decimal))  str = ((decimal) value).ToString("G");
						else if (type == typeof(DateTime)) str = ((DateTime)value).ToString("yyy-MM-dd hh:mm:ss");
						else if (type == typeof(TimeSpan))
						{
							str = new string(((TimeSpan)value).ToString().SkipWhile(c => c == '0' || c == ':').ToArray());
							if (str.Length > 0 && str[0] == '.')
								str = "0" + str;
						}
						else str = value.ToString();

						items.list.Add(str);
					}
				}

				foreach (var item in itemValues.Values)
					if (item.list.Count == max)
						item.list.Add("");
			}

			stringBuilder
				.Append("Count : ").Append(itemValues.Count > 0 ? itemValues.First().Value.list.Count : 0).AppendLine()
				;

			var lens = itemValues.Keys.Select(m => m.Length).ToArray();

			var k = 0;
			foreach (var values in itemValues)
			{
				lens[k] = Math.Max(lens[k], values.Value.list.Max(s => s?.Length ?? 0));
				k++;
			}

			void PrintDivider()
			{
				foreach (var len in lens)
					stringBuilder.Append("+-").Append('-', len).Append("-");
				stringBuilder.Append("+").AppendLine();
			}

			PrintDivider();

			k = 0;
			foreach (var name in itemValues.Keys)
			{
				stringBuilder.Append("| ").Append(name).Append(' ', lens[k] - name.Length).Append(" ");
				k++;
			}

			stringBuilder.Append("|").AppendLine();

			PrintDivider();

			for (var l = 0; l >= 0; l++)
			{
				k = 0;
				foreach (var values in itemValues.Values)
				{
					if (l >= values.list.Count)
					{
						l = int.MinValue;
						break;
					}

					stringBuilder.Append("| ");

					var type  = values.type.ToNullableUnderlying();
					var right = false;

					switch (Type.GetTypeCode(type))
					{
						case TypeCode.Byte:
						case TypeCode.DateTime:
						case TypeCode.Decimal:
						case TypeCode.Double:
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.SByte:
						case TypeCode.Single:
						case TypeCode.UInt16:
						case TypeCode.UInt32:
						case TypeCode.UInt64:
							right = true;
							break;
						default:
							if (type == typeof(TimeSpan))
								right = true;
							break;
					}

					var value = values.list[l];

					if (right)
						stringBuilder.Append(' ', lens[k] - (value?.Length ?? 0)).Append(value);
					else
						stringBuilder.Append(value).Append(' ', lens[k] - (value?.Length ?? 0));

					k++;
					stringBuilder.Append(" ");
				}

				if (l >= 0)
					stringBuilder.Append("|").AppendLine();
			}

			PrintDivider();

			return stringBuilder;
		}

		public static string ToDiagnosticString1<T>(this IEnumerable<T> source)
		{
			return source.ToDiagnosticString(new StringBuilder()).ToString();
		}

		public static string ToDiagnosticString(this IEnumerable<IDictionary<string,object>> source)
		{
			return source.ToDiagnosticString(new StringBuilder()).ToString();
		}
	}
}
