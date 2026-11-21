using DiagnosticLogAnalyzer.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiagnosticLogAnalyzer.Correlation
{
	public sealed class DiagnosticSessionCorrelator
	{
		private sealed class WindowLink
		{
			public ScenarioRecord Scenario { get; set; }
			public GuiWindowRecord Window { get; set; }
		}

		public DiagnosticSessionCorrelationResult Correlate(AnalysisInput input, IEnumerable<DiagnosticEvent> events)
		{
			if (input == null)
			{
				throw new ArgumentNullException(nameof(input));
			}
			if (events == null)
			{
				throw new ArgumentNullException(nameof(events));
			}
			if (input.Scenarios == null || input.PerformanceSessions == null)
			{
				throw new ArgumentException("Input sessions and scenarios are required.", nameof(input));
			}

			DiagnosticSessionCorrelationResult result = new DiagnosticSessionCorrelationResult();
			Dictionary<string, List<WindowLink>> windowsBySession = new Dictionary<string, List<WindowLink>>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, List<PerformanceSessionRecord>> performanceBySession = new Dictionary<string, List<PerformanceSessionRecord>>(StringComparer.OrdinalIgnoreCase);

			foreach (ScenarioRecord scenario in input.Scenarios)
			{
				if (scenario == null || scenario.GuiWindows == null)
				{
					throw new ArgumentException("Invalid scenario windows.", nameof(input));
				}

				foreach (GuiWindowRecord window in scenario.GuiWindows)
				{
					if (window == null)
					{
						throw new ArgumentException("Null Gui window.", nameof(input));
					}

					string id = normalizeSessionId(window.SessionId);

					if (id == null)
					{
						continue;
					}
					if (!windowsBySession.TryGetValue(id, out List<WindowLink> links))
					{
						links = new List<WindowLink>();
						windowsBySession.Add(id, links);
					}

					links.Add(new WindowLink { Scenario = scenario, Window = window });
				}
			}

			foreach (PerformanceSessionRecord session in input.PerformanceSessions)
			{
				if (session == null)
				{
					throw new ArgumentException("Null Performance Session.", nameof(input));
				}

				string id = normalizeSessionId(session.SessionId);
				if (id == null)
				{
					continue;
				}

				if (!performanceBySession.TryGetValue(id, out List<PerformanceSessionRecord> sessions))
				{
					sessions = new List<PerformanceSessionRecord>();
					performanceBySession.Add(id, sessions);
				}

				sessions.Add(session);
			}

			foreach (DiagnosticEvent diagnosticEvent in events)
			{
				if (diagnosticEvent == null)
				{
					throw new ArgumentException("Null diagnostic event.", nameof(events));
				}

				string id = normalizeSessionId(diagnosticEvent.SessionId);

				List<WindowLink> links = null;
				List<PerformanceSessionRecord> sessions = null;

				if (id != null)
				{
					windowsBySession.TryGetValue(id, out links);
					performanceBySession.TryGetValue(id, out sessions);
				}

				int windowCount = links?.Count ?? 0;
				int performanceCount = sessions?.Count ?? 0;
				DiagnosticEventCorrelation item = new DiagnosticEventCorrelation { Event = diagnosticEvent };

				if (windowCount > 1 || performanceCount > 1)
				{
					item.Status = DiagnosticSessionMatchStatus.Ambiguous;
					item.Reason = "SessionId appears in multiple Gui windows or Performance Sessions.";
				}
				else if (windowCount == 1)
				{
					WindowLink link = links[0];

					PerformanceSessionRecord session = performanceCount == 1 ? sessions[0] : null;

					if (session != null && !referencesWindow(session, link))
					{
						item.Status = DiagnosticSessionMatchStatus.Ambiguous;
						item.Reason = "Performance Session mapping conflicts with Gui window.";
					}
					else
					{
						item.Status = DiagnosticSessionMatchStatus.WindowMatched;
						item.Scenario = link.Scenario;
						item.Window = link.Window;
						item.PerformanceSession = session;
						item.Reason = session == null ? "Matched Gui window; no Performance Session." : "Matched Gui window and Performance Session.";
					}
				}
				else if (performanceCount == 1)
				{
					item.Status = DiagnosticSessionMatchStatus.PerformanceOnly;
					item.PerformanceSession = sessions[0];
					item.Reason = "Matched Performance Session, but no Gui window is linked by SessionId.";
				}
				else
				{
					item.Status = DiagnosticSessionMatchStatus.Unmatched;
					item.Reason = "SessionId does not match a known Gui window or Performance Session.";
				}

				result.Events.Add(item);
			}

			foreach (IGrouping<string, DiagnosticEventCorrelation> group in result.Events.Where(x => x.Status == DiagnosticSessionMatchStatus.Ambiguous).GroupBy(x => x.Event.SessionId ?? "", StringComparer.OrdinalIgnoreCase))
			{
				result.Issues.Add($"Ambiguous SessionId={group.Key}: {group.First().Reason} ({group.Count()} events)");
			}

			return result;
		}

		private static string normalizeSessionId(string value)
		{
			if (string.IsNullOrWhiteSpace(value)) { return null; }
			return value.Trim().ToLowerInvariant();
		}

		private static bool referencesWindow(PerformanceSessionRecord session, WindowLink link)
		{
			// 対応付けられていないPerformance Sessionからは、Gui起動区間を確定できない。
			if (string.IsNullOrEmpty(session.ScenarioFile) || !session.WindowOrder.HasValue)
			{
				return true;
			}

			return string.Equals(session.ScenarioFile, link.Scenario.SourceFile, StringComparison.OrdinalIgnoreCase) && session.WindowOrder.Value == link.Window.WindowOrder;
		}
	}
}
