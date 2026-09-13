using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ShikigamiDataAuditor.Report
{
	public class CsvAuditReportWriter
	{
		public string Write(IReadOnlyList<AuditResult> results)
		{
			List<string> lines = new List<string>
			{
				"outcome,rarity,shikigami,stat,app_value,official_value,reference_value,stat_scope,evidence,source_url,fetched_at,notes"
			};
			lines.AddRange(results.Select(writeRow));
			return string.Join(Environment.NewLine, lines) + Environment.NewLine;
		}

		private static string writeRow(AuditResult result)
		{
			return string.Join(",", new[]
			{
				escape(result.Outcome.ToString()),
				escape(result.Rarity),
				escape(result.ShikigamiName),
				escape(result.StatType.ToString()),
				escape(formatNumber(result.AppValue)),
				escape(formatNumber(result.OfficialValue)),
				escape(formatNumber(result.ReferenceValue)),
				escape(result.StatScope.ToString()),
				escape(result.Evidence),
				escape(result.SourceUrl),
				escape(result.FetchedAt.HasValue ? result.FetchedAt.Value.ToString("o") : ""),
				escape(result.Notes)
			});
		}

		private static string formatNumber(double? value)
		{
			return value.HasValue ? value.Value.ToString("0.######", CultureInfo.InvariantCulture) : "";
		}

		private static string escape(string value)
		{
			string text = value ?? "";
			if (text.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
			{
				return text;
			}
			return "\"" + text.Replace("\"", "\"\"") + "\"";
		}
	}
}
