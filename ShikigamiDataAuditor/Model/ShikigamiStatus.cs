using System;
using System.Collections.Generic;

namespace ShikigamiDataAuditor.Model
{
	public class ShikigamiStatus
	{
		public string Rarity { get; set; }
		public string Name { get; set; }
		public StatScope StatScope { get; set; }
		public IReadOnlyDictionary<StatType, double> Values { get; set; }

		public ShikigamiStatus()
		{
			Values = new Dictionary<StatType, double>();
		}

		public bool TryGetValue(StatType statType, out double value)
		{
			return Values.TryGetValue(statType, out value);
		}

		public string GetKey()
		{
			return (Rarity ?? "").Trim().ToUpperInvariant() + "\t" + (Name ?? "").Trim();
		}
	}
}
