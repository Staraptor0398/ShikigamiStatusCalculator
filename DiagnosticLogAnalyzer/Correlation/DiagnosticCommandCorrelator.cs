using DiagnosticLogAnalyzer.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DiagnosticLogAnalyzer.Correlation
{
	public sealed class DiagnosticCommandCorrelator
	{
		private static readonly string[] OpenCommands = { "OPEN GUI", "LAUNCH GUI" };
		private static readonly string[] CloseCommands = { "CLOSE GUI" };
		private static readonly string[] ReloadCommands = { "RELOAD SHIKIGAMI", "OPEN GUI", "LAUNCH GUI", "RECOVER SHIKIGAMI" };
		private static readonly string[] LoadCommands = { "LOAD BUILD", "LOAD MITAMA", "LOAD SAVED BUILD", "LOAD SAVED MITAMA", "TRY LOAD BUILD", "TRY LOAD MITAMA" };
		private static readonly string[] SaveCommands = { "SAVE BUILD", "SAVE MITAMA", "SAVE SNAPSHOT" };
		private static readonly string[] CalculationCommands = { "CALC", "CALCULATE" };
		private static readonly string[] ComparisonCommands = { "COMPARE SNAPSHOT", "COMPARE SAVED SNAPSHOT" };
		private static readonly string[] ClearCommands = { "CLEAR", "CLEAR SHIKIGAMI" };
		private static readonly string[] RegisterCommands = { "REGISTER SHIKIGAMI" };
		private static readonly string[] EditCommands = { "EDIT SHIKIGAMI" };
		private static readonly string[] RecoveryCommands = { "RECOVER SHIKIGAMI", "WAIT SHIKIGAMI AUTO REPAIR", "RELOAD SHIKIGAMI" };
		private static readonly string[] InputChangeCommands = { "EQUIP MITAMA", "CLEAR", "CLEAR SHIKIGAMI" };

		public DiagnosticCommandCorrelationResult Correlate(DiagnosticSessionCorrelationResult sessions)
		{
			if (sessions == null) { throw new ArgumentNullException(nameof(sessions)); }
			if (sessions.Events == null) { throw new ArgumentException("Session events are required.", nameof(sessions)); }

			DiagnosticCommandCorrelationResult result = new DiagnosticCommandCorrelationResult();
			foreach (DiagnosticEventCorrelation session in sessions.Events)
			{
				if (session == null || session.Event == null)
				{
					throw new ArgumentException("Invalid session event.", nameof(sessions));
				}

				DiagnosticCommandCorrelation item = new DiagnosticCommandCorrelation { SessionCorrelation = session };
				result.Events.Add(item);

				// 参考候補を確定したScenarioとして扱わない。
				if (session.Status != DiagnosticSessionMatchStatus.WindowMatched ||
					session.Scenario == null || session.Window == null)
				{
					item.Status = DiagnosticCommandMatchStatus.NoConfirmedWindow;
					continue;
				}

				string[] commandTypes = getCommandTypes(session.Event.Operation, session.Event.EventName, session.Event.Details);
				if (commandTypes == null)
				{
					item.Status = DiagnosticCommandMatchStatus.NotMappedOperation;
					continue;
				}

				if (session.Scenario.Commands == null)
				{
					throw new ArgumentException("Scenario commands are required.", nameof(sessions));
				}

				foreach (CommandRecord command in session.Scenario.Commands)
				{
					if (command == null) { throw new ArgumentException("Null Scenario command.", nameof(sessions)); }
					if (!commandTypes.Contains(command.Type, StringComparer.Ordinal)) { continue; }
					if (!DateTime.TryParseExact(command.Start, "yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture,
						DateTimeStyles.None, out DateTime start)) { continue; }
					if (!DateTime.TryParseExact(command.End, "yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture,
						DateTimeStyles.None, out DateTime end) || end < start) { continue; }

					// 区間端の同時刻は双方の候補として残し、曖昧な場合は確定しない。
					if (start <= session.Event.Timestamp && session.Event.Timestamp <= end)
					{
						item.Candidates.Add(command);
					}
				}

				if (item.Candidates.Count == 1)
				{
					item.Status = DiagnosticCommandMatchStatus.Matched;
					item.Command = item.Candidates[0];
				}
				else if (item.Candidates.Count > 1)
				{
					item.Status = DiagnosticCommandMatchStatus.Ambiguous;
				}
				else
				{
					item.Status = DiagnosticCommandMatchStatus.NoMatchingCommand;
				}
			}
			return result;
		}

		private static string[] getStateCommandTypes(string eventName, string details)
		{
			if (eventName != "Changed") { return null; }

			// 遷移前後の両方を確認し、IDLEへの復帰だけで操作種別を推測しない。
			switch (details)
			{
				case "Previous=IDLE Current=LOADING":
				case "Previous=LOADING Current=IDLE":
					return LoadCommands;
				case "Previous=IDLE Current=SAVING":
				case "Previous=SAVING Current=IDLE":
					return SaveCommands;
				case "Previous=IDLE Current=CLEARING":
				case "Previous=CLEARING Current=IDLE":
					return ClearCommands;
				default:
					return null;
			}
		}

		private static string[] getCommandTypes(string operation, string eventName, string details)
		{
			switch (operation)
			{
				case "Application":
					if (eventName == "Started" || eventName == "Initialized") { return OpenCommands; }
					if (eventName == "Closing" || eventName == "Closed" || eventName == "ShutdownCheckpoint") { return CloseCommands; }
					return null;
				case "MainForm.State":
					return getStateCommandTypes(eventName, details);
				case "Calculation.Result":
					if (eventName == "DirtyChanged" && details == "Current=Dirty")
					{
						return InputChangeCommands;
					}
					return null;
				case "Shikigami.Reload": return ReloadCommands;
				case "SaveData.Load": return LoadCommands;
				case "SaveData.Save": return SaveCommands;
				case "Calculation": return CalculationCommands;
				case "Snapshot.Compare": return ComparisonCommands;
				case "MainForm.Clear": return ClearCommands;
				case "Shikigami.Register": return RegisterCommands;
				case "Shikigami.Edit": return EditCommands;
				case "Shikigami.Recovery":
				case "Shikigami.BrokenFile":
				case "Shikigami.AutoRestore": return RecoveryCommands;
				default: return null;
			}
		}
	}
}