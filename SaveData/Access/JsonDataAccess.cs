using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace SaveData.Access
{
	public static class JsonDataAccess
	{
		public static void Save<T>(string path, T data)
		{
			File.WriteAllText(path, SimpleJson.Write(data), new UTF8Encoding(false));
		}

		public static T Load<T>(string path)
		{
			return Deserialize<T>(ReadText(path));
		}

		public static string ReadText(string path, Func<string, IDisposable> track = null)
		{
			using (track?.Invoke("SaveData.Json.ReadFile"))
			{
				return File.ReadAllText(path, Encoding.UTF8);
			}
		}

		public static int GetVersion(string json)
		{
			Dictionary<string, object> root = SimpleJson.Parse(json) as Dictionary<string, object>;
			if (root == null)
			{
				throw new FormatException("Save data root must be an object.");
			}
			return root.TryGetValue("Version", out object value)
				? Convert.ToInt32(value, CultureInfo.InvariantCulture) : 1;
		}

		public static T Deserialize<T>(string json, Func<string, IDisposable> track = null)
		{
			using (track?.Invoke("SaveData.Json.Deserialize"))
			{
				return (T)SimpleJson.ConvertTo(SimpleJson.Parse(json), typeof(T));
			}
		}
	}

	// Narrow JSON parser/serializer: independent of third-party packages.
	// Handles the complete JSON syntax; mapping is restricted to SaveData POCO types.
	internal sealed class SimpleJson
	{
		private readonly string mSource;
		private int mPosition;

		private SimpleJson(string source)
		{
			mSource = source ?? throw new ArgumentNullException(nameof(source));
		}

		public static object Parse(string source)
		{
			SimpleJson reader = new SimpleJson(source);
			object value = reader.readValue(0);
			reader.skipSpace();
			if (reader.mPosition != source.Length)
			{
				throw new FormatException("Unexpected text after JSON value.");
			}
			return value;
		}

		private object readValue(int depth)
		{
			if (depth > 128)
			{
				throw new FormatException("JSON nesting exceeds 128 levels.");
			}
			skipSpace();
			if (mPosition >= mSource.Length)
			{
				throw new FormatException("Unexpected end of JSON.");
			}
			char c = mSource[mPosition];
			if (c == '{') return readObject(depth + 1);
			if (c == '[') return readArray(depth + 1);
			if (c == '"') return readString();
			if (c == 't') { require("true"); return true; }
			if (c == 'f') { require("false"); return false; }
			if (c == 'n') { require("null"); return null; }
			if (c == '-' || (c >= '0' && c <= '9')) return readNumber();
			throw new FormatException("Invalid JSON token at index " + mPosition);
		}

		private Dictionary<string, object> readObject(int depth)
		{
			expect('{');
			Dictionary<string, object> result = new Dictionary<string, object>(StringComparer.Ordinal);
			skipSpace();
			if (consume('}')) return result;
			while (true)
			{
				skipSpace();
				if (mPosition >= mSource.Length || mSource[mPosition] != '"')
					throw new FormatException("JSON object key must be a string.");
				string name = readString();
				skipSpace();
				expect(':');
				result[name] = readValue(depth);
				skipSpace();
				if (consume('}')) return result;
				expect(',');
			}
		}

		private List<object> readArray(int depth)
		{
			expect('[');
			List<object> result = new List<object>();
			skipSpace();
			if (consume(']')) return result;
			while (true)
			{
				result.Add(readValue(depth));
				skipSpace();
				if (consume(']')) return result;
				expect(',');
			}
		}

		private string readString()
		{
			expect('"');
			StringBuilder value = new StringBuilder();
			while (mPosition < mSource.Length)
			{
				char c = mSource[mPosition++];
				if (c == '"') return value.ToString();
				if (c < 0x20) throw new FormatException("Unescaped JSON control character.");
				if (c != '\\') { value.Append(c); continue; }
				if (mPosition >= mSource.Length) throw new FormatException("Incomplete JSON escape.");
				c = mSource[mPosition++];
				switch (c)
				{
					case '"': value.Append('"'); break;
					case '\\': value.Append('\\'); break;
					case '/': value.Append('/'); break;
					case 'b': value.Append('\b'); break;
					case 'f': value.Append('\f'); break;
					case 'n': value.Append('\n'); break;
					case 'r': value.Append('\r'); break;
					case 't': value.Append('\t'); break;
					case 'u':
						if (mPosition + 4 > mSource.Length) throw new FormatException("Incomplete Unicode escape.");
						string hex = mSource.Substring(mPosition, 4);
						if (!ushort.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort code))
							throw new FormatException("Invalid Unicode escape.");
						value.Append((char)code);
						mPosition += 4;
						break;
					default: throw new FormatException("Unknown JSON escape.");
				}
			}
			throw new FormatException("Unterminated JSON string.");
		}

		private object readNumber()
		{
			int begin = mPosition;
			consume('-');
			if (consume('0'))
			{
				if (mPosition < mSource.Length && char.IsDigit(mSource[mPosition]))
					throw new FormatException("Invalid leading zero.");
			}
			else
			{
				if (mPosition >= mSource.Length || mSource[mPosition] < '1' || mSource[mPosition] > '9')
					throw new FormatException("Invalid JSON number.");
				while (mPosition < mSource.Length && mSource[mPosition] >= '0' && mSource[mPosition] <= '9') mPosition++;
			}
			if (consume('.'))
			{
				int digits = mPosition;
				while (mPosition < mSource.Length && mSource[mPosition] >= '0' && mSource[mPosition] <= '9') mPosition++;
				if (digits == mPosition) throw new FormatException("Missing digits after decimal point.");
			}
			if (consume('e') || consume('E'))
			{
				if (!consume('+')) consume('-');
				int digits = mPosition;
				while (mPosition < mSource.Length && mSource[mPosition] >= '0' && mSource[mPosition] <= '9') mPosition++;
				if (digits == mPosition) throw new FormatException("Missing exponent digits.");
			}
			string number = mSource.Substring(begin, mPosition - begin);
			if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double result)
				|| double.IsInfinity(result) || double.IsNaN(result))
				throw new FormatException("Invalid JSON number.");
			return result;
		}

		private void require(string literal)
		{
			if (mPosition + literal.Length > mSource.Length ||
				string.CompareOrdinal(mSource, mPosition, literal, 0, literal.Length) != 0)
				throw new FormatException("Invalid JSON literal.");
			mPosition += literal.Length;
		}

		private void skipSpace()
		{
			while (mPosition < mSource.Length)
			{
				char c = mSource[mPosition];
				if (c != ' ' && c != '\n' && c != '\r' && c != '\t') return;
				mPosition++;
			}
		}

		private bool consume(char c)
		{
			if (mPosition >= mSource.Length || mSource[mPosition] != c) return false;
			mPosition++;
			return true;
		}

		private void expect(char c)
		{
			skipSpace();
			if (!consume(c)) throw new FormatException("Expected '" + c + "' at index " + mPosition);
		}

		public static object ConvertTo(object value, Type target)
		{
			if (value == null) return null;
			if (target == typeof(string)) return (string)value;
			if (target == typeof(double)) return Convert.ToDouble(value, CultureInfo.InvariantCulture);
			if (target == typeof(int)) return Convert.ToInt32(value, CultureInfo.InvariantCulture);
			if (target == typeof(bool)) return (bool)value;
			if (target.IsGenericType && target.GetGenericTypeDefinition() == typeof(List<>))
			{
				Type itemType = target.GetGenericArguments()[0];
				IList list = (IList)Activator.CreateInstance(target);
				foreach (object item in (List<object>)value) list.Add(ConvertTo(item, itemType));
				return list;
			}
			Dictionary<string, object> data = value as Dictionary<string, object>;
			if (data == null) throw new FormatException("Unexpected JSON value for " + target.Name);
			object model = Activator.CreateInstance(target);
			foreach (PropertyInfo property in target.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (!property.CanWrite || property.GetIndexParameters().Length != 0) continue;
				if (data.TryGetValue(property.Name, out object member))
				{
					property.SetValue(model, ConvertTo(member, property.PropertyType));
				}
				else if (target.Name == "StatusSaveData" && property.Name == "Defense"
					&& data.TryGetValue("Deffense", out object legacy))
				{
					property.SetValue(model, ConvertTo(legacy, property.PropertyType));
				}
			}
			return model;
		}

		public static string Write(object value)
		{
			StringBuilder result = new StringBuilder();
			writeValue(result, value, 0);
			return result.ToString();
		}

		private static void writeValue(StringBuilder output, object value, int depth)
		{
			if (depth > 128) throw new InvalidOperationException("JSON nesting exceeds 128 levels.");
			if (value == null) { output.Append("null"); return; }
			if (value is string text) { writeString(output, text); return; }
			if (value is bool boolean) { output.Append(boolean ? "true" : "false"); return; }
			if (value is int || value is long || value is double || value is float || value is decimal)
			{
				double numeric = Convert.ToDouble(value, CultureInfo.InvariantCulture);
				if (double.IsNaN(numeric) || double.IsInfinity(numeric))
					throw new InvalidOperationException("JSON cannot represent NaN or Infinity.");
				output.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
				return;
			}
			if (value is IEnumerable sequence)
			{
				output.Append('[');
				bool first = true;
				foreach (object item in sequence)
				{
					if (!first) output.Append(',');
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
				if (!property.CanRead || property.GetIndexParameters().Length != 0) continue;
				if (!initial) output.Append(',');
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
			foreach (char c in text)
			{
				switch (c)
				{
					case '"': output.Append("\\\""); break;
					case '\\': output.Append("\\\\"); break;
					case '\n': output.Append("\\n"); break;
					case '\r': output.Append("\\r"); break;
					case '\t': output.Append("\\t"); break;
					case '\b': output.Append("\\b"); break;
					case '\f': output.Append("\\f"); break;
					default:
						if (c < 0x20) output.Append("\\u" + ((int)c).ToString("x4"));
						else output.Append(c);
						break;
				}
			}
			output.Append('"');
		}
	}
}
