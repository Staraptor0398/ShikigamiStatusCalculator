using SaveData.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SaveData.Access
{
	public static class JsonDataAccess
	{
		public static void Save<T>(string path, T data)
		{
			File.WriteAllText(path, JsonSerializer.Write(data), new UTF8Encoding(false));
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
			Dictionary<string, object> root = JsonParser.Parse(json) as Dictionary<string, object>;

			if (root == null)
			{
				throw new FormatException("Save data root must be an object.");
			}

			return root.TryGetValue("Version", out object value) ? (int)JsonObjectConverter.ConvertTo(value, typeof(int)) : 1;
		}

		public static T Deserialize<T>(string json, Func<string, IDisposable> track = null)
		{
			using (track?.Invoke("SaveData.Json.Deserialize"))
			{
				return (T)JsonObjectConverter.ConvertTo(JsonParser.Parse(json), typeof(T));
			}
		}
	}
}
