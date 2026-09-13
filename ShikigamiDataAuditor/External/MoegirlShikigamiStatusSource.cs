using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShikigamiDataAuditor.External
{
	public class MoegirlShikigamiStatusSource : IShikigamiStatusSource
	{
		private readonly MediaWikiClient mClient;
		private readonly MoegirlHtmlParser mHtmlParser;
		private readonly ExternalStatusCache mCache;
		private readonly bool mNoNetwork;
		private readonly TimeSpan mCacheMaxAge;

		public MoegirlShikigamiStatusSource(MediaWikiClient client, MoegirlHtmlParser htmlParser, ExternalStatusCache cache, bool noNetwork, TimeSpan cacheMaxAge)
		{
			mClient = client;
			mHtmlParser = htmlParser;
			mCache = cache;
			mNoNetwork = noNetwork;
			mCacheMaxAge = cacheMaxAge;
		}

		public async Task<ExternalStatusResult> GetStatusAsync(ExternalNameMapEntry nameMapEntry)
		{
			if (string.IsNullOrWhiteSpace(nameMapEntry.PageTitle))
			{
				return failure("External page title is empty.");
			}

			ExternalCacheEntry cached;
			bool hasCache = mCache.TryLoad(nameMapEntry, out cached);

			bool cacheIsStale = !hasCache || DateTime.UtcNow - cached.FetchedAt > mCacheMaxAge;

			if (hasCache && !cacheIsStale)
			{
				return mCache.ToResult(cached, false);
			}

			if (mNoNetwork)
			{
				return hasCache ? mCache.ToResult(cached, true) : failure("Network is disabled and cache was not found.");
			}

			try
			{
				MediaWikiPageContent pageContent = await mClient.GetPageContentAsync(nameMapEntry.PageTitle).ConfigureAwait(false);

				IReadOnlyList<ShikigamiStatus> statuses = mHtmlParser.Parse(pageContent.Content, nameMapEntry.Rarity, nameMapEntry.ShikigamiName);

				if (!hasUsableStatus(statuses))
				{
					throw new InvalidOperationException("No status value could be parsed from the external page.");
				}

				ExternalCacheEntry cacheEntry = mCache.CreateEntry(nameMapEntry, pageContent, pageContent.SourceUrl, statuses);

				mCache.Save(nameMapEntry, cacheEntry);

				ExternalStatusResult result = mCache.ToResult(cacheEntry, false);

				result.IsCache = false;

				return result;
			}
			catch (Exception exception)
			{
				if (hasCache)
				{
					ExternalStatusResult result = mCache.ToResult(cached, true);

					result.ErrorMessage = exception.Message;

					return result;
				}

				return failure(exception.Message);
			}
		}

		private static ExternalStatusResult failure(string message)
		{
			return new ExternalStatusResult
			{
				Statuses = new List<ShikigamiStatus>(),
				Succeeded = false,
				ErrorMessage = message
			};
		}

		private static bool hasUsableStatus(IReadOnlyList<ShikigamiStatus> statuses)
		{
			foreach (ShikigamiStatus status in statuses)
			{
				if (status.StatScope != StatScope.UNKNOWN && status.Values.Count >= 3)
				{
					return true;
				}
			}

			return false;
		}
	}
}
