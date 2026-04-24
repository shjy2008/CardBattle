using Assets.Scripts.utility;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace Assets.Scripts.logics
{
    [Serializable]
    public class BattleGroupData
    {
        public static int maxAmount = 1000;

        [JsonProperty("unitsMAX")]
        private Dictionary<string, int> unitsMAX; // record the maximum amount for each unit in this group

        [JsonProperty("units")]
        private Dictionary<string, int> units; // key is the unit id in the group, value is its current amount.

        public BattleGroupData()
        {
            unitsMAX = new Dictionary<string, int>();
            units = new Dictionary<string, int>();
            incomingLossDict = new Dictionary<string, int>();
        }

        public BattleGroupData(BattleGroupData copyData)
        {
            unitsMAX = new Dictionary<string, int>(copyData.GetUnitsMAX());
            units = new Dictionary<string, int>(copyData.GetUnits());
            incomingLossDict = new Dictionary<string, int>(copyData.GetIncomingLossDict()); // TODO:这个变量什么用的？也是这样拷贝吗？
        }

        public void AddUnit(string id, int amount)
        {
            if (!units.ContainsKey(id)) units.Add(id, 0);
            units[id] += amount;

            if (!unitsMAX.ContainsKey(id)) unitsMAX.Add(id, 0);
            unitsMAX[id] = Mathf.Max(units[id], unitsMAX[id]);

            if (!incomingLossDict.ContainsKey(id)) incomingLossDict.Add(id, 0);
        }

        public Dictionary<string, int> GetUnitsMAX() { return unitsMAX; }
        public Dictionary<string, int> GetUnits() { return units; }
        public Dictionary<string, int> GetIncomingLossDict() { return incomingLossDict; }

        // TODO: it is for testing only. Later to delete it.
        public void InitByRandom()
        {
            var idPrefixList = new List<string>() { "unit_00", "unit_50", "unit_60" };

            // randomly add 3 types unit with random amount.
            for (int i = 0; i < 3; i++)
            {
                int index = UnityEngine.Random.Range(1, 35);
                int j = UnityEngine.Random.Range(0, idPrefixList.Count);
                var idPrefix = idPrefixList[j];
                if (j == 1 || j == 2) index = Mathf.Min(j, 8);
                string id = idPrefix + (index <= 9 ? "0" + index : index);
                int amount = UnityEngine.Random.Range(200, 300);
                AddUnit(id, amount);
            }
        }

        public int GetTotalAmountBeforeLoss()
        {
            return units.Values.Sum();
        }

        public int GetTotalMaxAmount()
        {
            return unitsMAX.Values.Sum();
        }

        //public int GetAmount(string id)
        //{
        //    return units.ContainsKey(id) ? units[id] : 0;
        //}

        public int GetAmountByBattleGroupType(BattleGroupType targetType, bool needApplyLoss)
        {
            int result = 0;
            foreach (var id in units.Keys)
            {
                var amount = needApplyLoss ? GetAmountAfterLoss(id) : GetAmountBeforeLoss(id);
                if (amount <= 0) continue;
                var unitTabData = Table_unit.data[id];
                var groupType = Utils.ConvertStrToEnum<BattleGroupType>(unitTabData.group);
                if (groupType == targetType) result += amount;
            }
            return result;
        }

        public HashSet<string> GetUnitIDs(bool needApplyLoss)
        {
            var result = new HashSet<string>();
            foreach (var id in units.Keys)
            {
                var amount = needApplyLoss ? GetAmountAfterLoss(id) : GetAmountBeforeLoss(id);
                if (amount > 0) result.Add(id);
            }
            return result;
        }

        public HashSet<string> GetUnitIDs(BattleGroupType targetType, bool needApplyLoss)
        {
            var result = new HashSet<string>();
            foreach (var id in units.Keys)
            {
                var amount = needApplyLoss ? GetAmountAfterLoss(id) : GetAmountBeforeLoss(id);
                if (amount > 0)
                {
                    var unitTabData = Table_unit.data[id];
                    var groupType = Utils.ConvertStrToEnum<BattleGroupType>(unitTabData.group);
                    if (groupType == targetType) result.Add(id);
                }
            }
            return result;
        }

        // [Note] not apply loss
        public Dictionary<string, int> GetUnitsDictCopyBeforeLoss()
        {
            return new Dictionary<string, int>(units);
        }

        //public Dictionary<string, int> GetUnits(bool needApplyLoss)
        //{
        //    var result = new Dictionary<string, int>();
        //    foreach(var id in units.Keys)
        //    {
        //        var amount = needApplyLoss ? GetAmountAfterLoss(id) : GetAmountBeforeLoss(id);
        //        if (amount > 0) result.Add(id, amount);
        //    }
        //    return result;
        //}

        //public Dictionary<string, int> GetUnits(BattleGroupType targetType, bool needApplyLoss)
        //{
        //    var result = new Dictionary<string, int>();
        //    foreach (var id in units.Keys)
        //    {
        //        var amount = needApplyLoss ? GetAmountAfterLoss(id) : GetAmountBeforeLoss(id);
        //        if (amount <= 0) continue;
        //        var unitTabData = Table_unit.data[id];
        //        var groupType = Utils.ConvertStrToEnum<BattleGroupType>(unitTabData.group);
        //        if (groupType == targetType) result.Add(id, amount);
        //    }
        //    return result;
        //}

        // return the BattleGroupType of units with the max amount
        public BattleGroupType GetMajorBattleGroupType(bool needApplyLoss)
        {
            int maxAmount = 0;
            string unitID = null;
            foreach (var id in units.Keys)
            {
                var amount = needApplyLoss ? GetAmountAfterLoss(id) : GetAmountBeforeLoss(id);
                if (amount > maxAmount)
                {
                    maxAmount = amount;
                    unitID = id;
                }
            }
            if (unitID == null) return BattleGroupType.NONE;

            var unitTabData = Table_unit.data[unitID];
            return Utils.ConvertStrToEnum<BattleGroupType>(unitTabData.group);
        }

        public bool IsEmpty(bool needApplyLoss)
        {
            if (needApplyLoss) return GetTotalAmountAfterLoss() <= 0;
            else return units.Count <= 0;
        }

        public bool HasBattleGroupType(BattleGroupType targetType, bool needApplyLoss)
        {
            foreach (var kv in units)
            {
                var unitTabData = Table_unit.data[kv.Key];
                var amount = needApplyLoss ? GetAmountAfterLoss(kv.Key) : GetAmountBeforeLoss(kv.Key);
                if (amount <= 0) continue;
                var groupType = Utils.ConvertStrToEnum<BattleGroupType>(unitTabData.group);
                if (groupType == targetType) return true;
            }
            return false;
        }

        //public bool ContaintsAtLeastOneUnitID(ref HashSet<string> unitIDs)
        //{
        //    foreach (var kv in units) if (unitIDs.Contains(kv.Key)) return true;
        //    return false;
        //}

        public Table_unit.Data GetUnitTableData(BattleGroupType targetType, bool needApplyLoss)
        {
            foreach (var kv in units)
            {
                var unitTabData = Table_unit.data[kv.Key];
                var groupType = Utils.ConvertStrToEnum<BattleGroupType>(unitTabData.group);
                if (groupType == targetType) return unitTabData;
            }
            return new Table_unit.Data();
        }

        //public string GetUnitID(BattleGroupType targetType)
        //{
        //    foreach (var kv in units)
        //    {
        //        var unitTabData = Table_unit.data[kv.Key];
        //        var groupType = Utils.ConvertStrToEnum<BattleGroupType>(unitTabData.group);
        //        if (groupType == targetType) return kv.Key;
        //    }
        //    return null;
        //}

        #region unit loss pre-computation
        [JsonProperty("incomingLossDict")]
        private Dictionary<string, int> incomingLossDict; // key is the unit id in the group, value is its incoming loss.

        // it will be called right after ending preview a card
        // or after using a card to apply loss
        public void ResetIncomingLossDict()
        {
            incomingLossDict.Clear();
            foreach (var kv in units) incomingLossDict.Add(kv.Key, 0);
        }

        // it will be called during the card preview.
        public void CacheLoss(int incomingLoss)
        {
            // distribute the loss over all units
            // same method as CalculateActualLoss()
            int totalAmount = 0;
            var pickedUnitIDs = new Queue<string>();
            foreach (var kv in units)
            {
                if (kv.Value > 0)
                {
                    totalAmount += kv.Value;
                    pickedUnitIDs.Enqueue(kv.Key);
                }
            }

            Debug.Assert(incomingLoss <= totalAmount);

            while (pickedUnitIDs.Count > 0 && incomingLoss > 0)
            {
                var unitID = pickedUnitIDs.Dequeue();
                int unitAmount = GetAmountAfterLoss(unitID);

                // randomly picke a num for minusing the preLoss
                int num = UnityEngine.Random.Range(unitAmount / 2, unitAmount * 2); // random pick quickly
                num = Mathf.Min(num, unitAmount); // make sure: num <= unitAmount
                if (num < unitAmount) pickedUnitIDs.Enqueue(unitID); // if there are remaining units, add it back

                num = Mathf.Min(num, incomingLoss); // make sure: num <= preLoss
                incomingLoss -= num;
                incomingLossDict[unitID] += num;
            }
        }

        // it will be called right after the card is used.
        public void ApplyLoss()
        {
            var unitIDList = units.Keys.ToList();
            foreach (var unitID in unitIDList)
            {
                units[unitID] = GetAmountAfterLoss(unitID);
                if (units[unitID] <= 0)
                {
                    units.Remove(unitID);
                    incomingLossDict.Remove(unitID);
                }
            }

            ResetIncomingLossDict();
        }

        public int GetAmountBeforeLoss(string id)
        {
            return units.TryGetValue(id, out var amount) ? amount : 0;
        }

        public int GetAmountAfterLoss(string id)
        {
            if (!units.TryGetValue(id, out var amount)) return 0;
            incomingLossDict.TryGetValue(id, out var incomingLoss);
            return amount - incomingLoss;
        }

        public int GetTotalAmountAfterLoss()
        {
            int result = 0;
            foreach (var unitID in units.Keys)
            {
                result += GetAmountAfterLoss(unitID);
            }
            return result;
        }

        public int GetTotalIncomingLoss()
        {
            return incomingLossDict.Values.Sum();
        }

        // TODO: to delete it later
        public bool Test()
        {
            foreach (var id in units.Keys)
            {
                int incomingLoss = incomingLossDict[id];

                if (units[id] < incomingLoss) return true;
            }
            return false;
        }
        #endregion

        public float GetTotalValues(bool needApplyLoss, ref Dictionary<string, float> inputDict)
        {
            float totalValues = 0;
            foreach (var kv in units)
            {
                var amount = needApplyLoss ? GetAmountAfterLoss(kv.Key) : GetAmountBeforeLoss(kv.Key);
                if (amount <= 0) continue;
                var value = inputDict[kv.Key];
                totalValues += (amount * value);
            }
            return totalValues;
        }

        public void ApplyLossRatio(float ratio)
        {
            Debug.Assert(ratio > 0);
            var unitIDList = units.Keys.ToList();
            foreach (var unitID in unitIDList)
            {
                // it is ok to be < 0, because we will remove it below
                units[unitID] = Mathf.RoundToInt(GetAmountAfterLoss(unitID) * (1f - ratio));
                if (units[unitID] <= 0)
                {
                    units.Remove(unitID);
                    incomingLossDict.Remove(unitID);
                }
            }
        }

        // 获得复活部分士兵后的BattleGroupData，origin:打之前，after:打之后，healRate 0-1
        public static BattleGroupData GetBattleGroupDataAfterPartlyHeal(BattleGroupData originData, BattleGroupData afterData, float healRate)
        {
            Dictionary<string, int> resultDict = new Dictionary<string, int>();
            foreach (var unitId2Num in originData.units)
            {
                int originNum = unitId2Num.Value;
                int afterNum = 0;
                afterData.GetUnits().TryGetValue(unitId2Num.Key, out afterNum);
                int diffNum = originNum - afterNum;
                resultDict[unitId2Num.Key] = afterNum + (int)(diffNum * healRate);
            }

            BattleGroupData ret = new BattleGroupData();
            foreach (var unitId2Num in resultDict)
            {
                ret.AddUnit(unitId2Num.Key, unitId2Num.Value);
            }
            return ret;
        }
    }
}