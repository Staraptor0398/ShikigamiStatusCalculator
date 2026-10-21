using SaveData.Definition;
using System;
using System.Text.Json.Nodes;

namespace SaveData.Migration
{
	public static class SaveDataMigrator
	{
		public static string Migrate(string json, SaveDataVersion targetVersion, int sourceVersion)
		{
			if (json == null)
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

			JsonObject root = JsonNode.Parse(json) as JsonObject;
			if (root == null)
			{
				throw new FormatException("Save data root must be an object.");
			}

			int version = sourceVersion;
			while (version < targetVersion.Version)
			{
				switch (version)
				{
					case 1:
						migrateV1ToV2(root, targetVersion.Type);
						root = new JsonObject
						{
							["Version"] = 2,
							["Data"] = root.DeepClone()
						};
						version = 2;
						break;

					default:
						throw new NotSupportedException($"Unsupported save data version: {version}");
				}
			}

			return root.ToJsonString();
		}

		private static void migrateV1ToV2(JsonObject root, SaveDataType type)
		{
			switch (type)
			{
				case SaveDataType.MitamaSet:
					migrateMitamaSet(root);
					break;
				case SaveDataType.Build:
				case SaveDataType.CalculationSnapshot:
					migrateMitamaSet(root["MitamaSet"] as JsonObject);
					break;
				default:
					throw new NotSupportedException($"Unsupported save data type: {type}");
			}
		}

		private static void migrateMitamaSet(JsonObject set)
		{
			if (set == null)
			{
				return;
			}
			migrateEffects(set["SetEffects"] as JsonArray);
			migrateEffects(set["UniqueEffects"] as JsonArray);
		}

		private static void migrateEffects(JsonArray effects)
		{
			if (effects == null)
			{
				return;
			}

			foreach (JsonNode item in effects)
			{
				if (!(item is JsonObject effect))
				{
					continue;
				}

				JsonNode type = effect["Type"]?.DeepClone();
				JsonNode value = effect["Value"]?.DeepClone();
				effect.Remove("Type");
				effect.Remove("Value");
				effect["Stat"] = new JsonObject
				{
					["Type"] = type,
					["Value"] = value
				};
			}
		}
	}
}
