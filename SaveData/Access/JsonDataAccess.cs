using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace SaveData.Access
{
	public static class JsonDataAccess
	{
		private static readonly JsonSerializerOptions mOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			WriteIndented = true
		};

		public static void Save<T>(string path, T data)
		{
			string json = JsonSerializer.Serialize(data, mOptions);
			File.WriteAllText(path, json, new UTF8Encoding(false));
		}

		public static T Load<T>(string path)
		{
			return Deserialize<T>(ReadText(path));
		}

		public static string ReadText(string path, Func<string, IDisposable> track = null)
		{
			using (track?.Invoke("SaveData.Json.ReadFile"))
			{
				return File.ReadAllText(path, Encoding.UTF8);
			}
		}

		public static int GetVersion(string json)
		{
			using (JsonDocument document = JsonDocument.Parse(json))
			{
				JsonElement root = document.RootElement;
				if (root.TryGetProperty("Version", out JsonElement version))
				{
					return version.GetInt32();
				}
				return 1;
			}
		}

		public static T Deserialize<T>(string json, Func<string, IDisposable> track = null)
		{
			using (track?.Invoke("SaveData.Json.Deserialize"))
			{
				return JsonSerializer.Deserialize<T>(json, mOptions);
			}
		}
	}
}
