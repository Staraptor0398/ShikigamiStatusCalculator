using ShikigamiDataAuditor.Access;
using ShikigamiDataAuditor.Audit;
using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ShikigamiDataAuditor.Application
{
	public class ShikigamiDataOfficialChangeApplicator
	{
		private readonly OfficialChangeResolver mOfficialChangeResolver;
		private readonly StatScopeMatcher mStatScopeMatcher;
		private readonly AuditOutcomeResolver mAuditOutcomeResolver;

		public ShikigamiDataOfficialChangeApplicator(OfficialChangeResolver officialChangeResolver, StatScopeMatcher statScopeMatcher, AuditOutcomeResolver auditOutcomeResolver)
		{
			mOfficialChangeResolver = officialChangeResolver;
			mStatScopeMatcher = statScopeMatcher;
			mAuditOutcomeResolver = auditOutcomeResolver;
		}

		public IReadOnlyList<AuditResult> Apply(string appDataPath, string officialHistoryPath)
		{
			ShikigamiDataCsvReader appReader = new ShikigamiDataCsvReader();
			OfficialChangeHistoryCsvReader officialReader = new OfficialChangeHistoryCsvReader();

			IReadOnlyList<ShikigamiStatus> appStatuses = appReader.Read(appDataPath);
			IReadOnlyList<OfficialStatChange> officialChanges = officialReader.Read(officialHistoryPath);
			IReadOnlyList<OfficialStatChange> latestOfficialChanges = mOfficialChangeResolver.ResolveLatest(officialChanges, DateTime.Today);

			Dictionary<string, ShikigamiStatus> appByKey = appStatuses.ToDictionary(status => status.GetKey(), status => status, StringComparer.OrdinalIgnoreCase);
			List<AuditResult> appliedChanges = new List<AuditResult>();

			foreach (OfficialStatChange change in latestOfficialChanges)
			{
				if (!change.CurrentAppScope || !mStatScopeMatcher.IsOfficialComparableToApp(change))
				{
					continue;
				}

				ShikigamiStatus appStatus;

				if (!appByKey.TryGetValue(change.GetShikigamiKey(), out appStatus))
				{
					continue;
				}

				double appValue;

				if (!appStatus.TryGetValue(change.StatType, out appValue))
				{
					continue;
				}

				if (mAuditOutcomeResolver.ResolveOfficial(appValue, change.NewValue) != AuditOutcome.CONFIRMED_MISMATCH)
				{
					continue;
				}

				Dictionary<StatType, double> values = appStatus.Values.ToDictionary(pair => pair.Key, pair => pair.Value);
				values[change.StatType] = change.NewValue;
				appStatus.Values = values;

				appliedChanges.Add(new AuditResult
				{
					Outcome = AuditOutcome.CONFIRMED_MISMATCH,
					Rarity = change.Rarity,
					ShikigamiName = change.ShikigamiName,
					StatType = change.StatType,
					AppValue = appValue,
					OfficialValue = change.NewValue,
					StatScope = change.StatScope,
					Evidence = "Applied latest comparable Japanese official change.",
					SourceUrl = change.SourceUrl,
					Notes = change.Notes
				});
			}

			if (appliedChanges.Count > 0)
			{
				write(appDataPath, appStatuses);
			}

			return appliedChanges;
		}

		private static void write(string filePath, IReadOnlyList<ShikigamiStatus> statuses)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append("レア度,式神名,攻撃力,HP,防御力,素早さ,会心率,会心DMG,効果命中,効果抵抗\r\n");

			foreach (ShikigamiStatus status in statuses)
			{
				builder.Append(escape(status.Rarity));
				builder.Append(',');
				builder.Append(escape(status.Name));
				builder.Append(',');
				builder.Append(format(status.Values[StatType.ATTACK]));
				builder.Append(',');
				builder.Append(format(status.Values[StatType.HP]));
				builder.Append(',');
				builder.Append(format(status.Values[StatType.DEFENSE]));
				builder.Append(',');
				builder.Append(format(status.Values[StatType.SPEED]));
				builder.Append(',');
				builder.Append(format(status.Values[StatType.CRITICAL_RATE]));
				builder.Append(',');
				builder.Append(format(status.Values[StatType.CRITICAL_DAMAGE]));
				builder.Append(',');
				builder.Append(format(status.Values[StatType.EFFECT_HIT]));
				builder.Append(',');
				builder.Append(format(status.Values[StatType.EFFECT_RESIST]));
				builder.Append("\r\n");
			}

			string tempPath = filePath + ".tmp";

			try
			{
				File.WriteAllText(tempPath, builder.ToString(), new UTF8Encoding(false));
				File.Copy(tempPath, filePath, true);
			}
			finally
			{
				if (File.Exists(tempPath))
				{
					File.Delete(tempPath);
				}
			}
		}

		private static string format(double value)
		{
			return value.ToString("F6", CultureInfo.InvariantCulture);
		}

		private static string escape(string value)
		{
			string normalized = value ?? "";

			if (normalized.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
			{
				return normalized;
			}

			return "\"" + normalized.Replace("\"", "\"\"") + "\"";
		}
	}
}
