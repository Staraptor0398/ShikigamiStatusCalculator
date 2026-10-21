using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SaveData.Migration
{
	internal static class JsonMigrationWriter
	{
		public static string Write(object root)
		{
			StringBuilder output = new StringBuilder();
			append(output, root);
			return output.ToString();
		}

		private static void append(StringBuilder output, object value)
		{
			if (value == null) { output.Append("null"); return; }
			if (value is string text) { writeString(output, text); return; }
			if (value is bool b) { output.Append(b ? "true" : "false"); return; }
			if (value is Dictionary<string, object> map)
			{
				output.Append('{');
				bool first = true;
				foreach (KeyValuePair<string, object> pair in map)
				{
					if (!first) output.Append(',');
					writeString(output, pair.Key);
					output.Append(':');
					append(output, pair.Value);
					first = false;
				}
				output.Append('}');
				return;
			}
			if (value is IList list)
			{
				output.Append('[');
				for (int i = 0; i < list.Count; i++)
				{
					if (i > 0) output.Append(',');
					append(output, list[i]);
				}
				output.Append(']');
				return;
			}
			output.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
		}

		private static void writeString(StringBuilder output, string value)
		{
			output.Append('"');
			foreach (char c in value)
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
