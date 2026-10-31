using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace SaveData.Serialization
{
	internal static class JsonSerializer
	{
		public static string Write(object value)
		{
			StringBuilder result = new StringBuilder();

			writeValue(result, value, 0);

			return result.ToString();
		}

		private static void writeValue(StringBuilder output, object value, int depth)
		{
			if (depth > 128)
			{
				throw new InvalidOperationException("JSON nesting exceeds 128 levels.");
			}

			if (value == null)
			{
				output.Append("null");
				return;
			}

			if (value is string text)
			{
				writeString(output, text);
				return;
			}

			if (value is bool boolean)
			{
				output.Append(boolean ? "true" : "false");
				return;
			}

			if (value is int || value is long || value is double || value is float || value is decimal)
			{
				double numeric = Convert.ToDouble(value, CultureInfo.InvariantCulture);

				if (double.IsNaN(numeric) || double.IsInfinity(numeric))
				{
					throw new InvalidOperationException("JSON cannot represent NaN or Infinity.");
				}

				output.Append(Convert.ToString(value, CultureInfo.InvariantCulture));

				return;
			}

			if (value is IEnumerable sequence)
			{
				output.Append('[');

				bool first = true;

				foreach (object item in sequence)
				{
					if (!first)
					{
						output.Append(',');
					}

					writeValue(output, item, depth + 1);
					first = false;
				}

				output.Append(']');
				return;
			}

			output.Append('{');

			bool initial = true;

			foreach (PropertyInfo property in value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (!property.CanRead || property.GetIndexParameters().Length != 0)
				{
					continue;
				}

				if (!initial)
				{
					output.Append(',');
				}

				writeString(output, property.Name);
				output.Append(':');

				writeValue(output, property.GetValue(value), depth + 1);

				initial = false;
			}

			output.Append('}');
		}

		private static void writeString(StringBuilder output, string text)
		{
			output.Append('"');

			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];

				if (char.IsHighSurrogate(c))
				{
					if (i + 1 >= text.Length || !char.IsLowSurrogate(text[i + 1]))
					{
						throw new ArgumentException("String contains an unpaired high surrogate.");
					}

					output.Append(c);
					output.Append(text[++i]);
					continue;
				}

				if (char.IsLowSurrogate(c))
				{
					throw new ArgumentException("String contains an unpaired low surrogate.");
				}

				switch (c)
				{
					case '"':
						output.Append("\\\"");
						break;

					case '\\':
						output.Append("\\\\");
						break;

					case '\n':
						output.Append("\\n");
						break;

					case '\r':
						output.Append("\\r");
						break;

					case '\t':
						output.Append("\\t");
						break;

					case '\b':
						output.Append("\\b");
						break;

					case '\f':
						output.Append("\\f");
						break;

					default:
						if (c < 0x20)
						{
							output.Append("\\u" + ((int)c).ToString("x4"));
						}
						else
						{
							output.Append(c);
						}

						break;
				}
			}

			output.Append('"');
		}
	}
}
