using System;

namespace Gui.Function
{
	internal sealed class SaveDataRollbackException : Exception
	{
		public SaveDataRollbackException(Exception applyException, Exception rollbackException) : base("SaveData rollback failed.", new AggregateException(applyException, rollbackException))
		{
			if (applyException == null)
			{
				throw new ArgumentNullException(nameof(applyException));
			}

			if (rollbackException == null)
			{
				throw new ArgumentNullException(nameof(rollbackException));
			}
		}
	}
}
