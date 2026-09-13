using ShikigamiDataAuditor.Model;

namespace ShikigamiDataAuditor.Audit
{
	public class StatScopeMatcher
	{
		public bool IsOfficialComparableToApp(OfficialStatChange change)
		{
			if (!change.CurrentAppScope)
			{
				return false;
			}

			switch (change.StatScope)
			{
				case StatScope.AWAKENED_LEVEL_40:
				case StatScope.MAX_LEVEL_BASE:
					return true;
				case StatScope.AWAKENED:
				case StatScope.AWAKENED_BASE:
					return change.StatType == StatType.SPEED || isLevelIndependent(change.StatType);
				case StatScope.INITIAL:
				case StatScope.BASE:
					return isLevelIndependent(change.StatType);
				default:
					return false;
			}
		}

		public bool IsReferenceComparableToApp(StatScope scope, StatType statType)
		{
			switch (scope)
			{
				case StatScope.AWAKENED_LEVEL_40:
					return true;
				case StatScope.AWAKENED:
				case StatScope.AWAKENED_BASE:
					return statType == StatType.SPEED || isLevelIndependent(statType);
				default:
					return false;
			}
		}

		private static bool isLevelIndependent(StatType statType)
		{
			return statType == StatType.CRITICAL_RATE || statType == StatType.CRITICAL_DAMAGE || statType == StatType.EFFECT_HIT || statType == StatType.EFFECT_RESIST;
		}
	}
}
