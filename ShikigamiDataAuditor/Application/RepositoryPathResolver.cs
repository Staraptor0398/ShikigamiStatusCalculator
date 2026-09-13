using System;
using System.IO;

namespace ShikigamiDataAuditor.Application
{
	public static class RepositoryPathResolver
	{
		public static string FindRepositoryRoot()
		{
			string fromCurrentDirectory = findFrom(Environment.CurrentDirectory);

			if (fromCurrentDirectory != null)
			{
				return fromCurrentDirectory;
			}

			string fromApplicationDirectory = findFrom(AppDomain.CurrentDomain.BaseDirectory);

			return fromApplicationDirectory ?? Environment.CurrentDirectory;
		}

		private static string findFrom(string startPath)
		{
			DirectoryInfo directory = new DirectoryInfo(startPath);

			while (directory != null)
			{
				if (File.Exists(Path.Combine(directory.FullName, "Gui", "Data", "ShikigamiData.csv")))
				{
					return directory.FullName;
				}

				directory = directory.Parent;
			}

			return null;
		}
	}
}
