//#file：./tab/status.xlsx, sheet：status，output：status.cs

using System.Collections.Generic;

public class Table_status
{
	public struct Data
	{
		public string id;
		public string name;
		public bool ifBuff;
		public float maxValue;
		public float minValue;
		public string description;
		public string iconBuff;
		public string iconDebuff;
		public bool isPercentage;
		public List<string> effects;

		public Data(string _id = "", string _name = "", bool _ifBuff = false, float _maxValue = 0.0f, float _minValue = 0.0f, string _description = "", string _iconBuff = "", string _iconDebuff = "", bool _isPercentage = false, List<string> _effects = null)
		{
			id = _id;
			name = _name;
			ifBuff = _ifBuff;
			maxValue = _maxValue;
			minValue = _minValue;
			description = _description;
			iconBuff = _iconBuff;
			iconDebuff = _iconDebuff;
			isPercentage = _isPercentage;
			effects = _effects;
		}
	}
	public static Dictionary<string, Data> data = new Dictionary<string, Data> {
		{"DISCIPLINE", new Data(_id: "DISCIPLINE", _name: "Discipline", _ifBuff: true, _maxValue: 100.0f, _minValue: -20.0f, _description: "<link=\"tip_info_001\"><b>Defense</b></link> {}%. ", _iconBuff: "battle_state_high_discipline", _iconDebuff: "battle_state_low_discipline", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""DEFENSE_MODIFIER_EFFECT"", ""actualValueExpression"":""0.05*value""}" })},
		{"MORAL", new Data(_id: "MORAL", _name: "Moral", _ifBuff: true, _maxValue: 100.0f, _minValue: -20.0f, _description: "Attack {}%.", _iconBuff: "battle_state_high_moral", _iconDebuff: "battle_state_low_moral", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""ATTACK_MODIFIER_EFFECT"", ""actualValueExpression"":""0.05*value""}" })},
		{"CHAOS", new Data(_id: "CHAOS", _name: "Chaos", _ifBuff: false, _maxValue: 20.0f, _minValue: 0.0f, _description: "Units take {}% Damage.", _iconBuff: "battle_state_disorder", _iconDebuff: "battle_state_disorder", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""UNITLOSS_MODIFIER_EFFECT"", ""actualValueExpression"":""0.5*value""}" })},
		{"FLEE", new Data(_id: "FLEE", _name: "Fleeing", _ifBuff: false, _maxValue: 20.0f, _minValue: 0.0f, _description: "By the end of each turn, {}% of army\'s units Flee the battlefield.", _iconBuff: "battle_state_fleeting", _iconDebuff: "battle_state_fleeting", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""UNITLOSS_EACHROUND_EFFECT"", ""actualValueExpression"":""0.05*value""}" })},
		{"COUNTERATTACK_MODIFIER", new Data(_id: "COUNTERATTACK_MODIFIER", _name: "Counter Attack", _ifBuff: true, _maxValue: 50.0f, _minValue: -10.0f, _description: "Conterattack Damage {}%.", _iconBuff: "COUNTERATTACK_MODIFIER", _iconDebuff: "COUNTERATTACK_DEBUFF_MODIFIER", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""COUNTERATTACK_MODIFIER_EFFECT"", ""actualValueExpression"":""0.5*value""}" })},
		{"INHERIT_ALL_DEFENSE", new Data(_id: "INHERIT_ALL_DEFENSE", _name: "Inherit All Defense", _ifBuff: true, _maxValue: 10.0f, _minValue: 0.0f, _description: "Defense can be inherited over {} round(s).", _iconBuff: "INHERIT_ALL_DEFENSE", _iconDebuff: "INHERIT_ALL_DEFENSE", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
		{"MORAL_DAMAGE_MODIFIER", new Data(_id: "MORAL_DAMAGE_MODIFIER", _name: "Damage on Moral", _ifBuff: true, _maxValue: 50.0f, _minValue: -10.0f, _description: "Damage on Moral {} %.", _iconBuff: "MORAL_DAMAGE_MODIFIER", _iconDebuff: "MORAL_DAMAGE_DEBUFF_MODIFIER", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""MORAL_DAMAGE_MODIFIER_EFFECT"", ""actualValueExpression"":""0.1*value""}" })},
		{"IGNORE_DEFENSE", new Data(_id: "IGNORE_DEFENSE", _name: "Ignore Defense", _ifBuff: true, _maxValue: 10.0f, _minValue: 0.0f, _description: "Igonore Defense for Next {}  Card(s) You Played.", _iconBuff: "IGNORE_DEFENSE", _iconDebuff: "IGNORE_DEFENSE", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
		{"DEFENSE_DAMAGE_MODIFIER", new Data(_id: "DEFENSE_DAMAGE_MODIFIER", _name: "Demage to Defense", _ifBuff: true, _maxValue: 50.0f, _minValue: -10.0f, _description: "Demage to Defense {} %.", _iconBuff: "DEFENSE_DAMAGE_MODIFIER", _iconDebuff: "DEFENSE_DAMAGE_DEBUFF_MODIFIER", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""DEFENSE_DAMAGE_MODIFIER_EFFECT"", ""actualValueExpression"":""0.1*value""}" })},
		{"DISCIPLINE_DAMAGE_MODIFIER", new Data(_id: "DISCIPLINE_DAMAGE_MODIFIER", _name: "Damage on Discipline", _ifBuff: true, _maxValue: 50.0f, _minValue: -10.0f, _description: "Damage on Discipline {} %.", _iconBuff: "DISCIPLINE_DAMAGE_MODIFIER", _iconDebuff: "DISCIPLINE_DAMAGE_DEBUFF_MODIFIER", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""DISCIPLINE_DAMAGE_MODIFIER_EFFECT"", ""actualValueExpression"":""0.1*value""}" })},
		{"ACTION_POINT_NEXT_TURN", new Data(_id: "ACTION_POINT_NEXT_TURN", _name: "Action Point Next Turn", _ifBuff: true, _maxValue: 20.0f, _minValue: -10.0f, _description: "Next Turn, Your Action Point {} .", _iconBuff: "ACTION_POINT_NEXT_TURN", _iconDebuff: "ACTION_POINT_NEXT_TURN_DEBUFF", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
		{"DRAW_METAL_CARD_NEXT_TURN", new Data(_id: "DRAW_METAL_CARD_NEXT_TURN", _name: "Draw Metal Card Next Turn", _ifBuff: true, _maxValue: 20.0f, _minValue: 0.0f, _description: "Next Turn, You Draw {}  Additional Metal Card(s) .", _iconBuff: "DRAW_METAL_CARD_NEXT_TURN", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
		{"DRAW_WOOD_CARD_NEXT_TURN", new Data(_id: "DRAW_WOOD_CARD_NEXT_TURN", _name: "Draw Wood Card Next Turn", _ifBuff: true, _maxValue: 20.0f, _minValue: 0.0f, _description: "Next Turn, You Draw {}  Additional Wood Card(s) .", _iconBuff: "DRAW_WOOD_CARD_NEXT_TURN", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
		{"DRAW_WATER_CARD_NEXT_TURN", new Data(_id: "DRAW_WATER_CARD_NEXT_TURN", _name: "Draw Water Card Next Turn", _ifBuff: true, _maxValue: 20.0f, _minValue: 0.0f, _description: "Next Turn, You Draw {}  Additional Water Card(s) .", _iconBuff: "DRAW_WATER_CARD_NEXT_TURN", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
		{"DRAW_FIRE_CARD_NEXT_TURN", new Data(_id: "DRAW_FIRE_CARD_NEXT_TURN", _name: "Draw Fire Card Next Turn", _ifBuff: true, _maxValue: 20.0f, _minValue: 0.0f, _description: "Next Turn, You Draw {}  Additional Fire Card(s) .", _iconBuff: "DRAW_FIRE_CARD_NEXT_TURN", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
		{"DRAW_EARTH_CARD_NEXT_TURN", new Data(_id: "DRAW_EARTH_CARD_NEXT_TURN", _name: "Draw Earth Card Next Turn", _ifBuff: true, _maxValue: 20.0f, _minValue: 0.0f, _description: "Next Turn, You Draw {}  Additional Earth Card(s) .", _iconBuff: "DRAW_EARTH_CARD_NEXT_TURN", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
		{"DRAW_CARD_NEXT_TURN", new Data(_id: "DRAW_CARD_NEXT_TURN", _name: "Draw Card Next Turn", _ifBuff: true, _maxValue: 20.0f, _minValue: -10.0f, _description: "Next Turn, You Draw {}  Additional Card(s) .", _iconBuff: "DRAW_CARD_NEXT_TURN", _iconDebuff: "DRAW_CARD_NEXT_TURN_DEBUFF", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
		{"INHERIT_ALL_ACTION_POINT", new Data(_id: "INHERIT_ALL_ACTION_POINT", _name: "Inherit all action point", _ifBuff: true, _maxValue: 1.0f, _minValue: 0.0f, _description: "Action point can be inherited in next turn.", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
		{"DAMAGE_TAKEN_MODIFIER", new Data(_id: "DAMAGE_TAKEN_MODIFIER", _name: "Damage taked modifier", _ifBuff: false, _maxValue: 20.0f, _minValue: 0.0f, _description: "Units take {}% Damage.", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""UNITLOSS_MODIFIER_EFFECT"", ""actualValueExpression"":""0.5*value""}" })},
		{"ATTACK_MODIFIER", new Data(_id: "ATTACK_MODIFIER", _name: "Attack Modifier", _ifBuff: true, _maxValue: 20.0f, _minValue: -20.0f, _description: "Attack {}%.", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""ATTACK_MODIFIER_EFFECT"", ""actualValueExpression"":""0.05*value""}" })},
		{"DEFENSE_MODIFIER", new Data(_id: "DEFENSE_MODIFIER", _name: "Defense Modifier", _ifBuff: true, _maxValue: 20.0f, _minValue: -20.0f, _description: "<link=\"tip_info_001\"><b>Defense</b></link> {}%. ", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""DEFENSE_MODIFIER_EFFECT"", ""actualValueExpression"":""0.05*value""}" })},
		{"HEAL_AFTER_BATTLE", new Data(_id: "HEAL_AFTER_BATTLE", _name: "Heal After Battle", _ifBuff: true, _maxValue: 1.0f, _minValue: 0.0f, _description: "Units will recover {}% amount after battle.", _iconBuff: "HEAL_AFTER_BATTLE", _isPercentage: true, _effects: new List<string>(){ @"{""statusEffectType"":""HEAL_AFTER_BATTLE_EFFECT"", ""actualValueExpression"":""value""}" })},
		{"ACTION_AFTER_TURN", new Data(_id: "ACTION_AFTER_TURN", _name: "Action After Turn", _ifBuff: true, _maxValue: 99.0f, _minValue: 0.0f, _description: "Actions will be used after {} turn(s).", _iconBuff: "DELAYED_FUNCTIONS", _isPercentage: false, _effects: new List<string>(){ @"{""statusEffectType"":""NONE"", ""actualValueExpression"":""value""}" })},
	};
}
