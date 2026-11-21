using DiagnosticLogAnalyzer.Model;
using DiagnosticLogAnalyzer.Parser;
using DiagnosticLogAnalyzerTest.TestUtil;

namespace DiagnosticLogAnalyzerTest.TestCase
{
	[TestClass]
	public class DiagnosticLogParser_Test
	{
		private const string SessionId = "0123456789abcdef0123456789abcdef";
		private const string EventLine = "2026-11-20 15:27:46.123 [INFO] SessionId=" + SessionId + " Operation=Calculation Event=Started ShikigamiSelected=True";

		private static DiagnosticLogParseResult Parse(TemporaryDirectory directory, string contents)
		{
			string path = directory.Write("Gui/Log/events.log", contents);
			return new DiagnosticLogParser().Parse(path, "Gui/Log/events.log");
		}

		[TestMethod]
		public void ValidEventParsesAllFields()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, EventLine);
				Assert.AreEqual(1, result.Events.Count);
				Assert.AreEqual(0, result.Issues.Count);
				DiagnosticEvent item = result.Events[0];
				Assert.AreEqual("Gui/Log/events.log", item.SourceFile);
				Assert.AreEqual(1, item.LineNumber);
				Assert.AreEqual(new DateTime(2026, 11, 20, 15, 27, 46, 123), item.Timestamp);
				Assert.AreEqual("INFO", item.Level);
				Assert.AreEqual(SessionId, item.SessionId);
				Assert.AreEqual("Calculation", item.Operation);
				Assert.AreEqual("Started", item.EventName);
				Assert.AreEqual("ShikigamiSelected=True", item.Details);
			}
		}

		[TestMethod]
		public void MultipleEventsPreserveOrderAndLineNumber()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				string second = EventLine.Replace("15:27:46.123", "15:27:46.124").Replace("Event=Started", "Event=Completed");
				DiagnosticLogParseResult result = Parse(directory, EventLine + "\r\n" + second + "\r\n");
				Assert.AreEqual(2, result.Events.Count);
				Assert.AreEqual(1, result.Events[0].LineNumber);
				Assert.AreEqual(2, result.Events[1].LineNumber);
				Assert.AreEqual("Completed", result.Events[1].EventName);
			}
		}

		[TestMethod]
		public void WarningAndErrorLevelsAreSupported()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				string text = EventLine.Replace("[INFO]", "[WARNING]") + "\n" + EventLine.Replace("[INFO]", "[ERROR]");
				DiagnosticLogParseResult result = Parse(directory, text);
				Assert.AreEqual(2, result.Events.Count);
				Assert.AreEqual("WARNING", result.Events[0].Level);
				Assert.AreEqual("ERROR", result.Events[1].Level);
			}
		}

		[TestMethod]
		public void DetailsAreOptional()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, EventLine.Replace(" ShikigamiSelected=True", ""));
				Assert.AreEqual(1, result.Events.Count);
				Assert.AreEqual("", result.Events[0].Details);
			}
		}

		[TestMethod]
		public void JapaneseAndEscapedDetailsArePreservedAsRawText()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				string details = "Message=式神データ Exception=first\\nsecond\\\\path";
				DiagnosticLogParseResult result = Parse(directory, EventLine.Replace("ShikigamiSelected=True", details));
				Assert.AreEqual(0, result.Issues.Count);
				Assert.AreEqual(details, result.Events[0].Details);
			}
		}

		[TestMethod]
		public void SessionIdIsNormalizedToLowercase()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, EventLine.Replace(SessionId, SessionId.ToUpperInvariant()));
				Assert.AreEqual(1, result.Events.Count);
				Assert.AreEqual(SessionId, result.Events[0].SessionId);
			}
		}

		[TestMethod]
		public void EmptyAndWhitespaceLinesAreIgnoredWithoutChangingPhysicalLineNumbers()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, "\n  \n" + EventLine + "\n");
				Assert.AreEqual(1, result.Events.Count);
				Assert.AreEqual(3, result.Events[0].LineNumber);
				Assert.AreEqual(0, result.Issues.Count);
			}
		}

		[TestMethod]
		public void MalformedLineAddsIssueAndContinues()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, EventLine + "\nmalformed\n" + EventLine);
				Assert.AreEqual(2, result.Events.Count);
				Assert.AreEqual(1, result.Issues.Count);
				Assert.AreEqual(2, result.Issues[0].LineNumber);
				Assert.AreEqual("Gui/Log/events.log", result.Issues[0].SourceFile);
				Assert.AreEqual("Invalid diagnostic event record.", result.Issues[0].Message);
				Assert.AreEqual(3, result.Events[1].LineNumber);
			}
		}

		[TestMethod]
		public void InvalidCalendarDateIsReported()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, EventLine.Replace("2026-11-20", "2026-02-30"));
				Assert.AreEqual(0, result.Events.Count);
				Assert.AreEqual(1, result.Issues.Count);
			}
		}

		[TestMethod]
		public void InvalidClockTimeIsReported()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, EventLine.Replace("15:27:46.123", "25:27:46.123"));
				Assert.AreEqual(0, result.Events.Count);
				Assert.AreEqual(1, result.Issues.Count);
			}
		}

		[TestMethod]
		public void InvalidSessionIdIsReported()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, EventLine.Replace(SessionId, "not-a-guid"));
				Assert.AreEqual(0, result.Events.Count);
				Assert.AreEqual(1, result.Issues.Count);
			}
		}

		[TestMethod]
		public void UnsupportedLevelIsReported()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, EventLine.Replace("[INFO]", "[DEBUG]"));
				Assert.AreEqual(0, result.Events.Count);
				Assert.AreEqual(1, result.Issues.Count);
			}
		}

		[TestMethod]
		public void MissingOperationIsReported()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, EventLine.Replace(" Operation=Calculation", ""));
				Assert.AreEqual(0, result.Events.Count);
				Assert.AreEqual(1, result.Issues.Count);
			}
		}

		[TestMethod]
		public void MissingEventIsReported()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, EventLine.Replace(" Event=Started", ""));
				Assert.AreEqual(0, result.Events.Count);
				Assert.AreEqual(1, result.Issues.Count);
			}
		}

		[TestMethod]
		public void EmptyFileHasNoEventsOrIssues()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParseResult result = Parse(directory, "");
				Assert.AreEqual(0, result.Events.Count);
				Assert.AreEqual(0, result.Issues.Count);
			}
		}

		[TestMethod]
		public void Utf8BomIsAccepted()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				string path = directory.Write("Gui/Log/events.log", EventLine);
				File.WriteAllText(path, EventLine, new System.Text.UTF8Encoding(true));
				DiagnosticLogParseResult result = new DiagnosticLogParser().Parse(path, "source.log");
				Assert.AreEqual(1, result.Events.Count);
				Assert.AreEqual("source.log", result.Events[0].SourceFile);
				Assert.AreEqual(0, result.Issues.Count);
			}
		}

		[TestMethod]
		public void MissingLogFileThrowsFileNotFound()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				Assert.ThrowsExactly<FileNotFoundException>(() => new DiagnosticLogParser().Parse(System.IO.Path.Combine(directory.Path, "absent.log"), "absent.log"));
			}
		}

		[TestMethod]
		public void ParserCanBeReusedWithoutRetainingPreviousResults()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				DiagnosticLogParser parser = new DiagnosticLogParser();
				string valid = directory.Write("valid.log", EventLine);
				string invalid = directory.Write("invalid.log", "broken");
				DiagnosticLogParseResult first = parser.Parse(valid, "valid.log");
				DiagnosticLogParseResult second = parser.Parse(invalid, "invalid.log");
				Assert.AreEqual(1, first.Events.Count);
				Assert.AreEqual(0, first.Issues.Count);
				Assert.AreEqual(0, second.Events.Count);
				Assert.AreEqual(1, second.Issues.Count);
			}
		}
	}
}
