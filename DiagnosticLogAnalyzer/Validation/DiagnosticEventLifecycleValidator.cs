using DiagnosticLogAnalyzer.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiagnosticLogAnalyzer.Validation
{
	public sealed class DiagnosticEventLifecycleValidator
	{
		public DiagnosticEventLifecycleValidationResult Validate(DiagnosticCommandCorrelationResult correlations)
		{
			if (correlations == null) { throw new ArgumentNullException(nameof(correlations)); }
			if (correlations.Events == null) { throw new ArgumentException("Command correlations are required.", nameof(correlations)); }

			DiagnosticEventLifecycleValidationResult result = new DiagnosticEventLifecycleValidationResult();
			List<DiagnosticCommandCorrelation> selected = new List<DiagnosticCommandCorrelation>();
			foreach (DiagnosticCommandCorrelation correlation in correlations.Events)
			{
				if (correlation == null || correlation.SessionCorrelation == null || correlation.SessionCorrelation.Event == null)
				{
					throw new ArgumentException("Invalid diagnostic correlation.", nameof(correlations));
				}

				// 時刻による参考候補を確定相関へ昇格させない。
				if (correlation.Status != DiagnosticCommandMatchStatus.Matched || correlation.Command == null ||
					correlation.SessionCorrelation.Status != DiagnosticSessionMatchStatus.WindowMatched ||
					correlation.SessionCorrelation.Window == null || correlation.SessionCorrelation.Scenario == null ||
					!isTarget(correlation.SessionCorrelation.Event.Operation))
				{
					result.ExcludedEvents++;
					continue;
				}
				selected.Add(correlation);
			}

			// 起動区間・コマンド・Operationがすべて同じイベントだけを一単位とする。
			// 操作ごとの一意なIDはログにないため、同一コマンド内の複数試行は判定保留にする。
			foreach (var group in selected.GroupBy(x => new
			{
				x.SessionCorrelation.Scenario,
				x.SessionCorrelation.Window,
				x.Command,
				x.SessionCorrelation.Event.Operation
			}))
			{
				DiagnosticEventLifecycle item = new DiagnosticEventLifecycle
				{
					Scenario = group.Key.Scenario,
					Window = group.Key.Window,
					Command = group.Key.Command,
					Operation = group.Key.Operation
				};
				item.Events.AddRange(group.OrderBy(x => x.SessionCorrelation.Event.Timestamp)
					.ThenBy(x => x.SessionCorrelation.Event.SourceFile, StringComparer.Ordinal)
					.ThenBy(x => x.SessionCorrelation.Event.LineNumber));
				validateLifecycle(item);
				result.Lifecycles.Add(item);
			}
			return result;
		}

		private static bool isTarget(string operation)
		{
			return operation == "Calculation" || operation == "SaveData.Load" || operation == "SaveData.Save" ||
				operation == "MainForm.Clear" || operation == "Snapshot.Compare";
		}

		private static void setVerdict(DiagnosticEventLifecycle item, DiagnosticLifecycleVerdict verdict, string reason)
		{
			item.Verdict = verdict;
			item.Reason = reason;
		}

		private static void validateLifecycle(DiagnosticEventLifecycle item)
		{
			List<DiagnosticEvent> events = item.Events.Select(x => x.SessionCorrelation.Event).ToList();
			List<DiagnosticEvent> started = events.Where(x => x.EventName == "Started").ToList();
			List<DiagnosticEvent> completed = events.Where(x => x.EventName == "Completed").ToList();
			List<DiagnosticEvent> failed = events.Where(x => x.EventName == "Failed").ToList();
			List<DiagnosticEvent> validated = events.Where(x => x.EventName == "Validated").ToList();
			List<DiagnosticEvent> rejected = events.Where(x => x.EventName == "ValidationRejected").ToList();

			// イベント数の重複は、再試行と重複記録を区別できないためFAILにしない。
			if (started.Count > 1 || completed.Count > 1 || failed.Count > 1 ||
				validated.Count > 1 || rejected.Count > 1)
			{
				setVerdict(item, DiagnosticLifecycleVerdict.Inconclusive, "Multiple attempts or duplicated lifecycle events cannot be distinguished.");
				return;
			}

			int terminalCount = completed.Count + failed.Count + rejected.Count;
			if (terminalCount > 1)
			{
				setVerdict(item, DiagnosticLifecycleVerdict.Inconclusive, "Multiple terminal outcomes cannot be assigned to a single attempt.");
				return;
			}

			if (item.Operation == "Calculation" && rejected.Count == 1)
			{
				// Guiでは計算ボタン押下時にStartedを記録し、その後の入力検証で拒否する。
				// Startedが欠落しても、ValidationRejected単独ならログ不足を許容する。
				if (validated.Count != 0)
				{
					setVerdict(item, DiagnosticLifecycleVerdict.Inconclusive, "ValidationRejected is mixed with an unexpected Validated event.");
				}
				else if (started.Count == 0)
				{
					setVerdict(item, DiagnosticLifecycleVerdict.Pass, "ValidationRejected is observed without Started.");
				}
				else if (isStrictlyReversed(started[0], rejected[0]))
				{
					// 拒否が先に記録された場合、別試行やログ欠落の可能性を否定できない。
					setVerdict(item, DiagnosticLifecycleVerdict.Inconclusive, "ValidationRejected precedes Started; attempt boundaries are unknown.");
				}
				else
				{
					setVerdict(item, DiagnosticLifecycleVerdict.Pass, "ValidationRejected follows Started in the same Calculation command.");
				}
				return;
			}

			if (started.Count != 1 || completed.Count + failed.Count != 1)
			{
				setVerdict(item, DiagnosticLifecycleVerdict.Inconclusive, "Started or terminal event is not fully observed.");
				return;
			}

			DiagnosticEvent terminal = completed.Count == 1 ? completed[0] : failed[0];
			if (isStrictlyReversed(started[0], terminal))
			{
				setVerdict(item, DiagnosticLifecycleVerdict.Fail, "Terminal event timestamp precedes Started.");
				return;
			}

			if (item.Operation == "SaveData.Load")
			{
				if (validated.Count == 1 && isStrictlyReversed(started[0], validated[0]))
				{
					setVerdict(item, DiagnosticLifecycleVerdict.Fail, "Validated timestamp precedes Started.");
					return;
				}
				if (completed.Count == 1 && validated.Count == 1 && isStrictlyReversed(validated[0], completed[0]))
				{
					setVerdict(item, DiagnosticLifecycleVerdict.Fail, "Completed timestamp precedes Validated.");
					return;
				}
				if (completed.Count == 1 && validated.Count == 0)
				{
					setVerdict(item, DiagnosticLifecycleVerdict.Inconclusive, "Validated is not observed before successful completion.");
					return;
				}
			}

			if ((item.Operation == "SaveData.Save" || item.Operation == "MainForm.Clear" || item.Operation == "Snapshot.Compare") &&
				events.Any(x => x.EventName != "Started" && x.EventName != "Completed" && x.EventName != "Failed"))
			{
				// 保存・クリア・比較はStartedと終端イベントのみを定義する。
				// 未定義イベントを無視してPASSにしない。
				setVerdict(item, DiagnosticLifecycleVerdict.Inconclusive, "Unexpected lifecycle events are observed for this operation.");
				return;
			}

			// FailedはGui操作の失敗を示すが、ライフサイクルの記録としては整合している。
			setVerdict(item, DiagnosticLifecycleVerdict.Pass, failed.Count == 1
				? "Operation failure is recorded with a preceding Started event."
				: "Expected lifecycle events are observed.");
		}

		private static bool isStrictlyReversed(DiagnosticEvent first, DiagnosticEvent second)
		{
			// ミリ秒精度で同時刻なら、ファイルへの記録順による誤判定を避ける。
			return second.Timestamp < first.Timestamp;
		}
	}
}
