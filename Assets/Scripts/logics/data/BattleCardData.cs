using Assets.Scripts.BattleField;
using Assets.Scripts.utility;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using UnityEngine;

namespace Assets.Scripts.logics
{
    public enum BattleGroupType
    {
        NONE, // 

        HEAVY,
        CAVALRY, // 
        PROJECTILE, // 
        FIREARM, // 
        SPECIAL, // 
        MELEE, //
        SPEAR, // 
        SIEGE //
    }

    public enum BattleActionType
    {
        NONE,

        ATTACK,
        DEFENSE,
        MODIFIER,
        SPECIAL
    }

    public enum BattleTargetType
    {
        NONE,

        CENTER_ENEMY, // the first front line enemies in the groupside TOP_MIDDLE
        FLANKS_ENEMY, // left/right/front line enemies in the groupside TOP_LEFT/TOP_RIGHT if it exists. Otherwise, the TOP_MIDDLE will be included.
        FRONT_ENEMY, // front line enemies in all groupsides.
        BACK_ENEMY, // back line enemies in all groupsides.
        ALL_ENEMY, // all enemies in all groupsides.
        
        CENTER_FRIEND,
        FLANKS_FRIEND,
        FRONT_FRIEND,
        BACK_FRIEND,
        ALL_FRIEND,

        // [NOTE] below means all friend unit types...
        ALL_CAVALRY,
        ALL_FIREARM,
        ALL_SPECIAL,
        ALL_MELEE,
        ALL_SPEAR,
        ALL_SIEGE,
        ALL_PROJECTILE,
        ALL_HEAVY
    }

    // as long as the BattleActionEffectType exists in status.xlsl, then put IsUpdateStatusActionAttribute on it with 'true'.
    public class StatusFlag : Attribute { }

    public class DistantAttackFlag : Attribute { }

    public class ParabolaAttackFlag : Attribute { }

    public class CoverAllEnemiesInGroupSideFlag : Attribute { }

    public class AbleToTurnFlag : Attribute { }

    public class PlayCombatEffectFlag : Attribute { }

    public class PlayDistantEffectFlag : Attribute { }

    public class PlayDefenseEffectFlag : Attribute { }

    public class PlayUpdateStatusEffectFlag : Attribute { }

    public class DrawCardFlag : Attribute { }

    public class ReduceCardCostFlag : Attribute { }

    public class MultiplierFlag : Attribute { }

    public class SpecialFunctionFlag : Attribute { }

    public class DelayedFunctionFlag : Attribute { }

    public enum BattleActionEffectType
    {
        NONE,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        DISCIPLINE,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        MORAL,

        [StatusFlag]
        CHAOS,

        [StatusFlag]
        FLEE,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        MORAL_DAMAGE_MODIFIER,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        COUNTERATTACK_MODIFIER,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        DISCIPLINE_DAMAGE_MODIFIER,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        DEFENSE_DAMAGE_MODIFIER,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        ATTACK_MODIFIER,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        DEFENSE_MODIFIER,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        DAMAGE_TAKEN_MODIFIER,

        [MultiplierFlag, PlayUpdateStatusEffectFlag]
        MORAL_MULTIPLIER,

        [MultiplierFlag, PlayUpdateStatusEffectFlag]
        DISCIPLINE_MULTIPLIER,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        INHERIT_ALL_ACTION_POINT,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        ACTION_POINT_NEXT_TURN,

        [SpecialFunctionFlag]
        ACTION_POINT,

        [StatusFlag, SpecialFunctionFlag, DelayedFunctionFlag]
        ACTION_AFTER_TURN, 

        // TODO: need to implement
        ACTION_WHEN_ATTACK,
        ACTION_WHEN_DEFENSE,
        ACTION_WHEN_SPECIAL,
        ACTION_WHEN_TACTIC,
        ACTION_WHEN_STRATEGY,
        GAIN_TACTIC_CARD,
        GAIN_STRATEGY_CARD,
        COPY_STRATEGY_CARD,
        COPY_TACTIC_CARD,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        HEAL_AFTER_BATTLE,

        [StatusFlag, DrawCardFlag, PlayUpdateStatusEffectFlag]
        DRAW_METAL_CARD_NEXT_TURN,
        
        [StatusFlag, DrawCardFlag, PlayUpdateStatusEffectFlag]
        DRAW_WOOD_CARD_NEXT_TURN,
        
        [StatusFlag, DrawCardFlag, PlayUpdateStatusEffectFlag]
        DRAW_WATER_CARD_NEXT_TURN,
        
        [StatusFlag, DrawCardFlag, PlayUpdateStatusEffectFlag]
        DRAW_FIRE_CARD_NEXT_TURN,
        
        [StatusFlag, DrawCardFlag, PlayUpdateStatusEffectFlag]
        DRAW_EARTH_CARD_NEXT_TURN,

        [StatusFlag, DrawCardFlag, PlayUpdateStatusEffectFlag]
        DRAW_CARD_NEXT_TURN,

        //NEXT_TURN,

        [SpecialFunctionFlag]
        BURN_CARD,

        [SpecialFunctionFlag]
        DISCARD_CARD,

        [SpecialFunctionFlag, DrawCardFlag]
        DRAW_METAL_CARD,

        [SpecialFunctionFlag, DrawCardFlag]
        DRAW_WOOD_CARD,
        
        [SpecialFunctionFlag, DrawCardFlag]
        DRAW_WATER_CARD,
        
        [SpecialFunctionFlag, DrawCardFlag]
        DRAW_FIRE_CARD,
        
        [SpecialFunctionFlag, DrawCardFlag]
        DRAW_EARTH_CARD,
        
        [SpecialFunctionFlag, DrawCardFlag]
        DRAW_CARD,

        [SpecialFunctionFlag, ReduceCardCostFlag]
        REDUCE_COST_BY_1,

        [SpecialFunctionFlag, ReduceCardCostFlag]
        REDUCE_COST_BY_2,
        
        [SpecialFunctionFlag, ReduceCardCostFlag]
        REDUCE_COST_BY_3,

        //LEAVE_FIELD,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        INHERIT_ALL_DEFENSE,

        [StatusFlag, PlayUpdateStatusEffectFlag]
        IGNORE_DEFENSE,

        [PlayDefenseEffectFlag]
        TERRAIN_DEFENSE,

        [PlayDefenseEffectFlag]
        FORTIFICATION_DEFENSE,

        [PlayDefenseEffectFlag]
        ELASTIC_DEFENSE,

        [PlayDefenseEffectFlag]
        FORMATION_DEFENSE,

        [PlayDefenseEffectFlag]
        ARMOUR_DEFENSE,

        //COUNTERATTACK,

        [DistantAttackFlag, ParabolaAttackFlag, CoverAllEnemiesInGroupSideFlag, PlayDistantEffectFlag]
        SIEGE_ATTACK,

        [DistantAttackFlag, ParabolaAttackFlag, CoverAllEnemiesInGroupSideFlag, PlayDistantEffectFlag]
        PROJECTILE_ATTACK,

        [DistantAttackFlag, PlayDistantEffectFlag]
        ARMOUR_PIERCING_ATTACK,

        [AbleToTurnFlag, PlayCombatEffectFlag]
        PRIOTIZED_ATTACK,

        [AbleToTurnFlag, PlayCombatEffectFlag]
        MELEE_ATTACK,

        MAX
    }

    public enum BattleElementType
    {
        WATER = 1,
        WOOD = 2,
        FIRE = 3,
        EARTH = 4,
        METAL = 5,

        NONE = -1
    }

    public enum BattleBasicValueType
    {
        REMAINING_ACTION_POINT,
        ALL_CARD_NUMBER,
        METAL_CARD_NUMBER,
        WOOD_CARD_NUMBER,
        WATER_CARD_NUMBER,
        FIRE_CARD_NUMBER,
        EARTH_CARD_NUMBER,

        ALL_CARD_PLAYED,
        METAL_CARD_PLAYED,
        WOOD_CARD_PLAYED,
        WATER_CARD_PLAYED,
        FIRE_CARD_PLAYED,
        EARTH_CARD_PLAYED,

        MAX
    }

    public struct BattleFunction
    {
        public BattleTargetType from;
        public BattleTargetType target;
        public BattleActionEffectType effect;
        public string basicValueExpression;

        public BattleFunction(BattleTargetType _from, BattleTargetType _target, BattleActionEffectType _effect, string _basicValueExpression)
        {
            from = _from;
            target = _target;
            effect = _effect;
            basicValueExpression = _basicValueExpression;
        }

        public float EvaluateBasicValue()
        {
            if (string.IsNullOrEmpty(basicValueExpression)) return 0f;

            string evalStr = basicValueExpression;
            for (int i = 0; i < (int)BattleBasicValueType.MAX; i++) 
            {
                var basicValueType = (BattleBasicValueType)i;
                var basicValueTypeStr = basicValueType.ToString();
                if(evalStr.Contains(basicValueTypeStr))
                {
                    float ratio = BattleLogics.GetBasicValueRatio(basicValueType);
                    evalStr = evalStr.Replace(basicValueTypeStr, ratio.ToString());
                }
            }

            DataTable table = new DataTable();
            float evalRes = Convert.ToSingle(table.Compute(evalStr, ""));
            return evalRes;
        }

#if UNITY_EDITOR
        public void VerifyBasicValueExpression()
        {
            if (string.IsNullOrEmpty(basicValueExpression)) return;

            string evalStr = basicValueExpression;
            for (int i = 0; i < (int)BattleBasicValueType.MAX; i++)
            {
                var basicValueType = (BattleBasicValueType)i;
                var basicValueTypeStr = basicValueType.ToString();
                if (evalStr.Contains(basicValueTypeStr))
                {
                    float ratio = 1f;
                    evalStr = evalStr.Replace(basicValueTypeStr, ratio.ToString());
                }
            }

            DataTable table = new DataTable();
            Convert.ToSingle(table.Compute(evalStr, ""));
        }
#endif
    }

    public class BattleCardData
    {
        public Table_action.Data actionData; // raw data from Table(.xlsl)

        public int minusCost = 0; // 有些效果可以减费

        public List<BattleActionType> actionTypeList;
        public List<BattleFunction> functionList;

        public BattleCardData() 
        {
            actionTypeList = new List<BattleActionType>(); 
            functionList = new List<BattleFunction>(); 
        }

        public BattleCardData(string _id)
        {
            actionData = Table_action.data[_id];
            actionTypeList = new List<BattleActionType>();
            functionList = new List<BattleFunction>();

            foreach (var actionTypeStr in actionData.actionTypes)
            {
                var actionType = Utils.ConvertStrToEnum<BattleActionType>(actionTypeStr);
                //Debug.LogFormat("actionTypeStr: {0}, actionType: {1}", actionTypeStr, actionType);
                actionTypeList.Add(actionType);
            }

            if (actionData.functions != null)
            {
                foreach (var functionStr in actionData.functions)
                {
                    try
                    {
                        BattleFunction function = JsonConvert.DeserializeObject<BattleFunction>(functionStr);
                        functionList.Add(function);
                    }
                    catch (Exception e)
                    {
                        Debug.LogFormat("Error happens in cardID :{0}", _id);
                        Debug.LogFormat("jsonStr: {0}\n", functionStr);

                        Debug.LogException(e);
                    }
                }
            }
        }

        // [Note] this constructor will not set the Table_action 
        public BattleCardData(List<BattleActionType> _actionTypeList, List<BattleFunction> _functionList)
        {
            actionTypeList = _actionTypeList;
            functionList = _functionList;
        }
    }

    public class BattleDelayedCardData
    {
        public int roundNum; // at which roundNum, this delayed card will be used
        public BattleCardData battleCardData; // it will store the functions which need to be delayed to use.
        public BattleDelayedCardData(int _remainTurnNum) { roundNum = _remainTurnNum; battleCardData = new BattleCardData(); }

        public void AddBattleCardData(BattleCardData origCardData)
        {
            // deep copy first
            List<BattleActionType> copyActionTypeList = new List<BattleActionType>(origCardData.actionTypeList);
            List<BattleFunction> copyfunctionList = new List<BattleFunction>(origCardData.functionList);

            // find the index where 'ACTION_AFTER_TURN'
            int result = -1;
            for (int i = 0; i < copyfunctionList.Count; i++) 
            {
                if (copyfunctionList[i].effect == BattleActionEffectType.ACTION_AFTER_TURN) { result = i; break; }
            }
            Debug.Assert(result != -1);

            copyActionTypeList.RemoveAt(result);
            copyfunctionList.RemoveAt(result);

            // put it into the current data
            battleCardData.actionTypeList.AddRange(copyActionTypeList);
            battleCardData.functionList.AddRange(copyfunctionList);
        }
    }
}