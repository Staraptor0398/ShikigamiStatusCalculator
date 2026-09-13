namespace ShikigamiDataAuditor.DataGeneration
{
	public class ExternalNameMapValidationResult
	{
		public bool IsValid { get; private set; }
		public ExternalNameMapValidationFailureReason FailureReason { get; private set; }
		public string SourceUrl { get; private set; }
		public string Message { get; private set; }

		public static ExternalNameMapValidationResult Valid(string sourceUrl)
		{
			return new ExternalNameMapValidationResult
			{
				IsValid = true,
				FailureReason = ExternalNameMapValidationFailureReason.NONE,
				SourceUrl = sourceUrl ?? "",
				Message = ""
			};
		}

		public static ExternalNameMapValidationResult Invalid(ExternalNameMapValidationFailureReason failureReason, string message)
		{
			return new ExternalNameMapValidationResult
			{
				IsValid = false,
				FailureReason = failureReason,
				SourceUrl = "",
				Message = message ?? ""
			};
		}
	}
}
