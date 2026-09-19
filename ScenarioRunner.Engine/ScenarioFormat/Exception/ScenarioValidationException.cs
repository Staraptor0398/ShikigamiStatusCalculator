using System;

namespace ScenarioRunner.ScenarioFormat
{
	public class ScenarioValidationException : Exception
	{
		public ScenarioValidationException(string message) : base(message)
		{
		}
	}
}
