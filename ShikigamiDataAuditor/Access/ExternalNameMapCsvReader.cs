using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;

namespace ShikigamiDataAuditor.Access
{
	public class ExternalNameMapCsvReader
	{
		private static readonly IReadOnlyList<string> HEADERS = new[]
		{
			"rarity", "shikigami", "source", "page_title", "enabled", "notes"
		};

		public IReadOnlyList<ExternalNameMapEntry> Read(string filePath)
		{
			CsvDocument document = CsvDocument.Load(filePath, HEADERS);

			List<ExternalNameMapEntry> entries = new List<ExternalNameMapEntry>();

			foreach (IReadOnlyDictionary<string, string> row in document.Rows)
			{
				bool enabled;

				if (!bool.TryParse(row["enabled"], out enabled))
				{
					throw new FormatException("Invalid enabled value: " + row["enabled"]);
				}

				entries.Add(new ExternalNameMapEntry
				{
					Rarity = row["rarity"].Trim(),
					ShikigamiName = row["shikigami"].Trim(),
					Source = row["source"].Trim(),
					PageTitle = row["page_title"].Trim(),
					Enabled = enabled,
					Notes = row["notes"].Trim()
				});
			}

			return entries;
		}
	}
}
