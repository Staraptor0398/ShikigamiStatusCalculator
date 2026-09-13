using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace ShikigamiDataAuditor.Official
{
	public class OfficialNewsIndexParser
	{
		private const string OFFICIAL_HOST = "www.onmyojigame.jp";

		private static readonly Uri BASE_URI = new Uri("https://www.onmyojigame.jp/");
		private static readonly Regex LINK_PATTERN = new Regex(@"<a\b[^>]*\bhref\s*=\s*[""'](?<href>[^""']+)[""'][^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex UPDATE_ARTICLE_PATH_PATTERN = new Regex(@"^/news/update/\d{4}/\d{2}/\d{2}/[^/]+\.html$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex PAGE_COUNT_PATTERN = new Regex(@"(?<current>\d+)\s*/\s*(?<total>\d+)", RegexOptions.Compiled);
		private static readonly Regex PAGE_LINK_PATTERN = new Regex(@"index_(?<page>\d+)\.html", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex TAG_PATTERN = new Regex(@"<[^>]+>", RegexOptions.Compiled);

		public IReadOnlyList<string> ParseUpdateUrls(string html)
		{
			if (html == null)
			{
				throw new ArgumentNullException(nameof(html));
			}

			HashSet<string> urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (Match match in LINK_PATTERN.Matches(html))
			{
				string href = HttpUtility.HtmlDecode(match.Groups["href"].Value).Trim();

				Uri uri;

				if (!Uri.TryCreate(BASE_URI, href, out uri))
				{
					continue;
				}

				if (!string.Equals(uri.Host, OFFICIAL_HOST, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!UPDATE_ARTICLE_PATH_PATTERN.IsMatch(uri.AbsolutePath))
				{
					continue;
				}

				urls.Add(uri.GetLeftPart(UriPartial.Path));
			}

			return urls.OrderBy(url => url, StringComparer.OrdinalIgnoreCase).ToList();
		}

		public int ParsePageCount(string html)
		{
			if (html == null)
			{
				throw new ArgumentNullException(nameof(html));
			}

			int pageCount = 1;
			string text = HttpUtility.HtmlDecode(TAG_PATTERN.Replace(html, " "));

			foreach (Match match in PAGE_COUNT_PATTERN.Matches(text))
			{
				int total;

				if (int.TryParse(match.Groups["total"].Value, out total) && total > pageCount)
				{
					pageCount = total;
				}
			}

			foreach (Match match in PAGE_LINK_PATTERN.Matches(html))
			{
				int page;

				if (int.TryParse(match.Groups["page"].Value, out page) && page > pageCount)
				{
					pageCount = page;
				}
			}

			return pageCount;
		}
	}
}
