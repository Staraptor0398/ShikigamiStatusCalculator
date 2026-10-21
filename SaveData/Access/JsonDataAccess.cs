using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;

namespace SaveData.Access
{
	public static class JsonDataAccess
	{
		public static void Save<T>(string path, T data)
		{
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
			using (MemoryStream stream = new MemoryStream())
			{
				serializer.WriteObject(stream, data);
				File.WriteAllText(path, Encoding.UTF8.GetString(stream.ToArray()), new UTF8Encoding(false));
			}
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
			// This version probe is intentionally separate from DataContractJsonSerializer.
			JavaScriptSerializer parser = new JavaScriptSerializer();
			parser.MaxJsonLength = int.MaxValue;
			var root = parser.DeserializeObject(json) as System.Collections.Generic.Dictionary<string, object>;
			if (root == null)
			{
				throw new FormatException("Save data root must be an object.");
			}
			if (!root.TryGetValue("Version", out object version))
			{
				return 1;
			}
			return Convert.ToInt32(version);
		}

		public static T Deserialize<T>(string json, Func<string, IDisposable> track = null)
		{
			using (track?.Invoke("SaveData.Json.Deserialize"))
			{
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
				using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
				{
					return (T)serializer.ReadObject(stream);
				}
			}
		}
	}
}
