using System.Runtime.ExceptionServices;

namespace GuiTest.TestUtil
{
	internal static class StaTestRunner
	{
		public static void Run(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			Exception failure = null;

			Thread thread = new Thread(() =>
			{
				try
				{
					action();
				}
				catch (Exception ex)
				{
					failure = ex;
				}
			});

			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();

			if (failure != null)
			{
				ExceptionDispatchInfo.Capture(failure).Throw();
			}
		}
	}
}
