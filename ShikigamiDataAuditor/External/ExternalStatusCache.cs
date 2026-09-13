using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace ShikigamiDataAuditor.External
{
	public class ExternalStatusCache
	{
		private readonly string mCacheDirectoryPath;

		public ExternalStatusCache(string cacheDirectoryPath)
		{
			mCacheDirectoryPath = cacheDirectoryPath;
		}

		public void Save(ExternalNameMapEntry nameMapEntry, ExternalCacheEntry cacheEntry)
		{
			string filePath = getFilePath(nameMapEntry);
			Directory.CreateDirectory(Path.GetDirectoryName(filePath));
			JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
			File.WriteAllText(filePath, serializer.Serialize(cacheEntry), new UTF8Encoding(true));
		}

		public bool TryLoad(ExternalNameMapEntry nameMapEntry, out ExternalCacheEntry cacheEntry)
		{
			string filePath = getFilePath(nameMapEntry);

			if (!File.Exists(filePath))
			{
				cacheEntry = null;
				return false;
			}

			try
			{
				JavaScriptSerializer serializer = new JavaScriptSerializer
				{
					MaxJsonLength = int.MaxValue
				};

				cacheEntry = serializer.Deserialize<ExternalCacheEntry>(File.ReadAllText(filePath, Encoding.UTF8));
				return cacheEntry != null;
			}
			catch
			{
				cacheEntry = null;
				return false;
			}
		}

		public ExternalCacheEntry CreateEntry(ExternalNameMapEntry nameMapEntry, MediaWikiPageContent pageContent, string sourceUrl, IReadOnlyList<ShikigamiStatus> statuses)
		{
			return new ExternalCacheEntry
			{
				Source = nameMapEntry.Source,
				Rarity = nameMapEntry.Rarity,
				ShikigamiName = nameMapEntry.ShikigamiName,
				PageTitle = nameMapEntry.PageTitle,
				FetchedAt = DateTime.UtcNow,
				RevisionId = pageContent.RevisionId,
				SourceUrl = sourceUrl,
				RawContent = pageContent.Content,
				ContentType = pageContent.Property,
				Statuses = statuses.Select(toCachedStatus).ToList()
			};
		}

		public ExternalStatusResult ToResult(ExternalCacheEntry cacheEntry, bool stale)
		{
			return new ExternalStatusResult
			{
				Statuses = (cacheEntry.Statuses ?? new List<CachedStatus>()).Select(fromCachedStatus).ToList(),
				FetchedAt = cacheEntry.FetchedAt,
				RevisionId = cacheEntry.RevisionId,
				SourceUrl = cacheEntry.SourceUrl,
				IsCache = true,
				IsStale = stale,
				Succeeded = true
			};
		}

		private string getFilePath(ExternalNameMapEntry nameMapEntry)
		{
			string source = sanitize(nameMapEntry.Source);
			string fileName = sanitize(nameMapEntry.Rarity + "_" + nameMapEntry.ShikigamiName) + ".json";
			return Path.Combine(mCacheDirectoryPath, source, fileName);
		}

		private static string sanitize(string value)
		{
			char[] invalid = Path.GetInvalidFileNameChars();
			string sanitized = new string((value ?? "").Select(character => invalid.Contains(character) ? '_' : character).ToArray());
			return string.IsNullOrWhiteSpace(sanitized) ? "unknown" : sanitized;
		}

		private static CachedStatus toCachedStatus(ShikigamiStatus status)
		{
			return new CachedStatus
			{
				Rarity = status.Rarity,
				Name = status.Name,
				StatScope = status.StatScope.ToString(),
				Values = status.Values.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value)
			};
		}

		private static ShikigamiStatus fromCachedStatus(CachedStatus status)
		{
			StatScope scope;

			if (!Enum.TryParse(status.StatScope, out scope))
			{
				scope = StatScope.UNKNOWN;
			}

			Dictionary<StatType, double> values = new Dictionary<StatType, double>();

			foreach (KeyValuePair<string, double> pair in status.Values ?? new Dictionary<string, double>())
			{
				StatType statType;

				if (Enum.TryParse(pair.Key, out statType))
				{
					values[statType] = pair.Value;
				}
			}

			return new ShikigamiStatus
			{
				Rarity = status.Rarity,
				Name = status.Name,
				StatScope = scope,
				Values = values
			};
		}
	}
}
