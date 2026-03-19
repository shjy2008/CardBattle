using System;
using System.Collections.Generic;
using Assets.Scripts.data;
using Assets.Scripts.logics;
using Assets.Scripts.managers.archivemgr;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.utility;

public class BattleCardPileData
{
    private bool isClient; // true: me, false: enemy
    //private Dictionary<string, int> cardPileDict; // cardId -> num

    private List<BattleCardData> pileCardList;

    public BattleCardPileData(bool _isClient)
    {
        isClient = _isClient;

        //cardPileDict = new Dictionary<string, int>();

        //// TODO: 现在先1-30各一张牌在牌堆
        //for (int i = 0; i < 30; ++i)
        //{
        //    cardPileDict.Add(string.Format("tactic_{0:0000}", i + 1), 1);
        //}
        //cardPileDict["tactic_0006"] = 3; // 这个特殊卡有3张

        if (isClient)
        {
            pileCardList = new List<BattleCardData>();
            var cardIdList = ArchiveManager.Instance.GetCurrentArchiveData().playerData.cardIdList;
            foreach (string cardId in cardIdList)
            {
                BattleCardData cardData = new BattleCardData(cardId);
                pileCardList.Add(cardData);
            }

            //pileCardList = new List<BattleCardData>(ArchiveManager.Instance.GetCurrentArchiveData().playerData.cardDataList);
        }
        else
        {
            pileCardList = new List<BattleCardData>();

            for (int i = 0; i < 50; ++i)
            {
                string cardId = string.Format("tactic_{0:0000}", i + 1);
                BattleCardData _cardData = new BattleCardData(cardId);
                pileCardList.Add(_cardData);
            }
            //for (int i = 0; i < 2; ++i)
            //{
            //    BattleCardData cardData_6 = new BattleCardData("tactic_0006");
            //    pileCardList.Add(cardData_6);
            //}
        }
    }

    public List<BattleCardData> SelectCardsByElements(int selectCount, BattleElementType chooseElementType = BattleElementType.NONE)
    {

        List<GroupSide> groupSideList;
        if (isClient)
        {
            groupSideList = new List<GroupSide>() { GroupSide.BOTTOM_LEFT, GroupSide.BOTTOM_MIDDLE, GroupSide.BOTTOM_RIGHT };
        }
        else
        {
            groupSideList = new List<GroupSide>() { GroupSide.TOP_LEFT, GroupSide.TOP_MIDDLE, GroupSide.TOP_RIGHT };
        }

        Battle curBattle = BattleManager.Instance.GetCurrentBattle();

        List<BattleGroupData> battleGroupDataList = new List<BattleGroupData>();
        foreach (GroupSide side in groupSideList)
        {
            int rowStart, rowEnd, colStart, colEnd;
            BattleLogics.GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = rowStart; row < rowEnd; row++)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    var battleGroupData = curBattle.GetBattleGroupData(row, col);
                    battleGroupDataList.Add(battleGroupData);
                }
            }
        }

        Dictionary<BattleElementType, float> elementWeightDict = GetElementWeightDict(battleGroupDataList);

        List<Tuple<BattleCardData, float>> cardAndWeightList = new List<Tuple<BattleCardData, float>>();
        foreach (BattleCardData cardData in pileCardList)
        {
            List<string> cardElements = cardData.actionData.element;
            float cardWeight = 0;
            bool hasElementType = false;
            foreach (string elementStr in cardElements)
            {
                BattleElementType elementType = Utils.ConvertStrToEnum<BattleElementType>(elementStr);
                cardWeight += elementWeightDict[elementType];
                if (chooseElementType == BattleElementType.NONE || elementType == chooseElementType)
                {
                    hasElementType = true;
                }
            }
            if (hasElementType)
            {
                cardAndWeightList.Add(new(cardData, cardWeight));
            }
        }

        List<BattleCardData> pickedCards = new List<BattleCardData>();
        for (int i = 0; i < selectCount; ++i)
        {
            if (pileCardList.Count == 0 || cardAndWeightList.Count == 0)
                break;

            float totalWeight = 0.0f;
            foreach (var cardAndWeight in cardAndWeightList)
            {
                totalWeight += cardAndWeight.Item2;
            }
            float randomFloat = UnityEngine.Random.Range(0.0f, totalWeight);
            foreach (var cardAndWeight in cardAndWeightList)
            {
                randomFloat -= cardAndWeight.Item2;
                if (randomFloat <= 0.0f)
                {
                    pickedCards.Add(cardAndWeight.Item1);
                    pileCardList.Remove(cardAndWeight.Item1);
                    cardAndWeightList.Remove(cardAndWeight);
                    break;
                }
            }
        }

        return pickedCards;
    }

    public void AddCardData(BattleCardData cardData)
    {
        pileCardList.Add(cardData);
    }

    public static Dictionary<BattleElementType, float> GetElementWeightDict(List<BattleGroupData> battleGroupDataList)
    {
        // dict to store portion of elements of units in battle
        Dictionary<BattleElementType, float> elementWeightDict = new Dictionary<BattleElementType, float>();

        List<BattleElementType> elementsList = new List<BattleElementType>() { BattleElementType.WATER, BattleElementType.WOOD,
            BattleElementType.FIRE, BattleElementType.EARTH, BattleElementType.METAL };
        foreach (var element in elementsList)
        {
            elementWeightDict[element] = 0;
        }

        foreach (BattleGroupData battleGroupData in battleGroupDataList)
        {
            var units = battleGroupData.GetUnits();
            foreach (var kv in units)
            {
                var unitId = kv.Key;
                var unitNum = kv.Value;
                var unitTableData = Table_unit.data[unitId];
                BattleGroupType groupType = Utils.ConvertStrToEnum<BattleGroupType>(unitTableData.group);
                switch (groupType)
                {
                    case BattleGroupType.HEAVY:
                        {
                            elementWeightDict[BattleElementType.METAL] += (unitNum);
                        }
                        break;
                    case BattleGroupType.SPEAR:
                    case BattleGroupType.MELEE:
                    case BattleGroupType.PROJECTILE:
                        {
                            elementWeightDict[BattleElementType.WOOD] += (unitNum);
                        }
                        break;
                    case BattleGroupType.FIREARM:
                        {
                            elementWeightDict[BattleElementType.FIRE] += (unitNum);
                        }
                        break;
                    case BattleGroupType.CAVALRY:
                        {
                            elementWeightDict[BattleElementType.WATER] += (unitNum);
                        }
                        break;
                    case BattleGroupType.SPECIAL:
                        {
                            elementWeightDict[BattleElementType.EARTH] += (unitNum);
                        }
                        break;
                    case BattleGroupType.SIEGE:
                        {
                            elementWeightDict[BattleElementType.FIRE] += (unitNum * 0.7f);
                            elementWeightDict[BattleElementType.EARTH] += (unitNum * 0.3f);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        return elementWeightDict;
    }

}
