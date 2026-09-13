using ShikigamiDataAuditor.Model;
using System;

namespace ShikigamiDataAuditor.Audit
{
	public class AuditOutcomeResolver
	{
		private const double COMPARISON_TOLERANCE = 0.000001;

		public AuditOutcome ResolveOfficial(double appValue, double officialValue)
		{
			return equals(appValue, officialValue) ? AuditOutcome.MATCH : AuditOutcome.CONFIRMED_MISMATCH;
		}

		public AuditOutcome ResolveReference(double appValue, double referenceValue)
		{
			return equals(appValue, referenceValue) ? AuditOutcome.MATCH : AuditOutcome.REFERENCE_MISMATCH;
		}

		public bool Equals(double left, double right)
		{
			return equals(left, right);
		}

		private static bool equals(double left, double right)
		{
			return Math.Abs(left - right) <= COMPARISON_TOLERANCE;
		}
	}
}
