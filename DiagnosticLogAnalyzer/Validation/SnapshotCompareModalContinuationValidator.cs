using DiagnosticLogAnalyzer.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiagnosticLogAnalyzer.Validation
{
	public sealed class SnapshotCompareModalContinuationValidator
	{
		public DiagnosticModalContinuationResult Validate(DiagnosticEventLifecycleValidationResult lifecycles,
			DiagnosticCommandCorrelationResult correlations, DiagnosticCommandExecutionContextResult contexts)
		{
			if (lifecycles == null) { throw new ArgumentNullException(nameof(lifecycles)); }
			if (correlations == null) { throw new ArgumentNullException(nameof(correlations)); }
			if (contexts == null) { throw new ArgumentNullException(nameof(contexts)); }

			DiagnosticModalContinuationResult result = new DiagnosticModalContinuationResult();
			foreach (DiagnosticEventLifecycle lifecycle in lifecycles.Lifecycles.Where(x =>
				x.Operation == "Snapshot.Compare" && x.Verdict == DiagnosticLifecycleVerdict.Inconclusive))
			{
				DiagnosticModalContinuation item = new DiagnosticModalContinuation { Lifecycle = lifecycle };
				result.Items.Add(item);
				validateOne(item, correlations, contexts);
			}
			return result;
		}

		private static void unresolved(DiagnosticModalContinuation item, string reason)
		{
			item.Status = DiagnosticModalContinuationStatus.Unresolved;
			item.Reason = reason;
		}

		private static void ambiguous(DiagnosticModalContinuation item, string reason)
		{
			item.Status = DiagnosticModalContinuationStatus.Ambiguous;
			item.Reason = reason;
		}

		private static void validateOne(DiagnosticModalContinuation item, DiagnosticCommandCorrelationResult correlations,
			DiagnosticCommandExecutionContextResult contexts)
		{
			DiagnosticEventLifecycle lifecycle = item.Lifecycle;
			if (lifecycle.Scenario == null || lifecycle.Window == null || lifecycle.Command == null ||
				lifecycle.Scenario.Commands == null || !isCompareCommand(lifecycle.Command.Type) ||
				lifecycle.Command.Result != "PASS")
			{
				unresolved(item, "A successful comparison command and confirmed Gui window are required.");
				return;
			}

			// 同じコマンドでStartedだけが確定している場合に限定する。
			if (lifecycle.Events.Count != 1 || lifecycle.Events[0].SessionCorrelation.Event.EventName != "Started")
			{
				unresolved(item, "Exactly one confirmed Started event is required.");
				return;
			}

			DiagnosticEvent started = lifecycle.Events[0].SessionCorrelation.Event;
			List<CommandRecord> following = lifecycle.Scenario.Commands
				.Where(x => x.LineNumber > lifecycle.Command.LineNumber)
				.OrderBy(x => x.LineNumber).ToList();
			List<CommandRecord> checks = new List<CommandRecord>();
			CommandRecord close = null;
			foreach (CommandRecord command in following)
			{
				if (command.Type == "CHECK SNAPSHOT COMPARISON" && command.Result == "PASS")
				{
					checks.Add(command);
					continue;
				}
				if (command.Type == "CLOSE DIALOG" && command.Result == "PASS") { close = command; }
				break;
			}
			if (checks.Count == 0 || close == null)
			{
				unresolved(item, "Comparison checks followed by a successful CLOSE DIALOG are required.");
				return;
			}

			// 同一SessionId・Gui起動区間のCompletedだけを参考候補とする。
			List<DiagnosticCommandExecutionContext> candidates = contexts.Events.Where(x =>
				x.Status == DiagnosticCommandContextStatus.ObservedDuringCommand &&
				ReferenceEquals(x.ActiveCommand, close) &&
				x.CommandCorrelation.Status == DiagnosticCommandMatchStatus.NoMatchingCommand &&
				correlations.Events.Contains(x.CommandCorrelation) &&
				x.CommandCorrelation.SessionCorrelation.Status == DiagnosticSessionMatchStatus.WindowMatched &&
				ReferenceEquals(x.CommandCorrelation.SessionCorrelation.Scenario, lifecycle.Scenario) &&
				ReferenceEquals(x.CommandCorrelation.SessionCorrelation.Window, lifecycle.Window) &&
				string.Equals(x.CommandCorrelation.SessionCorrelation.Event.SessionId, started.SessionId, StringComparison.OrdinalIgnoreCase) &&
				x.CommandCorrelation.SessionCorrelation.Event.Operation == "Snapshot.Compare" &&
				x.CommandCorrelation.SessionCorrelation.Event.EventName == "Completed" &&
				x.CommandCorrelation.SessionCorrelation.Event.Timestamp >= started.Timestamp).ToList();
			if (candidates.Count == 0)
			{
				unresolved(item, "No unique Completed event was observed during the expected CLOSE DIALOG.");
				return;
			}
			if (candidates.Count > 1)
			{
					ambiguous(item, "Multiple Completed events were observed during the expected CLOSE DIALOG.");
					return;
			}

			DiagnosticEvent completed = candidates[0].CommandCorrelation.SessionCorrelation.Event;
			// 途中に別の比較試行や終端イベントがあれば、因果関係を確定しない。
			bool conflictingEvent = correlations.Events.Any(x =>
				x.SessionCorrelation != null && x.SessionCorrelation.Event != null &&
				ReferenceEquals(x.SessionCorrelation.Scenario, lifecycle.Scenario) &&
				ReferenceEquals(x.SessionCorrelation.Window, lifecycle.Window) &&
				string.Equals(x.SessionCorrelation.Event.SessionId, started.SessionId, StringComparison.OrdinalIgnoreCase) &&
				x.SessionCorrelation.Event.Operation == "Snapshot.Compare" &&
				x.SessionCorrelation.Event.Timestamp >= started.Timestamp &&
				x.SessionCorrelation.Event.Timestamp <= completed.Timestamp &&
				!ReferenceEquals(x.SessionCorrelation.Event, started) &&
				!ReferenceEquals(x.SessionCorrelation.Event, completed));
			if (conflictingEvent)
			{
				ambiguous(item, "Another Snapshot.Compare event overlaps the modal continuation.");
				return;
			}

			// 参照元のMatched/NoMatchingCommandや元のライフサイクル判定は変更しない。
			item.Status = DiagnosticModalContinuationStatus.Resolved;
			item.TerminalContext = candidates[0];
			item.Reason = "Completed was observed during the matching modal CLOSE DIALOG after comparison checks.";
		}

		private static bool isCompareCommand(string type)
		{
			return type == "COMPARE SNAPSHOT" || type == "COMPARE SAVED SNAPSHOT";
		}
	}
}