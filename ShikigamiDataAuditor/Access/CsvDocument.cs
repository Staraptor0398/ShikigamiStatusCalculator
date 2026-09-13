using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShikigamiDataAuditor.Access
{
	internal class CsvDocument
	{
		public IReadOnlyList<string> Headers { get; private set; }
		public IReadOnlyList<IReadOnlyDictionary<string, string>> Rows { get; private set; }

		private CsvDocument()
		{
			Headers = new List<string>();
			Rows = new List<IReadOnlyDictionary<string, string>>();
		}

		public static CsvDocument Load(string filePath, IReadOnlyList<string> expectedHeaders)
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("CSV file was not found.", filePath);
			}

			string[] lines = File.ReadAllLines(filePath, new UTF8Encoding(true));

			if (lines.Length == 0)
			{
				throw new InvalidDataException("CSV file is empty: " + filePath);
			}

			List<string> headers = parseLine(lines[0]);

			if (headers.Count > 0)
			{
				headers[0] = headers[0].TrimStart('\uFEFF');
			}

			validateHeaders(headers, expectedHeaders, filePath);

			List<IReadOnlyDictionary<string, string>> rows = new List<IReadOnlyDictionary<string, string>>();

			for (int index = 1; index < lines.Length; index++)
			{
				if (string.IsNullOrWhiteSpace(lines[index]))
				{
					continue;
				}

				List<string> values = parseLine(lines[index]);

				if (values.Count != headers.Count)
				{
					throw new InvalidDataException($"CSV column count is invalid. File={filePath}, Line={index + 1}, Expected={headers.Count}, Actual={values.Count}");
				}

				Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

				for (int column = 0; column < headers.Count; column++)
				{
					row.Add(headers[column], values[column]);
				}

				rows.Add(row);
			}

			return new CsvDocument
			{
				Headers = headers,
				Rows = rows
			};
		}

		private static void validateHeaders(IReadOnlyList<string> actual, IReadOnlyList<string> expected, string filePath)
		{
			if (actual.Count != expected.Count)
			{
				throw new InvalidDataException("CSV header count is invalid: " + filePath);
			}

			for (int index = 0; index < expected.Count; index++)
			{
				if (!string.Equals(actual[index], expected[index], StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidDataException($"CSV header is invalid. File={filePath}, Column={index + 1}, Expected={expected[index]}, Actual={actual[index]}");
				}
			}
		}

		private static List<string> parseLine(string line)
		{
			List<string> values = new List<string>();
			StringBuilder value = new StringBuilder();
			bool quoted = false;

			for (int index = 0; index < line.Length; index++)
			{
				char character = line[index];

				if (character == '"')
				{
					if (quoted && index + 1 < line.Length && line[index + 1] == '"')
					{
						value.Append('"');
						index++;
					}
					else
					{
						quoted = !quoted;
					}
				}
				else if (character == ',' && !quoted)
				{
					values.Add(value.ToString());
					value.Clear();
				}
				else
				{
					value.Append(character);
				}
			}

			if (quoted)
			{
				throw new InvalidDataException("CSV quote is not closed.");
			}

			values.Add(value.ToString());

			return values;
		}
	}
}
