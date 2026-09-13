using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ShikigamiDataAuditor.Official
{
	public class OfficialAnnouncementCache
	{
		private const string CACHE_DIRECTORY_NAME = "official";

		private readonly string mCacheDirectoryPath;

		public OfficialAnnouncementCache(string cacheDirectoryPath)
		{
			if (string.IsNullOrWhiteSpace(cacheDirectoryPath))
			{
				throw new ArgumentException("Cache directory path is empty.", nameof(cacheDirectoryPath));
			}

			mCacheDirectoryPath = Path.Combine(cacheDirectoryPath, CACHE_DIRECTORY_NAME);
		}

		public bool TryRead(string sourceUrl, out string html)
		{
			return TryRead(sourceUrl, null, out html);
		}

		public bool TryRead(string sourceUrl, TimeSpan? maxAge, out string html)
		{
			string cacheFilePath = getCacheFilePath(sourceUrl);

			if (!File.Exists(cacheFilePath))
			{
				html = null;
				return false;
			}

			if (maxAge.HasValue)
			{
				DateTime lastWriteTime = File.GetLastWriteTimeUtc(cacheFilePath);
				TimeSpan age = DateTime.UtcNow - lastWriteTime;

				if (age > maxAge.Value)
				{
					html = null;
					return false;
				}
			}

			html = File.ReadAllText(cacheFilePath, Encoding.UTF8);
			return true;
		}

		public void Write(string sourceUrl, string html)
		{
			if (html == null)
			{
				throw new ArgumentNullException(nameof(html));
			}

			Directory.CreateDirectory(mCacheDirectoryPath);

			string cacheFilePath = getCacheFilePath(sourceUrl);
			string temporaryFilePath = cacheFilePath + ".tmp";

			File.WriteAllText(temporaryFilePath, html, new UTF8Encoding(false));

			if (File.Exists(cacheFilePath))
			{
				File.Delete(cacheFilePath);
			}

			File.Move(temporaryFilePath, cacheFilePath);
		}

		private string getCacheFilePath(string sourceUrl)
		{
			if (string.IsNullOrWhiteSpace(sourceUrl))
			{
				throw new ArgumentException("Source URL is empty.", nameof(sourceUrl));
			}

			return Path.Combine(mCacheDirectoryPath, createCacheFileName(sourceUrl));
		}

		private static string createCacheFileName(string sourceUrl)
		{
			using (SHA256 sha256 = SHA256.Create())
			{
				byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(sourceUrl));
				StringBuilder builder = new StringBuilder(hash.Length * 2);

				foreach (byte value in hash)
				{
					builder.Append(value.ToString("x2"));
				}

				return builder + ".html";
			}
		}
	}
}
