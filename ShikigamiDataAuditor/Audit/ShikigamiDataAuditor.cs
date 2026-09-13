using ShikigamiDataAuditor.External;
using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikigamiDataAuditor.Audit
{
	public class ShikigamiDataAuditor
	{
		private readonly OfficialChangeResolver mOfficialChangeResolver;
		private readonly StatScopeMatcher mStatScopeMatcher;
		private readonly AuditOutcomeResolver mAuditOutcomeResolver;
		private readonly IShikigamiStatusSource mExternalStatusSource;

		public ShikigamiDataAuditor(OfficialChangeResolver officialChangeResolver, StatScopeMatcher statScopeMatcher, AuditOutcomeResolver auditOutcomeResolver, IShikigamiStatusSource externalStatusSource)
		{
			mOfficialChangeResolver = officialChangeResolver;
			mStatScopeMatcher = statScopeMatcher;
			mAuditOutcomeResolver = auditOutcomeResolver;
			mExternalStatusSource = externalStatusSource;
		}

		public async Task<AuditRunResult> AuditAsync(IReadOnlyList<ShikigamiStatus> appStatuses, IReadOnlyList<OfficialStatChange> officialChanges, IReadOnlyList<ExternalNameMapEntry> externalNameMapEntries)
		{
			Dictionary<string, ShikigamiStatus> appByKey = appStatuses.ToDictionary(status => status.GetKey(), status => status, StringComparer.OrdinalIgnoreCase);

			IReadOnlyList<OfficialStatChange> latestOfficialChanges = mOfficialChangeResolver.ResolveLatest(officialChanges, DateTime.Today);

			Dictionary<string, OfficialStatChange> officialByAuditKey = latestOfficialChanges.Where(change => change.CurrentAppScope).ToDictionary(change => change.GetAuditKey(), change => change, StringComparer.OrdinalIgnoreCase);

			Dictionary<string, ExternalStatusResult> externalByShikigami = await loadExternalResultsAsync(appByKey, externalNameMapEntries).ConfigureAwait(false);

			List<AuditResult> results = new List<AuditResult>();

			foreach (OfficialStatChange change in officialByAuditKey.Values)
			{
				ShikigamiStatus appStatus;

				if (!appByKey.TryGetValue(change.GetShikigamiKey(), out appStatus))
				{
					results.Add(createMissingShikigamiResult(change.Rarity, change.ShikigamiName, change.StatType, change.SourceUrl));
					continue;
				}

				double appValue;

				if (!appStatus.TryGetValue(change.StatType, out appValue))
				{
					results.Add(createInvalidDataResult(change, "App data does not contain the target stat."));
					continue;
				}

				if (!mStatScopeMatcher.IsOfficialComparableToApp(change))
				{
					results.Add(createScopeMismatchResult(change, appValue));
					continue;
				}

				results.Add(new AuditResult
				{
					Outcome = mAuditOutcomeResolver.ResolveOfficial(appValue, change.NewValue),
					Rarity = change.Rarity,
					ShikigamiName = change.ShikigamiName,
					StatType = change.StatType,
					AppValue = appValue,
					OfficialValue = change.NewValue,
					StatScope = change.StatScope,
					Evidence = "Latest applicable Japanese official change",
					SourceUrl = change.SourceUrl,
					Notes = change.Notes
				});

				addExternalSourceState(results, change, externalByShikigami, appValue);
			}

			addExternalOnlyResults(results, appByKey, officialByAuditKey, externalNameMapEntries, externalByShikigami);
			addExternalFailures(results, appByKey, externalNameMapEntries, externalByShikigami);

			return new AuditRunResult
			{
				Results = results
					.OrderBy(result => result.Outcome)
					.ThenBy(result => result.Rarity)
					.ThenBy(result => result.ShikigamiName)
					.ThenBy(result => result.StatType)
					.ToList(),
				HasExternalFailure = externalByShikigami.Values.Any(result => !result.Succeeded || result.IsStale)
			};
		}

		private async Task<Dictionary<string, ExternalStatusResult>> loadExternalResultsAsync(IReadOnlyDictionary<string, ShikigamiStatus> appByKey, IEnumerable<ExternalNameMapEntry> entries)
		{
			Dictionary<string, ExternalStatusResult> results = new Dictionary<string, ExternalStatusResult>(StringComparer.OrdinalIgnoreCase);

			foreach (ExternalNameMapEntry entry in entries.Where(item => item.Enabled))
			{
				if (!appByKey.ContainsKey(entry.GetShikigamiKey()) || string.IsNullOrWhiteSpace(entry.PageTitle))
				{
					continue;
				}

				results[entry.GetShikigamiKey()] = await mExternalStatusSource.GetStatusAsync(entry).ConfigureAwait(false);
			}

			return results;
		}

		private void addExternalSourceState(ICollection<AuditResult> results, OfficialStatChange change, IReadOnlyDictionary<string, ExternalStatusResult> externalByShikigami, double appValue)
		{
			ExternalStatusResult externalResult;

			if (!externalByShikigami.TryGetValue(change.GetShikigamiKey(), out externalResult) || !externalResult.Succeeded)
			{
				return;
			}

			ShikigamiStatus referenceStatus = externalResult.Statuses.FirstOrDefault(status => mStatScopeMatcher.IsReferenceComparableToApp(status.StatScope, change.StatType) && status.Values.ContainsKey(change.StatType));

			if (referenceStatus == null)
			{
				return;
			}

			double referenceValue = referenceStatus.Values[change.StatType];

			if (externalResult.IsStale)
			{
				results.Add(createReferenceStateResult(AuditOutcome.MANUAL_REVIEW, change, appValue, referenceValue, externalResult, "Expired cache was used."));
			}
			else if (!mAuditOutcomeResolver.Equals(referenceValue, change.NewValue))
			{
				results.Add(createReferenceStateResult(AuditOutcome.SOURCE_STALE, change, appValue, referenceValue, externalResult, "External value conflicts with the latest Japanese official change."));
			}
		}

		private void addExternalOnlyResults(ICollection<AuditResult> results, IReadOnlyDictionary<string, ShikigamiStatus> appByKey, IReadOnlyDictionary<string, OfficialStatChange> officialByAuditKey, IEnumerable<ExternalNameMapEntry> entries, IReadOnlyDictionary<string, ExternalStatusResult> externalByShikigami)
		{
			foreach (ExternalNameMapEntry entry in entries.Where(item => item.Enabled))
			{
				ShikigamiStatus appStatus;
				ExternalStatusResult externalResult;

				if (!appByKey.TryGetValue(entry.GetShikigamiKey(), out appStatus) || !externalByShikigami.TryGetValue(entry.GetShikigamiKey(), out externalResult) || !externalResult.Succeeded)
				{
					continue;
				}

				foreach (ShikigamiStatus referenceStatus in externalResult.Statuses)
				{
					foreach (KeyValuePair<StatType, double> pair in referenceStatus.Values)
					{
						string auditKey = entry.GetShikigamiKey() + "\t" + pair.Key;

						if (officialByAuditKey.ContainsKey(auditKey))
						{
							continue;
						}

						double appValue;

						if (!appStatus.TryGetValue(pair.Key, out appValue))
						{
							continue;
						}

						if (!mStatScopeMatcher.IsReferenceComparableToApp(referenceStatus.StatScope, pair.Key))
						{
							continue;
						}

						AuditOutcome outcome;
						string evidence;

						if (externalResult.IsStale)
						{
							outcome = AuditOutcome.MANUAL_REVIEW;
							evidence = "Expired external cache; comparison is not conclusive.";
						}
						else
						{
							outcome = mAuditOutcomeResolver.ResolveReference(appValue, pair.Value);
							evidence = "External reference only; not an official confirmation.";
						}

						results.Add(new AuditResult
						{
							Outcome = outcome,
							Rarity = entry.Rarity,
							ShikigamiName = entry.ShikigamiName,
							StatType = pair.Key,
							AppValue = appValue,
							ReferenceValue = pair.Value,
							StatScope = referenceStatus.StatScope,
							Evidence = evidence,
							SourceUrl = externalResult.SourceUrl,
							FetchedAt = externalResult.FetchedAt,
							Notes = externalResult.IsCache ? "Cache" : "Network"
						});
					}
				}
			}
		}

		private static void addExternalFailures(ICollection<AuditResult> results, IReadOnlyDictionary<string, ShikigamiStatus> appByKey, IEnumerable<ExternalNameMapEntry> entries, IReadOnlyDictionary<string, ExternalStatusResult> externalByShikigami)
		{
			foreach (ExternalNameMapEntry entry in entries.Where(item => item.Enabled))
			{
				if (!appByKey.ContainsKey(entry.GetShikigamiKey()))
				{
					results.Add(createMissingShikigamiResult(entry.Rarity, entry.ShikigamiName, StatType.UNKNOWN, ""));
					continue;
				}

				if (string.IsNullOrWhiteSpace(entry.PageTitle))
				{
					results.Add(new AuditResult
					{
						Outcome = AuditOutcome.NAME_UNRESOLVED,
						Rarity = entry.Rarity,
						ShikigamiName = entry.ShikigamiName,
						StatType = StatType.UNKNOWN,
						StatScope = StatScope.UNKNOWN,
						Evidence = "External page title is empty."
					});
					continue;
				}

				ExternalStatusResult externalResult;

				if (!externalByShikigami.TryGetValue(entry.GetShikigamiKey(), out externalResult) || !externalResult.Succeeded)
				{
					results.Add(new AuditResult
					{
						Outcome = AuditOutcome.REFERENCE_NOT_FOUND,
						Rarity = entry.Rarity,
						ShikigamiName = entry.ShikigamiName,
						StatType = StatType.UNKNOWN,
						StatScope = StatScope.UNKNOWN,
						Evidence = "External data could not be retrieved or parsed.",
						Notes = externalResult == null ? "No result" : externalResult.ErrorMessage
					});
				}
			}
		}

		private static AuditResult createMissingShikigamiResult(string rarity, string name, StatType statType, string sourceUrl)
		{
			return new AuditResult
			{
				Outcome = AuditOutcome.SHIKIGAMI_NOT_FOUND,
				Rarity = rarity,
				ShikigamiName = name,
				StatType = statType,
				StatScope = StatScope.UNKNOWN,
				Evidence = "Shikigami was not found in app data.",
				SourceUrl = sourceUrl
			};
		}

		private static AuditResult createInvalidDataResult(OfficialStatChange change, string evidence)
		{
			return new AuditResult
			{
				Outcome = AuditOutcome.INVALID_DATA,
				Rarity = change.Rarity,
				ShikigamiName = change.ShikigamiName,
				StatType = change.StatType,
				OfficialValue = change.NewValue,
				StatScope = change.StatScope,
				Evidence = evidence,
				SourceUrl = change.SourceUrl
			};
		}

		private static AuditResult createScopeMismatchResult(OfficialStatChange change, double appValue)
		{
			return new AuditResult
			{
				Outcome = AuditOutcome.SCOPE_MISMATCH,
				Rarity = change.Rarity,
				ShikigamiName = change.ShikigamiName,
				StatType = change.StatType,
				AppValue = appValue,
				OfficialValue = change.NewValue,
				StatScope = change.StatScope,
				Evidence = "Official stat scope is not directly comparable to awakened level 40 app data.",
				SourceUrl = change.SourceUrl,
				Notes = change.Notes
			};
		}

		private static AuditResult createReferenceStateResult(AuditOutcome outcome, OfficialStatChange change, double appValue, double referenceValue, ExternalStatusResult externalResult, string evidence)
		{
			return new AuditResult
			{
				Outcome = outcome,
				Rarity = change.Rarity,
				ShikigamiName = change.ShikigamiName,
				StatType = change.StatType,
				AppValue = appValue,
				OfficialValue = change.NewValue,
				ReferenceValue = referenceValue,
				StatScope = change.StatScope,
				Evidence = evidence,
				SourceUrl = externalResult.SourceUrl,
				FetchedAt = externalResult.FetchedAt,
				Notes = externalResult.ErrorMessage
			};
		}
	}
}
