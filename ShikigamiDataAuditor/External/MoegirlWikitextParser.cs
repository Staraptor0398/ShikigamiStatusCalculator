using ShikigamiDataAuditor.Model;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ShikigamiDataAuditor.External
{
	public class MoegirlWikitextParser
	{
		public IReadOnlyList<ShikigamiStatus> Parse(string wikitext, string rarity, string shikigamiName)
		{
			List<IReadOnlyList<string>> rows = new List<IReadOnlyList<string>>();

			foreach (string block in Regex.Split(wikitext ?? "", @"(?m)^\s*\|-\s*$"))
			{
				List<string> cells = new List<string>();

				foreach (string line in block.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries))
				{
					string trimmed = line.Trim();

					if (!trimmed.StartsWith("|") && !trimmed.StartsWith("!"))
					{
						continue;
					}
					if (trimmed.StartsWith("{|", System.StringComparison.Ordinal))
					{
						continue;
					}

					string cellText = trimmed.Substring(1);
					string separator = trimmed.StartsWith("!") ? "!!" : "||";

					foreach (string cell in cellText.Split(new[] { separator }, System.StringSplitOptions.None))
					{
						cells.Add(cleanCell(cell));
					}
				}

				if (cells.Count > 0)
				{
					rows.Add(cells);
				}
			}

			IReadOnlyList<ShikigamiStatus> tableStatuses = StatusTableParser.Parse(rows, rarity, shikigamiName);

			if (tableStatuses.Count > 0)
			{
				return tableStatuses;
			}

			return StatusTextParser.Parse(wikitext, rarity, shikigamiName);
		}

		private static string cleanCell(string value)
		{
			string withoutLinks = Regex.Replace(value ?? "", @"\[\[(?:[^\]|]+\|)?([^\]]+)\]\]", "$1");
			string withoutTemplates = Regex.Replace(withoutLinks, @"\{\{[^{}]*\}\}", " ");
			int attributeSeparator = withoutTemplates.LastIndexOf('|');

			return (attributeSeparator >= 0 ? withoutTemplates.Substring(attributeSeparator + 1) : withoutTemplates)
				.Replace("'''", "")
				.Replace("''", "")
				.Trim();
		}
	}
}
