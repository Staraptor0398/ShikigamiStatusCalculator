using SaveData.Definition;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

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

			JavaScriptSerializer serializer = new JavaScriptSerializer();
			serializer.MaxJsonLength = int.MaxValue;
			Dictionary<string, object> root = serializer.DeserializeObject(json) as Dictionary<string, object>;
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
						root = new Dictionary<string, object>
						{
							["Version"] = 2,
							["Data"] = root
						};
						version = 2;
						break;
					default:
						throw new NotSupportedException($"Unsupported save data version: {version}");
				}
			}
			return serializer.Serialize(root);
		}

		private static void migrateV1ToV2(Dictionary<string, object> root, SaveDataType type)
		{
			switch (type)
			{
				case SaveDataType.MitamaSet:
					migrateMitamaSet(root);
					break;
				case SaveDataType.Build:
				case SaveDataType.CalculationSnapshot:
					migrateMitamaSet(root.TryGetValue("MitamaSet", out object value)
						? value as Dictionary<string, object> : null);
					break;
				default:
					throw new NotSupportedException($"Unsupported save data type: {type}");
			}
		}

		private static void migrateMitamaSet(Dictionary<string, object> set)
		{
			if (set == null)
			{
				return;
			}
			migrateEffects(set.TryGetValue("SetEffects", out object effects) ? effects as object[] : null);
			migrateEffects(set.TryGetValue("UniqueEffects", out object unique) ? unique as object[] : null);
		}

		private static void migrateEffects(object[] effects)
		{
			if (effects == null)
			{
				return;
			}
			foreach (object item in effects)
			{
				Dictionary<string, object> effect = item as Dictionary<string, object>;
				if (effect == null)
				{
					continue;
				}
				effect.TryGetValue("Type", out object type);
				effect.TryGetValue("Value", out object value);
				effect.Remove("Type");
				effect.Remove("Value");
				effect["Stat"] = new Dictionary<string, object>
				{
					["Type"] = type,
					["Value"] = value
				};
			}
		}
	}
}
