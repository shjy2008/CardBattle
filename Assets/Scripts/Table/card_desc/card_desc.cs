//#file：./tab/card_desc.xlsx, sheet：card_desc，output：card_desc.cs

using System.Collections.Generic;

public class Table_card_desc
{
	public struct Data
	{
		public string Enumeration;
		public string Description_en;
		public string Description_cn;

		public Data(string _Enumeration = "", string _Description_en = "", string _Description_cn = "")
		{
			Enumeration = _Enumeration;
			Description_en = _Description_en;
			Description_cn = _Description_cn;
		}
	}
	public static Dictionary<string, Data> data = new Dictionary<string, Data> {
		{"NONE", new Data(_Enumeration: "NONE", _Description_en: "NaN (Not Applicable)", _Description_cn: "不适用")},
		{"CENTER_ENEMY", new Data(_Enumeration: "CENTER_ENEMY", _Description_en: "Enemy Center", _Description_cn: "敌方中心")},
		{"FLANKS_ENEMY", new Data(_Enumeration: "FLANKS_ENEMY", _Description_en: "Enemy Flanks", _Description_cn: "敌方侧翼")},
		{"FRONT_ENEMY", new Data(_Enumeration: "FRONT_ENEMY", _Description_en: "Enemy Front", _Description_cn: "敌方前方")},
		{"BACK_ENEMY", new Data(_Enumeration: "BACK_ENEMY", _Description_en: "Enemy Back", _Description_cn: "敌方后方")},
		{"ALL_ENEMY", new Data(_Enumeration: "ALL_ENEMY", _Description_en: "All Enemy Units", _Description_cn: "所有敌人")},
		{"CENTER_FRIEND", new Data(_Enumeration: "CENTER_FRIEND", _Description_en: "Your Center", _Description_cn: "我方中心")},
		{"FLANKS_FRIEND", new Data(_Enumeration: "FLANKS_FRIEND", _Description_en: "Your Flanks", _Description_cn: "我方侧翼")},
		{"FRONT_FRIEND", new Data(_Enumeration: "FRONT_FRIEND", _Description_en: "Your Front", _Description_cn: "我方前方")},
		{"BACK_FRIEND", new Data(_Enumeration: "BACK_FRIEND", _Description_en: "Your Back", _Description_cn: "我方后方")},
		{"ALL", new Data(_Enumeration: "ALL", _Description_en: "All Your Units", _Description_cn: "所有我方单位")},
		{"ALL_FRIEND", new Data(_Enumeration: "ALL_FRIEND", _Description_en: "All Your Units", _Description_cn: "所有我方单位")},
		{"ALL_CAVALRY", new Data(_Enumeration: "ALL_CAVALRY", _Description_en: "Your Cavalry", _Description_cn: "你的骑兵")},
		{"ALL_RANGED", new Data(_Enumeration: "ALL_RANGED", _Description_en: "Your Ranged Units", _Description_cn: "你的远程单位")},
		{"ALL_FIREARM", new Data(_Enumeration: "ALL_FIREARM", _Description_en: "Your Firearm Units", _Description_cn: "你的火器单位")},
		{"ALL_SPECIAL", new Data(_Enumeration: "ALL_SPECIAL", _Description_en: "Your Special Units", _Description_cn: "你的特种单位")},
		{"ALL_MELEE", new Data(_Enumeration: "ALL_MELEE", _Description_en: "Your Melee Units", _Description_cn: "你的近战单位")},
		{"ALL_SPEAR", new Data(_Enumeration: "ALL_SPEAR", _Description_en: "Your Spear Units", _Description_cn: "你的矛兵单位")},
		{"ALL_SIEGE", new Data(_Enumeration: "ALL_SIEGE", _Description_en: "Your Siege Units", _Description_cn: "你的攻城器械")},
		{"ALL_HEAVY", new Data(_Enumeration: "ALL_HEAVY", _Description_en: "Your Heavy Units", _Description_cn: "你的重装步兵")},
		{"ALL_PROJECTILE", new Data(_Enumeration: "ALL_PROJECTILE", _Description_en: "Your Projectile Units", _Description_cn: "你的弹射单位")},
		{"YOU", new Data(_Enumeration: "YOU", _Description_en: "You", _Description_cn: "你")},
		{"ENEMY", new Data(_Enumeration: "ENEMY", _Description_en: "Enemy", _Description_cn: "敌人")},
		{"DISCIPLINE", new Data(_Enumeration: "DISCIPLINE", _Description_en: "<b>Discipline</b> of <target> <basicValue>", _Description_cn: "<target>的<b>纪律</b><basicValue>")},
		{"MORAL", new Data(_Enumeration: "MORAL", _Description_en: "<b>Morale</b> of <target> <basicValue>", _Description_cn: "<target>的<b>士气</b><basicValue>")},
		{"MORAL_DAMAGE_MODIFIER", new Data(_Enumeration: "MORAL_DAMAGE_MODIFIER", _Description_en: "<b>Morale Damage Effect</b> on Others by <target> <basicValue>", _Description_cn: "<target>对他人的<b>士气打击效果</b><basicValue>")},
		{"DISCIPLINE_DAMAGE_MODIFIER", new Data(_Enumeration: "DISCIPLINE_DAMAGE_MODIFIER", _Description_en: "<b>Discipline Damage Effect</b> on Others by <target> <basicValue>", _Description_cn: "<target>对他人的<b>纪律打击效果</b><basicValue>")},
		{"DEFENSE_DAMAGE_MODIFIER", new Data(_Enumeration: "DEFENSE_DAMAGE_MODIFIER", _Description_en: "<b>Defense Damage</b> on Others by <target> <basicValue>", _Description_cn: "<target>对他人的<b>防御的伤害</b><basicValue>")},
		{"ATTACK_MODIFIER", new Data(_Enumeration: "ATTACK_MODIFIER", _Description_en: "<b>Damage</b> on Others by <target> <basicValue>", _Description_cn: "<target>对他人的<b>伤害</b><basicValue>")},
		{"IGNORE_DEFENSE", new Data(_Enumeration: "IGNORE_DEFENSE", _Description_en: "Next <basicValue> attack(s) by <target> ignores enemy <b>Defense</b>", _Description_cn: "<target>的下<basicValue>次攻击无视敌人<b>防御</b>")},
		{"COUNTERATTACK_MODIFIER", new Data(_Enumeration: "COUNTERATTACK_MODIFIER", _Description_en: "<b>Counterattack Damage</b> by <target> <basicValue>", _Description_cn: "<target>的<b>反击伤害</b><basicValue>")},
		{"INHERIT_ALL_DEFENSE", new Data(_Enumeration: "INHERIT_ALL_DEFENSE", _Description_en: "Next <basicValue> turn(s), <target> inherits all <b>Defense</b>", _Description_cn: "<target>下<basicValue>回合可以继承所有<b>防御值</b>")},
		{"ACTION_POINT_NEXT_TURN", new Data(_Enumeration: "ACTION_POINT_NEXT_TURN", _Description_en: "Next Turn <b>Action Points</b> for <target> <basicValue>", _Description_cn: "<target>下一回合的<b>行动力</b><basicValue>")},
		{"DRAW_METAL_CARD_NEXT_TURN", new Data(_Enumeration: "DRAW_METAL_CARD_NEXT_TURN", _Description_en: "Draw <basicValue> <b>Metal Card(s)</b> Next Turn for <target>", _Description_cn: "<target>下一回合抽<basicValue>张<b>金属性卡片</b>")},
		{"DRAW_WOOD_CARD_NEXT_TURN", new Data(_Enumeration: "DRAW_WOOD_CARD_NEXT_TURN", _Description_en: "Draw <basicValue> <b>Wood Card(s)</b> Next Turn for <target>", _Description_cn: "<target>下一回合抽<basicValue>张<b>木属性卡片</b>")},
		{"DRAW_WATER_CARD_NEXT_TURN", new Data(_Enumeration: "DRAW_WATER_CARD_NEXT_TURN", _Description_en: "Draw <basicValue> <b>Water Card(s)</b> Next Turn for <target>", _Description_cn: "<target>下一回合抽<basicValue>张<b>水属性卡片</b>")},
		{"DRAW_FIRE_CARD_NEXT_TURN", new Data(_Enumeration: "DRAW_FIRE_CARD_NEXT_TURN", _Description_en: "Draw <basicValue> <b>Fire Card(s)</b> Next Turn for <target>", _Description_cn: "<target>下一回合抽<basicValue>张<b>火属性卡片</b>")},
		{"DRAW_EARTH_CARD_NEXT_TURN", new Data(_Enumeration: "DRAW_EARTH_CARD_NEXT_TURN", _Description_en: "Draw <basicValue> <b>Earth Card(s)</b> Next Turn for <target>", _Description_cn: "<target>下一回合抽<basicValue>张<b>土属性卡片</b>")},
		{"BURN_CARD", new Data(_Enumeration: "BURN_CARD", _Description_en: "<b>Burn</b> <basicValue> card(s) by <target>", _Description_cn: "<target><b>烧毁</b><basicValue>张卡片")},
		{"NEXT_TURN", new Data(_Enumeration: "NEXT_TURN", _Description_en: "<target> enters next turn", _Description_cn: "<target>进入下一回合")},
		{"LEAVE_FIELD", new Data(_Enumeration: "LEAVE_FIELD", _Description_en: "<target> cannot act for <basicValue> turn(s)", _Description_cn: "<basicValue>回合内<target>无法行动")},
		{"TERRAIN_DEFENSE", new Data(_Enumeration: "TERRAIN_DEFENSE", _Description_en: "<from> provides <basicValue> <b>Terrain Defense</b>", _Description_cn: "<from>提供<basicValue><b>地形防御</b>")},
		{"FORTIFICATION_DEFENSE", new Data(_Enumeration: "FORTIFICATION_DEFENSE", _Description_en: "<from> provides <basicValue> <b>Fortification Defense</b>", _Description_cn: "<from>提供<basicValue><b>工事防御</b>")},
		{"ELASTIC_DEFENSE", new Data(_Enumeration: "ELASTIC_DEFENSE", _Description_en: "<from> provides <basicValue> <b>Elastic Defense</b>", _Description_cn: "<from>提供<basicValue><b>弹性防御</b>")},
		{"FORMATION_DEFENSE", new Data(_Enumeration: "FORMATION_DEFENSE", _Description_en: "<from> provides <basicValue> <b>Formation Defense</b>", _Description_cn: "<from>提供<basicValue><b>阵型防御</b>")},
		{"ARMOUR_DEFENSE", new Data(_Enumeration: "ARMOUR_DEFENSE", _Description_en: "<from> provides <basicValue> <b>Armour Defense</b>", _Description_cn: "<from>提供<basicValue><b>护甲防御</b>")},
		{"SIEGE_ATTACK", new Data(_Enumeration: "SIEGE_ATTACK", _Description_en: "<from> inflicts <basicValue> <b>Siege Damage</b> on <target>", _Description_cn: "<from>对<target>造成<basicValue><b>攻城伤害</b>")},
		{"PROJECTILE_ATTACK", new Data(_Enumeration: "PROJECTILE_ATTACK", _Description_en: "<from> inflicts <basicValue> <b>Projectile Damage</b> on <target>", _Description_cn: "<from>对<target>造成<basicValue><b>投射伤害</b>")},
		{"ARMOUR_PIERCING_ATTACK", new Data(_Enumeration: "ARMOUR_PIERCING_ATTACK", _Description_en: "<from> inflicts <basicValue> <b>Armour-Piercing Damage</b> on <target>", _Description_cn: "<from>对<target>造成<basicValue><b>穿甲伤害</b>")},
		{"PRIOTIZED_ATTACK", new Data(_Enumeration: "PRIOTIZED_ATTACK", _Description_en: "<from> inflicts <basicValue> <b>Prioritized Damage</b> on <target>", _Description_cn: "<from>对<target>造成<basicValue><b>优先伤害</b>")},
		{"MELEE_ATTACK", new Data(_Enumeration: "MELEE_ATTACK", _Description_en: "<from> inflicts <basicValue> <b>Melee Damage</b> on <target>", _Description_cn: "<from>对<target>造成<basicValue><b>近战伤害</b>")},
		{"ACTION_AFTER_TURN", new Data(_Enumeration: "ACTION_AFTER_TURN", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"DISCARD_CARD", new Data(_Enumeration: "DISCARD_CARD", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"HEAL_AFTER_BATTLE", new Data(_Enumeration: "HEAL_AFTER_BATTLE", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"MORAL_MULTIPLIER", new Data(_Enumeration: "MORAL_MULTIPLIER", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"DAMAGE_TAKEN_MODIFIER", new Data(_Enumeration: "DAMAGE_TAKEN_MODIFIER", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"DEFENSE_MODIFIER", new Data(_Enumeration: "DEFENSE_MODIFIER", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"INHERIT_ALL_ACTION_POINT", new Data(_Enumeration: "INHERIT_ALL_ACTION_POINT", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"DRAW_CARD_NEXT_TURN", new Data(_Enumeration: "DRAW_CARD_NEXT_TURN", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"DRAW_METAL_CARD", new Data(_Enumeration: "DRAW_METAL_CARD", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"DRAW_WOOD_CARD", new Data(_Enumeration: "DRAW_WOOD_CARD", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"DRAW_WATER_CARD", new Data(_Enumeration: "DRAW_WATER_CARD", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"DRAW_FIRE_CARD", new Data(_Enumeration: "DRAW_FIRE_CARD", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"DRAW_EARTH_CARD", new Data(_Enumeration: "DRAW_EARTH_CARD", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"DRAW_CARD", new Data(_Enumeration: "DRAW_CARD", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"REDUCE_COST_BY_1", new Data(_Enumeration: "REDUCE_COST_BY_1", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"REDUCE_COST_BY_2", new Data(_Enumeration: "REDUCE_COST_BY_2", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
		{"REDUCE_COST_BY_3", new Data(_Enumeration: "REDUCE_COST_BY_3", _Description_en: "Not finished. Need to update the card_desc.xlsx", _Description_cn: "Not finished. Need to update the card_desc.xlsx")},
	};
}
