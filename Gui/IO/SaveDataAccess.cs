using System.IO;
using System.Text;
using Newtonsoft.Json;
using Gui.SaveData;

namespace Gui.IO
{
	public static class SaveDataAccess
	{
		public static void SaveBuild(string path, BuildSaveData data)
		{
			save(path, data);
		}

		public static BuildSaveData LoadBuild(string path)
		{
			return load<BuildSaveData>(path);
		}

		public static void SaveMitamaSet(string path, MitamaSetSaveData data)
		{
			save(path, data);
		}

		public static MitamaSetSaveData LoadMitamaSet(string path)
		{
			return load<MitamaSetSaveData>(path);
		}

		private static void save<T>(string path, T data)
		{
			string json = JsonConvert.SerializeObject(data, Formatting.Indented);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
			using (var writer = new StreamWriter(stream, Encoding.UTF8))
			{
				writer.Write(json);
			}
		}

		private static T load<T>(string path) {
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				string json = reader.ReadToEnd();

				return JsonConvert.DeserializeObject<T>(json);
			}
		}
	}
}