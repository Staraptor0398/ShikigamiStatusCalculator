using ShikigamiDataAuditor.External;
using ShikigamiDataAuditor.Model;
using System;
using System.Threading.Tasks;

namespace ShikigamiDataAuditor.DataGeneration
{
	public class ExternalNameMapPageValidator
	{
		private readonly MediaWikiClient mClient;

		public ExternalNameMapPageValidator(MediaWikiClient client)
		{
			mClient = client ?? throw new ArgumentNullException(nameof(client));
		}

		public async Task<ExternalNameMapValidationResult> ValidateAsync(ExternalNameMapEntry entry)
		{
			if (entry == null)
			{
				throw new ArgumentNullException(nameof(entry));
			}

			if (string.IsNullOrWhiteSpace(entry.PageTitle))
			{
				return ExternalNameMapValidationResult.Invalid(ExternalNameMapValidationFailureReason.PAGE_TITLE_EMPTY, "Page title is empty.");
			}

			try
			{
				MediaWikiPageContent pageContent = await mClient.GetPageContentAsync(entry.PageTitle).ConfigureAwait(false);

				return ExternalNameMapValidationResult.Valid(pageContent.SourceUrl);
			}
			catch (MediaWikiPageNotFoundException exception)
			{
				return ExternalNameMapValidationResult.Invalid(ExternalNameMapValidationFailureReason.PAGE_NOT_FOUND, exception.Message);
			}
			catch (Exception exception)
			{
				throw new InvalidOperationException("Failed to verify Moegirl page: " + entry.PageTitle, exception);
			}
		}
	}
}
