using ShikigamiDataAuditor.Model;
using ShikigamiDataAuditor.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShikigamiDataAuditor.Access
{
	public class AuditReportFileWriter
	{
		private readonly MarkdownAuditReportWriter mMarkdownWriter;
		private readonly CsvAuditReportWriter mCsvWriter;

		public AuditReportFileWriter(MarkdownAuditReportWriter markdownWriter, CsvAuditReportWriter csvWriter)
		{
			mMarkdownWriter = markdownWriter;
			mCsvWriter = csvWriter;
		}

		public AuditReportPaths Write(string outputDirectoryPath, IReadOnlyList<AuditResult> results, DateTime executedAt, string appDataPath, string officialHistoryPath, string externalNameMapPath)
		{
			Directory.CreateDirectory(outputDirectoryPath);

			string baseName = "ShikigamiDataAudit_" + executedAt.ToString("yyyyMMdd_HHmmss");
			string markdownPath = Path.Combine(outputDirectoryPath, baseName + ".md");
			string csvPath = Path.Combine(outputDirectoryPath, baseName + ".csv");

			File.WriteAllText(markdownPath, mMarkdownWriter.Write(results, executedAt, appDataPath, officialHistoryPath, externalNameMapPath), new UTF8Encoding(true));

			File.WriteAllText(csvPath, mCsvWriter.Write(results), new UTF8Encoding(true));

			return new AuditReportPaths
			{
				MarkdownPath = markdownPath,
				CsvPath = csvPath
			};
		}
	}

	public class AuditReportPaths
	{
		public string MarkdownPath { get; set; }
		public string CsvPath { get; set; }
	}
}
