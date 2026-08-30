using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ScenarioRunner.Form.Applicator
{
	public static class RichTextBoxScrollApplicator
	{
		private const int EM_GETFIRSTVISIBLELINE = 0x00CE;
		private const int EM_LINESCROLL = 0x00B6;
		private const int SCROLL_LINE_COUNT = 3;

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		public static void Apply(RichTextBox richTextBox, int lineNumber)
		{
			if (richTextBox == null)
			{
				throw new ArgumentNullException(nameof(richTextBox));
			}

			if (lineNumber < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(lineNumber));
			}

			int firstVisibleLine = SendMessage(richTextBox.Handle, EM_GETFIRSTVISIBLELINE, IntPtr.Zero, IntPtr.Zero).ToInt32();
			int visibleLineCount = richTextBox.ClientSize.Height / richTextBox.Font.Height;
			int scrollThresholdLine = firstVisibleLine + visibleLineCount * 2 / 3;

			if (lineNumber < scrollThresholdLine)
			{
				return;
			}

			SendMessage(richTextBox.Handle, EM_LINESCROLL, IntPtr.Zero, new IntPtr(SCROLL_LINE_COUNT));
		}
	}
}
