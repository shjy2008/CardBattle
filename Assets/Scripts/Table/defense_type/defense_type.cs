//#file：./tab/defense_type.xlsx, sheet：defense_type，output：defense_type.cs

using System.Collections.Generic;

public class Table_defense_type
{
	public struct Data
	{
		public string id;
		public float bonusFromFormation;
		public float bonusFromCentral;
		public float bonusFromFlank;

		public Data(string _id = "", float _bonusFromFormation = 0.0f, float _bonusFromCentral = 0.0f, float _bonusFromFlank = 0.0f)
		{
			id = _id;
			bonusFromFormation = _bonusFromFormation;
			bonusFromCentral = _bonusFromCentral;
			bonusFromFlank = _bonusFromFlank;
		}
	}
	public static Dictionary<string, Data> data = new Dictionary<string, Data> {
		{"TERRAIN_DEFENSE", new Data(_id: "TERRAIN_DEFENSE")},
		{"FORTIFICATION_DEFENSE", new Data(_id: "FORTIFICATION_DEFENSE")},
		{"ELASTIC_DEFENSE", new Data(_id: "ELASTIC_DEFENSE", _bonusFromFlank: 0.4f)},
		{"FORMATION_DEFENSE", new Data(_id: "FORMATION_DEFENSE", _bonusFromFormation: 0.8f, _bonusFromCentral: 0.4f)},
		{"ARMOUR_DEFENSE", new Data(_id: "ARMOUR_DEFENSE")},
	};
}
