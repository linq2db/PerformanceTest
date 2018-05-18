using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

using LinqToDB.Extensions;
using LinqToDB.Mapping;
using LinqToDB.Reflection;

namespace Tests.Tools
{
	public static class Extensions
	{
		class ValueHolder<T>
		{
			public T Value;
		}

		public static StringBuilder ToDiagnosticString<T>(this IEnumerable<T> source, StringBuilder stringBuilder)
		{
			if (MappingSchema.Default.IsScalarType(typeof(T)))
				return source.Select(value => new ValueHolder<T> { Value = value }).ToDiagnosticString(stringBuilder);

			var ta         = TypeAccessor.GetAccessor<T>();
			var itemValues = new List<string[]>();

			foreach (var item in source)
			{
				var values = new string[ta.Members.Count];

				for (var i = 0; i < ta.Members.Count; i++)
				{
					var member = ta.Members[i];
					var value  = member.GetValue(item);
					var type   = ta.Members[i].Type.ToNullableUnderlying();

					if      (value == null)            values[i] = "<NULL>";
					else if (type == typeof(decimal))  values[i] = ((decimal) value).ToString("G");
					else if (type == typeof(DateTime)) values[i] = ((DateTime)value).ToString("yyy-MM-dd hh:mm:ss");
					else if (type == typeof(TimeSpan))
					{
						values[i] = new string(((TimeSpan)value).ToString().SkipWhile(c => c == '0' || c == ':').ToArray());
						if (values[i][0] == '.')
							values[i] = "0" + values[i];
					}
					else values[i] = value.ToString();
				}

				itemValues.Add(values);
			}

			stringBuilder
				.Append("Count : ").Append(itemValues.Count).AppendLine()
				;

			var lens = ta.Members.Select(m => m.Name.Length).ToArray();

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
				var member = ta.Members[i];
				stringBuilder.Append("| ").Append(member.Name).Append(' ', lens[i] - member.Name.Length).Append(" ");
			}

			stringBuilder.Append("|").AppendLine();

			PrintDivider();

			foreach (var values in itemValues)
			{
				for (var i = 0; i < lens.Length; i++)
				{
					stringBuilder.Append("| ");

					var type  = ta.Members[i].Type.ToNullableUnderlying();
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

			stringBuilder
				.AppendLine()
				;

			return stringBuilder;
		}

		public static string ToDiagnosticString<T>(this IEnumerable<T> source)
		{
			return source.ToDiagnosticString(new StringBuilder()).ToString();
		}
	}
}
