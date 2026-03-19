using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.BattleField;
using System;
using Assets.Scripts.data;
using Core.Events;
using System.Linq;
using Assets.Scripts.utility.CommonInterface;
using Assets.Scripts.managers.archivemgr;
using Assets.Scripts.common;
using Assets.Scripts.utility;
using Assets.Scripts.managers.battlemgr;

namespace Assets.Scripts.logics
{
    public enum RoundType
    {
        NONE,
        PLAYER1_ROUND,
        PLAYER2_ROUND,
    }

    public enum BattleResult
    {
        NONE,
        PLAYER1_WIN,
        PLAYER2_WIN,
        DRAW
    }

    public class BattleRound
    {
        private List<RoundType> roundTypeList; // each battle will initialize it, according to which player the client is currently controlling.
        private int curRoundNum;

        public BattleRound(List<RoundType> _roundTypeList)
        {
            roundTypeList = _roundTypeList;
            curRoundNum = 1;
        }

        public RoundType GetCurRoundType()
        {
            return roundTypeList[(curRoundNum - 1) % 2];
        }

        public int GetCurRoundNum()
        {
            return curRoundNum;
        }

        public void OnNextRound()
        {
            curRoundNum++;
            Debug.LogFormat("curRoundNum: {0}, curRoundType: {1}", curRoundNum, GetCurRoundType().ToString());
        }
    }

    public class Battle : IUpdate, IDestroy
    {
        private List<BattleGroupData> battleFieldData;
        private Dictionary<Tuple<GroupSide, string>, BattleStatusData> battleStatusData;
        private Dictionary<Tuple<GroupSide, BattleActionEffectType>, float> battleDefenseData;
        private BattleRound battleRound;
        private BattleClientController clientController;
        private BattleBasicController remoteClientController;

        public void Init(int difficulty)
        {
            InitRoundAndControllers();
            InitBattleFieldData(difficulty);
            InitBattleStatusData();
            InitBattleDefenseData();
            InitEvents();
        }

        public void Destroy()
        {
            ReleaseEvents();
            DestroyConrollers();
        }

        public void Update()
        {
            clientController.Update();
            remoteClientController.Update();
        }

        public BattleAIController GetBattleAIController()
        {
            return (BattleAIController)remoteClientController;
        }

        private void InitRoundAndControllers()
        {
            // TODO: each battle has two Player1, Player2, and will also decide which player goes first
            // So, if FirstPlayer = Player1, and current client is Player1, then RoundTypeList is [PLAYER1_ROUND, PLAYER2_ROUND]

            battleRound = new BattleRound(new List<RoundType> { RoundType.PLAYER1_ROUND, RoundType.PLAYER2_ROUND });

            // TODO: for now just enable AI controller. If PVP, then it becomes remote controller.
            clientController = new BattleClientController();
            remoteClientController = new BattleAIController();

            // TODO: if it is PVP, we need their RoundType.
            clientController.Init();
            clientController.SetRoundType(RoundType.PLAYER1_ROUND);
            remoteClientController.Init();
            remoteClientController.SetRoundType(RoundType.PLAYER2_ROUND);
        }

        private void DestroyConrollers()
        {
            clientController.Destroy();
            clientController = null;
            remoteClientController.Destroy();
            remoteClientController = null;
        }

        private void InitBattleFieldData(int difficulty)
        {
            // BattleFieldData has two parts: one is enemey(for now it is just randomly initialized)
            // the other part is player's part, read from SaveData.

            battleFieldData = new List<BattleGroupData>();


            // enemey part: from [0, 35] --> row from 0 to 2, col from 0 to 11
            List<BattleGroupData> enemyDataList = new List<BattleGroupData>();
            for (int i = 0; i < 36; i++)
            {
                var battleGroupData = new BattleGroupData();
                //battleGroupData.InitByRandom();
                battleFieldData.Add(battleGroupData);
                enemyDataList.Add(battleGroupData);
            }

            // 难度*基础系数=sum of所有类型的（单位数量*单位消耗）+英雄的全部消耗。
            // 随机最少6种单位最多10种单位，可以先用难度*系数的数值除以单位类型，然后上下浮动40%。
            // 最后用单独的数值除以单一单位的消耗，得出对应单位的数量。基础系数暂定3000
            int totalCost = difficulty * 3000;
            List<string> allUnitKeys = Table_unit.data.Keys.ToList();
            int unitTypeCount = UnityEngine.Random.Range(6, 11); // 6-10种单位
            List<string> unitKeys = RandomUtils.ListChoice(allUnitKeys, unitTypeCount);
            int averageTotalCost = totalCost / unitTypeCount;
            foreach (string unitKey in unitKeys)
            {
                int randomTotalCost = (int)(averageTotalCost * (1 + UnityEngine.Random.Range(-0.4f, 0.4f)));
                int costPerUnit = Table_unit.data[unitKey].level;
                int count = randomTotalCost / costPerUnit;

                List<BattleGroupData> chosenEnemyGroupList = RandomUtils.ListChoice(enemyDataList, enemyDataList.Count / 3);
                foreach (BattleGroupData enemyGroup in chosenEnemyGroupList)
                {
                    enemyGroup.AddUnit(unitKey, count / chosenEnemyGroupList.Count);
                }

                // 筛选掉已经达到3种unit的group
                List<BattleGroupData> newEnemyDataList = new List<BattleGroupData>();
                foreach (BattleGroupData enemyGroup in enemyDataList)
                {
                    if (enemyGroup.GetUnits().Count < 3)
                    {
                        newEnemyDataList.Add(enemyGroup);
                    }
                }
                enemyDataList = newEnemyDataList;
            }


            // For test
            //int realTotalCost = 0; // 这个应该和totalCost差不多
            //foreach (BattleGroupData groupData in battleFieldData)
            //{
            //    var units = groupData.GetUnits();
            //    foreach (var kv in units)
            //    {
            //        realTotalCost += Table_unit.data[kv.Key].level * kv.Value;
            //    }
            //}


            // TODO: player's part: 
            //for (int i = 36; i < 72; i++)
            //{
            //    var battleGroupData = new BattleGroupData();
            //    battleGroupData.InitByRandom();
            //    battleFieldData.Add(battleGroupData);
            //}
            for (int i = 0; i < 36; ++i)
            {
                BattleGroupData battleGroupData = new BattleGroupData(ArchiveManager.Instance.GetCurrentArchiveData().playerData.ArmyData.IndexToBattleGroupData[i]);
                battleFieldData.Add(battleGroupData);
            }

            Debug.LogFormat("BattleFieldData.Count: {0}", battleFieldData.Count);
        }

        private void InitBattleStatusData()
        {
            battleStatusData = new Dictionary<Tuple<GroupSide, string>, BattleStatusData>();

            // load status from save file
            var archiveData = ArchiveManager.Instance.GetCurrentArchiveData();
            var unitMoralDict = archiveData.GetUnitMoralDictCopy();
            var unitDisciplineDict = archiveData.GetUnitDisciplineDictCopy();

            // compute all moral over a side
            BattleLogics.IterateGroupSide((GroupSide side) =>
            {
                // accelerate the codes, and also avoid divided by 0
                if (HasAtLeastOneGroupInGroupSide(side, false))
                {
                    int totalAmount = GetTotalAmountByGroupSide(side, false);

                    float totalMoral = 0;
                    float totalDiscipline = 0;
                    int rowStart, rowEnd, colStart, colEnd;
                    BattleLogics.GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
                    for (int row = rowStart; row < rowEnd; row++)
                    {
                        for (int col = colStart; col < colEnd; col++)
                        {
                            int dataIndex = row * 12 + col;
                            var battleGroupData = battleFieldData[dataIndex];
                            totalMoral += battleGroupData.GetTotalValues(false, ref unitMoralDict);
                            totalDiscipline += battleGroupData.GetTotalValues(false, ref unitDisciplineDict);
                        }
                    }
                    var totalMoralDelta = (totalMoral / totalAmount) - 0;
                    var totalDicsiplineDelta = (totalDiscipline / totalAmount) - 0;
                    // normally, we create a battle and init the status. At this moment, the battlepanel has not been created yet
                    // but below UpdateBattleStatus will send an event at next frame. So the battlepanel can still handle this event
                    UpdateBattleStatus(side, BattleActionEffectType.MORAL.ToString(), totalMoralDelta);
                    UpdateBattleStatus(side, BattleActionEffectType.DISCIPLINE.ToString(), totalDicsiplineDelta);

                    // send event at next frame
                    EventManager.Instance.SendEventAsync(Core.Events.EventType.Battle_OnStatusUpdate, side);
                }
            });
        }

        private void DoUpdateBattleStatus(GroupSide side, string statusID, float delta)
        {
            if (delta == 0) return;

            var statusKey = new Tuple<GroupSide, string>(side, statusID);

            if (!battleStatusData.ContainsKey(statusKey))
            {
                var statusData = new BattleStatusData(statusID);
                statusData.UpdateValue(delta);
                if (statusData.GetValue() != 0) battleStatusData[statusKey] = statusData;
            }
            else
            {
                battleStatusData[statusKey].UpdateValue(delta);
                if (battleStatusData[statusKey].GetValue() == 0) battleStatusData.Remove(statusKey);
            }
        }

        private void SpecialUpdateBattleStatus(GroupSide side, string statusID, float delta, string extraStatusID)
        {
            var statusKey = new Tuple<GroupSide, string>(side, statusID);
            float oldStatusValue = battleStatusData.ContainsKey(statusKey) ? battleStatusData[statusKey].GetValue() : 0f;
            float statusMIN = Table_status.data[statusID].minValue;
            if (delta < 0)
            {
                // when decreasing the moral/discipline
                if (oldStatusValue <= statusMIN)
                {
                    // only update flee/chaos
                    DoUpdateBattleStatus(side, extraStatusID, -delta);
                }
                else
                {
                    // decrease moral/discipline but try to add 'flee'/'chaos' status
                    float dist = Mathf.Max(0, oldStatusValue - statusMIN);
                    float statusDelta = Mathf.Min(dist, -delta);
                    float extraStatusDelta = -delta - statusDelta;
                    DoUpdateBattleStatus(side, statusID, -statusDelta);
                    DoUpdateBattleStatus(side, extraStatusID, +extraStatusDelta);
                }
            }
            else
            {
                // when increasing the moral/discipline
                if (oldStatusValue > statusMIN)
                {
                    // only update moral/discipline
                    DoUpdateBattleStatus(side, statusID, delta);
                }
                else
                {
                    // increase moral/discipline but try to remove 'flee'/'chaos' status
                    var extraStatusKey = new Tuple<GroupSide, string>(side, extraStatusID);
                    float oldExtraStatusValue = battleStatusData.ContainsKey(extraStatusKey) ? battleStatusData[extraStatusKey].GetValue() : 0f;
                    float extraStatusDelta = Mathf.Min(delta, oldExtraStatusValue);
                    DoUpdateBattleStatus(side, extraStatusID, -extraStatusDelta);
                    delta -= extraStatusDelta;
                    DoUpdateBattleStatus(side, statusID, delta);
                }
            }
        }

        public void UpdateBattleStatus(GroupSide side, string statusID, float delta)
        {
            if (delta == 0) return;

            // special case
            if(statusID == BattleActionEffectType.MORAL.ToString())
            {
                SpecialUpdateBattleStatus(side, statusID, delta, BattleActionEffectType.FLEE.ToString());
            }
            else if(statusID == BattleActionEffectType.DISCIPLINE.ToString())
            {
                SpecialUpdateBattleStatus(side, statusID, delta, BattleActionEffectType.CHAOS.ToString());
            }
            else
            {
                DoUpdateBattleStatus(side, statusID, delta);
            }
        }

        public HashSet<BattleStatusData> GetBattleStatusDataSetByGroupSide(GroupSide side)
        {
            HashSet<BattleStatusData> result = new HashSet<BattleStatusData>();
            foreach (var kv in battleStatusData) if (kv.Key.Item1 == side) result.Add(kv.Value);
            return result;
        }

        public BattleStatusData GetBattleStatusData(GroupSide side, string statusID)
        {
            var statusKey = new Tuple<GroupSide, string>(side, statusID);
            return battleStatusData.ContainsKey(statusKey) ? battleStatusData[statusKey] : null;
        }

        private void InitBattleDefenseData()
        {
            battleDefenseData = new Dictionary<Tuple<GroupSide, BattleActionEffectType>, float>();
            BattleLogics.IterateGroupSide((GroupSide side) =>
            {
                battleDefenseData.Add(new(side, BattleActionEffectType.TERRAIN_DEFENSE), 0);
                battleDefenseData.Add(new(side, BattleActionEffectType.FORTIFICATION_DEFENSE), 0);
                battleDefenseData.Add(new(side, BattleActionEffectType.ELASTIC_DEFENSE), 0);
                battleDefenseData.Add(new(side, BattleActionEffectType.FORMATION_DEFENSE), 0);
                battleDefenseData.Add(new(side, BattleActionEffectType.ARMOUR_DEFENSE), 0);
            });
        }

        public void UpdateBattleDefense(Tuple<GroupSide, BattleActionEffectType> key, float delta)
        {
            battleDefenseData[key] += delta;
        }

        public float GetBattleDefenseByDefenseType(GroupSide side, BattleActionEffectType defenseType)
        {
            return battleDefenseData[new(side, defenseType)];
        }

        public float GetGroupsideBattleDefense(GroupSide side)
        {
            return BattleLogics.GetGroupsideBattleDefense(ref battleDefenseData, side);
        }

        // when switching to next round, we need to clean the defense for those start a new round.
        private void TryClearAllDefense()
        {
            var curRoundType = battleRound.GetCurRoundType();
            var clientRoundType = clientController.GetRoundType();
            var remoteRoundType = remoteClientController.GetRoundType();

            // clear all defense, unless it has a status named "INHERIT_ALL_DEFENSE"
            var keyList = battleDefenseData.Keys.ToList();
            var sides = new HashSet<GroupSide>();
            foreach (var key in keyList)
            {
                if (HasStatus(key.Item1, BattleActionEffectType.INHERIT_ALL_DEFENSE))
                {
                    // no worry about below logics. It will fix the value within the range [min, max] indicated by table data.
                    DoUpdateBattleStatus(key.Item1, BattleActionEffectType.INHERIT_ALL_DEFENSE.ToString(), -1f);
                    continue; // stop go below logics(avoid to clear defense)
                }

                if (key.Item1.IsBottom() && (clientRoundType == curRoundType)) battleDefenseData[key] = 0;

                if (key.Item1.IsTop() && (remoteRoundType == curRoundType)) battleDefenseData[key] = 0;

                sides.Add(key.Item1);
            }
            foreach (var side in sides) EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnDefenseUpdate, side);
        }

        private void TryDampingMoralDiscipline()
        {
            var roundNum = GetCurRoundNum();
            if(roundNum > 2 && (roundNum % 2 == 1))
            {
                // We need to damp client/remoteClient moral/discipline at each two rounds.
                string moralStatusID = BattleActionEffectType.MORAL.ToString();
                string disciplineStatusID = BattleActionEffectType.DISCIPLINE.ToString();
                var keyList = battleStatusData.Keys.ToList();
                var sides = new HashSet<GroupSide>();
                foreach (var key in keyList)
                {
                    if(key.Item2 == moralStatusID || key.Item2 == disciplineStatusID)
                    {
                        float value = battleStatusData[key].GetValue();
#if UNITY_EDITOR
                        Debug.Assert(value != 0);
#endif
                        float sign = Mathf.Sign(value);
                        float valueABS = Mathf.Abs(value);
                        float dampValueABS = Mathf.Min(3 + valueABS * 0.1f, BattleLogics.moralDeltaMAX);
                        // make it closer to zero.
                        float delta = -sign * Mathf.Min(dampValueABS, valueABS);
#if UNITY_EDITOR
                        Debug.Assert(sign * (value + delta) >= 0);
#endif
                        battleStatusData[key].UpdateValue(delta);
                        sides.Add(key.Item1);

                        // remove it if the value is 0 after being updated
                        if (battleStatusData[key].GetValue() == 0) battleStatusData.Remove(key);
                    }
                }
                foreach (var side in sides) EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnStatusUpdate, side);
            }
        }

        private void HandleOnNextRound()
        {
            if (GetBattleResult() != BattleResult.NONE)
            {
                Debug.Log("Battle is finished, can not go on next round.");
                return;
            }

            Debug.Log("HandleOnNextRound");

            bool isClientRoundOld = IsClientRound(); // for each client, it is always at the bottom.

            if (isClientRoundOld) clientController.OnRoundEnd();
            else remoteClientController.OnRoundEnd();

            battleRound.OnNextRound(); // need to call it to update round number first

            if (isClientRoundOld) remoteClientController.OnRoundStart();
            else clientController.OnRoundStart();

            TryClearAllDefense();
            TryDampingMoralDiscipline();
            TryHandleUnitLoss();
            TryGetExtraActionPointDrawCard(!isClientRoundOld);
        }

        private void InitEvents()
        {
            EventManager.Instance.RegisterEventHandler<Action>(Core.Events.EventType.Battle_OnNextRound, HandleOnNextRound);
            EventManager.Instance.RegisterEventHandler<Action>(Core.Events.EventType.UI_OnAfterUseCard, HandleOnAfterUseCard);
        }

        private void ReleaseEvents()
        {
            EventManager.Instance.UnRegisterEventHandler<Action>(Core.Events.EventType.Battle_OnNextRound, HandleOnNextRound);
            EventManager.Instance.UnRegisterEventHandler<Action>(Core.Events.EventType.UI_OnAfterUseCard, HandleOnAfterUseCard);
        }

        public bool HasStatus(GroupSide side, BattleActionEffectType statusType)
        {
            foreach (var kv in battleStatusData)
            {
                if (kv.Key.Item1 == side && kv.Value.GetStatusType() == statusType) return true;
            }
            return false;
        }

        public Dictionary<Tuple<GroupSide, BattleActionEffectType>, float> GetBattleDefenseDataCopy()
        {
            return new Dictionary<Tuple<GroupSide, BattleActionEffectType>, float>(battleDefenseData);
        }

        public BattleGroupData GetBattleGroupData(int row, int col)
        {
            return battleFieldData[row * 12 + col];
        }

        public List<BattleGroupData> GetClientBattleGroupDataList()
        {
            return battleFieldData.GetRange(36, 36);
        }

        public int GetTotalAmountByGroupSide(GroupSide side, bool needApplyLoss)
        {
            int totalAmount = 0;
            int rowStart, rowEnd, colStart, colEnd;
            BattleLogics.GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = rowStart; row < rowEnd; row++)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    var battleGroupData = battleFieldData[row * 12 + col];
                    totalAmount += (needApplyLoss ? battleGroupData.GetTotalAmountAfterLoss() : battleGroupData.GetTotalAmountBeforeLoss());
                }
            }
            return totalAmount;
        }

        public bool HasAtLeastOneGroupInGroupSide(GroupSide side, bool needApplyLoss)
        {
            int rowStart, rowEnd, colStart, colEnd;
            BattleLogics.GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = rowStart; row < rowEnd; row++)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    var battleGroupData = battleFieldData[row * 12 + col];
                    if (!battleGroupData.IsEmpty(needApplyLoss)) return true;
                }
            }
            return false;
        }

        public int GetTotalIncomingLossByGroupSide(GroupSide side)
        {
            int totalLoss = 0;
            int rowStart, rowEnd, colStart, colEnd;
            BattleLogics.GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = rowStart; row < rowEnd; row++)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    var battleGroupData = battleFieldData[row * 12 + col];
                    totalLoss += battleGroupData.GetTotalIncomingLoss();
                }
            }
            return totalLoss;
        }

        public int GetTotalAmountAfterLoss(GroupSide side, int row)
        {
            int totalAmount = 0;
            int colStart, colEnd;
            BattleLogics.GetColStartEnd(side, out colStart, out colEnd);
            for (int col = colStart; col < colEnd; col++)
                totalAmount += battleFieldData[row * 12 + col].GetTotalAmountAfterLoss();
            return totalAmount;
        }

        public int GetTotalAmountAfterLoss(GroupSide side, int row, string unitID)
        {
            int totalAmount = 0;
            int colStart, colEnd;
            BattleLogics.GetColStartEnd(side, out colStart, out colEnd);
            for (int col = colStart; col < colEnd; col++)
                totalAmount += battleFieldData[row * 12 + col].GetAmountAfterLoss(unitID);
            return totalAmount;
        }

        // whether it is client's round for playing cards.
        public bool IsClientRound()
        {
            return battleRound.GetCurRoundType() == clientController.GetRoundType();
        }

        public int GetCurRoundNum()
        {
            return battleRound.GetCurRoundNum();
        }

        public void ResetBattleFieldDataIncomingLossDict()
        {
            foreach (var battleGroupData in battleFieldData) battleGroupData.ResetIncomingLossDict();
        }

        public void OnApplyUnitLoss(GroupSide side)
        {
            Debug.LogFormat("OnApplyUnitLoss, side:{0}", side.ToString());
            int rowStart, rowEnd, colStart, colEnd;
            BattleLogics.GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = rowStart; row < rowEnd; row++)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    int dataIndex = row * 12 + col;
                    battleFieldData[dataIndex].ApplyLoss();
                }
            }
        }

        // iterate all status, find the status with 'statusEffectType', get its ActualValue, sum it up.
        public float GetStatusActualValueSum(GroupSide side, StatusEffectType statusEffectType)
        {
            float result = 0f;
            foreach(var kv in battleStatusData) if (kv.Key.Item1 == side) result += kv.Value.GetStatusActualValue(statusEffectType);
            return result;
        }

        private BattleResult GetBattleResult()
        {
            const bool needApplyLoss = true;

            // top sides first, then bottom sides
            bool isTopAllDead = true;
            for (int row = 0; row < 3; row++) 
            {
                for (int col = 0; col < 12; col++) 
                {
                    var battleGroupData = battleFieldData[row * 12 + col];
                    if (!battleGroupData.IsEmpty(needApplyLoss))
                    {
                        isTopAllDead = false;
                        break;
                    }
                }
            }

            if(isTopAllDead)
                return clientController.GetRoundType() == RoundType.PLAYER1_ROUND ? BattleResult.PLAYER1_WIN : BattleResult.PLAYER2_WIN;

            bool isBottomAllDead = true;
            for (int row = 3; row < 6; row++) 
            {
                for (int col = 0; col < 12; col++) 
                {
                    var battleGroupData = battleFieldData[row * 12 + col];
                    if (!battleGroupData.IsEmpty(needApplyLoss))
                    {
                        isBottomAllDead = false;
                        break;
                    }
                }
            }

            // remoteClientController wins this battle.
            if(isBottomAllDead)
            {
                if (isTopAllDead) return BattleResult.DRAW;
                else return remoteClientController.GetRoundType() == RoundType.PLAYER1_ROUND ? BattleResult.PLAYER1_WIN : BattleResult.PLAYER2_WIN;
            }

            return BattleResult.NONE;
        }

        private void HandleOnAfterUseCard()
        {
            var battleResult = GetBattleResult();
            if (battleResult != BattleResult.NONE) EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnFinish, battleResult);

            // update status (ignore_defense). Because we need to minus 1 for its value after each card has been played.
            Action<Action<GroupSide>> callback = IsClientRound() ? BattleLogics.IterateBottomGroupSide : BattleLogics.IterateTopGroupSide;
            callback((GroupSide side) =>
            {
                if (HasStatus(side, BattleActionEffectType.IGNORE_DEFENSE))
                {
                    UpdateBattleStatus(side, BattleActionEffectType.IGNORE_DEFENSE.ToString(), -1f);

                    // send event at next frame
                    EventManager.Instance.SendEventAsync(Core.Events.EventType.Battle_OnStatusUpdate, side);
                }
            });
        }

        private void TryHandleUnitLoss()
        {
            BattleLogics.IterateGroupSide((GroupSide side) =>
            {
                float ratio = BattleLogics.GetStatusActualValueSum(side, StatusEffectType.UNITLOSS_EACHROUND_EFFECT);
                if (ratio <= 0) return;
                OnApplyUnitLossRatio(side, ratio);
                EventManager.Instance.SendEventSync(Core.Events.EventType.UI_OnUpdateHPBar, side);
            });
        }

        private void OnApplyUnitLossRatio(GroupSide side, float ratio)
        {
            int rowStart, rowEnd, colStart, colEnd;
            BattleLogics.GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = rowStart; row < rowEnd; row++)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    battleFieldData[row * 12 + col].ApplyLossRatio(ratio);
                }
            }
        }

        public bool IsBattleFinish()
        {
            return GetBattleResult() != BattleResult.NONE;
        }

        public RoundType GetClientRoundType()
        {
            return clientController.GetRoundType();
        }

        // [Note] below function is only for Player's Status...(which means not replying any group side)
        // also the status should have only "StatusEffectType.NONE" in it.
        private float ConsumeStatusAndGetValue(bool isClientController, BattleActionEffectType targetActionEffectType)
        {
            // TODO: because this status is not depending on GroupSide, 
            // but we do not have the concept of Player's Status for the time being,
            // so make it easy to just chek TOP_MIDDLE and BOTTOM_MIDDLE
            var activePlayerSide = isClientController ? GroupSide.BOTTOM_MIDDLE : GroupSide.TOP_MIDDLE;
            if (HasStatus(activePlayerSide, targetActionEffectType))
            {
                var statusKey = new Tuple<GroupSide, string>(activePlayerSide, targetActionEffectType.ToString());
                float value = battleStatusData[statusKey].GetValue();
                float actualValue = battleStatusData[statusKey].GetStatusActualValue(StatusEffectType.NONE); 

                // update all TOP/BOTTOM sides to remove this status.
                Action<Action<GroupSide>> callback = isClientController ? BattleLogics.IterateBottomGroupSide : BattleLogics.IterateTopGroupSide;
                callback((GroupSide side) =>
                {
                    DoUpdateBattleStatus(side, statusKey.Item2, -value); // it is equivalent to remove the status
                    // update the status UI
                    EventManager.Instance.SendEventAsync(Core.Events.EventType.Battle_OnStatusUpdate, side);
                });

                return actualValue;
            }
            return 0;
        }

        private int ConsumeActionPointStatus(bool isClientController)
        {
            return (int)ConsumeStatusAndGetValue(isClientController, BattleActionEffectType.ACTION_POINT_NEXT_TURN);
        }

        private int ConsumeDrawCardStatus(bool isClientController, BattleActionEffectType actionEffectType)
        {
            return (int)ConsumeStatusAndGetValue(isClientController, actionEffectType);
        }

        private void TryGetExtraActionPointDrawCard(bool isClientRound)
        {
            // extra action point at each round start
            int extraActionPoint = 0;

            extraActionPoint += ConsumeActionPointStatus(isClientRound);

            if(ConsumeStatusAndGetValue(isClientRound, BattleActionEffectType.INHERIT_ALL_ACTION_POINT) != 0)
            {
                EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnInheritAllActionPoint, isClientRound);
            }

            if (extraActionPoint != 0)
            {
                EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnUpdateActionPoint, isClientRound, extraActionPoint);
            }

            // extra draw card at each round start
            for (int i = 1; i < (int)(BattleActionEffectType.MAX); i++)
            {
                BattleActionEffectType actionEffectType = (BattleActionEffectType)i;
                if (Utils.HasAttribute<DrawCardFlag, BattleActionEffectType>(actionEffectType))
                {
                    int drawCardNumber = ConsumeDrawCardStatus(isClientRound, actionEffectType);
                    if (drawCardNumber != 0)
                        EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnDrawCard, isClientRound, drawCardNumber, actionEffectType);
                }
            }
        }

        // 返回战斗结束后复活的比例，返回0-1
        public float GetHealAfterBattleRatio()
        {
            // since it always return client's HealAfterBattleRatio, we can just use BOTTOM_MIDDLE
            return BattleLogics.GetStatusActualValueSum(GroupSide.BOTTOM_MIDDLE, StatusEffectType.HEAL_AFTER_BATTLE_EFFECT);
        }

        public void CountDownActionAfterTurnStatus()
        {
            // minus 1 for 'ACTION_AFTER_TURN' status
            Action<Action<GroupSide>> callback = IsClientRound() ? BattleLogics.IterateBottomGroupSide : BattleLogics.IterateTopGroupSide;
            callback((GroupSide side) =>
            {
                if (HasStatus(side, BattleActionEffectType.ACTION_AFTER_TURN))
                {

                    UpdateBattleStatus(side, BattleActionEffectType.ACTION_AFTER_TURN.ToString(), -1f);

                    // send event at next frame
                    EventManager.Instance.SendEventAsync(Core.Events.EventType.Battle_OnStatusUpdate, side);
                }
            });
        }

        public void ReplaceActionAfterTurnStatus(bool isClientRound, float value)
        {
            Action<Action<GroupSide>> callback = isClientRound ? BattleLogics.IterateBottomGroupSide : BattleLogics.IterateTopGroupSide;
            callback((GroupSide side) =>
            {
                float delta;
                if (HasStatus(side, BattleActionEffectType.ACTION_AFTER_TURN))
                {
                    var battleStatusData = GetBattleStatusData(side, BattleActionEffectType.ACTION_AFTER_TURN.ToString());
                    float oldValue = battleStatusData.GetValue();
                    float newValue = value;
                    delta = newValue - oldValue;
                }
                else
                {
                    delta = value;
                }

                UpdateBattleStatus(side, BattleActionEffectType.ACTION_AFTER_TURN.ToString(), delta);

                // send event at next frame
                EventManager.Instance.SendEventAsync(Core.Events.EventType.Battle_OnStatusUpdate, side);
            });
        }

        public bool IsClientAutoUseDelayedCard()
        {
            return clientController.GetIsAutoUseDelayedCard();
        }
    }

}