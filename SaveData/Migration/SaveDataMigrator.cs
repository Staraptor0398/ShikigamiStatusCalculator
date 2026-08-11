using Newtonsoft.Json.Linq;
using SaveData.Definition;
using System;

namespace SaveData.Migration
{
	public static class SaveDataMigrator
	{
		public static JObject Migrate(JObject root, SaveDataVersion targetVersion, int sourceVersion)
		{
			if (root == null)
			{
				return null;
			}

			if (targetVersion == null)
			{
				throw new ArgumentNullException(nameof(targetVersion));
			}

			if (sourceVersion > targetVersion.Version)
			{
				throw new NotSupportedException($"Downgrade migration is not supported: {sourceVersion} -> {targetVersion.Version}");
			}

			int version = sourceVersion;

			while (version < targetVersion.Version)
			{
				switch (version)
				{
					case 1:
						root = migrateV1ToV2(root, targetVersion.Type);
						version = 2;
						break;

					default:
						throw new NotSupportedException($"Unsupported save data version: {version}");
				}
			}

			return root;
		}

		private static JObject migrateV1ToV2(JObject root, SaveDataType saveDataType)
		{
			switch (saveDataType)
			{
				case SaveDataType.MitamaSet:
					migrateMitamaSetV1ToV2(root);
					break;

				case SaveDataType.Build:
					migrateMitamaSetV1ToV2(
					root["MitamaSet"] as JObject);
					break;

				case SaveDataType.CalculationSnapshot:
					migrateMitamaSetV1ToV2(
					root["MitamaSet"] as JObject);
					break;

				default:
					throw new NotSupportedException(
					$"Unsupported save data type: {saveDataType}");
			}

			return createV2Envelope(root);
		}

		private static void migrateMitamaSetV1ToV2(JObject mitamaSet)
		{
			if (mitamaSet == null)
			{
				return;
			}

			migrateSetEffectListV1ToV2(
			mitamaSet["SetEffects"] as JArray);

			migrateSetEffectListV1ToV2(
			mitamaSet["UniqueEffects"] as JArray);
		}

		private static void migrateSetEffectListV1ToV2(JArray effects)
		{
			if (effects == null)
			{
				return;
			}

			foreach (JToken token in effects)
			{
				if (!(token is JObject effect))
				{
					continue;
				}

				JToken type = effect["Type"];
				JToken value = effect["Value"];

				effect.Remove("Type");
				effect.Remove("Value");

				effect["Stat"] =
				new JObject
				{
					["Type"] = type,
					["Value"] = value
				};
			}
		}

		private static JObject createV2Envelope(JObject data)
		{
			return new JObject
			{
				["Version"] = 2,
				["Data"] = data
			};
		}
	}
}
