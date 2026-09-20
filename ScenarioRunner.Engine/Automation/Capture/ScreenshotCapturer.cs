using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace ScenarioRunner.Automation.Capture
{
	public class ScreenshotCapturer
	{
		private readonly string mCaptureDirectoryPath;

		public ScreenshotCapturer(string captureDirectoryPath)
		{
			if (string.IsNullOrWhiteSpace(captureDirectoryPath))
			{
				throw new ArgumentException("Capture directory path is empty.", nameof(captureDirectoryPath));
			}

			mCaptureDirectoryPath = captureDirectoryPath;
		}

		public string Capture(string scenarioPath, int lineNumber)
		{
			if (string.IsNullOrWhiteSpace(scenarioPath))
			{
				throw new ArgumentException("Scenario path is empty.", nameof(scenarioPath));
			}

			Rectangle virtualScreen = SystemInformation.VirtualScreen;

			if (virtualScreen.Width <= 0 || virtualScreen.Height <= 0)
			{
				throw new InvalidOperationException("Virtual screen bounds could not be resolved.");
			}

			Directory.CreateDirectory(mCaptureDirectoryPath);

			string fileName = createFileName(scenarioPath, lineNumber);
			string capturePath = Path.Combine(mCaptureDirectoryPath, fileName);

			using (var bitmap = new Bitmap(virtualScreen.Width, virtualScreen.Height))
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.CopyFromScreen(virtualScreen.Location, Point.Empty, virtualScreen.Size, CopyPixelOperation.SourceCopy);
				bitmap.Save(capturePath, ImageFormat.Png);
			}

			return capturePath;
		}

		private string createFileName(string scenarioPath, int lineNumber)
		{
			string scenarioName = Path.GetFileNameWithoutExtension(scenarioPath);
			scenarioName = sanitizeFileName(scenarioName);

			string lineText = lineNumber > 0 ? $"_Line{lineNumber:D2}" : "";

			return $"{scenarioName}{lineText}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png";
		}

		private string sanitizeFileName(string fileName)
		{
			foreach (char invalidChar in Path.GetInvalidFileNameChars())
			{
				fileName = fileName.Replace(invalidChar, '_');
			}

			return fileName;
		}
	}
}
