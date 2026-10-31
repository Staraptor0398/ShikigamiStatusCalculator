using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SaveData.Serialization
{
	internal static class JsonObjectConverter
	{
		public static object ConvertTo(object value, Type target)
		{
			if (value == null)
			{
				if (target.IsValueType && Nullable.GetUnderlyingType(target) == null)
				{
					throw new FormatException("Null cannot be converted to " + target.Name);
				}

				return null;
			}

			if (target == typeof(string))
			{
				if (!(value is string text))
				{
					throw new FormatException("Expected JSON string.");
				}

				return text;
			}

			if (target == typeof(double))
			{
				if (!(value is double number))
				{
					throw new FormatException("Expected JSON number.");
				}

				return number;
			}

			if (target == typeof(int))
			{
				if (!(value is double number))
				{
					throw new FormatException("Expected JSON number.");
				}

				if (double.IsNaN(number) || double.IsInfinity(number) || number < int.MinValue || number > int.MaxValue || Math.Truncate(number) != number)
				{
					throw new FormatException("Invalid Int32 value.");
				}

				return (int)number;
			}

			if (target == typeof(bool))
			{
				if (!(value is bool boolean))
				{
					throw new FormatException("Expected JSON boolean.");
				}

				return boolean;
			}

			if (target.IsGenericType && target.GetGenericTypeDefinition() == typeof(List<>))
			{
				if (!(value is List<object> items))
				{
					throw new FormatException("Expected JSON array.");
				}

				Type itemType = target.GetGenericArguments()[0];
				IList list = (IList)Activator.CreateInstance(target);

				foreach (object item in items)
				{
					list.Add(ConvertTo(item, itemType));
				}

				return list;
			}

			Dictionary<string, object> data = value as Dictionary<string, object>;

			if (data == null)
			{
				throw new FormatException("Unexpected JSON value for " + target.Name);
			}

			object model = Activator.CreateInstance(target);

			foreach (PropertyInfo property in target.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (!property.CanWrite || property.GetIndexParameters().Length != 0)
				{
					continue;
				}

				if (data.TryGetValue(property.Name, out object member))
				{
					property.SetValue(model, ConvertTo(member, property.PropertyType));
				}
				else if (target.Name == "StatusSaveData" && property.Name == "Defense" && data.TryGetValue("Deffense", out object legacy))
				{
					property.SetValue(model, ConvertTo(legacy, property.PropertyType));
				}
			}

			return model;
		}
	}
}
