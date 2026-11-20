using Gui.Definition;
using Gui.Logging;

namespace GuiTest.TestCase
{
	[TestClass]
	[DoNotParallelize]
	public class Logger_Test
	{
		private static readonly DateTime mTimestamp = new DateTime(2026, 11, 20, 12, 34, 56, 789);
		private static readonly Guid mSessionId = Guid.ParseExact("0123456789abcdef0123456789abcdef", "N");

		[TestMethod]
		public void FormatRequiredFields()
		{
			string actual = Logger.Format(
				"INFO",
				"Calculation",
				"Started",
				null,
				mTimestamp,
				mSessionId);

			string expected =
				"2026-11-20 12:34:56.789 [INFO] " +
				"SessionId=0123456789abcdef0123456789abcdef " +
				"Operation=Calculation Event=Started";

			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void FormatWithDetails()
		{
			string actual = Logger.Format(
				"WARNING",
				"Calculation",
				"ValidationRejected",
				"Outcome=INVALID_VALUE",
				mTimestamp,
				mSessionId);

			string expected =
				"2026-11-20 12:34:56.789 [WARNING] " +
				"SessionId=0123456789abcdef0123456789abcdef " +
				"Operation=Calculation Event=ValidationRejected " +
				"Outcome=INVALID_VALUE";

			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void FormatWithoutDetails()
		{
			string withoutDetails = Logger.Format(
				"INFO",
				"Application",
				"Started",
				null,
				mTimestamp,
				mSessionId);

			string withEmptyDetails = Logger.Format(
				"INFO",
				"Application",
				"Started",
				"",
				mTimestamp,
				mSessionId);

			Assert.AreEqual(withoutDetails, withEmptyDetails);
			Assert.IsFalse(withoutDetails.EndsWith(" "));
		}

		[TestMethod]
		public void FormatEscapesLineBreaksAndBackslashes()
		{
			string actual = Logger.Format(
				"ERROR",
				"Calculation",
				"Failed",
				"Message=First\r\nSecond\nThird Path=C:\\Test",
				mTimestamp,
				mSessionId);

			Assert.IsTrue(actual.Contains(@"Message=First\r\nSecond\nThird Path=C:\\Test"));

			Assert.IsFalse(actual.Contains("\r"));
			Assert.IsFalse(actual.Contains("\n"));
		}

		[TestMethod]
		public void FormatEscapesOperationAndEvent()
		{
			string actual = Logger.Format(
				"INFO",
				"Calc\r\nulation",
				"Start\ned",
				null,
				mTimestamp,
				mSessionId);

			Assert.IsTrue(actual.Contains(@"Operation=Calc\r\nulation Event=Start\ned"));

			Assert.IsFalse(actual.Contains("\r"));
			Assert.IsFalse(actual.Contains("\n"));
		}

		[TestMethod]
		public void SessionIdRemainsStable()
		{
			Guid first = Logger.SessionId;
			Guid second = Logger.SessionId;

			Assert.AreNotEqual(Guid.Empty, first);
			Assert.AreEqual(first, second);

			string text = first.ToString("N");

			Assert.AreEqual(32, text.Length);
			Assert.IsTrue(Guid.TryParseExact(text, "N", out Guid parsed));
			Assert.AreEqual(first, parsed);
		}

		[TestMethod]
		public void PublicMethodsWriteStructuredLog()
		{
			string operationPrefix = $"GuiTest.Logger.{Guid.NewGuid():N}";

			string logPathBefore = AppPath.LogFilePath;

			Logger.Info($"{operationPrefix}.Info", "Started");

			Logger.Warning(
				$"{operationPrefix}.Warning",
				"ValidationRejected",
				"Outcome=TEST");

			Logger.Error(
				$"{operationPrefix}.Error",
				"Failed",
				"Outcome=TEST");

			try
			{
				throw new InvalidOperationException(
					"Injected outer failure.",
					new IOException("Injected inner failure."));
			}
			catch (Exception ex)
			{
				Logger.Error(
					$"{operationPrefix}.Exception",
					"Failed",
					ex,
					"Stage=Test");
			}

			// 日付変更の境界でも、書き込み前後のログファイルを確認する。
			string[] logPaths =
			{
				logPathBefore,
				AppPath.LogFilePath
			};

			string[] lines = logPaths
				.Distinct()
				.Where(File.Exists)
				.SelectMany(File.ReadAllLines)
				.Where(x => x.Contains($"Operation={operationPrefix}."))
				.ToArray();

			Assert.AreEqual(4, lines.Length);

			string sessionId = Logger.SessionId.ToString("N");

			Assert.IsTrue(lines[0].Contains($"[INFO] SessionId={sessionId}"));
			Assert.IsTrue(lines[0].Contains($"Operation={operationPrefix}.Info Event=Started"));

			Assert.IsTrue(lines[1].Contains("[WARNING]"));
			Assert.IsTrue(lines[1].Contains("Event=ValidationRejected Outcome=TEST"));

			Assert.IsTrue(lines[2].Contains("[ERROR]"));
			Assert.IsTrue(lines[2].Contains("Event=Failed Outcome=TEST"));

			Assert.IsTrue(lines[3].Contains("[ERROR]"));
			Assert.IsTrue(lines[3].Contains("Stage=Test"));
			Assert.IsTrue(lines[3].Contains("System.InvalidOperationException"));
			Assert.IsTrue(lines[3].Contains("Injected outer failure."));
			Assert.IsTrue(lines[3].Contains("System.IO.IOException"));
			Assert.IsTrue(lines[3].Contains("Injected inner failure."));
		}
	}
}
