using ShikigamiDataAuditor.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ShikigamiDataAuditor.External
{
	public class MoegirlHtmlParser
	{
		private const int MIN_USABLE_STAT_COUNT = 3;

		public IReadOnlyList<ShikigamiStatus> Parse(string html, string rarity, string shikigamiName)
		{
			List<ShikigamiStatus> tableStatuses = new List<ShikigamiStatus>();

			foreach (Match tableMatch in Regex.Matches(html ?? "", @"<table\b[^>]*>(.*?)</table>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
			{
				IReadOnlyList<IReadOnlyList<string>> rows = extractRows(tableMatch.Groups[1].Value);

				if (rows.Count == 0)
				{
					continue;
				}

				IReadOnlyList<ShikigamiStatus> statuses = StatusTableParser.Parse(rows, rarity, shikigamiName);

				if (statuses.Count == 0)
				{
					continue;
				}

				tableStatuses.AddRange(statuses);
			}

			IReadOnlyList<ShikigamiStatus> mergedStatuses = mergeStatuses(tableStatuses);

			if (hasUsableStatus(mergedStatuses))
			{
				return mergedStatuses;
			}

			string withLines = Regex.Replace(html ?? "", @"</(?:tr|td|th|div|p|li|br)[^>]*>", "\n", RegexOptions.IgnoreCase);
			string withoutTags = Regex.Replace(withLines, "<[^>]+>", " ");

			return StatusTextParser.Parse(HttpUtility.HtmlDecode(withoutTags), rarity, shikigamiName);
		}

		private static IReadOnlyList<IReadOnlyList<string>> extractRows(string tableHtml)
		{
			List<IReadOnlyList<string>> rows = new List<IReadOnlyList<string>>();

			foreach (Match rowMatch in Regex.Matches(tableHtml ?? "", @"<tr\b[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
			{
				List<string> cells = Regex.Matches(
					rowMatch.Groups[1].Value,
					@"<t[dh]\b[^>]*>(.*?)</t[dh]>",
					RegexOptions.IgnoreCase | RegexOptions.Singleline)
					.Cast<Match>()
					.Select(match => cleanCell(match.Groups[1].Value))
					.ToList();

				if (cells.Count > 0)
				{
					rows.Add(cells);
				}
			}

			return rows;
		}

		private static IReadOnlyList<ShikigamiStatus> mergeStatuses(IEnumerable<ShikigamiStatus> statuses)
		{
			Dictionary<StatScope, ShikigamiStatus> mergedStatuses = new Dictionary<StatScope, ShikigamiStatus>();
			Dictionary<StatScope, Dictionary<StatType, double>> mergedValues = new Dictionary<StatScope, Dictionary<StatType, double>>();

			foreach (ShikigamiStatus status in statuses)
			{
				ShikigamiStatus target;
				Dictionary<StatType, double> values;

				if (!mergedStatuses.TryGetValue(status.StatScope, out target))
				{
					values = new Dictionary<StatType, double>();

					target = new ShikigamiStatus
					{
						Rarity = status.Rarity,
						Name = status.Name,
						StatScope = status.StatScope,
						Values = values
					};

					mergedStatuses.Add(status.StatScope, target);
					mergedValues.Add(status.StatScope, values);
				}
				else
				{
					values = mergedValues[status.StatScope];
				}

				foreach (KeyValuePair<StatType, double> pair in status.Values)
				{
					if (!values.ContainsKey(pair.Key))
					{
						values.Add(pair.Key, pair.Value);
					}
				}
			}

			return mergedStatuses.Values.ToList();
		}

		private static bool hasUsableStatus(IReadOnlyList<ShikigamiStatus> statuses)
		{
			foreach (ShikigamiStatus status in statuses)
			{
				if (status.StatScope != StatScope.UNKNOWN && status.Values != null && status.Values.Count >= MIN_USABLE_STAT_COUNT)
				{
					return true;
				}
			}

			return false;
		}

		private static string cleanCell(string value)
		{
			string withoutTags = Regex.Replace(value ?? "", "<[^>]+>", " ");

			return HttpUtility.HtmlDecode(withoutTags).Trim();
		}
	}
}
