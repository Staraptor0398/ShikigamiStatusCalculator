using ScenarioRunner.Automation.Interop;
using ScenarioRunner.Automation.Model;
using System;
using System.Drawing;

namespace ScenarioRunner.Form.Applicator
{
	public static class FormBoundsApplicator
	{
		public static void Apply(System.Windows.Forms.Form form, WindowBounds bounds)
		{
			if (form == null)
			{
				throw new ArgumentNullException(nameof(form));
			}

			if (bounds == null)
			{
				throw new ArgumentNullException(nameof(bounds));
			}

			form.Bounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);

			WindowBounds adjustedBounds = WindowFrameHelper.GetAdjustedBounds(form.Handle, bounds);

			form.Bounds = new Rectangle(adjustedBounds.X, adjustedBounds.Y, adjustedBounds.Width, adjustedBounds.Height);
		}
	}
}
