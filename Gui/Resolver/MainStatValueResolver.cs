using Gui.Common;

namespace Gui.Resolver
{
	public static class MainStatValueResolver
	{
		public static double Resolve(string statText, int slot)
		{
			double statValue = 0.0;

			switch (slot)
			{
				case 1:
					statValue = 486.0;
					break;
				case 2:
					if (statText == DisplayText.Speed)
					{
						statValue = 57.0;
					}
					else
					{
						statValue = 55.0;
					}
					break;
				case 3:
					statValue = 104.0;
					break;
				case 4:
					statValue = 55.0;
					break;
				case 5:
					statValue = 2052.0;
					break;
				case 6:
					if (statText == DisplayText.CriticalDamage)
					{
						statValue = 89.0;
					}
					else
					{
						statValue = 55.0;
					}
					break;
				default:
					break;
			}

			return statValue;
		}
	}
}
