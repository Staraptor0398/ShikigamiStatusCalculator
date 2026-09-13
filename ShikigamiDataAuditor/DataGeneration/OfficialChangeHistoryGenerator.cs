using ShikigamiDataAuditor.Model;
using ShikigamiDataAuditor.Official;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShikigamiDataAuditor.DataGeneration
{
	public class OfficialChangeHistoryGenerator
	{
		private const double VALUE_TOLERANCE = 0.000001;

		private static readonly Regex SOURCE_DATE_PATTERN = new Regex(@"/(?<year>\d{4})/(?<month>\d{2})/(?<day>\d{2})/", RegexOptions.Compiled);

		private readonly OfficialAnnouncementClient mClient;
		private readonly OfficialAnnouncementParser mAnnouncementParser;
		private readonly OfficialStatChangeDiscovery mStatChangeDiscovery;
		private readonly OfficialChangeHistorySeedValidator mSeedValidator;
		private readonly OfficialNewsCrawler mNewsCrawler;

		public OfficialChangeHistoryGenerator(OfficialAnnouncementClient client, OfficialAnnouncementParser announcementParser, OfficialStatChangeDiscovery statChangeDiscovery, OfficialChangeHistorySeedValidator seedValidator)
		{
			mClient = client ?? throw new ArgumentNullException(nameof(client));
			mAnnouncementParser = announcementParser ?? throw new ArgumentNullException(nameof(announcementParser));
			mStatChangeDiscovery = statChangeDiscovery ?? throw new ArgumentNullException(nameof(statChangeDiscovery));
			mSeedValidator = seedValidator ?? throw new ArgumentNullException(nameof(seedValidator));
			mNewsCrawler = new OfficialNewsCrawler(mClient, new OfficialNewsIndexParser());
		}

		public async Task<IReadOnlyList<OfficialStatChange>> GenerateAsync(IEnumerable<OfficialStatChange> existingChanges, IEnumerable<ShikigamiStatus> appStatuses)
		{
			if (existingChanges == null)
			{
				throw new ArgumentNullException(nameof(existingChanges));
			}

			if (appStatuses == null)
			{
				throw new ArgumentNullException(nameof(appStatuses));
			}

			List<OfficialStatChange> existingChangeList = existingChanges.ToList();
			List<ShikigamiStatus> appStatusList = appStatuses.ToList();
			List<ShikigamiStatus> knownStatuses = createKnownStatuses(appStatusList, existingChangeList);
			HashSet<string> appKeys = new HashSet<string>(appStatusList.Select(status => status.GetKey()), StringComparer.OrdinalIgnoreCase);

			IReadOnlyList<string> discoveredSourceUrls = await mNewsCrawler.DiscoverUpdateUrlsAsync().ConfigureAwait(false);
			List<string> sourceUrls = createSourceUrlList(discoveredSourceUrls, existingChangeList);

			writeUrlDiscoverySummary(discoveredSourceUrls, existingChangeList);

			List<OfficialStatChange> generatedChanges = new List<OfficialStatChange>();

			foreach (string sourceUrl in sourceUrls)
			{
				List<OfficialStatChange> sourceExistingChanges = existingChangeList
					.Where(change => string.Equals(change.SourceUrl, sourceUrl, StringComparison.OrdinalIgnoreCase))
					.ToList();

				DateTime fallbackEffectiveDate = resolveFallbackEffectiveDate(sourceUrl, sourceExistingChanges);

				string html;

				try
				{
					html = await mClient.GetHtmlAsync(sourceUrl).ConfigureAwait(false);
				}
				catch (Exception exception)
				{
					throw new InvalidOperationException("Failed to fetch official announcement: " + sourceUrl, exception);
				}

				OfficialAnnouncement announcement;

				try
				{
					announcement = mAnnouncementParser.Parse(sourceUrl, html, fallbackEffectiveDate);
				}
				catch (Exception exception)
				{
					if (sourceExistingChanges.Count == 0)
					{
						continue;
					}

					throw new InvalidOperationException("Failed to parse known official announcement: " + sourceUrl, exception);
				}

				IReadOnlyList<OfficialStatChange> discoveredChanges = mStatChangeDiscovery.Discover(announcement, knownStatuses);

				if (discoveredChanges.Count == 0)
				{
					if (sourceExistingChanges.Count > 0)
					{
						throw new InvalidOperationException("Known official announcement contains no discovered stat changes: " + sourceUrl);
					}

					continue;
				}

				Console.WriteLine("[FETCH] " + sourceUrl);

				if (sourceExistingChanges.Count > 0)
				{
					OfficialChangeHistoryValidationResult validationResult = mSeedValidator.Validate(sourceExistingChanges, discoveredChanges);
					Console.WriteLine("[DISCOVERY] Existing matched: " + validationResult.MatchedCount + "/" + validationResult.SeedCount + ", New: " + validationResult.ExtraCount);
				}
				else
				{
					Console.WriteLine("[DISCOVERY] Existing matched: 0/0, New: " + discoveredChanges.Count);
				}

				foreach (OfficialStatChange discoveredChange in discoveredChanges)
				{
					OfficialStatChange existingChange = findExistingChange(discoveredChange, existingChangeList);
					OfficialStatChange generatedChange = createGeneratedChange(discoveredChange, existingChange, appKeys);

					addGeneratedChange(generatedChanges, generatedChange);
				}
			}

			List<OfficialStatChange> result = generatedChanges
				.OrderByDescending(change => change.EffectiveDate)
				.ThenBy(change => change.Rarity)
				.ThenBy(change => change.ShikigamiName)
				.ThenBy(change => change.StatType)
				.ThenBy(change => change.StatScope)
				.ToList();

			foreach (OfficialStatChange change in result)
			{
				bool isNew = findExistingChange(change, existingChangeList) == null;
				writeGeneratedChange(change, isNew);
			}

			return result;
		}

		private static List<string> createSourceUrlList(IEnumerable<string> discoveredSourceUrls, IEnumerable<OfficialStatChange> existingChanges)
		{
			HashSet<string> urls = new HashSet<string>(discoveredSourceUrls, StringComparer.OrdinalIgnoreCase);

			foreach (OfficialStatChange existingChange in existingChanges)
			{
				if (!string.IsNullOrWhiteSpace(existingChange.SourceUrl))
				{
					urls.Add(existingChange.SourceUrl.Trim());
				}
			}

			return urls.OrderByDescending(url => url, StringComparer.OrdinalIgnoreCase).ToList();
		}

		private static void writeUrlDiscoverySummary(IReadOnlyCollection<string> discoveredSourceUrls, IEnumerable<OfficialStatChange> existingChanges)
		{
			HashSet<string> discoveredUrls = new HashSet<string>(discoveredSourceUrls, StringComparer.OrdinalIgnoreCase);

			List<string> existingUrls = existingChanges
				.Where(change => !string.IsNullOrWhiteSpace(change.SourceUrl))
				.Select(change => change.SourceUrl.Trim())
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			int coveredCount = existingUrls.Count(discoveredUrls.Contains);

			Console.WriteLine("[URL DISCOVERY] Found: " + discoveredUrls.Count + ", Existing covered: " + coveredCount + "/" + existingUrls.Count);
		}

		private static DateTime resolveFallbackEffectiveDate(string sourceUrl, IReadOnlyList<OfficialStatChange> existingChanges)
		{
			if (existingChanges.Count > 0)
			{
				return existingChanges
					.OrderByDescending(change => change.EffectiveDate)
					.First()
					.EffectiveDate;
			}

			Match match = SOURCE_DATE_PATTERN.Match(sourceUrl);

			if (!match.Success)
			{
				throw new FormatException("Could not determine fallback date from official announcement URL: " + sourceUrl);
			}

			int year = int.Parse(match.Groups["year"].Value, CultureInfo.InvariantCulture);
			int month = int.Parse(match.Groups["month"].Value, CultureInfo.InvariantCulture);
			int day = int.Parse(match.Groups["day"].Value, CultureInfo.InvariantCulture);

			return new DateTime(year, month, day);
		}

		private static OfficialStatChange createGeneratedChange(OfficialStatChange discoveredChange, OfficialStatChange existingChange, ISet<string> appKeys)
		{
			return new OfficialStatChange
			{
				EffectiveDate = discoveredChange.EffectiveDate,
				AnnouncementDate = discoveredChange.AnnouncementDate,
				Rarity = discoveredChange.Rarity,
				ShikigamiName = discoveredChange.ShikigamiName,
				StatScope = discoveredChange.StatScope,
				StatType = discoveredChange.StatType,
				OldValue = discoveredChange.OldValue,
				NewValue = discoveredChange.NewValue,
				Unit = discoveredChange.Unit,
				OldValueKnown = discoveredChange.OldValueKnown,
				CurrentAppScope = appKeys.Contains(discoveredChange.GetShikigamiKey()),
				ReviewPriority = existingChange == null ? "" : existingChange.ReviewPriority,
				AppAction = existingChange == null ? "" : existingChange.AppAction,
				SourceUrl = discoveredChange.SourceUrl,
				Notes = existingChange == null ? "" : existingChange.Notes
			};
		}

		private static void addGeneratedChange(List<OfficialStatChange> generatedChanges, OfficialStatChange candidate)
		{
			OfficialStatChange duplicate = generatedChanges.FirstOrDefault(change => isSameEvent(change, candidate));

			if (duplicate == null)
			{
				generatedChanges.Add(candidate);
				return;
			}

			if (shouldPreferCandidate(duplicate, candidate))
			{
				int index = generatedChanges.IndexOf(duplicate);
				generatedChanges[index] = candidate;

				Console.WriteLine("[DUPLICATE] Replaced later announcement: " + duplicate.SourceUrl + " -> " + candidate.SourceUrl);
				return;
			}

			Console.WriteLine("[DUPLICATE] Ignored later announcement: " + candidate.SourceUrl);
		}

		private static bool shouldPreferCandidate(OfficialStatChange current, OfficialStatChange candidate)
		{
			if (candidate.AnnouncementDate.Date < current.AnnouncementDate.Date)
			{
				return true;
			}

			if (candidate.AnnouncementDate.Date > current.AnnouncementDate.Date)
			{
				return false;
			}

			return string.Compare(candidate.SourceUrl, current.SourceUrl, StringComparison.OrdinalIgnoreCase) < 0;
		}

		private static OfficialStatChange findExistingChange(OfficialStatChange discoveredChange, IEnumerable<OfficialStatChange> existingChanges)
		{
			return existingChanges.FirstOrDefault(existingChange => isSameChange(existingChange, discoveredChange));
		}

		private static bool isSameChange(OfficialStatChange left, OfficialStatChange right)
		{
			if (!string.Equals(left.SourceUrl, right.SourceUrl, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			return isSameEvent(left, right);
		}

		private static bool isSameEvent(OfficialStatChange left, OfficialStatChange right)
		{
			if (!string.Equals(left.Rarity, right.Rarity, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (!string.Equals(left.ShikigamiName, right.ShikigamiName, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (left.StatScope != right.StatScope || left.StatType != right.StatType)
			{
				return false;
			}

			if (left.OldValueKnown != right.OldValueKnown)
			{
				return false;
			}

			if (left.OldValueKnown && !areEqual(left.OldValue.Value, right.OldValue.Value))
			{
				return false;
			}

			return areEqual(left.NewValue, right.NewValue);
		}

		private static List<ShikigamiStatus> createKnownStatuses(IEnumerable<ShikigamiStatus> appStatuses, IEnumerable<OfficialStatChange> existingChanges)
		{
			List<ShikigamiStatus> knownStatuses = appStatuses.ToList();
			HashSet<string> knownKeys = new HashSet<string>(knownStatuses.Select(status => status.GetKey()), StringComparer.OrdinalIgnoreCase);

			foreach (OfficialStatChange existingChange in existingChanges)
			{
				string key = existingChange.GetShikigamiKey();

				if (knownKeys.Contains(key))
				{
					continue;
				}

				knownStatuses.Add(new ShikigamiStatus
				{
					Rarity = existingChange.Rarity,
					Name = existingChange.ShikigamiName,
					StatScope = StatScope.UNKNOWN
				});

				knownKeys.Add(key);
			}

			return knownStatuses;
		}

		private static void writeGeneratedChange(OfficialStatChange change, bool isNew)
		{
			string oldValue = change.OldValueKnown ? change.OldValue.Value.ToString(CultureInfo.InvariantCulture) : "?";
			string newValue = change.NewValue.ToString(CultureInfo.InvariantCulture);
			string prefix = isNew ? "[NEW]" : "[OK]";

			Console.WriteLine(prefix + " " + change.Rarity + " " + change.ShikigamiName + " " + change.StatType + " " + oldValue + " -> " + newValue);
		}

		private static bool areEqual(double left, double right)
		{
			return Math.Abs(left - right) <= VALUE_TOLERANCE;
		}
	}
}
