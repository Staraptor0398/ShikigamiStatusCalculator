using DiagnosticLogAnalyzer.Model;
using System;
using System.Globalization;

namespace DiagnosticLogAnalyzer.Correlation
{
	public sealed class DiagnosticCommandContextAnalyzer
	{
		private const string TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fff";

		public DiagnosticCommandExecutionContextResult Analyze(DiagnosticCommandCorrelationResult correlations)
		{
			if (correlations == null) { throw new ArgumentNullException(nameof(correlations)); }
			if (correlations.Events == null) { throw new ArgumentException("Command correlations are required.", nameof(correlations)); }

			DiagnosticCommandExecutionContextResult result = new DiagnosticCommandExecutionContextResult();

			foreach (DiagnosticCommandCorrelation correlation in correlations.Events)
			{
				if (correlation == null)
				{
					throw new ArgumentException("Null command correlation.", nameof(correlations));
				}
				if (correlation.Status != DiagnosticCommandMatchStatus.NoMatchingCommand) { continue; }

				DiagnosticEventCorrelation session = correlation.SessionCorrelation;
				if (session == null || session.Status != DiagnosticSessionMatchStatus.WindowMatched ||
					session.Event == null || session.Scenario == null || session.Window == null ||
					session.Scenario.Commands == null)
				{
					throw new ArgumentException("NoMatchingCommand requires a confirmed window and Scenario commands.", nameof(correlations));
				}

				DiagnosticCommandExecutionContext context = new DiagnosticCommandExecutionContext
				{
					CommandCorrelation = correlation
				};
				foreach (CommandRecord command in session.Scenario.Commands)
				{
					if (command == null) { throw new ArgumentException("Null Scenario command.", nameof(correlations)); }
					if (!DateTime.TryParseExact(command.Start, TimestampFormat, CultureInfo.InvariantCulture,
						DateTimeStyles.None, out DateTime start)) { continue; }
					if (!DateTime.TryParseExact(command.End, TimestampFormat, CultureInfo.InvariantCulture,
						DateTimeStyles.None, out DateTime end) || end < start) { continue; }

					// 確定相関と同じ閉区間を使う。境界で重複した場合は選択しない。
					if (start <= session.Event.Timestamp && session.Event.Timestamp <= end)
					{
						context.ActiveCommands.Add(command);
					}
				}

				if (context.ActiveCommands.Count == 1)
				{
					context.Status = DiagnosticCommandContextStatus.ObservedDuringCommand;
					context.ActiveCommand = context.ActiveCommands[0];
				}
				else if (context.ActiveCommands.Count > 1)
				{
					context.Status = DiagnosticCommandContextStatus.MultipleActiveCommands;
				}
				else
				{
					context.Status = DiagnosticCommandContextStatus.NoActiveCommand;
				}

				result.Events.Add(context);
			}
			return result;
		}
	}
}
