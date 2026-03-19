using Assets.Scripts.logics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Assets.Scripts.BattleField;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.utility;
using Assets.Scripts.panel.BattlePanel;

namespace Assets.Scripts.logics
{
    public enum GroupSide
    {
        TOP_LEFT,
        TOP_MIDDLE,
        TOP_RIGHT,

        BOTTOM_LEFT,
        BOTTOM_MIDDLE,
        BOTTOM_RIGHT
    }

    public enum AttackedDir
    {
        NONE = 0,
        TOP = 1 << 0,  // 1
        BOTTOM = 1 << 1, // 2
        LEFT = 1 << 2, // 4
        RIGHT = 1 << 3 // 8
    }

    // from top to bottom, which row is active.
    public enum ActiveRow
    {
        NONE = 0,

        TOP_ONE = 1 << 0,  // 1
        TOP_TWO = 1 << 1,  // 2
        TOP_THREE = 1 << 2, // 4

        BOTTOM_ONE = 1 << 3,  // 8
        BOTTOM_TWO = 1 << 4,  // 16
        BOTTOM_THREE = 1 << 5 // 32
    }

    public static class BattleLogics
    {
        #region get/set/check logics

        // return true if two side is at the same big column in vertical direction. 
        // E.g. TOP_LEFT AND BOTTOM_LEFT.
        public static bool IsSameVerticalDir(GroupSide side1, GroupSide side2)
        {
            return (side1.IsLeft() && side2.IsLeft()) ||
                (side1.IsMiddle() && side2.IsMiddle()) ||
                (side1.IsRight() && side2.IsRight());
        }

        public static void IterateGroupSide(Action<GroupSide> action)
        {
            action?.Invoke(GroupSide.TOP_LEFT);
            action?.Invoke(GroupSide.TOP_MIDDLE);
            action?.Invoke(GroupSide.TOP_RIGHT);
            action?.Invoke(GroupSide.BOTTOM_LEFT);
            action?.Invoke(GroupSide.BOTTOM_MIDDLE);
            action?.Invoke(GroupSide.BOTTOM_RIGHT);
        }

        public static void IterateTopGroupSide(Action<GroupSide> action)
        {
            action?.Invoke(GroupSide.TOP_LEFT);
            action?.Invoke(GroupSide.TOP_MIDDLE);
            action?.Invoke(GroupSide.TOP_RIGHT);
        }

        public static void IterateBottomGroupSide(Action<GroupSide> action)
        {
            action?.Invoke(GroupSide.BOTTOM_LEFT);
            action?.Invoke(GroupSide.BOTTOM_MIDDLE);
            action?.Invoke(GroupSide.BOTTOM_RIGHT);
        }

        public static bool IsLeft(this GroupSide side) { return side == GroupSide.TOP_LEFT || side == GroupSide.BOTTOM_LEFT; }

        public static bool IsRight(this GroupSide side) { return side == GroupSide.TOP_RIGHT || side == GroupSide.BOTTOM_RIGHT; }

        public static bool IsMiddle(this GroupSide side) { return side == GroupSide.TOP_MIDDLE || side == GroupSide.BOTTOM_MIDDLE; }

        public static bool IsTop(this GroupSide side) { return side == GroupSide.TOP_LEFT || side == GroupSide.TOP_MIDDLE || side == GroupSide.TOP_RIGHT; }

        public static bool IsBottom(this GroupSide side) { return side == GroupSide.BOTTOM_LEFT || side == GroupSide.BOTTOM_MIDDLE || side == GroupSide.BOTTOM_RIGHT; }

        public static bool IsDiagonal(GroupSide side1, GroupSide side2)
        {
            return (side1 == GroupSide.BOTTOM_LEFT && side2 == GroupSide.TOP_RIGHT) ||
                (side2 == GroupSide.BOTTOM_LEFT && side1 == GroupSide.TOP_RIGHT) ||
                (side1 == GroupSide.BOTTOM_RIGHT && side2 == GroupSide.TOP_LEFT) ||
                (side2 == GroupSide.BOTTOM_RIGHT && side1 == GroupSide.TOP_LEFT);
        }

        public static void GetRowStartEnd(GroupSide side, out int rowStart, out int rowEnd)
        {
            if (side.IsTop()) { rowStart = 0; rowEnd = 3; }
            else { rowStart = 3; rowEnd = 6; }
        }

        public static void GetColStartEnd(GroupSide side, out int colStart, out int colEnd)
        {
            if (side.IsLeft()) { colStart = 0; colEnd = 3; }
            else if (side.IsMiddle()) { colStart = 3; colEnd = 9; }
            else { colStart = 9; colEnd = 12; }
        }

        public static void GetRowColStartEnd(
            GroupSide side,
            out int rowStart, out int rowEnd,
            out int colStart, out int colEnd)
        {
            GetRowStartEnd(side, out rowStart, out rowEnd);
            GetColStartEnd(side, out colStart, out colEnd);
        }

        public static void GetGroupSideAndRowWithIndex(int index, out GroupSide groupSide, out int row)
        {
            row = index / 12;
            groupSide = GroupSide.TOP_LEFT;

            List<GroupSide> allGroupSides = new List<GroupSide>() {
                GroupSide.TOP_LEFT, GroupSide.TOP_MIDDLE, GroupSide.TOP_RIGHT, GroupSide.BOTTOM_LEFT, GroupSide.BOTTOM_MIDDLE, GroupSide.BOTTOM_RIGHT
            };
            foreach (GroupSide side in allGroupSides)
            {
                int rowStart, rowEnd, colStart, colEnd;
                GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
                for (int tempRow = rowStart; tempRow < rowEnd; ++tempRow)
                {
                    for (int tempCol = colStart; tempCol < colEnd; ++tempCol)
                    {
                        int tempIndex = tempRow * 12 + tempCol;
                        if (tempIndex == index)
                        {
                            groupSide = side;
                        }
                    }
                }
            }
        }

        public static int GetFirstNonEmptyRow(GroupSide side, bool isAscending, bool needApplyLoss)
        {
            // 'isAscending' true mean [from row=0 to row=2] or [from row=3 to row=5]
            // false mean backward
            int rowStep = isAscending ? +1 : -1;
            int rowStart, rowEnd, colStart, colEnd;
            GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = isAscending ? rowStart : rowEnd - 1; rowStart <= row && row < rowEnd; row += rowStep)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    var battleGroupData = GetBattleGroupDataFromCurrentBattle(row, col);
                    if (!battleGroupData.IsEmpty(needApplyLoss)) return row;
                }
            }
            return -1;
        }

        public static int GetFirstNonEmptyCol(GroupSide side, bool isAscending, bool needApplyLoss)
        {
            // 'isAscending' true mean [from col=colStart to col=colEnd], false means backwards.
            int rowStart, rowEnd, colStart, colEnd;
            GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);

            int colStep = isAscending ? +1 : -1;
            for (int col = isAscending ? colStart : colEnd - 1; colStart <= col && col < colEnd; col += colStep)
            {
                for(int row = rowStart; row<rowEnd; row++)
                {
                    var battleGroupData = GetBattleGroupDataFromCurrentBattle(row, col);
                    if (!battleGroupData.IsEmpty(needApplyLoss)) return col;
                }
            }
            return -1;
        }

        public static GroupSide GetGroupSide(int row, int col)
        {
            // TODO: don't use reflection for better performance.
            string str1 = row < 3 ? "TOP" : "BOTTOM";
            string str2;
            if (0 <= col && col < 3) str2 = "LEFT";
            else if (3 <= col && col < 9) str2 = "MIDDLE";
            else str2 = "RIGHT";
            return Utils.ConvertStrToEnum<GroupSide>(str1 + "_" + str2);
        }

        public static BattleGroupData GetBattleGroupDataFromCurrentBattle(int row, int col)
        {
            return BattleManager.Instance.GetCurrentBattle().GetBattleGroupData(row, col);
        }

        // check whether this GroupSide contains at least one group with the specific GroupType.
        public static bool HasBattleGroupTypeInGroupSide(GroupSide side, BattleGroupType groupType, bool needApplyLoss)
        {
            int rowStart, rowEnd, colStart, colEnd;
            GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = rowStart; row < rowEnd; row++)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    var battleGroupData = GetBattleGroupDataFromCurrentBattle(row, col);
                    if (battleGroupData.HasBattleGroupType(groupType, needApplyLoss)) return true;
                }
            }
            return false;
        }

        // check whether the GroupSide contains at least one non-empty BattleGroupData
        public static bool HasAtLeastOneGroupInGroupSide(GroupSide side, bool needApplyLoss)
        {
            return BattleManager.Instance.GetCurrentBattle().HasAtLeastOneGroupInGroupSide(side, needApplyLoss);
        }

        // below function always assumes that
        // (1) enemy is top, friend is bottom if isClientRound is 'true'.
        // (2) enemy is bottom, friend is top if isClientRound is 'false'.
        public static HashSet<GroupSide> GetGroupSides(BattleTargetType targetType, bool isClientRound, bool needApplyLoss)
        {
            var result = new HashSet<GroupSide>();

            switch (targetType)
            {
                case BattleTargetType.CENTER_ENEMY:
                case BattleTargetType.CENTER_FRIEND:
                    {
                        if ((targetType == BattleTargetType.CENTER_ENEMY && isClientRound) || 
                            (targetType == BattleTargetType.CENTER_FRIEND && !isClientRound))
                        {
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.TOP_MIDDLE, needApplyLoss)) result.Add(GroupSide.TOP_MIDDLE);
                        }
                        else
                        {
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_MIDDLE, needApplyLoss)) result.Add(GroupSide.BOTTOM_MIDDLE);
                        }
                    }
                    break;

                case BattleTargetType.FLANKS_ENEMY:
                case BattleTargetType.FLANKS_FRIEND:
                    {
                        if ((targetType == BattleTargetType.FLANKS_ENEMY && isClientRound) ||
                            (targetType == BattleTargetType.FLANKS_FRIEND && !isClientRound))
                        {
                            if (IsLeftMostGroupSide(GroupSide.TOP_LEFT, needApplyLoss)) result.Add(GroupSide.TOP_LEFT);
                            else if (IsLeftMostGroupSide(GroupSide.TOP_MIDDLE, needApplyLoss)) result.Add(GroupSide.TOP_MIDDLE);

                            if (IsRightMostGroupSide(GroupSide.TOP_RIGHT, needApplyLoss)) result.Add(GroupSide.TOP_RIGHT);
                            else if (IsRightMostGroupSide(GroupSide.TOP_MIDDLE, needApplyLoss)) result.Add(GroupSide.TOP_MIDDLE);
                        }
                        else
                        {
                            if (IsLeftMostGroupSide(GroupSide.BOTTOM_LEFT, needApplyLoss)) result.Add(GroupSide.BOTTOM_LEFT);
                            else if (IsLeftMostGroupSide(GroupSide.BOTTOM_MIDDLE, needApplyLoss)) result.Add(GroupSide.BOTTOM_MIDDLE);

                            if (IsRightMostGroupSide(GroupSide.BOTTOM_RIGHT, needApplyLoss)) result.Add(GroupSide.BOTTOM_RIGHT);
                            else if (IsRightMostGroupSide(GroupSide.BOTTOM_MIDDLE, needApplyLoss)) result.Add(GroupSide.BOTTOM_MIDDLE);
                        }
                    }
                    break;

                case BattleTargetType.FRONT_ENEMY:
                case BattleTargetType.BACK_ENEMY:
                case BattleTargetType.ALL_ENEMY:
                    {
                        if(isClientRound)
                        {
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.TOP_LEFT, needApplyLoss)) result.Add(GroupSide.TOP_LEFT);
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.TOP_MIDDLE, needApplyLoss)) result.Add(GroupSide.TOP_MIDDLE);
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.TOP_RIGHT, needApplyLoss)) result.Add(GroupSide.TOP_RIGHT);
                        }
                        else
                        {
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_LEFT, needApplyLoss)) result.Add(GroupSide.BOTTOM_LEFT);
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_MIDDLE, needApplyLoss)) result.Add(GroupSide.BOTTOM_MIDDLE);
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_RIGHT, needApplyLoss)) result.Add(GroupSide.BOTTOM_RIGHT);
                        }
                    }
                    break;

                case BattleTargetType.FRONT_FRIEND:
                case BattleTargetType.BACK_FRIEND:
                case BattleTargetType.ALL_FRIEND:
                    {
                        if(isClientRound)
                        {
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_LEFT, needApplyLoss)) result.Add(GroupSide.BOTTOM_LEFT);
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_MIDDLE, needApplyLoss)) result.Add(GroupSide.BOTTOM_MIDDLE);
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_RIGHT, needApplyLoss)) result.Add(GroupSide.BOTTOM_RIGHT);
                        }
                        else
                        {
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.TOP_LEFT, needApplyLoss)) result.Add(GroupSide.TOP_LEFT);
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.TOP_MIDDLE, needApplyLoss)) result.Add(GroupSide.TOP_MIDDLE);
                            if (HasAtLeastOneGroupInGroupSide(GroupSide.TOP_RIGHT, needApplyLoss)) result.Add(GroupSide.TOP_RIGHT);
                        }
                    }
                    break;

                // [NOTE] below means all friend unit types...
                case BattleTargetType.ALL_CAVALRY:
                case BattleTargetType.ALL_FIREARM:
                case BattleTargetType.ALL_SPECIAL:
                case BattleTargetType.ALL_MELEE:
                case BattleTargetType.ALL_SPEAR:
                case BattleTargetType.ALL_SIEGE:
                case BattleTargetType.ALL_PROJECTILE:
                case BattleTargetType.ALL_HEAVY:
                    {
                        string groupTypeStr = targetType.ToString().Split("_")[1];
                        var groupType = Utils.ConvertStrToEnum<BattleGroupType>(groupTypeStr);
                        if(isClientRound)
                        {
                            if (HasBattleGroupTypeInGroupSide(GroupSide.BOTTOM_LEFT, groupType, needApplyLoss)) result.Add(GroupSide.BOTTOM_LEFT);
                            if (HasBattleGroupTypeInGroupSide(GroupSide.BOTTOM_MIDDLE, groupType, needApplyLoss)) result.Add(GroupSide.BOTTOM_MIDDLE);
                            if (HasBattleGroupTypeInGroupSide(GroupSide.BOTTOM_RIGHT, groupType, needApplyLoss)) result.Add(GroupSide.BOTTOM_RIGHT);
                        }
                        else
                        {
                            if (HasBattleGroupTypeInGroupSide(GroupSide.TOP_LEFT, groupType, needApplyLoss)) result.Add(GroupSide.TOP_LEFT);
                            if (HasBattleGroupTypeInGroupSide(GroupSide.TOP_MIDDLE, groupType, needApplyLoss)) result.Add(GroupSide.TOP_MIDDLE);
                            if (HasBattleGroupTypeInGroupSide(GroupSide.TOP_RIGHT, groupType, needApplyLoss)) result.Add(GroupSide.TOP_RIGHT);
                        }
                    }
                    break;

                case BattleTargetType.NONE:
                default: break;
            }
            return result;
        }
        
        public static int GetActiveRows(GroupSide side, bool needApplyLoss)
        {
            int activeRow = (int)ActiveRow.NONE;
            int rowStart, rowEnd, colStart, colEnd;
            GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = rowStart; row < rowEnd; row++)
            {
                bool hasOneActive = false;
                for (int col = colStart; col < colEnd; col++)
                {
                    var battleGroupData = GetBattleGroupDataFromCurrentBattle(row, col);
                    if (!battleGroupData.IsEmpty(needApplyLoss))
                    {
                        hasOneActive = true;
                        break;
                    }
                }
                if (hasOneActive) activeRow |= (1 << row);
            }
            return activeRow;
        }

        public static ActiveRow GetFirstActiveRow(GroupSide side, bool isAscending, bool needApplyLoss)
        {
            //int rowStart, _;
            //GetRowStartEnd(side, out rowStart, out _);
            int firstRow = GetFirstNonEmptyRow(side, isAscending, needApplyLoss);
            return (ActiveRow)(1 << firstRow);
        }

        public static HashSet<Tuple<int, int, BattleGroupType>> GetUnitsByGroupType(GroupSide side, BattleGroupType targetGroupType, bool needApplyLoss)
        {
            var result = new HashSet<Tuple<int, int, BattleGroupType>>();
            int rowStart, rowEnd, colStart, colEnd;
            GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = rowStart; row < rowEnd; row++)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    var battleGroupData = GetBattleGroupDataFromCurrentBattle(row, col);
                    var unitIDs = battleGroupData.GetUnitIDs(targetGroupType, needApplyLoss);
                    if (unitIDs.Count > 0) result.Add(new(row, col, targetGroupType));
                }
            }
            return result;
        }

        public static HashSet<Tuple<int, int, BattleGroupType>> GetAllUnits(GroupSide side, bool needApplyLoss)
        {
            int rowStart, rowEnd, colStart, colEnd;
            GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            return GetUnitsByRowColStartEnd(rowStart, rowEnd, colStart, colEnd, needApplyLoss);
        }

        public static HashSet<Tuple<int, int, BattleGroupType>> GetUnitsByRowColStartEnd(int rowStart, int rowEnd, int colStart, int colEnd, bool needApplyLoss)
        {
            var result = new HashSet<Tuple<int, int, BattleGroupType>>();
            for (int row = rowStart; row < rowEnd; row++)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    var battleGroupData = GetBattleGroupDataFromCurrentBattle(row, col);
                    var unitIDs= battleGroupData.GetUnitIDs(needApplyLoss);
                    foreach (var unitID in unitIDs)
                    {
                        var unitTabData = Table_unit.data[unitID];
                        var groupType = Utils.ConvertStrToEnum<BattleGroupType>(unitTabData.group);
                        result.Add(new(row, col, groupType));
                    }
                }
            }
            return result;
        }

        // get all friend unitIDs in GroupSide according to the target type.
        // only used for attack action type card.
        public static HashSet<Tuple<int, int, BattleGroupType>> GetAttackerUnitsByTargetType_AtkAction(
            GroupSide side, BattleTargetType targetType, bool isClientRound, bool needApplyLoss)
        {
            var result = new HashSet<Tuple<int, int, BattleGroupType>>();
            switch (targetType)
            {
                case BattleTargetType.ALL_FRIEND:
                case BattleTargetType.CENTER_FRIEND:
                case BattleTargetType.FLANKS_FRIEND:
                    {
                        // once we specifying the groupside, those BattleTargetType is actually covering all groups.
                        result.UnionWith(GetAllUnits(side, needApplyLoss));
                    }
                    break;

                case BattleTargetType.FRONT_FRIEND:
                case BattleTargetType.BACK_FRIEND:
                    {
                        bool isAscending = 
                            (isClientRound && targetType == BattleTargetType.FRONT_FRIEND) ||
                            (!isClientRound && targetType == BattleTargetType.BACK_FRIEND);
                        int colStart, colEnd;
                        GetColStartEnd(side, out colStart, out colEnd);
                        // we only need the first row(the front or back)
                        int row = GetFirstNonEmptyRow(side, isAscending, needApplyLoss);
                        result.UnionWith(GetUnitsByRowColStartEnd(row, row + 1, colStart, colEnd, needApplyLoss));
                    }
                    break;

                // [NOTE] below means all unit types...
                case BattleTargetType.ALL_CAVALRY:
                case BattleTargetType.ALL_FIREARM:
                case BattleTargetType.ALL_SPECIAL:
                case BattleTargetType.ALL_MELEE:
                case BattleTargetType.ALL_SPEAR:
                case BattleTargetType.ALL_SIEGE:
                case BattleTargetType.ALL_PROJECTILE:
                case BattleTargetType.ALL_HEAVY:
                    {
                        string groupTypeStr = targetType.ToString().Split("_")[1];
                        var groupType = Utils.ConvertStrToEnum<BattleGroupType>(groupTypeStr);
                        result.UnionWith(GetUnitsByGroupType(side, groupType, needApplyLoss));
                    }
                    break;

                case BattleTargetType.NONE:
                default: break;
            }
            return result;
        }

        public static bool IsDistantAttack(this BattleActionEffectType actionEffectType)
        {
            return Utils.HasAttribute<DistantAttackFlag, BattleActionEffectType>(actionEffectType);
            //return actionEffectType == BattleActionEffectType.PROJECTILE_ATTACK ||
            //    actionEffectType == BattleActionEffectType.ARMOUR_PIERCING_ATTACK ||
            //    actionEffectType == BattleActionEffectType.SIEGE_ATTACK;
        }

        public static bool IsParabolaAttack(this BattleActionEffectType actionEffectType)
        {
            return Utils.HasAttribute<ParabolaAttackFlag, BattleActionEffectType>(actionEffectType);
            //return actionEffectType == BattleActionEffectType.PROJECTILE_ATTACK ||
            //    actionEffectType == BattleActionEffectType.SIEGE_ATTACK;
        }

        public static bool IsCoverAllEnemiesInGroupSide(this BattleActionEffectType actionEffectType)
        {
            return Utils.HasAttribute<CoverAllEnemiesInGroupSideFlag, BattleActionEffectType>(actionEffectType);
            //return actionEffectType == BattleActionEffectType.PROJECTILE_ATTACK ||
            //    actionEffectType == BattleActionEffectType.SIEGE_ATTACK;
        }

        // whether the attack can turn left/right/around
        public static bool IsAbleToTurn(this BattleActionEffectType actionEffectType)
        {
            return Utils.HasAttribute<AbleToTurnFlag, BattleActionEffectType>(actionEffectType);
            //return actionEffectType == BattleActionEffectType.PRIOTIZED_ATTACK ||
            //    actionEffectType == BattleActionEffectType.MELEE_ATTACK;
        }

        public static bool IsLeftMostGroupSide(GroupSide side, bool needApplyLoss)
        {
            if (side.IsLeft() && HasAtLeastOneGroupInGroupSide(side, needApplyLoss)) return true;
            else
            {
                if (side == GroupSide.TOP_MIDDLE && !HasAtLeastOneGroupInGroupSide(GroupSide.TOP_LEFT, needApplyLoss)) return true;
                if (side == GroupSide.BOTTOM_MIDDLE && !HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_LEFT, needApplyLoss)) return true;
            }
            return false;
        }

        public static bool IsRightMostGroupSide(GroupSide side, bool needApplyLoss)
        {
            if (side.IsRight() && HasAtLeastOneGroupInGroupSide(side, needApplyLoss)) return true;
            else
            {
                if (side == GroupSide.TOP_MIDDLE && !HasAtLeastOneGroupInGroupSide(GroupSide.TOP_RIGHT, needApplyLoss)) return true;
                if (side == GroupSide.BOTTOM_MIDDLE && !HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_RIGHT, needApplyLoss)) return true;
            }
            return false;
        }

        // get all enemies unitIDs according to the target type.
        // only used for attack action type card.
        public static HashSet<Tuple<int, int, BattleGroupType>> GetDefenderUnitsByTargetType_AtkAction(
            GroupSide side,
            BattleTargetType targetType, 
            BattleActionEffectType actionEffectType,
            bool isClientRound,
            bool needApplyLoss)
        {
            var result = new HashSet<Tuple<int, int, BattleGroupType>>();
            bool isAbleToTurn = IsAbleToTurn(actionEffectType);
            switch (targetType)
            {
                case BattleTargetType.FLANKS_ENEMY:
                    {
                        // [Note] we need to include all enemies in the left/right groupside
                        // for those projectile(arrow) attack, it need to cover all.
                        // for those fire_arm attack or melee attack, we can use FRONT/BACK/CENTER target type.
                        if(actionEffectType.IsCoverAllEnemiesInGroupSide())
                        {
                            result.UnionWith(GetAllUnits(side, needApplyLoss));
                        }
                        else
                        {

                            // only cover the left-most col and the first row to attacker, for LEFT
                            // only cover the right-most col and the first row to attacker, for RIGHT

                            int rowStart, rowEnd, colStart, colEnd;
                            GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);

                            // (1) If current side is already left-most, then check all attacks coming from left direction if this attack can turn left
                            bool isLeftMost = IsLeftMostGroupSide(side, needApplyLoss);
                            if (isAbleToTurn && isLeftMost)
                            {
                                int colLeftMost = GetFirstNonEmptyCol(side, true, needApplyLoss);
                                if (colLeftMost != -1)
                                {
                                    result.UnionWith(GetUnitsByRowColStartEnd(rowStart, rowEnd, colLeftMost, colLeftMost + 1, needApplyLoss));
                                }
                            }

                            // (2) then check all attacks coming from right direction
                            bool isRightMost = IsRightMostGroupSide(side, needApplyLoss);
                            if(isAbleToTurn && isRightMost)
                            {
                                int colRightMost = GetFirstNonEmptyCol(side, false, needApplyLoss);
                                if (colRightMost != -1)
                                {
                                    result.UnionWith(GetUnitsByRowColStartEnd(rowStart, rowEnd, colRightMost, colRightMost + 1, needApplyLoss));
                                }
                            }

                            // (3) finally check all attacks coming from bottom or top direction.
                            if(isLeftMost || isRightMost)
                            {
                                int rowBottomMost = GetFirstNonEmptyRow(side, !isClientRound, false);
                                if (rowBottomMost != -1)
                                    result.UnionWith(GetUnitsByRowColStartEnd(rowBottomMost, rowBottomMost + 1, colStart, colEnd, needApplyLoss));
                            }
                        }

                    }
                    break;

                case BattleTargetType.CENTER_ENEMY:
                case BattleTargetType.FRONT_ENEMY:
                case BattleTargetType.BACK_ENEMY:
                    {

#if UNITY_EDITOR
                        if (targetType == BattleTargetType.BACK_ENEMY)
                            Debug.Assert(isAbleToTurn); // must be able to turn otherwise can not hit the back
#endif

                        if (targetType == BattleTargetType.CENTER_ENEMY && actionEffectType.IsCoverAllEnemiesInGroupSide())
                        {
                            // hit all
                            result.UnionWith(GetAllUnits(side, needApplyLoss));
                        }
                        else
                        {
                            // only hit one row
                            bool isAscending = 
                                (isClientRound && targetType == BattleTargetType.BACK_ENEMY) || 
                                (!isClientRound && targetType == BattleTargetType.FRONT_ENEMY);
                            int colStart, colEnd;
                            GetColStartEnd(side, out colStart, out colEnd);
                            // we only need the first row(the front)
                            int rowBottomMost = GetFirstNonEmptyRow(side, isAscending, false);
                            if (rowBottomMost != -1)
                                result.UnionWith(GetUnitsByRowColStartEnd(rowBottomMost, rowBottomMost + 1, colStart, colEnd, needApplyLoss));
                        }
                    }
                    break;

                case BattleTargetType.ALL_ENEMY:
                    {
                        // TODO: I do not know whether it is necessary to check whether is cover all actionEffectType.
                        // it is the same when we are only retrieving the defenders.
                        result.UnionWith(GetAllUnits(side, needApplyLoss));
                    }
                    break;

                case BattleTargetType.NONE:
                default: break;
            }
            return result;
        }

        // this function will not consider attack direction. Only return the enemies match the targetType.
        public static HashSet<Tuple<int, int, BattleGroupType>> GetUnitsByTargetType(
            GroupSide side, BattleTargetType targetType, bool isClientRound, bool needApplyLoss)
        {
            var result = new HashSet<Tuple<int, int, BattleGroupType>>();
            switch (targetType)
            {
                case BattleTargetType.ALL_FRIEND:
                case BattleTargetType.CENTER_FRIEND:
                case BattleTargetType.FLANKS_FRIEND:
                case BattleTargetType.ALL_ENEMY:
                case BattleTargetType.CENTER_ENEMY:
                case BattleTargetType.FLANKS_ENEMY:
                    {
                        // once we specifying the groupside, those BattleTargetType is actually covering all groups.
                        result.UnionWith(GetAllUnits(side, needApplyLoss));
                    }
                    break;

                case BattleTargetType.FRONT_FRIEND:
                case BattleTargetType.BACK_FRIEND:
                case BattleTargetType.FRONT_ENEMY:
                case BattleTargetType.BACK_ENEMY:
                    {
                        bool isAscending = 
                            (isClientRound && (targetType == BattleTargetType.FRONT_FRIEND || targetType == BattleTargetType.BACK_ENEMY)) ||
                            (!isClientRound && (targetType == BattleTargetType.BACK_FRIEND || targetType == BattleTargetType.FRONT_ENEMY));
                        int colStart, colEnd;
                        GetColStartEnd(side, out colStart, out colEnd);
                        // we only need the first row(the front or back)
                        int row = GetFirstNonEmptyRow(side, isAscending, needApplyLoss);
                        result.UnionWith(GetUnitsByRowColStartEnd(row, row + 1, colStart, colEnd, needApplyLoss));
                    }
                    break;

                // [NOTE] below means all friend unit types...
                case BattleTargetType.ALL_CAVALRY:
                case BattleTargetType.ALL_FIREARM:
                case BattleTargetType.ALL_SPECIAL:
                case BattleTargetType.ALL_MELEE:
                case BattleTargetType.ALL_SPEAR:
                case BattleTargetType.ALL_SIEGE:
                case BattleTargetType.ALL_PROJECTILE:
                case BattleTargetType.ALL_HEAVY:
                    {
                        string groupTypeStr = targetType.ToString().Split("_")[1];
                        var groupType = Utils.ConvertStrToEnum<BattleGroupType>(groupTypeStr);
                        result.UnionWith(GetUnitsByGroupType(side, groupType, needApplyLoss));
                    }
                    break;

                case BattleTargetType.NONE:
                default: break;
            }
            return result;
        }

        public static List<Tuple<int, int>> ConvertIndexesGroupTypeToIndexList(ref HashSet<Tuple<int, int, BattleGroupType>> units)
        {
            var result = new HashSet<Tuple<int, int>>(); // avoid repeating
            foreach (var unit in units) result.Add(new(unit.Item1, unit.Item2));
            return result.ToList();
        }

        //public static HashSet<string> ConvertIndexesGroupTypeToUnitIDs(ref HashSet<Tuple<int, int, BattleGroupType>> allUnits)
        //{
        //    var result = new HashSet<string>();
        //    if (allUnits != null)
        //    {
        //        foreach (var unitTuple in allUnits)
        //        {
        //            var battleGroupData = GetBattleGroupDataFromCurrentBattle(unitTuple.Item1, unitTuple.Item2);
        //            result.Add(battleGroupData.GetUnitID(unitTuple.Item3));
        //        }
        //    }
        //    return result;
        //}

        //public static HashSet<string> ConvertGroupIndicesToIDs(ref HashSet<Tuple<int, int>> unitIndices)
        //{
        //    var result = new HashSet<string>();
        //    foreach (var unitIndex in unitIndices)
        //    {
        //        var battleGroupData = GetBattleGroupDataFromCurrentBattle(unitIndex.Item1, unitIndex.Item2);
        //        result.UnionWith(battleGroupData.GetUnits().Keys);
        //    }
        //    return result;
        //}

        public static bool HasActiveRow(int activeRows, ActiveRow target)
        {
            return (activeRows & (int)target) == (int)target;
        }

        public static bool IsAtkAction(BattleActionType actionType)
        {
            return actionType == BattleActionType.ATTACK;
        }

        public static bool IsDefAction(BattleActionType actionType)
        {
            return actionType == BattleActionType.DEFENSE;
        }

        public static bool HasAttackedDir(int atkDirs, AttackedDir target)
        {
            return (atkDirs & (int)target) == (int)target;
        }

        // return a curve arrow direction: from which groupside to which groupside with the attack direction
        public static void GetCurveArrowParams(
            BattleTargetType targetType,
            BattleActionEffectType actionEffectType,
            ref HashSet<GroupSide> startSides,
            ref HashSet<GroupSide> endSides,
            out HashSet<Tuple<GroupSide, GroupSide, AttackedDir>> atkArrowParams)
        {
            // If enemies have left/right sides, then its middle side will no be attacked from left/right direction.
            // left/right sides always die first, the middle dies at the end.
            bool needApplyLoss = false;
            atkArrowParams = new HashSet<Tuple<GroupSide, GroupSide, AttackedDir>>();
            bool isCoverAll = IsCoverAllEnemiesInGroupSide(actionEffectType);
            bool isAbleToTurn = IsAbleToTurn(actionEffectType);
            foreach (var startSide in startSides)
            {
                foreach (var endSide in endSides)
                {
                    switch (targetType)
                    {
                        case BattleTargetType.ALL_ENEMY:
                        case BattleTargetType.FLANKS_ENEMY:
                        case BattleTargetType.CENTER_ENEMY:
                            {
                                // (1) 攻击方最左/右翼 打 敌方最左/右翼 相对面. 如果不是cover全部并且可以转弯的话, 加上侧面
                                // (2) 攻击方中间 打 敌方中间 相对面

                                if (IsLeftMostGroupSide(startSide, needApplyLoss) && IsLeftMostGroupSide(endSide, needApplyLoss))
                                {
                                    // 相对面
                                    atkArrowParams.Add(new(startSide, endSide,
                                        startSide.IsBottom() ? AttackedDir.BOTTOM : AttackedDir.TOP));

                                    // 侧翼
                                    if (!isCoverAll && isAbleToTurn) atkArrowParams.Add(new(startSide, endSide, AttackedDir.LEFT));
                                }

                                if (IsRightMostGroupSide(startSide, needApplyLoss) && IsRightMostGroupSide(endSide, needApplyLoss))
                                {
                                    // 相对面
                                    atkArrowParams.Add(new(startSide, endSide,
                                        startSide.IsBottom() ? AttackedDir.BOTTOM : AttackedDir.TOP));

                                    // 侧翼
                                    if (!isCoverAll && isAbleToTurn) atkArrowParams.Add(new(startSide, endSide, AttackedDir.RIGHT));
                                }

                                if (startSide.IsMiddle() && endSide.IsMiddle()) 
                                {
                                    // 相对面
                                    atkArrowParams.Add(new(startSide, endSide,
                                        startSide.IsBottom() ? AttackedDir.BOTTOM : AttackedDir.TOP));
                                }
                            }
                            break;

                        case BattleTargetType.BACK_ENEMY:
                        case BattleTargetType.FRONT_ENEMY:
                            {
                                // (1) 攻击方最左/中/右翼 打 敌方最左/中/右翼 的 相对面. 
                                // (2) 攻击方最左/右翼 打 敌方最左/右翼 的 相背面. 
                                
                                if (targetType == BattleTargetType.FRONT_ENEMY)
                                {
                                    if ((IsLeftMostGroupSide(startSide, needApplyLoss) && IsLeftMostGroupSide(endSide, needApplyLoss)) ||
                                        (IsRightMostGroupSide(startSide, needApplyLoss) && IsRightMostGroupSide(endSide, needApplyLoss)) ||
                                        (startSide.IsMiddle() && endSide.IsMiddle()))
                                    {
                                        // 相对面
                                        atkArrowParams.Add(new(startSide, endSide,
                                            startSide.IsBottom() ? AttackedDir.BOTTOM : AttackedDir.TOP));
                                    }
                                }
                                else
                                {
                                    if (isAbleToTurn &&
                                        ((IsLeftMostGroupSide(startSide, needApplyLoss) && IsLeftMostGroupSide(endSide, needApplyLoss)) ||
                                        (IsRightMostGroupSide(startSide, needApplyLoss) && IsRightMostGroupSide(endSide, needApplyLoss))))
                                    {
                                        // 相背面
                                        atkArrowParams.Add(new(startSide, endSide,
                                            startSide.IsBottom() ? AttackedDir.TOP : AttackedDir.BOTTOM));
                                    }
                                }
                            }
                            break;

                        case BattleTargetType.NONE:
                        default: break;
                    }
                }
            }
        }
        
        #endregion

        #region ATK/DEF computation
        public static float GetBasicDefense(this Table_unit.Data data, BattleActionEffectType defenseType,
            GroupSide defenderSide, Tuple<int, int, BattleGroupType> tuple)
        {
            Table_defense_type.Data defenseTypeData = Table_defense_type.data[defenseType.ToString()];
            float basicDefense = 1;
            float bonusFromCentral = defenseTypeData.bonusFromCentral;
            float bonusFromFlank = defenseTypeData.bonusFromFlank;
            float bonusFromFormation = defenseTypeData.bonusFromFormation;

            if (defenderSide.IsLeft() || defenderSide.IsRight())
            {
                basicDefense *= (1 + bonusFromFlank);
            }

            if (defenderSide.IsMiddle())
                basicDefense *= (1 + bonusFromCentral);

            int row = tuple.Item1;
            string unitId = data.id;
            Battle curBattle = BattleManager.Instance.GetCurrentBattle();
            int unitAmountInRow = curBattle.GetTotalAmountAfterLoss(defenderSide, row, unitId);
            int totalAmountInRow = curBattle.GetTotalAmountAfterLoss(defenderSide, row);
            float ratio;
            if (totalAmountInRow == 0)
                ratio = 0;
            else
                ratio = (float)unitAmountInRow / totalAmountInRow;
            basicDefense *= (1 + ratio * bonusFromFormation);

            return basicDefense;
        }

        public static float GetBasicAttack(this Table_unit.Data data, BattleActionEffectType attackType,
            GroupSide attackerSide, Tuple<int, int, BattleGroupType> tuple)
        {
            Table_attack_type.Data attackTypeData = Table_attack_type.data[attackType.ToString()];
            float basicAttack = attackTypeData.damage;
            float bonusFromFormation = attackTypeData.bonusFromFormation;

            // Bonus from Flank bonusFromFlank，如果发起攻击的部队在侧翼则得到这个增益
            if (attackerSide.IsLeft() || attackerSide.IsRight())
            {
                float bonusFromFlank = attackTypeData.bonusFromFlank;
                basicAttack *= (1 + bonusFromFlank);
            }

            // Bonus from Formation
            // bonusFromFormation，以行来计算，如果一行两侧的方阵和自己方阵的单位都是同样的部队，则触发该效果，
            // 同质的部队占比越高，这个效果越明显，如果第一排所有部队都是火枪兵，则增益效果达到100%，反之如果一半是火枪兵，则达到50%
            int row = tuple.Item1;
            string unitId = data.id;
            Battle curBattle = BattleManager.Instance.GetCurrentBattle();
            int unitAmountInRow = curBattle.GetTotalAmountAfterLoss(attackerSide, row, unitId);
            int totalAmountInRow = curBattle.GetTotalAmountAfterLoss(attackerSide, row);
            float ratio;
            if (totalAmountInRow == 0)
                ratio = 0;
            else
                ratio = (float)unitAmountInRow / totalAmountInRow;
            basicAttack *= (1 + ratio * bonusFromFormation);

            return basicAttack;
        }

        public static float CalculateDefense(
            BattleActionEffectType defenseType,
            float basicValue,
            GroupSide side,
            bool needApplyLoss,
            ref HashSet<Tuple<int, int, BattleGroupType>> defenders)
        {
            float totalDefenseValue = 0;
            float defenseModifier = GetStatusActualValueSum(side, StatusEffectType.DEFENSE_MODIFIER_EFFECT);
            foreach (var tuple in defenders)
            {
                var battleGroupData = GetBattleGroupDataFromCurrentBattle(tuple.Item1, tuple.Item2);
                var unitIDs = battleGroupData.GetUnitIDs(tuple.Item3, needApplyLoss);
                foreach (var unitID in unitIDs)
                {
                    var unitTabData = Table_unit.data[unitID];
                    //var unitAmount = amountDict[new(side, unitID)];
                    var unitAmount = battleGroupData.GetAmountAfterLoss(unitID);
                    totalDefenseValue +=
                        Mathf.Sqrt(unitTabData.basicEquipment) *
                        Mathf.Sqrt(unitTabData.basicTraining) *
                        unitAmount *
                        unitTabData.GetBasicDefense(defenseType, side, tuple) *
                        (1 + defenseModifier) *
                        basicValue;
                }
            }

            //return totalDefenseValue;
            return totalDefenseValue / 1000; // TODO: just for testing
        }

        public static float CalculateAttack(
            BattleActionEffectType attackType,
            float basicValue,
            GroupSide side,
            bool needApplyLoss,
            ref HashSet<Tuple<int, int, BattleGroupType>> attackers)
        {
            float totalAttackValue = 0;
            float attackModifier = GetStatusActualValueSum(side, StatusEffectType.ATTACK_MODIFIER_EFFECT);
            attackModifier += GetWeatherEffectModifier(attackType);
            foreach (var tuple in attackers)
            {
                var battleGroupData = GetBattleGroupDataFromCurrentBattle(tuple.Item1, tuple.Item2);
                var unitIDs = battleGroupData.GetUnitIDs(tuple.Item3, needApplyLoss);
                foreach (var unitID in unitIDs)
                {
                    var unitTabData = Table_unit.data[unitID];
                    //var unitAmount = amountDict[new(side, unitID)];
                    var unitAmount = battleGroupData.GetAmountAfterLoss(unitID);
                    totalAttackValue +=
                        Mathf.Sqrt(unitTabData.basicEquipment) *
                        Mathf.Sqrt(unitTabData.basicTraining) *
                        unitAmount *
                        unitTabData.GetBasicAttack(attackType, side, tuple) *
                        (1 + attackModifier) *
                        basicValue;
                }
            }

            return totalAttackValue;
        }

        private static int GetRange(this Table_unit.Data data)
        {
            // map the range from [-1, RANGE_MAX]
            if (data.range == 0) return data.priotizedAttack > 0 ? 0 : -1;
            return data.range;
        }

        private static int CalculateActualLoss(
            float atk, float atkToLoss, 
            ref HashSet<Tuple<int, int>> groupIndices, 
            out float attackerOverflowAtk)
        {
            // TODO: 伤害减员分配逻辑, 输入参数应该是 attacker或者defender的所有军团Index, 
            // 从这些index去读取军团的数量, 然后从中取一个值去 不断减少 preLoss 直到为0.

            int preLoss = (int)(atk * atkToLoss);

            if (preLoss <= 0)
            {
                attackerOverflowAtk = 0;
                return 0;
            }

            int curTotalAmount = 0;
            var pickedIndices = new Queue<Tuple<int, int>>();

            foreach (var tuple in groupIndices)
            {
                var battleGroupData = GetBattleGroupDataFromCurrentBattle(tuple.Item1, tuple.Item2);
                var unitAmount = battleGroupData.GetTotalAmountAfterLoss();
                if (unitAmount > 0)
                {
                    curTotalAmount += unitAmount;
                    pickedIndices.Enqueue(tuple);
                }
            }

            // check whether attack overflows
            float atkMax = curTotalAmount / atkToLoss;
            if(atk > atkMax)
            {
                // handle overflow
                attackerOverflowAtk = atk - atkMax;
            }
            else
            {
                attackerOverflowAtk = 0;
            }

            preLoss = Mathf.Min(preLoss, curTotalAmount); // clamp it

            // the idea is like checking each group in the queue, pick a number randomly from its 1 to its amount
            // once preLoss is 0, it ends the loop.
            while (pickedIndices.Count > 0 && preLoss > 0)
            {
                var tuple = pickedIndices.Dequeue();
                var battleGroupData = GetBattleGroupDataFromCurrentBattle(tuple.Item1, tuple.Item2);
                int unitAmount = battleGroupData.GetTotalAmountAfterLoss();

                // randomly picke a num for minusing the preLoss
                int num = UnityEngine.Random.Range(unitAmount / 2, unitAmount * 2); // random pick quickly
                num = Mathf.Min(num, unitAmount); // make sure: num <= unitAmount
                if (num < unitAmount) pickedIndices.Enqueue(tuple); // if there are remaining units, add it back

                num = Mathf.Min(num, preLoss); // make sure: num <= preLoss
                preLoss -= num;
                battleGroupData.CacheLoss(num);
#if UNITY_EDITOR
                // TODO: to delete it later
                if (battleGroupData.Test())
                {
                    battleGroupData.Test(); // PutianTEST
                    Debug.LogError("Can not reach here");
                }
#endif
            }

            return preLoss;
        }

        public static void CalculateAttackFromOneSideToOther(
            BattleActionEffectType attackType,
            float basicValue,
            GroupSide attackerSide,
            GroupSide defenderSide,
            AttackedDir attackedDir,
            ref HashSet<Tuple<int, int, BattleGroupType>> totalAttackers,
            ref HashSet<Tuple<int, int, BattleGroupType>> totalDefenders,
            ref Dictionary<Tuple<GroupSide, BattleActionEffectType>, float> battleDefenseData,
            ref Dictionary<GroupSide, float> defenseDamageModifiers,
            out float attackerAtk,
            out float defenderAtk,
            out float attackerCancelingAtk)
            //out int attackerLose,
            //out int defenderLose)
        {
            attackerAtk = 0;
            defenderAtk = 0;
            attackerCancelingAtk = 0;
            //attackerLose = 0;
            //defenderLose = 0;

            //var attackerUnitIDs = ConvertGroupIndicesToIDs(ref attackerIndexes);
            //var defenderUnitIDs = ConvertGroupIndicesToIDs(ref defenderIndexes);

            var attackerInRangeDict = new Dictionary<int, HashSet<Tuple<int, int, BattleGroupType>>>();
            var defenderInRangeDict = new Dictionary<int, HashSet<Tuple<int, int, BattleGroupType>>>();

            // convert the "HashSet<Tuple<int, int, BattleGroupType>>" to group indices so we can apply unit loss over all groups.
            var attackerGroupIndices = new HashSet<Tuple<int, int>>();
            var defenderGroupIndices = new HashSet<Tuple<int, int>>();

            // separate all attackers according to its range
            foreach (var attacker in totalAttackers)
            {
                var battleGroupData = GetBattleGroupDataFromCurrentBattle(attacker.Item1, attacker.Item2);
                var unitTabData = battleGroupData.GetUnitTableData(attacker.Item3, false);
                int range = unitTabData.GetRange();
                if (!attackerInRangeDict.ContainsKey(range))
                    attackerInRangeDict.Add(range, new HashSet<Tuple<int, int, BattleGroupType>>());
                attackerInRangeDict[range].Add(attacker);
                attackerGroupIndices.Add(new Tuple<int, int>(attacker.Item1, attacker.Item2));
            }

            // separate all defenders according to its range
            foreach (var defender in totalDefenders)
            {
                var battleGroupData = GetBattleGroupDataFromCurrentBattle(defender.Item1, defender.Item2);
                var unitTabData = battleGroupData.GetUnitTableData(defender.Item3, false);
                int range = unitTabData.GetRange();
                if (!defenderInRangeDict.ContainsKey(range))
                    defenderInRangeDict.Add(range, new HashSet<Tuple<int, int, BattleGroupType>>());
                defenderInRangeDict[range].Add(defender);
                defenderGroupIndices.Add(new Tuple<int, int>(defender.Item1, defender.Item2));
            }

            int RANGE_MAX = Table_constant.data["constant_0019"].param_int;
            float attackerUnitLossModifier = GetStatusActualValueSum(attackerSide, StatusEffectType.UNITLOSS_MODIFIER_EFFECT); // it means when being attacked, unitLoss * (1+modifier)
            float defenderUnitLossModifier = GetStatusActualValueSum(defenderSide, StatusEffectType.UNITLOSS_MODIFIER_EFFECT); // it means when being attacked, unitLoss * (1+modifier)
            float attackerAtkToLoss = Table_constant.data["constant_0020"].param_float * (1f + defenderUnitLossModifier);
            float defenderAtkToLoss = Table_constant.data["constant_0021"].param_float * (1f + attackerUnitLossModifier);
            float defenderCounterAttackModifier = GetStatusActualValueSum(defenderSide, StatusEffectType.COUNTERATTACK_MODIFIER_EFFECT);
            bool attackerHasIgnoreDefenseStatus = BattleManager.Instance.GetCurrentBattle().HasStatus(attackerSide, BattleActionEffectType.IGNORE_DEFENSE);
            bool defenderHasIgnoreDefenseStatus = BattleManager.Instance.GetCurrentBattle().HasStatus(defenderSide, BattleActionEffectType.IGNORE_DEFENSE);
            // the below idea is like:
            // Each range there might be an attacker and a defender. Attackers will use atks to decrease the amount of defenders,
            // defender will also use its atks to decrease the amount of attackers. If the target exists and it can be decreased,
            // then just applied minusing a atk on its amount. If not exists, this value will be used in next range,
            // until it reach the final range. It makes sure that all atks will be applied on all groups in the group side.

            float attackerOverflowAtk = 0;
            bool needApplyLoss = true;
            for (int i = RANGE_MAX; i >= -1; i--)
            {
                bool hasAttackerInRange = attackerInRangeDict.ContainsKey(i);
                bool hasDefenderInRange = defenderInRangeDict.ContainsKey(i);
                if (!hasAttackerInRange && !hasDefenderInRange) continue;

                var attackersInRange = hasAttackerInRange ? attackerInRangeDict[i] : null;
                var defendersInRange = hasDefenderInRange ? defenderInRangeDict[i] : null;
                //var attackerUnitIDs = ConvertIndexesGroupTypeToUnitIDs(ref attackersInRange);
                //var defenderUnitIDs = ConvertIndexesGroupTypeToUnitIDs(ref defendersInRange);

                if (hasAttackerInRange)
                {
                    // update the defenders amount if they exist in "amountDict" according to attacker's atk
                    //var attackersInRange = attackerInRangeDict[i];

                    float atk = CalculateAttack(attackType, basicValue, attackerSide, needApplyLoss, ref attackersInRange);
                    
                    atk += attackerOverflowAtk; // attackerExtraAtk is coming from last range(the overflow part)

                    float newAtk = attackerHasIgnoreDefenseStatus ? atk :
                        UpdateAtkViaDefenseData(atk, attackerSide, defenderSide, ref battleDefenseData, ref defenseDamageModifiers, attackType);

                    attackerCancelingAtk += (atk - newAtk);
                    
                    atk = newAtk; // update the atk

                    attackerAtk += atk;

                    // update the amount of defenders.
                    //defenderLose += CalculateActualLoss(atk, attackerRatio, ref defenderGroupIndices, out attackerOverflowAtk);
                    CalculateActualLoss(atk, attackerAtkToLoss, ref defenderGroupIndices, out attackerOverflowAtk);
                }


                if (hasDefenderInRange)
                {
                    //var defendersInRange = defenderInRangeDict[i];
                    float atk = CalculateAttack(attackType, basicValue, defenderSide, needApplyLoss, ref defendersInRange);

                    atk *= (1f + defenderCounterAttackModifier);
                    atk = defenderHasIgnoreDefenseStatus ? atk :
                        UpdateAtkViaDefenseData(atk, defenderSide, attackerSide, ref battleDefenseData, ref defenseDamageModifiers, attackType);

                    defenderAtk += atk;

                    // update the amount of attackers.
                    //attackerLose += CalculateActualLoss(atk, defenderRatio, ref attackerGroupIndices, out _);
                    CalculateActualLoss(atk, defenderAtkToLoss, ref attackerGroupIndices, out _);

                    //int totalLose = (int)(atk * defenderRatio);
                    //attackerLose += totalLose;
                    //UpdateAmountDict(totalLose, attackerSide, ref attackerUnitIDs, ref amountDict);
                }
            }

            // When reach below, it means no more defense!!!
            if(attackerOverflowAtk > 0)
            {
                // apply this overflow atk to other groups in the same GroupSide of defenderGroupIndices
                ApplyLossToOtherGroupsByAttackedDir(
                    attackerOverflowAtk, attackerAtkToLoss, 
                    defenderSide, attackedDir, 
                    ref defenderGroupIndices);
            }
        }

        // E.g. If attackedDir is vertical, like TOP or BOTTOM, then it means we only consider varying the rowIndex.
        // If now groups are the first row, and attackedDir is BOTTOM, then otherGroups should be any groups not in the first row.
        public static void ApplyLossToOtherGroupsByAttackedDir(
            float atk, float atkToLoss,
            GroupSide side, AttackedDir attackedDir, 
            ref HashSet<Tuple<int, int>> groupIndices)
        {
            if (attackedDir == AttackedDir.TOP || attackedDir == AttackedDir.BOTTOM)
            {
                var rowOccupyDict = new HashSet<int>(); // record the occupied row indices.
                foreach (var groupIndex in groupIndices) rowOccupyDict.Add(groupIndex.Item1);

                bool isAscending = attackedDir == AttackedDir.TOP;
                int rowStep = isAscending ? +1 : -1;
                int rowStart, rowEnd, colStart, colEnd;
                GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
                for (int row = isAscending ? rowStart : rowEnd - 1; rowStart <= row && row < rowEnd; row += rowStep)
                {
                    if (atk <= 0) break;
                    if (rowOccupyDict.Contains(row)) continue;

                    var otherGroupIndices = new HashSet<Tuple<int, int>>();
                    for (int col = colStart; col < colEnd; col++) otherGroupIndices.Add(new Tuple<int, int>(row, col));

                    // apply loss on the next row, repeat it until atk <= 0 or no more rows.
                    float extraAtk;
                    CalculateActualLoss(atk, atkToLoss, ref otherGroupIndices, out extraAtk);
                    atk = extraAtk;
                }
            }
            else if (attackedDir == AttackedDir.LEFT || attackedDir == AttackedDir.RIGHT)
            {
                var colOccupyDict = new HashSet<int>(); // record the occupied col indices.
                foreach (var groupIndex in groupIndices) colOccupyDict.Add(groupIndex.Item2);

                bool isAscending = attackedDir == AttackedDir.LEFT;
                int rowStart, rowEnd, colStart, colEnd;
                GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
                int colStep = isAscending ? +1 : -1;
                for (int col = isAscending ? colStart : colEnd - 1; colStart <= col && col < colEnd; col += colStep)
                {
                    if (atk <= 0) break;
                    if (colOccupyDict.Contains(col)) continue;

                    var otherGroupIndices = new HashSet<Tuple<int, int>>();
                    for (int row = rowStart; row < rowEnd; row++) otherGroupIndices.Add(new Tuple<int, int>(row, col));

                    // apply loss on the next col, repeat it until atk <= 0 or no more cols.
                    float extraAtk;
                    CalculateActualLoss(atk, atkToLoss, ref otherGroupIndices, out extraAtk);
                    atk = extraAtk;
                }
            }
        }

        public static float GetGroupsideBattleDefense(
            ref Dictionary<Tuple<GroupSide, BattleActionEffectType>, float> battleDefenseData, 
            GroupSide side)
        {
            float totalDef = 0;
            totalDef += battleDefenseData[new(side, BattleActionEffectType.TERRAIN_DEFENSE)];
            totalDef += battleDefenseData[new(side, BattleActionEffectType.FORTIFICATION_DEFENSE)];
            totalDef += battleDefenseData[new(side, BattleActionEffectType.ELASTIC_DEFENSE)];
            totalDef += battleDefenseData[new(side, BattleActionEffectType.FORMATION_DEFENSE)];
            totalDef += battleDefenseData[new(side, BattleActionEffectType.ARMOUR_DEFENSE)];
            return totalDef;
        }

        public static float UpdateAtkViaDefenseData(
            float oriAtk,
            GroupSide attackerSide,
            GroupSide defenderSide,
            ref Dictionary<Tuple<GroupSide, BattleActionEffectType>, float> battleDefenseData,
            ref Dictionary<GroupSide, float> defenseDamageModifiers,
            BattleActionEffectType attackType)
        {
            // update the atk via defender's defense values
            float resAtk = oriAtk;

            var defenseTypeList = new List<BattleActionEffectType>()
            {
                BattleActionEffectType.TERRAIN_DEFENSE,
                BattleActionEffectType.FORTIFICATION_DEFENSE,
                BattleActionEffectType.ELASTIC_DEFENSE,
                BattleActionEffectType.FORMATION_DEFENSE,
                BattleActionEffectType.ARMOUR_DEFENSE
            };

            // minus atk according to current defense data.
            Tuple<GroupSide, BattleActionEffectType> key;
            float diff;

            for (int i = 0; i < defenseTypeList.Count; i++) 
            {
                var defenseType = defenseTypeList[i];
                key = new(defenderSide, defenseType);
                float atkAfterBonus = GetAtkAfterBonus(resAtk, defenderSide, attackType, defenseType);
                atkAfterBonus *= (1f + defenseDamageModifiers[attackerSide]); // put defense damage modifier in it
                diff = Mathf.Min(battleDefenseData[key], atkAfterBonus);
                battleDefenseData[key] -= diff;
                resAtk -= diff;
                if (resAtk <= 0) break;
            }

            return resAtk;
        }
        #endregion

        // 攻击力根据攻击类型和防御类型修正，attack_type表的bonusOnXXX
        public static float GetAtkAfterBonus(float oriAtk, GroupSide defenderSide,
            BattleActionEffectType attackType, BattleActionEffectType defenseType)
        {
            float newAtk = oriAtk;

            Table_attack_type.Data attackTypeData = Table_attack_type.data[attackType.ToString()];
            float bonusOnFlanking = attackTypeData.bonusOnFlanking;
            newAtk *= (1 + bonusOnFlanking);

            string fieldName = "bonusOn";
            string defenseName = defenseType.ToString().Split("_")[0].ToLower();
            defenseName = char.ToUpper(defenseName[0]) + defenseName.Substring(1);
            fieldName += defenseName;

            float fieldValue = (float)attackTypeData.GetType().GetField(fieldName).GetValue(attackTypeData);
            newAtk *= (1 + fieldValue);

            return newAtk;
        }

        public const float moralDeltaMIN = -5, moralDeltaMAX = +5, disciplineDeltaMIN = -5, disciplineDeltaMAX = +5;
        public static float CalculateMoralDelta(float attackerAtk, float defenderAtk, float defenderMoralModifier)
        {
            if (defenderAtk == 0) return +5;

            float result = attackerAtk / defenderAtk - 1;
            if (result < 0)
            {
                // it means attacker is losing moral. The losing value can be amplified by modifier)
                result *= (1f + defenderMoralModifier);
            }
            return Mathf.Clamp(result, moralDeltaMIN, moralDeltaMAX);
        }

        public static float CalculateDisciplineDelta(float defenderAtk, float attackerCancelingAtk, float disciplineModifier)
        {
            if (defenderAtk == 0) return -5f;

            float result = -1 * (attackerCancelingAtk / defenderAtk);
            if (result < 0)
            {
                // disciplineModifier is for amplifying the delta when result is negative(it means one groupside's discipline is being damaged.)
                result *= (1f + disciplineModifier);
            }
            return Mathf.Clamp(result, disciplineDeltaMIN, disciplineDeltaMAX);
        }

        public static float CalculateDisciplineDelta(float gainDef, float ratio, int amount)
        {
            if (amount == 0) return +5;
            return Mathf.Clamp(gainDef * ratio / amount, disciplineDeltaMIN, disciplineDeltaMAX);
        }

        public static float GetBasicValueRatio(BattleBasicValueType valueType)
        {
            switch (valueType)
            {
                case BattleBasicValueType.REMAINING_ACTION_POINT:
                    return BattlePanel.GetBattleCardComponent().GetBattleCardCost().GetRestCost();
                case BattleBasicValueType.ALL_CARD_NUMBER:
                    return BattlePanel.GetBattleCardComponent().CardList.Count;
                case BattleBasicValueType.METAL_CARD_NUMBER:
                    return BattlePanel.GetBattleCardComponent().GetHandCardNumberByElement(BattleElementType.METAL);
                case BattleBasicValueType.WOOD_CARD_NUMBER:
                    return BattlePanel.GetBattleCardComponent().GetHandCardNumberByElement(BattleElementType.WOOD);
                case BattleBasicValueType.WATER_CARD_NUMBER:
                    return BattlePanel.GetBattleCardComponent().GetHandCardNumberByElement(BattleElementType.WATER);
                case BattleBasicValueType.FIRE_CARD_NUMBER:
                    return BattlePanel.GetBattleCardComponent().GetHandCardNumberByElement(BattleElementType.FIRE);
                case BattleBasicValueType.EARTH_CARD_NUMBER:
                    return BattlePanel.GetBattleCardComponent().GetHandCardNumberByElement(BattleElementType.EARTH);
                case BattleBasicValueType.ALL_CARD_PLAYED:
                    return BattlePanel.GetBattleCardComponent().GetBattleCardHistory().GetCurRoundUsedCardList().Count;
                case BattleBasicValueType.METAL_CARD_PLAYED:
                    return BattlePanel.GetBattleCardComponent().GetUsedCardNumberByElement(BattleElementType.METAL);
                case BattleBasicValueType.WOOD_CARD_PLAYED:
                    return BattlePanel.GetBattleCardComponent().GetUsedCardNumberByElement(BattleElementType.WOOD);
                case BattleBasicValueType.WATER_CARD_PLAYED:
                    return BattlePanel.GetBattleCardComponent().GetUsedCardNumberByElement(BattleElementType.WATER);
                case BattleBasicValueType.FIRE_CARD_PLAYED:
                    return BattlePanel.GetBattleCardComponent().GetUsedCardNumberByElement(BattleElementType.FIRE);
                case BattleBasicValueType.EARTH_CARD_PLAYED:
                    return BattlePanel.GetBattleCardComponent().GetUsedCardNumberByElement(BattleElementType.EARTH);
                default:
                    return 1f;
            }
        }

        public static float GetStatusActualValueSum(GroupSide side, StatusEffectType statusEffectType)
        {
            return BattleManager.Instance.GetCurrentBattle().GetStatusActualValueSum(side, statusEffectType);
        }

        private static float GetWeatherEffectModifier(BattleActionEffectType attackType)
        {
            // TODO
            return 0.0f;
        }

        private static Dictionary<GroupSide, float> GetXXXDamageModifiers(StatusEffectType statusEffectType)
        {
            var result = new Dictionary<GroupSide, float>();
            IterateGroupSide((GroupSide side) =>
            {
                result.Add(side, GetStatusActualValueSum(side, statusEffectType));
            });
            return result;
        }

        public static Dictionary<GroupSide, float> GetMoralDamageModifiers()
        {
            return GetXXXDamageModifiers(StatusEffectType.MORAL_DAMAGE_MODIFIER_EFFECT);
        }

        public static Dictionary<GroupSide, float> GetDisciplineDamageModifiers()
        {
            return GetXXXDamageModifiers(StatusEffectType.DISCIPLINE_DAMAGE_MODIFIER_EFFECT);
        }

        public static Dictionary<GroupSide, float> GetDefenseDamageModifiers()
        {
            return GetXXXDamageModifiers(StatusEffectType.DEFENSE_DAMAGE_MODIFIER_EFFECT);
        }
    }
}
