//#file：./tab/attack_type.xlsx, sheet：attack_type，output：attack_type.cs

using System.Collections.Generic;

public class Table_attack_type
{
	public struct Data
	{
		public string id;
		public int defaultRange;
		public float damage;
		public float damageOnMoral;
		public float bonusOnTerrain;
		public float bonusOnFortification;
		public float bonusOnElastic;
		public float bonusOnArmour;
		public float bonusOnFormation;
		public float bonusOnFlanking;
		public float bonusFromFlank;
		public float bonusFromFormation;
		public string icon;

		public Data(string _id = "", int _defaultRange = 0, float _damage = 0.0f, float _damageOnMoral = 0.0f, float _bonusOnTerrain = 0.0f, float _bonusOnFortification = 0.0f, float _bonusOnElastic = 0.0f, float _bonusOnArmour = 0.0f, float _bonusOnFormation = 0.0f, float _bonusOnFlanking = 0.0f, float _bonusFromFlank = 0.0f, float _bonusFromFormation = 0.0f, string _icon = "")
		{
			id = _id;
			defaultRange = _defaultRange;
			damage = _damage;
			damageOnMoral = _damageOnMoral;
			bonusOnTerrain = _bonusOnTerrain;
			bonusOnFortification = _bonusOnFortification;
			bonusOnElastic = _bonusOnElastic;
			bonusOnArmour = _bonusOnArmour;
			bonusOnFormation = _bonusOnFormation;
			bonusOnFlanking = _bonusOnFlanking;
			bonusFromFlank = _bonusFromFlank;
			bonusFromFormation = _bonusFromFormation;
			icon = _icon;
		}
	}
	public static Dictionary<string, Data> data = new Dictionary<string, Data> {
		{"SIEGE_ATTACK", new Data(_id: "SIEGE_ATTACK", _defaultRange: 5, _damage: 1.0f, _damageOnMoral: 2.0f, _bonusOnTerrain: 1.0f, _bonusOnFortification: 1.0f, _bonusOnElastic: 1.0f, _bonusOnArmour: 0.4f, _bonusOnFormation: 0.4f, _bonusOnFlanking: 0.0f, _bonusFromFormation: 0.2f)},
		{"PROJECTILE_ATTACK", new Data(_id: "PROJECTILE_ATTACK", _defaultRange: 4, _damage: 1.0f, _damageOnMoral: 1.0f, _bonusOnTerrain: 1.0f, _bonusOnFortification: -0.4f, _bonusOnElastic: 1.0f, _bonusOnArmour: -0.2f, _bonusOnFormation: 0.2f, _bonusOnFlanking: 0.0f, _bonusFromFlank: 0.5f, _bonusFromFormation: 0.2f)},
		{"ARMOUR_PIERCING_ATTACK", new Data(_id: "ARMOUR_PIERCING_ATTACK", _defaultRange: 3, _damage: 1.0f, _damageOnMoral: 1.5f, _bonusOnTerrain: 1.0f, _bonusOnFortification: -0.2f, _bonusOnElastic: 1.0f, _bonusOnArmour: 0.2f, _bonusOnFormation: 0.3f, _bonusOnFlanking: 0.0f, _bonusFromFormation: 0.6f)},
		{"PRIOTIZED_ATTACK", new Data(_id: "PRIOTIZED_ATTACK", _defaultRange: 2, _damage: 1.0f, _damageOnMoral: 2.0f, _bonusOnTerrain: -0.4f, _bonusOnFortification: -0.4f, _bonusOnElastic: 0.0f, _bonusOnArmour: 0.0f, _bonusOnFormation: -0.4f, _bonusOnFlanking: 1.0f, _bonusFromFormation: 0.0f)},
		{"MELEE_ATTACK", new Data(_id: "MELEE_ATTACK", _defaultRange: 1, _damage: 1.0f, _damageOnMoral: 1.0f, _bonusOnTerrain: -0.2f, _bonusOnFortification: -0.2f, _bonusOnElastic: 0.0f, _bonusOnArmour: 0.0f, _bonusOnFormation: -0.2f, _bonusOnFlanking: 0.5f, _bonusFromFormation: 0.4f)},
	};
}
