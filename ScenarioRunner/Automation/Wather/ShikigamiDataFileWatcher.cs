using System;
using System.IO;

namespace ScenarioRunner.Automation.Watcher
{
	public class ShikigamiDataFileWatcher : IDisposable
	{
		private readonly FileSystemWatcher mWatcher;

		public event Action<string> FileCreated;

		public ShikigamiDataFileWatcher(string directoryPath)
		{
			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				throw new ArgumentException("Directory path is empty.", nameof(directoryPath));
			}

			if (!Directory.Exists(directoryPath))
			{
				throw new DirectoryNotFoundException($"Directory was not found: {directoryPath}");
			}

			mWatcher = new FileSystemWatcher(directoryPath)
			{
				NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,

				IncludeSubdirectories = false,
				EnableRaisingEvents = false
			};

			mWatcher.Created += onFileCreated;
			mWatcher.Renamed += onFileRenamed;
		}

		public void Start()
		{
			mWatcher.EnableRaisingEvents = true;
		}

		public void Stop()
		{
			mWatcher.EnableRaisingEvents = false;
		}

		private void onFileCreated(object sender, FileSystemEventArgs e)
		{
			FileCreated?.Invoke(e.FullPath);
		}

		private void onFileRenamed(object sender, RenamedEventArgs e)
		{
			FileCreated?.Invoke(e.FullPath);
		}

		public void Dispose()
		{
			mWatcher.EnableRaisingEvents = false;

			mWatcher.Created -= onFileCreated;
			mWatcher.Renamed -= onFileRenamed;

			mWatcher.Dispose();
		}
	}
}
