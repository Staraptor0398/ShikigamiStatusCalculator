using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

namespace SaveData.Access
{
	public static class JsonDataAccess
	{
		public static void Save<T>(string path, T data)
		{
			string json = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(path, json, new UTF8Encoding(false));
		}

		public static T Load<T>(string path)
		{
			string json = File.ReadAllText(path, Encoding.UTF8);
			return JsonConvert.DeserializeObject<T>(json);
		}

		public static JObject LoadObject(string path)
		{
			string json = File.ReadAllText(path, Encoding.UTF8);
			return JObject.Parse(json);
		}
	}
}
