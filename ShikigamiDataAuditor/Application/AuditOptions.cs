using System;
using System.Globalization;
using System.IO;

namespace ShikigamiDataAuditor.Application
{
	public class AuditOptions
	{
		public string AppDataPath { get; set; }
		public string OfficialHistoryPath { get; set; }
		public string ExternalNameMapPath { get; set; }
		public string OutputDirectoryPath { get; set; }
		public string CacheDirectoryPath { get; set; }
		public bool GenerateExternalNameMap { get; set; }
		public bool GenerateOfficialChangeHistory { get; set; }
		public bool ApplyConfirmedChanges { get; set; }
		public bool RefreshOfficialCache { get; set; }
		public bool NoNetwork { get; set; }
		public bool ShowHelp { get; set; }
		public TimeSpan CacheMaxAge { get; set; }
		public TimeSpan RequestInterval { get; set; }
		public TimeSpan HttpTimeout { get; set; }

		public static AuditOptions Parse(string[] args)
		{
			string repositoryRoot = RepositoryPathResolver.FindRepositoryRoot();

			AuditOptions options = new AuditOptions
			{
				AppDataPath = Path.Combine(repositoryRoot, "Gui", "Data", "ShikigamiData.csv"),
				OfficialHistoryPath = Path.Combine(repositoryRoot, "ShikigamiDataAuditor", "Data", "OfficialChangeHistory.csv"),
				ExternalNameMapPath = Path.Combine(repositoryRoot, "ShikigamiDataAuditor", "Data", "ExternalNameMap.csv"),
				OutputDirectoryPath = Path.Combine(repositoryRoot, "ShikigamiDataAuditor", "Audit"),
				CacheDirectoryPath = Path.Combine(repositoryRoot, "ShikigamiDataAuditor", "Cache"),
				CacheMaxAge = TimeSpan.FromDays(7),
				RequestInterval = TimeSpan.FromSeconds(2),
				HttpTimeout = TimeSpan.FromSeconds(30)
			};

			for (int index = 0; index < args.Length; index++)
			{
				switch (args[index])
				{
					case "--app-data": options.AppDataPath = Path.GetFullPath(getValue(args, ref index)); break;
					case "--official-history": options.OfficialHistoryPath = Path.GetFullPath(getValue(args, ref index)); break;
					case "--external-name-map": options.ExternalNameMapPath = Path.GetFullPath(getValue(args, ref index)); break;
					case "--output": options.OutputDirectoryPath = Path.GetFullPath(getValue(args, ref index)); break;
					case "--cache": options.CacheDirectoryPath = Path.GetFullPath(getValue(args, ref index)); break;
					case "--generate-external-name-map": options.GenerateExternalNameMap = true; break;
					case "--generate-official-change-history": options.GenerateOfficialChangeHistory = true; break;
					case "--apply-confirmed-changes": options.ApplyConfirmedChanges = true; break;
					case "--refresh-official-cache": options.RefreshOfficialCache = true; break;
					case "--no-network": options.NoNetwork = true; break;
					case "--cache-max-age": options.CacheMaxAge = parseDuration(getValue(args, ref index)); break;
					case "--request-interval": options.RequestInterval = parseDuration(getValue(args, ref index)); break;
					case "--http-timeout": options.HttpTimeout = parseDuration(getValue(args, ref index)); break;
					case "--help": options.ShowHelp = true; break;
					default: throw new ArgumentException("Unknown option: " + args[index]);
				}
			}

			return options;
		}

		public static string GetUsage()
		{
			return "Usage: ShikigamiDataAuditor.exe [--app-data PATH] [--official-history PATH] " +
				"[--external-name-map PATH] [--output DIRECTORY] [--cache DIRECTORY] [--generate-external-name-map] " +
				"[--generate-official-change-history] [--apply-confirmed-changes] [--refresh-official-cache] [--no-network] " +
				"[--cache-max-age 7d] [--request-interval 2s] [--http-timeout 30s]";
		}

		private static string getValue(string[] args, ref int index)
		{
			index++;

			if (index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
			{
				throw new ArgumentException("Option value is missing.");
			}

			return args[index];
		}

		private static TimeSpan parseDuration(string value)
		{
			string normalized = value.Trim().ToLowerInvariant();

			double number;

			if (normalized.EndsWith("ms") && double.TryParse(normalized.Substring(0, normalized.Length - 2), NumberStyles.Float, CultureInfo.InvariantCulture, out number))
			{
				return TimeSpan.FromMilliseconds(number);
			}

			if (normalized.EndsWith("s") && double.TryParse(normalized.Substring(0, normalized.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out number))
			{
				return TimeSpan.FromSeconds(number);
			}

			if (normalized.EndsWith("m") && double.TryParse(normalized.Substring(0, normalized.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out number))
			{
				return TimeSpan.FromMinutes(number);
			}

			if (normalized.EndsWith("h") && double.TryParse(normalized.Substring(0, normalized.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out number))
			{
				return TimeSpan.FromHours(number);
			}

			if (normalized.EndsWith("d") && double.TryParse(normalized.Substring(0, normalized.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out number))
			{
				return TimeSpan.FromDays(number);
			}

			throw new FormatException("Invalid duration: " + value);
		}
	}
}
