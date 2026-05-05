using System.IO;
using System.Text;
using Newtonsoft.Json;
using Gui.SaveData;

namespace Gui.IO
{
	public static class SaveDataAccess
	{
		public static void Save(string path, SaveData.SaveData data)
		{
			string json=JsonConvert.SerializeObject(data, Formatting.Indented);

			using(var stream = new FileStream(path,FileMode.Create,FileAccess.Write,FileShare.None))
			using (var writer = new StreamWriter(stream, Encoding.UTF8))
			{
				writer.Write(json);
			}
		}

		public static SaveData.SaveData Load(string path)
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				string json = reader.ReadToEnd();

				return JsonConvert.DeserializeObject<SaveData.SaveData>(json);
			}
		}
	}
}