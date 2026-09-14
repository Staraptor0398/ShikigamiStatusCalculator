using ShikigamiDataAuditor.Access;
using ShikigamiDataAuditor.Application;
using ShikigamiDataAuditor.Audit;
using ShikigamiDataAuditor.DataGeneration;
using ShikigamiDataAuditor.External;
using ShikigamiDataAuditor.Model;
using ShikigamiDataAuditor.Official;
using ShikigamiDataAuditor.Report;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DataAuditor = ShikigamiDataAuditor.Audit.ShikigamiDataAuditor;

namespace ShikigamiDataAuditor
{
	internal static class Program
	{
		private const int EXIT_SUCCESS = 0;
		private const int EXIT_CONFIRMED_MISMATCH = 1;
		private const int EXIT_FATAL_ERROR = 2;
		private const int EXIT_EXTERNAL_FAILURE = 3;

		private static int Main(string[] args)
		{
			try
			{
				AuditOptions options = AuditOptions.Parse(args);

				if (options.ShowHelp)
				{
					Console.WriteLine(AuditOptions.GetUsage());
					return EXIT_SUCCESS;
				}

				if (options.GenerateExternalNameMap)
				{
					return generateExternalNameMap(options);
				}

				if (options.GenerateOfficialChangeHistory)
				{
					return generateOfficialChangeHistory(options);
				}

				if (options.ApplyConfirmedChanges)
				{
					return applyConfirmedChanges(options);
				}

				return run(options);
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine("[ERROR] " + exception.Message);
				Console.Error.WriteLine(exception);

				return EXIT_FATAL_ERROR;
			}
		}

		private static int run(AuditOptions options)
		{
			ShikigamiDataCsvReader appReader = new ShikigamiDataCsvReader();
			OfficialChangeHistoryCsvReader officialReader = new OfficialChangeHistoryCsvReader();
			ExternalNameMapCsvReader nameMapReader = new ExternalNameMapCsvReader();

			IReadOnlyList<ShikigamiStatus> appStatuses = appReader.Read(options.AppDataPath);
			IReadOnlyList<OfficialStatChange> officialChanges = officialReader.Read(options.OfficialHistoryPath);
			IReadOnlyList<ExternalNameMapEntry> nameMapEntries = nameMapReader.Read(options.ExternalNameMapPath);

			using (MediaWikiClient client = new MediaWikiClient(options.HttpTimeout, options.RequestInterval))
			{
				IShikigamiStatusSource externalSource = new MoegirlShikigamiStatusSource(client, new MoegirlHtmlParser(), new ExternalStatusCache(options.CacheDirectoryPath), options.NoNetwork, options.CacheMaxAge);

				DataAuditor auditor = new DataAuditor(new OfficialChangeResolver(), new StatScopeMatcher(), new AuditOutcomeResolver(), externalSource);

				AuditRunResult runResult = auditor.AuditAsync(appStatuses, officialChanges, nameMapEntries).GetAwaiter().GetResult();

				DateTime executedAt = DateTime.Now;

				AuditReportPaths paths = new AuditReportFileWriter(new MarkdownAuditReportWriter(), new CsvAuditReportWriter()).Write(options.OutputDirectoryPath, runResult.Results, executedAt, options.AppDataPath, options.OfficialHistoryPath, options.ExternalNameMapPath);

				writeSummary(runResult.Results, paths);

				if (runResult.Results.Any(result => result.Outcome == AuditOutcome.CONFIRMED_MISMATCH))
				{
					return EXIT_CONFIRMED_MISMATCH;
				}

				return runResult.HasExternalFailure ? EXIT_EXTERNAL_FAILURE : EXIT_SUCCESS;
			}
		}

		private static int generateExternalNameMap(AuditOptions options)
		{
			ShikigamiDataCsvReader appReader = new ShikigamiDataCsvReader();
			ExternalNameMapCsvReader nameMapReader = new ExternalNameMapCsvReader();
			ExternalNameMapCsvWriter writer = new ExternalNameMapCsvWriter();

			IReadOnlyList<ShikigamiStatus> appStatuses = appReader.Read(options.AppDataPath);
			IReadOnlyList<ExternalNameMapEntry> existingEntries = File.Exists(options.ExternalNameMapPath) ? nameMapReader.Read(options.ExternalNameMapPath) : new List<ExternalNameMapEntry>();

			using (MediaWikiClient client = new MediaWikiClient(options.HttpTimeout, options.RequestInterval))
			{
				ExternalNameMapPageValidator pageValidator = new ExternalNameMapPageValidator(client);
				ExternalNameMapGenerator generator = new ExternalNameMapGenerator(pageValidator);

				IReadOnlyList<ExternalNameMapEntry> entries = generator.GenerateAsync(appStatuses, existingEntries).GetAwaiter().GetResult();

				writer.Write(options.ExternalNameMapPath, entries);

				int enabledCount = entries.Count(entry => entry.Enabled);
				int unresolvedCount = entries.Count - enabledCount;

				Console.WriteLine();
				Console.WriteLine("External name map generation completed.");
				Console.WriteLine("Entries:    " + entries.Count);
				Console.WriteLine("Enabled:    " + enabledCount);
				Console.WriteLine("Unresolved: " + unresolvedCount);
				Console.WriteLine("CSV:        " + options.ExternalNameMapPath);
			}

			return EXIT_SUCCESS;
		}

		private static int generateOfficialChangeHistory(AuditOptions options)
		{
			if (!File.Exists(options.OfficialHistoryPath))
			{
				throw new FileNotFoundException("Official change history seed CSV was not found.", options.OfficialHistoryPath);
			}

			ShikigamiDataCsvReader appReader = new ShikigamiDataCsvReader();
			OfficialChangeHistoryCsvReader officialReader = new OfficialChangeHistoryCsvReader();
			OfficialChangeHistoryCsvWriter writer = new OfficialChangeHistoryCsvWriter();

			IReadOnlyList<ShikigamiStatus> appStatuses = appReader.Read(options.AppDataPath);
			IReadOnlyList<OfficialStatChange> seedChanges = officialReader.Read(options.OfficialHistoryPath);

			OfficialAnnouncementCache cache = new OfficialAnnouncementCache(options.CacheDirectoryPath);

			using (OfficialAnnouncementClient client = new OfficialAnnouncementClient(options.HttpTimeout, options.RequestInterval, cache, options.RefreshOfficialCache))
			{
				OfficialAdjustmentSectionParser sectionParser = new OfficialAdjustmentSectionParser();
				OfficialStatChangeDiscovery statChangeDiscovery = new OfficialStatChangeDiscovery(sectionParser);
				OfficialChangeHistorySeedValidator seedValidator = new OfficialChangeHistorySeedValidator();

				OfficialChangeHistoryGenerator generator = new OfficialChangeHistoryGenerator(client, new OfficialAnnouncementParser(), statChangeDiscovery, seedValidator);
				IReadOnlyList<OfficialStatChange> generatedChanges = generator.GenerateAsync(seedChanges, appStatuses).GetAwaiter().GetResult();

				writer.Write(options.OfficialHistoryPath, generatedChanges);

				Console.WriteLine();
				Console.WriteLine("Official change history generation completed.");
				Console.WriteLine("Entries: " + generatedChanges.Count);
				Console.WriteLine("CSV:     " + options.OfficialHistoryPath);
			}

			return EXIT_SUCCESS;
		}

		private static int applyConfirmedChanges(AuditOptions options)
		{
			ShikigamiDataOfficialChangeApplicator applicator = new ShikigamiDataOfficialChangeApplicator(new OfficialChangeResolver(), new StatScopeMatcher(), new AuditOutcomeResolver());

			Console.WriteLine("Applying confirmed official changes.");
			Console.WriteLine("Target: " + options.AppDataPath);
			Console.WriteLine("Source: " + options.OfficialHistoryPath);

			IReadOnlyList<AuditResult> appliedChanges = applicator.Apply(options.AppDataPath, options.OfficialHistoryPath);

			foreach (AuditResult result in appliedChanges)
			{
				string oldValue = result.AppValue.Value.ToString("0.######", CultureInfo.InvariantCulture);
				string newValue = result.OfficialValue.Value.ToString("0.######", CultureInfo.InvariantCulture);

				Console.WriteLine("[UPDATE] " + result.Rarity + " " + result.ShikigamiName + " " + StatTypeDefinition.GetDisplayName(result.StatType) + ": " + oldValue + " -> " + newValue);
			}

			Console.WriteLine();
			Console.WriteLine("Confirmed official change application completed.");
			Console.WriteLine("Updated: " + appliedChanges.Count);
			Console.WriteLine("CSV:     " + options.AppDataPath);

			return EXIT_SUCCESS;
		}

		private static void writeSummary(IReadOnlyList<AuditResult> results, AuditReportPaths paths)
		{
			Console.WriteLine("Shikigami data audit completed.");

			foreach (AuditOutcome outcome in Enum.GetValues(typeof(AuditOutcome)))
			{
				int count = results.Count(result => result.Outcome == outcome);

				if (count > 0)
				{
					Console.WriteLine($"  {outcome}: {count}");
				}
			}

			Console.WriteLine("Markdown: " + paths.MarkdownPath);
			Console.WriteLine("CSV:      " + paths.CsvPath);
		}
	}
}
