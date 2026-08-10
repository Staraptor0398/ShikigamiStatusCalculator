using Newtonsoft.Json;

namespace Gui.SaveData
{
	public class StatusSaveData
	{
		public double Attack { get; set; }

		public double HP { get; set; }

		public double Defense { get; set; }

		[JsonProperty("Deffense")]
		private double LegacyDeffense
		{
			set => Defense = value;
		}

		public double Speed { get; set; }

		public double CritRate { get; set; }

		public double CritDamage { get; set; }

		public double EffectHit { get; set; }

		public double EffectResist { get; set; }
	}
}
