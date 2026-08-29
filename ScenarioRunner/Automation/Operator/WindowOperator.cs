using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using ScenarioRunner.Automation.Interop;
using ScenarioRunner.Automation.Model;
using System;
using System.Linq;
using System.Threading;

namespace ScenarioRunner.Automation.Operator
{
	public class WindowOperator
	{
		private const int DEFAULT_TIMEOUT_MS = 5000;
		private const int DEFAULT_INTERVAL_MS = 100;

		public Window FindWindow(GuiSession session, Func<AutomationElement, bool> predicate)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			AutomationElement desktop = session.Automation.GetDesktop();

			AutomationElement windowElement = desktop.FindAllChildren(cf => cf.ByControlType(ControlType.Window)).FirstOrDefault(predicate);

			return windowElement?.AsWindow();
		}

		public bool Exists(GuiSession session, Func<AutomationElement, bool> predicate)
		{
			return FindWindow(session, predicate) != null;
		}

		public Window WaitForWindow(GuiSession session, Func<AutomationElement, bool> predicate)
		{
			return WaitForWindow(session, predicate, DEFAULT_TIMEOUT_MS, DEFAULT_INTERVAL_MS);
		}

		public Window WaitForWindow(GuiSession session, Func<AutomationElement, bool> predicate, int timeoutMs, int intervalMs)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			if (timeoutMs <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(timeoutMs));
			}

			if (intervalMs <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(intervalMs));
			}

			int elapsed = 0;

			while (elapsed < timeoutMs)
			{
				Window window = FindWindow(session, predicate);

				if (window != null)
				{
					return window;
				}

				Thread.Sleep(intervalMs);
				elapsed += intervalMs;
			}

			throw new InvalidOperationException($"Window was not found within {timeoutMs} ms.");
		}

		public Window WaitForFileDialog(GuiSession session)
		{
			if (session == null)
			{
				throw new ArgumentNullException(nameof(session));
			}

			return WaitForWindow(session, isFileDialog);
		}
		public void SetBounds(Window window, WindowBounds bounds)
		{
			if (window == null)
			{
				throw new ArgumentNullException(nameof(window));
			}

			if (bounds == null)
			{
				throw new ArgumentNullException(nameof(bounds));
			}

			var transformPattern = window.Patterns.Transform.Pattern;

			if (!transformPattern.CanMove.Value)
			{
				throw new InvalidOperationException("Window cannot be moved.");
			}

			if (!transformPattern.CanResize.Value)
			{
				throw new InvalidOperationException("Window cannot be resized.");
			}

			IntPtr windowHandle = window.Properties.NativeWindowHandle.Value;

			// 一度ターゲット位置へ仮配置する。
			// 不可視フレームは配置位置によって変化するため、
			// 移動後の状態で補正量を取得する必要がある。
			transformPattern.Move(bounds.X, bounds.Y);
			transformPattern.Resize(bounds.Width, bounds.Height);

			WindowBounds adjustedBounds = WindowFrameHelper.GetAdjustedBounds(windowHandle, bounds);

			transformPattern.Move(adjustedBounds.X, adjustedBounds.Y);

			transformPattern.Resize(adjustedBounds.Width, adjustedBounds.Height);
		}

		private bool isFileDialog(AutomationElement element)
		{
			AutomationElement[] comboBoxes = element.FindAllDescendants(cf => cf.ByControlType(ControlType.ComboBox));

			AutomationElement[] buttons = element.FindAllDescendants(cf => cf.ByControlType(ControlType.Button));

			bool hasFileNameInput = comboBoxes.Any(comboBox => comboBox.FindFirstDescendant(cf => cf.ByControlType(ControlType.Edit)) != null);

			bool hasOpenButton = buttons.Any(button => button.Name.StartsWith("開く", StringComparison.OrdinalIgnoreCase) || button.Name.StartsWith("Open", StringComparison.OrdinalIgnoreCase));

			return hasFileNameInput && hasOpenButton;
		}
	}
}
