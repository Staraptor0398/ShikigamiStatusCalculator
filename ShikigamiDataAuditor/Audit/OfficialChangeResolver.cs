using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShikigamiDataAuditor.Audit
{
	public class OfficialChangeResolver
	{
		public IReadOnlyList<OfficialStatChange> ResolveLatest(IEnumerable<OfficialStatChange> changes, DateTime effectiveAt)
		{
			return changes
				.Where(change => change.EffectiveDate.Date <= effectiveAt.Date)
				.GroupBy(change => change.GetAuditKey(), StringComparer.OrdinalIgnoreCase)
				.Select(group => group
					.OrderByDescending(change => change.EffectiveDate)
					.ThenByDescending(change => getScopePriority(change.StatScope))
					.First())
				.OrderBy(change => change.Rarity)
				.ThenBy(change => change.ShikigamiName)
				.ThenBy(change => change.StatType)
				.ToList();
		}

		private static int getScopePriority(StatScope scope)
		{
			switch (scope)
			{
				case StatScope.AWAKENED_LEVEL_40: return 7;
				case StatScope.MAX_LEVEL_BASE: return 6;
				case StatScope.AWAKENED_BASE: return 5;
				case StatScope.AWAKENED: return 4;
				case StatScope.BASE: return 3;
				case StatScope.INITIAL: return 2;
				case StatScope.AWAKENING_BONUS: return 1;
				default: return 0;
			}
		}
	}
}
