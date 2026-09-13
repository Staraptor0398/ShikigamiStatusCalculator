using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ShikigamiDataAuditor.DataGeneration
{
	public class OfficialChangeHistorySeedValidator
	{
		private const double VALUE_TOLERANCE = 0.000001;

		public OfficialChangeHistoryValidationResult Validate(IEnumerable<OfficialStatChange> seedChanges, IEnumerable<OfficialStatChange> discoveredChanges)
		{
			if (seedChanges == null)
			{
				throw new ArgumentNullException(nameof(seedChanges));
			}

			if (discoveredChanges == null)
			{
				throw new ArgumentNullException(nameof(discoveredChanges));
			}

			List<OfficialStatChange> seeds = seedChanges.ToList();
			List<OfficialStatChange> remainingDiscovered = discoveredChanges.ToList();
			int matchedCount = 0;

			foreach (OfficialStatChange seed in seeds)
			{
				OfficialStatChange matched = remainingDiscovered.FirstOrDefault(discovered => isMatch(seed, discovered));

				if (matched == null)
				{
					throw new InvalidOperationException(createMismatchMessage(seed, remainingDiscovered));
				}

				remainingDiscovered.Remove(matched);
				matchedCount++;
			}

			return new OfficialChangeHistoryValidationResult
			{
				SeedCount = seeds.Count,
				MatchedCount = matchedCount,
				ExtraChanges = remainingDiscovered
			};
		}

		private static bool isMatch(OfficialStatChange seed, OfficialStatChange discovered)
		{
			if (!string.Equals(seed.Rarity, discovered.Rarity, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (!string.Equals(seed.ShikigamiName, discovered.ShikigamiName, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}

			if (seed.StatScope != discovered.StatScope || seed.StatType != discovered.StatType)
			{
				return false;
			}

			if (seed.OldValueKnown != discovered.OldValueKnown)
			{
				return false;
			}

			if (seed.OldValueKnown && !areEqual(seed.OldValue.Value, discovered.OldValue.Value))
			{
				return false;
			}

			return areEqual(seed.NewValue, discovered.NewValue);
		}

		private static string createMismatchMessage(OfficialStatChange seed, IReadOnlyList<OfficialStatChange> discoveredChanges)
		{
			List<OfficialStatChange> related = discoveredChanges
				.Where(change => string.Equals(change.Rarity, seed.Rarity, StringComparison.OrdinalIgnoreCase))
				.Where(change => string.Equals(change.ShikigamiName, seed.ShikigamiName, StringComparison.OrdinalIgnoreCase))
				.ToList();

			string expected = formatChange(seed);

			if (related.Count == 0)
			{
				return "Discovered official changes do not contain the seed entry. Expected=" + expected;
			}

			string actual = string.Join(" | ", related.Select(formatChange));

			return "Discovered official change does not match the seed entry. Expected=" + expected + ", Discovered=" + actual;
		}

		private static string formatChange(OfficialStatChange change)
		{
			string oldValue = change.OldValueKnown ? change.OldValue.Value.ToString(CultureInfo.InvariantCulture) : "?";
			string newValue = change.NewValue.ToString(CultureInfo.InvariantCulture);

			return change.Rarity + " " + change.ShikigamiName + " " + change.StatScope + " " + change.StatType + " " + oldValue + " -> " + newValue;
		}

		private static bool areEqual(double left, double right)
		{
			return Math.Abs(left - right) <= VALUE_TOLERANCE;
		}
	}
}
