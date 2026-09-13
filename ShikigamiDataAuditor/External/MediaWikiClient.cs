using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShikigamiDataAuditor.External
{
	public class MediaWikiClient : IDisposable
	{
		private const int MAX_RESPONSE_BYTES = 5 * 1024 * 1024;
		private const int MAX_RETRY_COUNT = 4;
		private const int INITIAL_RETRY_DELAY_SECONDS = 2;
		private const string BASE_URL = "https://zh.moegirl.org.cn/";

		private readonly HttpClient mHttpClient;
		private readonly TimeSpan mRequestInterval;
		private DateTime mLastRequestAt;

		public MediaWikiClient(TimeSpan timeout, TimeSpan requestInterval)
		{
			mHttpClient = new HttpClient
			{
				Timeout = timeout
			};

			mHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ShikigamiDataAuditor/1.0");
			mHttpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml");
			mHttpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.9,en;q=0.8");

			mRequestInterval = requestInterval;
		}

		public async Task<MediaWikiPageContent> GetPageContentAsync(string pageTitle)
		{
			string sourceUrl = createPageUrl(pageTitle);

			for (int retryCount = 0; retryCount <= MAX_RETRY_COUNT; retryCount++)
			{
				await waitForRequestIntervalAsync().ConfigureAwait(false);

				using (HttpResponseMessage response = await mHttpClient.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
				{
					mLastRequestAt = DateTime.UtcNow;

					if (response.StatusCode == HttpStatusCode.NotFound)
					{
						throw new MediaWikiPageNotFoundException(pageTitle, sourceUrl);
					}

					if (isRetryableStatusCode(response.StatusCode))
					{
						if (retryCount >= MAX_RETRY_COUNT)
						{
							response.EnsureSuccessStatusCode();
						}

						TimeSpan retryDelay = getRetryDelay(response, retryCount);

						Console.WriteLine("[RETRY] " + pageTitle + " HTTP " + (int)response.StatusCode + " - waiting " + retryDelay.TotalSeconds + "s");

						await Task.Delay(retryDelay).ConfigureAwait(false);
						continue;
					}

					response.EnsureSuccessStatusCode();

					byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

					if (bytes.Length > MAX_RESPONSE_BYTES)
					{
						throw new InvalidOperationException("Moegirl response exceeded the size limit.");
					}

					return new MediaWikiPageContent
					{
						Content = System.Text.Encoding.UTF8.GetString(bytes),
						RevisionId = 0,
						Property = "html",
						SourceUrl = sourceUrl
					};
				}
			}

			throw new InvalidOperationException("Failed to retrieve Moegirl page: " + pageTitle);
		}

		public void Dispose()
		{
			mHttpClient.Dispose();
		}

		private async Task waitForRequestIntervalAsync()
		{
			TimeSpan elapsed = DateTime.UtcNow - mLastRequestAt;
			TimeSpan delay = mRequestInterval - elapsed;

			if (delay > TimeSpan.Zero)
			{
				await Task.Delay(delay).ConfigureAwait(false);
			}
		}

		private static bool isRetryableStatusCode(HttpStatusCode statusCode)
		{
			int code = (int)statusCode;

			return statusCode == HttpStatusCode.Forbidden ||
				code == 429 ||
				code >= 500;
		}

		private static TimeSpan getRetryDelay(HttpResponseMessage response, int retryCount)
		{
			if (response.Headers.RetryAfter != null)
			{
				if (response.Headers.RetryAfter.Delta.HasValue)
				{
					return response.Headers.RetryAfter.Delta.Value;
				}

				if (response.Headers.RetryAfter.Date.HasValue)
				{
					TimeSpan delay = response.Headers.RetryAfter.Date.Value.UtcDateTime - DateTime.UtcNow;

					if (delay > TimeSpan.Zero)
					{
						return delay;
					}
				}
			}

			double seconds = INITIAL_RETRY_DELAY_SECONDS * Math.Pow(2, retryCount);

			return TimeSpan.FromSeconds(seconds);
		}

		private static string createPageUrl(string pageTitle)
		{
			string encodedTitle = Uri.EscapeDataString(pageTitle ?? "")
				.Replace("%2F", "/")
				.Replace("%3A", ":");

			return BASE_URL + encodedTitle;
		}
	}

	public class MediaWikiPageContent
	{
		public string Content { get; set; }
		public long RevisionId { get; set; }
		public string Property { get; set; }
		public string SourceUrl { get; set; }
	}

	public class MediaWikiPageNotFoundException : Exception
	{
		public string PageTitle { get; private set; }
		public string SourceUrl { get; private set; }

		public MediaWikiPageNotFoundException(string pageTitle, string sourceUrl) : base("Moegirl page was not found: " + pageTitle)
		{
			PageTitle = pageTitle;
			SourceUrl = sourceUrl;
		}
	}
}
