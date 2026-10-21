using System.Text.Json.Serialization;

namespace SaveData.Model
{
	public class StatusSaveData
	{
		public double Attack { get; set; }
		public double HP { get; set; }
		public double Defense { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public double? Deffense
		{
			get => null;
			set
			{
				if (value.HasValue)
				{
					Defense = value.Value;
				}
			}
		}

		public double Speed { get; set; }
		public double CritRate { get; set; }
		public double CritDamage { get; set; }
		public double EffectHit { get; set; }
		public double EffectResist { get; set; }
	}
}
