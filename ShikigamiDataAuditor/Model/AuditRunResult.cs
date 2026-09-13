using System.Collections.Generic;

namespace ShikigamiDataAuditor.Model
{
	public class AuditRunResult
	{
		public IReadOnlyList<AuditResult> Results { get; set; }
		public bool HasExternalFailure { get; set; }

		public AuditRunResult()
		{
			Results = new List<AuditResult>();
		}
	}
}
