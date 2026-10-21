using System.Runtime.Serialization;

namespace SaveData.Model
{
	[DataContract]
	public class StatusSaveData
	{
		[DataMember] public double Attack { get; set; }
		[DataMember] public double HP { get; set; }
		[DataMember] public double Defense { get; set; }
		[DataMember] public double Speed { get; set; }
		[DataMember] public double CritRate { get; set; }
		[DataMember] public double CritDamage { get; set; }
		[DataMember] public double EffectHit { get; set; }
		[DataMember] public double EffectResist { get; set; }

		[DataMember(Name = "Deffense", EmitDefaultValue = false)]
		private double? LegacyDeffense
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
	}
}
