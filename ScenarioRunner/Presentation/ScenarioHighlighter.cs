using ScenarioRunner.ScenarioFormat;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ScenarioRunner.Presentation
{
	public class ScenarioHighlighter
	{
		private readonly RichTextBox mRichTextBox;

		public ScenarioHighlighter(RichTextBox richTextBox)
		{
			mRichTextBox = richTextBox ?? throw new ArgumentNullException(nameof(richTextBox));
		}

		public void ApplySyntax(Scenario scenario)
		{
			if (scenario == null)
			{
				throw new ArgumentNullException(nameof(scenario));
			}

			invokeIfRequired(() =>
			{
				resetTextColor();

				applyCommentHighlight();
				applyStructureHighlight();

				foreach (ScenarioStep step in scenario.Steps)
				{
					Color color = getCommandColor(step.CommandType);
					setLineTextColor(step.LineNumber, color);
				}

				clearSelection();
			});
		}

		public void ShowRunning(ScenarioStep step)
		{
			setExecutionBackColor(step, Color.LightYellow);
		}

		public void ShowPassed(ScenarioStep step)
		{
			setExecutionBackColor(step, Color.LightGreen);
		}

		public void ShowFailed(ScenarioStep step)
		{
			setExecutionBackColor(step, Color.LightCoral);
		}

		public void ResetExecutionState()
		{
			invokeIfRequired(() =>
			{
				mRichTextBox.SelectAll();
				mRichTextBox.SelectionBackColor = mRichTextBox.BackColor;

				clearSelection();
			});
		}

		private void setExecutionBackColor(ScenarioStep step, Color backColor)
		{
			if (step == null)
			{
				throw new ArgumentNullException(nameof(step));
			}

			invokeIfRequired(() =>
			{
				if (!selectLine(step.LineNumber))
				{
					return;
				}

				mRichTextBox.SelectionBackColor = backColor;

				clearSelection();
			});
		}

		private void applyStructureHighlight()
		{
			string[] lines = mRichTextBox.Lines;

			for (int i = 0; i < lines.Length; i++)
			{
				string trimmedLine = lines[i].Trim();

				if (trimmedLine != "START" && trimmedLine != "END")
				{
					continue;
				}

				if (!selectLine(i + 1))
				{
					continue;
				}

				mRichTextBox.SelectionColor = Color.DarkBlue;
			}
		}

		private void applyCommentHighlight()
		{
			string[] lines = mRichTextBox.Lines;

			for (int i = 0; i < lines.Length; i++)
			{
				string trimmedLine = lines[i].TrimStart();

				if (!trimmedLine.StartsWith("#"))
				{
					continue;
				}

				if (!selectLine(i + 1))
				{
					continue;
				}

				mRichTextBox.SelectionColor = Color.Gray;
			}
		}

		private void setLineTextColor(int lineNumber, Color color)
		{
			if (!selectLine(lineNumber))
			{
				return;
			}

			mRichTextBox.SelectionColor = color;
		}

		private bool selectLine(int lineNumber)
		{
			int lineIndex = lineNumber - 1;

			if (lineIndex < 0 || lineIndex >= mRichTextBox.Lines.Length)
			{
				return false;
			}

			int start = mRichTextBox.GetFirstCharIndexFromLine(lineIndex);

			if (start < 0)
			{
				return false;
			}

			int length = mRichTextBox.Lines[lineIndex].Length;

			mRichTextBox.Select(start, length);

			return true;
		}

		private Color getCommandColor(ScenarioCommandType commandType)
		{
			switch (commandType)
			{
				case ScenarioCommandType.CHECK_CALCULATION:
				case ScenarioCommandType.CHECK_SHIKIGAMI:
				case ScenarioCommandType.CHECK_DIALOG:
					return Color.DarkGreen;

				case ScenarioCommandType.WAIT_SHIKIGAMI_AUTO_REPAIR:
					return Color.DarkOrange;

				case ScenarioCommandType.BREAK_SHIKIGAMI_HEADER:
				case ScenarioCommandType.REMOVE_SHIKIGAMI:
				case ScenarioCommandType.CREATE_SHIKIGAMI_BACKUP:
				case ScenarioCommandType.RECOVER_SHIKIGAMI:
					return Color.DarkRed;

				case ScenarioCommandType.OPEN_GUI:
				case ScenarioCommandType.CLOSE_GUI:
				case ScenarioCommandType.CLOSE_DIALOG:
					return Color.DarkViolet;

				default:
					return mRichTextBox.ForeColor;
			}
		}

		private void resetTextColor()
		{
			mRichTextBox.SelectAll();
			mRichTextBox.SelectionColor = mRichTextBox.ForeColor;
		}

		private void clearSelection()
		{
			mRichTextBox.Select(0, 0);
		}

		private void invokeIfRequired(Action action)
		{
			if (mRichTextBox.InvokeRequired)
			{
				mRichTextBox.Invoke(action);
				return;
			}

			action();
		}
	}
}
