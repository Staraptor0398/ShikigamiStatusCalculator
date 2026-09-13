using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace ShikigamiDataAuditor.Official
{
	public class OfficialAnnouncementParser
	{
		private static readonly Regex SCRIPT_PATTERN = new Regex(@"<script\b[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
		private static readonly Regex STYLE_PATTERN = new Regex(@"<style\b[^>]*>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
		private static readonly Regex BREAK_PATTERN = new Regex(@"<(?:br|/p|/div|/li|/h[1-6]|/tr|/td|/th)\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		private static readonly Regex TAG_PATTERN = new Regex(@"<[^>]+>", RegexOptions.Compiled);
		private static readonly Regex ANNOUNCEMENT_DATE_PATTERN = new Regex(@"\b(?<year>20\d{2})-(?<month>\d{1,2})-(?<day>\d{1,2})\b", RegexOptions.Compiled);
		private static readonly Regex JAPANESE_DATE_PATTERN = new Regex(@"(?<year>20\d{2})年(?<month>\d{1,2})月(?<day>\d{1,2})日", RegexOptions.Compiled);
		private static readonly Regex MONTH_DAY_PATTERN = new Regex(@"(?<month>\d{1,2})月(?<day>\d{1,2})日", RegexOptions.Compiled);
		private static readonly Regex WHITESPACE_PATTERN = new Regex(@"[\t\u3000 ]+", RegexOptions.Compiled);

		public OfficialAnnouncement Parse(string sourceUrl, string html, DateTime fallbackEffectiveDate)
		{
			if (string.IsNullOrWhiteSpace(sourceUrl))
			{
				throw new ArgumentException("Source URL is empty.", nameof(sourceUrl));
			}

			if (string.IsNullOrWhiteSpace(html))
			{
				throw new ArgumentException("Official announcement HTML is empty.", nameof(html));
			}

			IReadOnlyList<string> lines = extractLines(html);
			DateTime announcementDate = parseAnnouncementDate(lines, sourceUrl);
			DateTime effectiveDate = parseEffectiveDate(lines, announcementDate) ?? fallbackEffectiveDate;

			return new OfficialAnnouncement
			{
				SourceUrl = sourceUrl,
				AnnouncementDate = announcementDate,
				EffectiveDate = effectiveDate,
				Lines = lines
			};
		}

		private static IReadOnlyList<string> extractLines(string html)
		{
			string text = SCRIPT_PATTERN.Replace(html, "\n");
			text = STYLE_PATTERN.Replace(text, "\n");
			text = BREAK_PATTERN.Replace(text, "\n");
			text = TAG_PATTERN.Replace(text, "");
			text = WebUtility.HtmlDecode(text);
			text = text.Replace('\u00A0', ' ').Replace("\r", "\n");

			string[] rawLines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
			List<string> lines = new List<string>();

			foreach (string rawLine in rawLines)
			{
				string line = WHITESPACE_PATTERN.Replace(rawLine, " ").Trim();

				if (!string.IsNullOrWhiteSpace(line))
				{
					lines.Add(line);
				}
			}

			return lines;
		}

		private static DateTime parseAnnouncementDate(IReadOnlyList<string> lines, string sourceUrl)
		{
			foreach (string line in lines)
			{
				Match match = ANNOUNCEMENT_DATE_PATTERN.Match(line);

				if (match.Success)
				{
					return createDate(match, sourceUrl);
				}
			}

			throw new FormatException("Announcement date was not found: " + sourceUrl);
		}

		private static DateTime? parseEffectiveDate(IReadOnlyList<string> lines, DateTime announcementDate)
		{
			for (int index = 0; index < lines.Count; index++)
			{
				if (!lines[index].Contains("メンテナンス日時"))
				{
					continue;
				}

				for (int candidateIndex = index + 1; candidateIndex < lines.Count && candidateIndex <= index + 5; candidateIndex++)
				{
					Match match = JAPANESE_DATE_PATTERN.Match(lines[candidateIndex]);

					if (match.Success)
					{
						return createDate(match, "");
					}
				}
			}

			for (int index = 0; index < lines.Count; index++)
			{
				if (!lines[index].Contains("メンテナンス") && !lines[index].Contains("日時"))
				{
					continue;
				}

				for (int candidateIndex = index; candidateIndex < lines.Count && candidateIndex <= index + 5; candidateIndex++)
				{
					Match match = MONTH_DAY_PATTERN.Match(lines[candidateIndex]);

					if (match.Success)
					{
						int month = int.Parse(match.Groups["month"].Value, CultureInfo.InvariantCulture);
						int day = int.Parse(match.Groups["day"].Value, CultureInfo.InvariantCulture);

						return new DateTime(announcementDate.Year, month, day);
					}
				}
			}

			return null;
		}

		private static DateTime createDate(Match match, string sourceUrl)
		{
			int year = int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture);
			int month = int.Parse(match.Groups["month"].Value, CultureInfo.InvariantCulture);
			int day = int.Parse(match.Groups["day"].Value, CultureInfo.InvariantCulture);

			try
			{
				return new DateTime(year, month, day);
			}
			catch (ArgumentOutOfRangeException exception)
			{
				throw new FormatException("Invalid date in official announcement: " + sourceUrl, exception);
			}
		}
	}
}
