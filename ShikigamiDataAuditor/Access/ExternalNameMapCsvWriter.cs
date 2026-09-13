using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShikigamiDataAuditor.Access
{
	public class ExternalNameMapCsvWriter
	{
		public void Write(string filePath, IEnumerable<ExternalNameMapEntry> entries)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("File path is empty.", nameof(filePath));
			}

			if (entries == null)
			{
				throw new ArgumentNullException(nameof(entries));
			}

			string directoryPath = Path.GetDirectoryName(filePath);

			if (!string.IsNullOrWhiteSpace(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			StringBuilder builder = new StringBuilder();

			builder.AppendLine("rarity,shikigami,source,page_title,enabled,notes");

			foreach (ExternalNameMapEntry entry in entries)
			{
				builder.Append(escape(entry.Rarity));
				builder.Append(",");
				builder.Append(escape(entry.ShikigamiName));
				builder.Append(",");
				builder.Append(escape(entry.Source));
				builder.Append(",");
				builder.Append(escape(entry.PageTitle));
				builder.Append(",");
				builder.Append(entry.Enabled ? "true" : "false");
				builder.Append(",");
				builder.Append(escape(entry.Notes));
				builder.AppendLine();
			}

			string temporaryFilePath = filePath + ".tmp";

			try
			{
				File.WriteAllText(temporaryFilePath, builder.ToString(), new UTF8Encoding(true));

				if (File.Exists(filePath))
				{
					File.Replace(temporaryFilePath, filePath, null);
				}
				else
				{
					File.Move(temporaryFilePath, filePath);
				}
			}
			finally
			{
				if (File.Exists(temporaryFilePath))
				{
					File.Delete(temporaryFilePath);
				}
			}
		}

		private static string escape(string value)
		{
			string text = value ?? "";

			if (!text.Contains(",") && !text.Contains("\"") && !text.Contains("\r") && !text.Contains("\n"))
			{
				return text;
			}

			return "\"" + text.Replace("\"", "\"\"") + "\"";
		}
	}
}
