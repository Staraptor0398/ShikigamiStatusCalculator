using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikigamiDataAuditor.Official
{
	public class OfficialNewsCrawler
	{
		private const string FIRST_INDEX_URL = "https://www.onmyojigame.jp/news/news/index.html";
		private const string INDEX_URL_FORMAT = "https://www.onmyojigame.jp/news/news/index_{0}.html";

		private static readonly TimeSpan INDEX_CACHE_MAX_AGE = TimeSpan.FromHours(12);

		private readonly OfficialAnnouncementClient mClient;
		private readonly OfficialNewsIndexParser mIndexParser;

		public OfficialNewsCrawler(OfficialAnnouncementClient client, OfficialNewsIndexParser indexParser)
		{
			mClient = client ?? throw new ArgumentNullException(nameof(client));
			mIndexParser = indexParser ?? throw new ArgumentNullException(nameof(indexParser));
		}

		public async Task<IReadOnlyList<string>> DiscoverUpdateUrlsAsync()
		{
			HashSet<string> urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			string firstPageHtml = await mClient.GetHtmlAsync(FIRST_INDEX_URL, INDEX_CACHE_MAX_AGE).ConfigureAwait(false);
			int pageCount = mIndexParser.ParsePageCount(firstPageHtml);

			addUrls(urls, mIndexParser.ParseUpdateUrls(firstPageHtml));

			for (int page = 2; page <= pageCount; page++)
			{
				string indexUrl = string.Format(INDEX_URL_FORMAT, page);
				string html = await mClient.GetHtmlAsync(indexUrl, INDEX_CACHE_MAX_AGE).ConfigureAwait(false);

				addUrls(urls, mIndexParser.ParseUpdateUrls(html));
			}

			return urls.OrderByDescending(url => url, StringComparer.OrdinalIgnoreCase).ToList();
		}

		private static void addUrls(ISet<string> destination, IEnumerable<string> source)
		{
			foreach (string url in source)
			{
				destination.Add(url);
			}
		}
	}
}
