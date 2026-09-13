using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShikigamiDataAuditor.Official
{
	public class OfficialAnnouncementClient : IDisposable
	{
		private const int MAX_RESPONSE_BYTES = 5 * 1024 * 1024;
		private const int MAX_RETRY_COUNT = 4;
		private const int INITIAL_RETRY_DELAY_SECONDS = 2;

		private readonly HttpClient mHttpClient;
		private readonly TimeSpan mRequestInterval;
		private readonly OfficialAnnouncementCache mCache;
		private readonly bool mRefreshCache;
		private DateTime mLastRequestAt;

		public OfficialAnnouncementClient(TimeSpan timeout, TimeSpan requestInterval, OfficialAnnouncementCache cache, bool refreshCache)
		{
			mHttpClient = new HttpClient
			{
				Timeout = timeout
			};

			mHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ShikigamiDataAuditor/1.0");
			mHttpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml");
			mHttpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ja-JP,ja;q=0.9,en;q=0.8");

			mRequestInterval = requestInterval;
			mCache = cache ?? throw new ArgumentNullException(nameof(cache));
			mRefreshCache = refreshCache;
		}

		public Task<string> GetHtmlAsync(string sourceUrl)
		{
			return GetHtmlAsync(sourceUrl, null);
		}

		public async Task<string> GetHtmlAsync(string sourceUrl, TimeSpan? cacheMaxAge)
		{
			if (string.IsNullOrWhiteSpace(sourceUrl))
			{
				throw new ArgumentException("Source URL is empty.", nameof(sourceUrl));
			}

			if (!mRefreshCache)
			{
				string cachedHtml;

				if (mCache.TryRead(sourceUrl, cacheMaxAge, out cachedHtml))
				{
					return cachedHtml;
				}
			}

			string html = await getRemoteHtmlAsync(sourceUrl).ConfigureAwait(false);

			mCache.Write(sourceUrl, html);

			return html;
		}

		public void Dispose()
		{
			mHttpClient.Dispose();
		}

		private async Task<string> getRemoteHtmlAsync(string sourceUrl)
		{
			for (int retryCount = 0; retryCount <= MAX_RETRY_COUNT; retryCount++)
			{
				await waitForRequestIntervalAsync().ConfigureAwait(false);

				using (HttpResponseMessage response = await mHttpClient.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
				{
					mLastRequestAt = DateTime.UtcNow;

					if (isRetryableStatusCode(response.StatusCode))
					{
						if (retryCount >= MAX_RETRY_COUNT)
						{
							response.EnsureSuccessStatusCode();
						}

						TimeSpan retryDelay = getRetryDelay(response, retryCount);

						Console.WriteLine("[RETRY] " + sourceUrl + " HTTP " + (int)response.StatusCode + " - waiting " + retryDelay.TotalSeconds + "s");

						await Task.Delay(retryDelay).ConfigureAwait(false);
						continue;
					}

					response.EnsureSuccessStatusCode();

					byte[] bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

					if (bytes.Length > MAX_RESPONSE_BYTES)
					{
						throw new InvalidOperationException("Official announcement response exceeded the size limit: " + sourceUrl);
					}

					return System.Text.Encoding.UTF8.GetString(bytes);
				}
			}

			throw new InvalidOperationException("Failed to retrieve official announcement: " + sourceUrl);
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

			return statusCode == HttpStatusCode.Forbidden || code == 429 || code >= 500;
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
	}
}
