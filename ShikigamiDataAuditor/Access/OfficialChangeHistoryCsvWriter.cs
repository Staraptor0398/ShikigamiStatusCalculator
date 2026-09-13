using ShikigamiDataAuditor.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ShikigamiDataAuditor.Access
{
	public class OfficialChangeHistoryCsvWriter
	{
		public void Write(string filePath, IEnumerable<OfficialStatChange> changes)
		{
			if (string.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException("File path is empty.", nameof(filePath));
			}

			if (changes == null)
			{
				throw new ArgumentNullException(nameof(changes));
			}

			string directoryPath = Path.GetDirectoryName(filePath);

			if (!string.IsNullOrWhiteSpace(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			StringBuilder builder = new StringBuilder();

			builder.AppendLine("effective_date,announcement_date,rarity,shikigami,stat_scope,stat,old_value,new_value,unit,old_value_known,current_app_scope,review_priority,app_action,source_url,notes");

			foreach (OfficialStatChange change in changes)
			{
				builder.Append(change.EffectiveDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
				builder.Append(",");
				builder.Append(change.AnnouncementDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
				builder.Append(",");
				builder.Append(escape(change.Rarity));
				builder.Append(",");
				builder.Append(escape(change.ShikigamiName));
				builder.Append(",");
				builder.Append(getScopeName(change.StatScope));
				builder.Append(",");
				builder.Append(getStatName(change.StatType));
				builder.Append(",");
				builder.Append(change.OldValue.HasValue ? change.OldValue.Value.ToString(CultureInfo.InvariantCulture) : "");
				builder.Append(",");
				builder.Append(change.NewValue.ToString(CultureInfo.InvariantCulture));
				builder.Append(",");
				builder.Append(escape(change.Unit));
				builder.Append(",");
				builder.Append(change.OldValueKnown ? "true" : "false");
				builder.Append(",");
				builder.Append(change.CurrentAppScope ? "true" : "false");
				builder.Append(",");
				builder.Append(escape(change.ReviewPriority));
				builder.Append(",");
				builder.Append(escape(change.AppAction));
				builder.Append(",");
				builder.Append(escape(change.SourceUrl));
				builder.Append(",");
				builder.Append(escape(change.Notes));
				builder.AppendLine();
			}

			writeAtomically(filePath, builder.ToString());
		}

		private static void writeAtomically(string filePath, string content)
		{
			string temporaryFilePath = filePath + ".tmp";

			try
			{
				File.WriteAllText(temporaryFilePath, content, new UTF8Encoding(true));

				if (File.Exists(filePath))
				{
					File.Replace(temporaryFilePath, filePath, null);
				}
				else
				{
					File.Move(temporaryFilePath, filePath);
				}
			}
			finally
			{
				if (File.Exists(temporaryFilePath))
				{
					File.Delete(temporaryFilePath);
				}
			}
		}

		private static string getScopeName(StatScope scope)
		{
			switch (scope)
			{
				case StatScope.INITIAL: return "initial";
				case StatScope.BASE: return "base";
				case StatScope.MAX_LEVEL_BASE: return "max_level_base";
				case StatScope.AWAKENED: return "awakened";
				case StatScope.AWAKENED_BASE: return "awakened_base";
				case StatScope.AWAKENED_LEVEL_40: return "awakened_level_40";
				case StatScope.AWAKENING_BONUS: return "awakening_bonus";
				case StatScope.UNKNOWN: return "unknown";
				default: throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
			}
		}

		private static string getStatName(StatType statType)
		{
			switch (statType)
			{
				case StatType.ATTACK: return "attack";
				case StatType.HP: return "hp";
				case StatType.DEFENSE: return "defense";
				case StatType.SPEED: return "speed";
				case StatType.CRITICAL_RATE: return "critical_rate";
				case StatType.CRITICAL_DAMAGE: return "critical_damage";
				case StatType.EFFECT_HIT: return "effect_hit";
				case StatType.EFFECT_RESIST: return "effect_resist";
				default: throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
			}
		}

		private static string escape(string value)
		{
			string text = value ?? "";

			if (!text.Contains(",") && !text.Contains("\"") && !text.Contains("\r") && !text.Contains("\n"))
			{
				return text;
			}

			return "\"" + text.Replace("\"", "\"\"") + "\"";
		}
	}
}
