using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ShikigamiDataAuditor.Report
{
	public class MarkdownAuditReportWriter
	{
		public string Write(IReadOnlyList<AuditResult> results, DateTime executedAt, string appDataPath, string officialHistoryPath, string externalNameMapPath)
		{
			StringBuilder builder = new StringBuilder();

			builder.AppendLine("# 式神データ監査結果");
			builder.AppendLine();
			builder.AppendLine("- 実行日時: " + executedAt.ToString("yyyy-MM-dd HH:mm:ss zzz"));
			builder.AppendLine("- アプリデータ: `" + appDataPath + "`");
			builder.AppendLine("- 公式変更履歴: `" + officialHistoryPath + "`");
			builder.AppendLine("- 外部名称対応: `" + externalNameMapPath + "`");
			builder.AppendLine();
			builder.AppendLine("## 集計");
			builder.AppendLine();
			builder.AppendLine("| 判定 | 件数 |");
			builder.AppendLine("|---|---:|");

			foreach (AuditOutcome outcome in Enum.GetValues(typeof(AuditOutcome)))
			{
				builder.AppendLine($"| {outcome} | {results.Count(result => result.Outcome == outcome)} |");
			}

			foreach (IGrouping<AuditOutcome, AuditResult> group in results.GroupBy(result => result.Outcome).OrderBy(group => group.Key))
			{
				builder.AppendLine();
				builder.AppendLine("## " + group.Key);
				builder.AppendLine();
				builder.AppendLine("| レア度 | 式神 | ステータス | アプリ | 公式 | 外部 | 範囲 | 根拠 | 出典 |");
				builder.AppendLine("|---|---|---|---:|---:|---:|---|---|---|");

				foreach (AuditResult result in group)
				{
					builder.AppendLine("| " + string.Join(" | ", new[]
					{
						escape(result.Rarity),
						escape(result.ShikigamiName),
						escape(StatTypeDefinition.GetDisplayName(result.StatType)),
						formatNumber(result.AppValue),
						formatNumber(result.OfficialValue),
						formatNumber(result.ReferenceValue),
						escape(result.StatScope.ToString()),
						escape(result.Evidence),
						formatLink(result.SourceUrl)
					}) + " |");
				}
			}

			return builder.ToString();
		}

		private static string formatNumber(double? value)
		{
			return value.HasValue ? value.Value.ToString("0.######", CultureInfo.InvariantCulture) : "-";
		}

		private static string formatLink(string url)
		{
			return string.IsNullOrWhiteSpace(url) ? "-" : "[確認](" + url.Replace(")", "%29") + ")";
		}

		private static string escape(string value)
		{
			return (value ?? "-").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
		}
	}
}
