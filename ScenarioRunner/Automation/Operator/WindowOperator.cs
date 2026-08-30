using FlaUI.Core.AutomationElements;
using ScenarioRunner.Automation.Interop;
using ScenarioRunner.Automation.Model;
using System;

namespace ScenarioRunner.Automation.Operator
{
	public class WindowOperator
	{
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
	}
}
