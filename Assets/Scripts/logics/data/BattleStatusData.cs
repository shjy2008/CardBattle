using Assets.Scripts.utility;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.logics
{
    public enum StatusEffectType
    {
        NONE,
        ATTACK_MODIFIER_EFFECT,
        DEFENSE_MODIFIER_EFFECT,
        UNITLOSS_EACHROUND_EFFECT,
        UNITLOSS_MODIFIER_EFFECT,
        COUNTERATTACK_MODIFIER_EFFECT,
        MORAL_DAMAGE_MODIFIER_EFFECT,
        DISCIPLINE_DAMAGE_MODIFIER_EFFECT,
        DEFENSE_DAMAGE_MODIFIER_EFFECT,
        HEAL_AFTER_BATTLE_EFFECT,
    }

    public class BattleStatusEffect
    {
        public StatusEffectType statusEffectType;
        public string actualValueExpression;

        public BattleStatusEffect(StatusEffectType _statusEffectType, string _actualValueExpression)
        {
            statusEffectType = _statusEffectType;
            actualValueExpression = _actualValueExpression;
        }

        public float Evaluate(float value)
        {
            string evalStr = actualValueExpression.Replace("value", value.ToString());
            //Debug.LogFormat("evalStr:{0}", evalStr);

            DataTable table = new DataTable();
            float evalRes = Convert.ToSingle(table.Compute(evalStr, ""));
            //Debug.LogFormat("evalRes:{0}", evalRes);
            return evalRes;
        }
    }

    public class BattleStatusData
    {
        public Table_status.Data tabData;

        private float value; // "layer of buffer" <--> "value of status"

        // one status can not have repeating StatusEffectType.
        private List<BattleStatusEffect> effectList;

        public BattleStatusData(string _id)
        {
            tabData = Table_status.data[_id];
            value = 0;
            effectList = new List<BattleStatusEffect>();

#if UNITY_EDITOR
            var record = new HashSet<StatusEffectType>();
#endif
            foreach(var item in tabData.effects)
            {
                var statusEffect = JsonConvert.DeserializeObject<BattleStatusEffect>(item);
#if UNITY_EDITOR
                if (record.Contains(statusEffect.statusEffectType))
                    Debug.LogErrorFormat("Can not have repeating statusEffectType : {0}", statusEffect.statusEffectType.ToString());
#endif
                effectList.Add(statusEffect);
            }
        }

        // "layer of buffer" <--> "value of status"
        public float GetValue()
        {
            return value; 
        }

        public string GetValueString()
        {
            return value.ToString("0.0").TrimEnd('0').TrimEnd('.');
        }

        public List<float> GetAllStatusActualValues()
        {
            var result = new List<float>();
            for (int i = 0; i < effectList.Count; i++)
            {
                var effectType = effectList[i].statusEffectType;
                var actualValue = GetStatusActualValue(effectType);
                result.Add(actualValue);
            }
            return result;
        }

        public void UpdateValue(float delta)
        {
            value = Mathf.Clamp(value + delta, tabData.minValue, tabData.maxValue);
            //UnityEngine.Debug.LogFormat("UpdateValue, value: {0}", value);
            //if(value == 0)
            //{
            //    StackTrace st = new StackTrace(true);
            //    UnityEngine.Debug.Log("Call Stack: ");
            //    foreach (var frame in st.GetFrames())
            //        UnityEngine.Debug.LogFormat("{0} in {1}: {2}", frame.GetMethod(), frame.GetFileName(), frame.GetFileLineNumber());
            //}
        }

        public string GetIconPath()
        {
            string iconName = (tabData.ifBuff && value >= 0) ? tabData.iconBuff : tabData.iconDebuff;
            return string.Format("images/status_icons/{0}", iconName);
        }

        public BattleActionEffectType GetStatusType()
        {
            return Utils.ConvertStrToEnum<BattleActionEffectType>(tabData.id);
        }

        public float GetStatusActualValue(StatusEffectType effectType)
        {
            foreach(var effect in effectList)
            {
                if (effect.statusEffectType == effectType) return effect.Evaluate(value);
            }
            return 0f;
        }
    }
}

