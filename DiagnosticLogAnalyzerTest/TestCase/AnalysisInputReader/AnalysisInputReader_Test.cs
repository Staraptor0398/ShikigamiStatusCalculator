using DiagnosticLogAnalyzer.Model;
using DiagnosticLogAnalyzer.Reader;
using DiagnosticLogAnalyzerTest.TestUtil;

namespace DiagnosticLogAnalyzerTest.TestCase
{
	[TestClass]
	public class AnalysisInputReader_Test
	{
		private const string ValidJson =
		"{\"schemaVersion\":1,\"runId\":\"44763361961\",\"diagnosticLogs\":[{\"path\":\"Gui/Log/ShikigamiApp_20261120.log\"}]," +
		"\"scenarios\":[{\"scenario\":\"Test.scenario\",\"sourceFile\":\"ScenarioRunner/Log/Test.log\",\"result\":\"PASS\"," +
		"\"start\":\"2026-11-20T15:27:44.000\",\"end\":\"2026-11-20T15:27:46.000\",\"commands\":[],\"guiWindows\":[]}]," +
		"\"performanceSessions\":[{\"sessionId\":\"0123456789abcdef0123456789abcdef\",\"sessionOrder\":1," +
		"\"endTime\":\"2026-11-20T15:27:46.000\",\"matchQuality\":\"window-time\",\"scenarioFile\":\"ScenarioRunner/Log/Test.log\",\"windowOrder\":1}]," +
		"\"preparationIssues\":[]}";

		private static AnalysisInput Read(TemporaryDirectory directory, string json)
		{
			return AnalysisInputReader.Read(directory.Write("DiagnosticAnalysisInput.json", json));
		}

		[TestMethod]
		public void ValidInputIsReadWithoutLosingFields()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				AnalysisInput input = Read(directory, ValidJson);
				Assert.AreEqual(1, input.SchemaVersion);
				Assert.AreEqual("44763361961", input.RunId);
				Assert.AreEqual(1, input.DiagnosticLogs.Count);
				Assert.AreEqual("Gui/Log/ShikigamiApp_20261120.log", input.DiagnosticLogs[0].Path);
				Assert.AreEqual("Test.scenario", input.Scenarios[0].Scenario);
				Assert.AreEqual("PASS", input.Scenarios[0].Result);
				Assert.AreEqual(1, input.PerformanceSessions.Count);
				Assert.AreEqual("0123456789abcdef0123456789abcdef", input.PerformanceSessions[0].SessionId);
			}
		}

		[TestMethod]
		public void EmptyArraysAreSupported()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				string json = "{\"schemaVersion\":1,\"runId\":\"1\",\"diagnosticLogs\":[],\"scenarios\":[],\"performanceSessions\":[],\"preparationIssues\":[]}";
				AnalysisInput input = Read(directory, json);
				Assert.AreEqual(0, input.DiagnosticLogs.Count);
				Assert.AreEqual(0, input.Scenarios.Count);
			}
		}

		[TestMethod]
		public void PreparationIssuesAreRetained()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				AnalysisInput input = Read(directory, ValidJson.Replace("\"preparationIssues\":[]", "\"preparationIssues\":[\"No Performance log\"]"));
				Assert.AreEqual("No Performance log", input.PreparationIssues[0]);
			}
		}

		[TestMethod]
		public void UnsupportedSchemaIsRejected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				Assert.ThrowsExactly<InvalidDataException>(() => Read(directory, ValidJson.Replace("\"schemaVersion\":1", "\"schemaVersion\":2")));
			}
		}

		[TestMethod]
		public void NonNumericRunIdIsRejected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				Assert.ThrowsExactly<InvalidDataException>(() => Read(directory, ValidJson.Replace("44763361961", "run-id")));
			}
		}

		[TestMethod]
		public void MissingRequiredArrayIsRejected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				Assert.ThrowsExactly<InvalidDataException>(() => Read(directory, ValidJson.Replace("\"preparationIssues\":[]", "\"preparationIssues\":null")));
			}
		}

		[TestMethod]
		public void MissingRequiredJsonPropertyIsRejected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				string json = ValidJson.Replace(",\"preparationIssues\":[]", "");
				Assert.ThrowsExactly<System.Runtime.Serialization.SerializationException>(() => Read(directory, json));
			}
		}

		[TestMethod]
		public void NullScenarioIsRejected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				string json = ValidJson.Replace("\"scenarios\":[{\"scenario\":\"Test.scenario\"", "\"scenarios\":[null,{\"scenario\":\"Test.scenario\"");
				Assert.ThrowsExactly<InvalidDataException>(() => Read(directory, json));
			}
		}

		[TestMethod]
		public void NullDiagnosticLogIsRejected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				string json = ValidJson.Replace("\"diagnosticLogs\":[{\"path\":\"Gui/Log/ShikigamiApp_20261120.log\"}]", "\"diagnosticLogs\":[null]");
				Assert.ThrowsExactly<InvalidDataException>(() => Read(directory, json));
			}
		}

		[TestMethod]
		public void InvalidPerformanceSessionIsRejected()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				string json = ValidJson.Replace("\"sessionId\":\"0123456789abcdef0123456789abcdef\"", "\"sessionId\":\"\"");
				Assert.ThrowsExactly<InvalidDataException>(() => Read(directory, json));
			}
		}

		[TestMethod]
		public void NullOrEmptyInputPathIsRejected()
		{
			Assert.ThrowsExactly<ArgumentException>(() => AnalysisInputReader.Read(null));
			Assert.ThrowsExactly<ArgumentException>(() => AnalysisInputReader.Read("  "));
		}

		[TestMethod]
		public void MissingInputFileThrowsFileNotFound()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				Assert.ThrowsExactly<FileNotFoundException>(() => AnalysisInputReader.Read(System.IO.Path.Combine(directory.Path, "absent.json")));
			}
		}

		[TestMethod]
		public void ResolveSourcePathAllowsNestedPortablePath()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				string actual = AnalysisInputReader.ResolveSourcePath(directory.Path, "Gui/bin/x64/Release/Log/events.log");
				Assert.AreEqual(System.IO.Path.Combine(directory.Path, "Gui", "bin", "x64", "Release", "Log", "events.log"), actual);
			}
		}

		[TestMethod]
		public void ResolveSourcePathRejectsDirectoryTraversal()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				Assert.ThrowsExactly<InvalidDataException>(() => AnalysisInputReader.ResolveSourcePath(directory.Path, "../outside.log"));
				Assert.ThrowsExactly<InvalidDataException>(() => AnalysisInputReader.ResolveSourcePath(directory.Path, "Gui/../../outside.log"));
			}
		}

		[TestMethod]
		public void ResolveSourcePathRejectsWindowsAndRootedPaths()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				Assert.ThrowsExactly<InvalidDataException>(() => AnalysisInputReader.ResolveSourcePath(directory.Path, "C:/Windows/file.log"));
				Assert.ThrowsExactly<InvalidDataException>(() => AnalysisInputReader.ResolveSourcePath(directory.Path, "Gui\\Log\\events.log"));
				Assert.ThrowsExactly<InvalidDataException>(() => AnalysisInputReader.ResolveSourcePath(directory.Path, "/absolute.log"));
			}
		}

		[TestMethod]
		public void ResolveSourcePathRejectsEmptyPath()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				Assert.ThrowsExactly<InvalidDataException>(() => AnalysisInputReader.ResolveSourcePath(directory.Path, ""));
				Assert.ThrowsExactly<InvalidDataException>(() => AnalysisInputReader.ResolveSourcePath(directory.Path, null));
			}
		}

		[TestMethod]
		public void ResolveSourcePathDoesNotConfuseSiblingWithRoot()
		{
			using (TemporaryDirectory directory = new TemporaryDirectory())
			{
				Assert.ThrowsExactly<InvalidDataException>(() => AnalysisInputReader.ResolveSourcePath(directory.Path, "../" + System.IO.Path.GetFileName(directory.Path) + "2/events.log"));
			}
		}
	}
}
