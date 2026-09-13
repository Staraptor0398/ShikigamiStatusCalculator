using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShikigamiDataAuditor.Official
{
	public class OfficialAdjustmentSectionParser
	{
		private static readonly Regex NUMBERED_HEADING_PATTERN = new Regex(@"^\s*(?:\d+|[０-９]+)[\.．、]\s*", RegexOptions.Compiled);
		private static readonly Regex EXPLICIT_SHIKIGAMI_HEADING_PATTERN = new Regex(@"^(?<rarity>SP|SSR|SR|R|N)\s*(?:ランク|式神)\s*[・･]?\s*[「『]?(?<name>.+?)[」』]?\s*(?=(?:の)?(?:スキル|ステータス|基礎ステータス|調整|強化|修正|問題)|に関する|関連|：|:|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public IReadOnlyList<OfficialAdjustmentSection> Parse(OfficialAnnouncement announcement, IReadOnlyList<ShikigamiStatus> appStatuses)
		{
			if (announcement == null)
			{
				throw new ArgumentNullException(nameof(announcement));
			}

			if (appStatuses == null)
			{
				throw new ArgumentNullException(nameof(appStatuses));
			}

			List<ShikigamiStatus> statuses = appStatuses
				.Where(status => !string.IsNullOrWhiteSpace(status.Name))
				.OrderByDescending(status => status.Name.Length)
				.ToList();

			List<OfficialAdjustmentSection> sections = new List<OfficialAdjustmentSection>();

			for (int index = 0; index < announcement.Lines.Count; index++)
			{
				string line = announcement.Lines[index];

				if (!NUMBERED_HEADING_PATTERN.IsMatch(line))
				{
					continue;
				}

				string heading = NUMBERED_HEADING_PATTERN.Replace(line, "").Trim();
				ShikigamiStatus status = findShikigamiStatus(heading, statuses);

				string rarity;
				string shikigamiName;

				if (status != null)
				{
					rarity = status.Rarity;
					shikigamiName = status.Name;
				}
				else if (!tryParseExplicitShikigamiHeading(heading, out rarity, out shikigamiName))
				{
					continue;
				}

				int endIndex = findSectionEndIndex(announcement.Lines, index + 1);
				List<string> lines = announcement.Lines.Skip(index).Take(endIndex - index).ToList();

				sections.Add(new OfficialAdjustmentSection
				{
					Rarity = rarity,
					ShikigamiName = shikigamiName,
					Lines = lines
				});

				index = endIndex - 1;
			}

			return sections;
		}

		private static ShikigamiStatus findShikigamiStatus(string heading, IReadOnlyList<ShikigamiStatus> statuses)
		{
			foreach (ShikigamiStatus status in statuses)
			{
				if (isShikigamiHeading(heading, status.Name))
				{
					return status;
				}
			}

			return null;
		}

		private static bool tryParseExplicitShikigamiHeading(string heading, out string rarity, out string shikigamiName)
		{
			Match match = EXPLICIT_SHIKIGAMI_HEADING_PATTERN.Match(heading);

			if (!match.Success)
			{
				rarity = null;
				shikigamiName = null;
				return false;
			}

			rarity = match.Groups["rarity"].Value.ToUpperInvariant();
			shikigamiName = match.Groups["name"].Value.Trim();

			if (string.IsNullOrWhiteSpace(shikigamiName))
			{
				rarity = null;
				shikigamiName = null;
				return false;
			}

			return true;
		}

		private static bool isShikigamiHeading(string heading, string shikigamiName)
		{
			if (string.Equals(heading, shikigamiName, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			string shikigamiMarker = "式神" + shikigamiName;

			if (heading.IndexOf(shikigamiMarker, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}

			if (!heading.StartsWith(shikigamiName, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			string suffix = heading.Substring(shikigamiName.Length);

			if (string.IsNullOrWhiteSpace(suffix))
			{
				return true;
			}

			return suffix.Contains("調整") || suffix.Contains("強化") || suffix.Contains("修正");
		}

		private static int findSectionEndIndex(IReadOnlyList<string> lines, int startIndex)
		{
			for (int index = startIndex; index < lines.Count; index++)
			{
				if (NUMBERED_HEADING_PATTERN.IsMatch(lines[index]))
				{
					return index;
				}
			}

			return lines.Count;
		}
	}
}
