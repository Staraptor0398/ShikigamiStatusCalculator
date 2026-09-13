using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShikigamiDataAuditor.DataGeneration
{
	public class ExternalNameMapGenerator
	{
		private const string SOURCE_NAME = "moegirl";

		private readonly ExternalNameMapPageValidator mPageValidator;
		private readonly MoegirlNameResolver mNameResolver;

		public ExternalNameMapGenerator(ExternalNameMapPageValidator pageValidator)
		{
			mPageValidator = pageValidator ?? throw new ArgumentNullException(nameof(pageValidator));
			mNameResolver = new MoegirlNameResolver();
		}

		public async Task<IReadOnlyList<ExternalNameMapEntry>> GenerateAsync(IEnumerable<ShikigamiStatus> appStatuses, IEnumerable<ExternalNameMapEntry> existingEntries)
		{
			if (appStatuses == null)
			{
				throw new ArgumentNullException(nameof(appStatuses));
			}

			Dictionary<string, ExternalNameMapEntry> existingMap = createExistingMap(existingEntries);
			List<ExternalNameMapEntry> results = new List<ExternalNameMapEntry>();

			foreach (ShikigamiStatus status in appStatuses.OrderBy(status => getRarityOrder(status.Rarity)).ThenBy(status => status.Name, StringComparer.Ordinal))
			{
				ExternalNameMapEntry entry = await generateEntryAsync(status, existingMap).ConfigureAwait(false);
				results.Add(entry);
			}

			return results;
		}

		private async Task<ExternalNameMapEntry> generateEntryAsync(ShikigamiStatus status, IReadOnlyDictionary<string, ExternalNameMapEntry> existingMap)
		{
			string key = createKey(status.Rarity, status.Name);
			ExternalNameMapEntry existingEntry;
			List<ExternalNameMapValidationResult> failedResults = new List<ExternalNameMapValidationResult>();

			if (existingMap.TryGetValue(key, out existingEntry) && !string.IsNullOrWhiteSpace(existingEntry.PageTitle))
			{
				ExternalNameMapEntry existingCandidate = createCandidate(status, existingEntry.PageTitle, existingEntry.Notes);
				ExternalNameMapValidationResult existingValidation = await mPageValidator.ValidateAsync(existingCandidate).ConfigureAwait(false);

				if (existingValidation.IsValid)
				{
					existingCandidate.Enabled = true;
					existingCandidate.Notes = createVerifiedExistingNotes(existingEntry.Notes);

					Console.WriteLine("[OK] " + status.Rarity + " " + status.Name + " -> " + existingCandidate.PageTitle);
					return existingCandidate;
				}

				failedResults.Add(existingValidation);
				writeValidationFailure(status, existingCandidate.PageTitle, existingValidation);
			}

			IReadOnlyList<string> candidates = mNameResolver.ResolveCandidates(status.Name);

			if (candidates.Count == 0)
			{
				Console.WriteLine("[NO_CANDIDATE] " + status.Rarity + " " + status.Name);
				return createUnresolvedEntry(status, existingEntry, candidates, failedResults);
			}

			foreach (string pageTitle in candidates)
			{
				if (existingEntry != null && string.Equals(pageTitle, existingEntry.PageTitle, StringComparison.Ordinal))
				{
					continue;
				}

				ExternalNameMapEntry candidate = createCandidate(status, pageTitle, "");
				ExternalNameMapValidationResult validation = await mPageValidator.ValidateAsync(candidate).ConfigureAwait(false);

				if (validation.IsValid)
				{
					candidate.Enabled = true;
					candidate.Notes = createResolvedNotes(status.Name, pageTitle);

					Console.WriteLine("[OK] " + status.Rarity + " " + status.Name + " -> " + candidate.PageTitle);
					return candidate;
				}

				failedResults.Add(validation);
				writeValidationFailure(status, pageTitle, validation);
			}

			return createUnresolvedEntry(status, existingEntry, candidates, failedResults);
		}

		private static Dictionary<string, ExternalNameMapEntry> createExistingMap(IEnumerable<ExternalNameMapEntry> existingEntries)
		{
			Dictionary<string, ExternalNameMapEntry> result = new Dictionary<string, ExternalNameMapEntry>(StringComparer.OrdinalIgnoreCase);

			if (existingEntries == null)
			{
				return result;
			}

			foreach (ExternalNameMapEntry entry in existingEntries)
			{
				if (entry == null)
				{
					continue;
				}

				string key = createKey(entry.Rarity, entry.ShikigamiName);

				if (!result.ContainsKey(key))
				{
					result.Add(key, entry);
				}
			}

			return result;
		}

		private static ExternalNameMapEntry createCandidate(ShikigamiStatus status, string pageTitle, string notes)
		{
			return new ExternalNameMapEntry
			{
				Rarity = status.Rarity,
				ShikigamiName = status.Name,
				Source = SOURCE_NAME,
				PageTitle = pageTitle,
				Enabled = false,
				Notes = notes ?? ""
			};
		}

		private static ExternalNameMapEntry createUnresolvedEntry(
			ShikigamiStatus status,
			ExternalNameMapEntry existingEntry,
			IReadOnlyList<string> candidates,
			IReadOnlyList<ExternalNameMapValidationResult> failedResults)
		{
			string pageTitle = "";

			if (existingEntry != null && !string.IsNullOrWhiteSpace(existingEntry.PageTitle))
			{
				pageTitle = existingEntry.PageTitle;
			}
			else if (candidates.Count > 0)
			{
				pageTitle = candidates[0];
			}

			string failureSummary = createFailureSummary(failedResults);

			Console.WriteLine("[UNRESOLVED] " + status.Rarity + " " + status.Name +
				(string.IsNullOrWhiteSpace(pageTitle) ? "" : " -> " + pageTitle) +
				(string.IsNullOrWhiteSpace(failureSummary) ? "" : " (" + failureSummary + ")"));

			return new ExternalNameMapEntry
			{
				Rarity = status.Rarity,
				ShikigamiName = status.Name,
				Source = SOURCE_NAME,
				PageTitle = pageTitle,
				Enabled = false,
				Notes = createUnresolvedNotes(failureSummary)
			};
		}

		private static void writeValidationFailure(ShikigamiStatus status, string pageTitle, ExternalNameMapValidationResult validation)
		{
			string logType = getFailureLogType(validation.FailureReason);

			Console.WriteLine("[" + logType + "] " + status.Rarity + " " + status.Name + " -> " + pageTitle + " : " + validation.Message);
		}

		private static string getFailureLogType(ExternalNameMapValidationFailureReason failureReason)
		{
			switch (failureReason)
			{
				case ExternalNameMapValidationFailureReason.PAGE_TITLE_EMPTY:
					return "INVALID_TITLE";
				case ExternalNameMapValidationFailureReason.PAGE_NOT_FOUND:
					return "NOT_FOUND";
				case ExternalNameMapValidationFailureReason.PARSE_FAILED:
					return "PARSE_FAILED";
				default:
					return "VALIDATION_FAILED";
			}
		}

		private static string createFailureSummary(IReadOnlyList<ExternalNameMapValidationResult> failedResults)
		{
			if (failedResults == null || failedResults.Count == 0)
			{
				return "";
			}

			int pageTitleEmptyCount = failedResults.Count(result => result.FailureReason == ExternalNameMapValidationFailureReason.PAGE_TITLE_EMPTY);
			int pageNotFoundCount = failedResults.Count(result => result.FailureReason == ExternalNameMapValidationFailureReason.PAGE_NOT_FOUND);
			int parseFailedCount = failedResults.Count(result => result.FailureReason == ExternalNameMapValidationFailureReason.PARSE_FAILED);

			List<string> parts = new List<string>();

			if (pageTitleEmptyCount > 0)
			{
				parts.Add("INVALID_TITLE=" + pageTitleEmptyCount);
			}

			if (pageNotFoundCount > 0)
			{
				parts.Add("NOT_FOUND=" + pageNotFoundCount);
			}

			if (parseFailedCount > 0)
			{
				parts.Add("PARSE_FAILED=" + parseFailedCount);
			}

			return string.Join(", ", parts);
		}

		private static string createUnresolvedNotes(string failureSummary)
		{
			if (string.IsNullOrWhiteSpace(failureSummary))
			{
				return "Moegirlの候補ページを自動確認できませんでした";
			}

			return "Moegirlの候補ページを自動確認できませんでした: " + failureSummary;
		}

		private static string createResolvedNotes(string shikigamiName, string pageTitle)
		{
			string pageName = getPageName(pageTitle);

			if (string.Equals(shikigamiName, pageName, StringComparison.Ordinal))
			{
				return "Moegirlページを自動確認済み";
			}

			return "名称候補からMoegirlページを自動解決: " + pageName;
		}

		private static string createVerifiedExistingNotes(string notes)
		{
			string normalized = (notes ?? "").Trim();

			if (string.IsNullOrWhiteSpace(normalized))
			{
				return "既存マッピングをMoegirlで確認済み";
			}

			if (normalized.Contains("確認済み"))
			{
				return normalized;
			}

			return normalized + " / Moegirlで確認済み";
		}

		private static string getPageName(string pageTitle)
		{
			int separatorIndex = pageTitle.IndexOf(':');

			if (separatorIndex < 0 || separatorIndex + 1 >= pageTitle.Length)
			{
				return pageTitle;
			}

			return pageTitle.Substring(separatorIndex + 1);
		}

		private static string createKey(string rarity, string shikigamiName)
		{
			return (rarity ?? "").Trim().ToUpperInvariant() + "\t" + (shikigamiName ?? "").Trim();
		}

		private static int getRarityOrder(string rarity)
		{
			switch ((rarity ?? "").Trim().ToUpperInvariant())
			{
				case "SP": return 0;
				case "SSR": return 1;
				case "SR": return 2;
				case "R": return 3;
				case "N": return 4;
				default: return 5;
			}
		}
	}
}
