using Assets.Scripts.logics;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.BattlePanel;
using Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAIController : BattleBasicController
{
    private BattleCardPileData battleCardPileData; // cards in pile
    private List<BattleCardData> handCardDataList; // cards in hand
    private Queue<BattleCardData> cardDataToUse; // cards to use
    private BattleCardCost battleCardCost;
    private BattleUseCard battleUseCard;

    //private List<Action> behaviorActions;
    //private int behaviorActionIndex;

    public static bool enable = true;

    private bool isAutoUseDelayedCard;
    private List<BattleDelayedCardData> battleDelayedCards;

    public BattleAIController()
    {
        //behaviorActions = new List<Action>();
        //behaviorActionIndex = 0;
        isAutoUseDelayedCard = false;
        battleDelayedCards = new List<BattleDelayedCardData>();
    }

    public override void Init()
    {
        battleCardPileData = new BattleCardPileData(false);
        cardDataToUse = new Queue<BattleCardData>();
        battleCardCost = new BattleCardCost();
        battleUseCard = new BattleUseCard(false);

        EventManager.Instance.RegisterEventHandler<Action>(Core.Events.EventType.UI_OnAfterUseCard, HandleOnAfterUseCard);
        EventManager.Instance.RegisterEventHandler<Action<BattleCardData>>(Core.Events.EventType.UI_OnPreviewCard, HandleOnPreviewCard);

        EventManager.Instance.RegisterEventHandler<Action<bool, int>>(Core.Events.EventType.Battle_OnUpdateActionPoint, HandleOnUpdateActionPoint);
        EventManager.Instance.RegisterEventHandler<Action<bool, int, BattleActionEffectType>>(Core.Events.EventType.Battle_OnDrawCard, HandleOnDrawCard);
        EventManager.Instance.RegisterEventHandler<Action<bool>>(Core.Events.EventType.Battle_OnInheritAllActionPoint, HandleOnInheritAllActionPoint);
        EventManager.Instance.RegisterEventHandler<Action<bool, int, BattleCardData>>(Core.Events.EventType.Battle_OnActionAfterTurn, HandleOnActionAfterTurn);
    }

    public override void Destroy()
    {
        EventManager.Instance.UnRegisterEventHandler<Action>(Core.Events.EventType.UI_OnAfterUseCard, HandleOnAfterUseCard);
        EventManager.Instance.UnRegisterEventHandler<Action<BattleCardData>>(Core.Events.EventType.UI_OnPreviewCard, HandleOnPreviewCard);

        EventManager.Instance.UnRegisterEventHandler<Action<bool, int>>(Core.Events.EventType.Battle_OnUpdateActionPoint, HandleOnUpdateActionPoint);
        EventManager.Instance.UnRegisterEventHandler<Action<bool, int, BattleActionEffectType>>(Core.Events.EventType.Battle_OnDrawCard, HandleOnDrawCard);
        EventManager.Instance.UnRegisterEventHandler<Action<bool>>(Core.Events.EventType.Battle_OnInheritAllActionPoint, HandleOnInheritAllActionPoint);
        EventManager.Instance.UnRegisterEventHandler<Action<bool, int, BattleCardData>>(Core.Events.EventType.Battle_OnActionAfterTurn, HandleOnActionAfterTurn);
    }

    public override void Update()
    {
        //Debug.Log("AI Controller Update");
    }

    public BattleCardPileData GetBattleCardPileData()
    {
        return battleCardPileData;
    }

    // preview all the picked cards 
    private void RandomPreviewCard()
    {
        BattleCardData cardData = cardDataToUse.Dequeue();
        handCardDataList.Remove(cardData);

        battleCardCost.UseCardConsumeCost(cardData);

        EventManager.Instance.SendEventSync(Core.Events.EventType.UI_OnPreviewCard, cardData);

        GameCore.DelayCall(1.0f, () =>
        {
            EventManager.Instance.SendEventSync(Core.Events.EventType.UI_OnUseCard);
        });

        // back to pile
        if (!cardData.actionData.ifSpecial)
        {
            battleCardPileData.AddCardData(cardData);
        }
    }

    private void HandleOnAfterUseCard()
    {
        if (BattleManager.Instance.IsClientRound()) return;

        if (isAutoUseDelayedCard)
        {
            TryUseDelayedCard(); // try to CONTINUE to auto use delayed cards
        }

        if (isAutoUseDelayedCard) return;

        var curBattle = BattleManager.Instance.GetCurrentBattle();
        if (!curBattle.IsBattleFinish() && !curBattle.IsClientRound())
        {
            if (cardDataToUse.Count > 0)
            {
                RandomPreviewCard();
            }
            else
            {
                EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnNextRound);
            }
        }
    }

    private void HandleOnPreviewCard(BattleCardData cardData)
    {
        // enemy use card
        if (!BattleManager.Instance.IsClientRound() && !isAutoUseDelayedCard)
        {
            //var cardData = BattlePanel.GetBattleFieldComponent().GetBattleCardData();

            string path = "ui_prefab/card/Card";
            GameObject prefabObj = ResourceManager.Instance.LoadResource(path, typeof(GameObject)) as GameObject;
            Transform parent = UIManager.Instance.GetOpenUI("BattlePanel").transform;
            GameObject cardObj = GameObject.Instantiate(prefabObj, parent);
            cardObj.GetComponent<BattleCard>().SetCardData(cardData);
            cardObj.GetComponent<BattleCard>().SetIsEnemy(true);

            GameCore.DelayCall(0.5f, () =>
            {
                battleUseCard.UseCard(cardData, cardObj);
            });
        }
    }

    private void HandleOnUpdateActionPoint(bool isClient, int pointDiff)
    {
        if (!isClient)
        {
            battleCardCost.UpdateCost(pointDiff);
        }
    }

    private void HandleOnDrawCard(bool isClient, int count, BattleActionEffectType effectType)
    {
        if (!isClient)
        {
            BattleElementType elementType = BattleCardComponent.GetElementTypeWithEffectType(effectType);
            List<BattleCardData> cardDataList = battleCardPileData.SelectCardsByElements(count, elementType);
            foreach (BattleCardData cardData in cardDataList)
            {
                handCardDataList.Add(cardData);
            }
        }
    }

    private void HandleOnInheritAllActionPoint(bool isClient)
    {
        if (!isClient)
        {
            battleCardCost.UpdateCost(battleCardCost.GetLastRoundRemainingCost());
        }
    }

    private void RandomPickCards()
    {
        battleCardCost.SetLastRoundRemainingCost();
        battleCardCost.ResetCost();
        handCardDataList = battleCardPileData.SelectCardsByElements(8);

        // find cards that best use the 8 points
        List<int> cardIndexList = GetBestUseCardIndexList();

        //int totalCost = 0;
        foreach (int i in cardIndexList)
        {
            // for test:
            //BattleCardData data = cardDataList[i];
            //int useCost = battleCardCost.GetCardCost(data);
            //totalCost += useCost;

            cardDataToUse.Enqueue(handCardDataList[i]);
        }
    }

    public override void OnRoundStart()
    {
        if(!enable)
        {
            Debug.Log("AI is not enable, use GMSetAIEnable with 'true' to enable it");
            EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnNextRound);
            return;
        }

        // minus 1 for all 'ACTION_AFTER_TURN' status
        BattleManager.Instance.GetCurrentBattle().CountDownActionAfterTurnStatus();

        TryUseDelayedCard(); // try to auto use delayed cards

        if(!isAutoUseDelayedCard)
        {
            Debug.Log("AI Controller start to make actions...");

            RandomPickCards(); // pick some cards for usage

            GameCore.DelayCall(1.0f, RandomPreviewCard);
        }
    }

    public override void OnRoundEnd()
    {
        if (handCardDataList == null) return;
        foreach (BattleCardData cardData in handCardDataList)
        {
            battleCardPileData.AddCardData(cardData);
        }
    }

    private List<int> GetBestUseCardIndexList()
    {
        int restCost = battleCardCost.GetRestCost();
        int minRestCostAfterUse = restCost;
        int maxPossiblyUseCardCount = 8;
        List<int> currBestList = null;
        for (int i = 1; i <= maxPossiblyUseCardCount; ++i) // choose 1 to 8 cards
        {
            List<List<int>> allPossibleLists = GetAllPossibleIndexLists(maxPossiblyUseCardCount, i);
            foreach (List<int> possibleList in allPossibleLists)
            {
                int restCostAfterUse = restCost;
                foreach (int index in possibleList)
                {
                    restCostAfterUse -= battleCardCost.GetCardCost(handCardDataList[index]);
                }
                if (restCostAfterUse == 0)
                    return possibleList;
                else if (restCostAfterUse > 0)
                {
                    minRestCostAfterUse = Math.Min(restCostAfterUse, minRestCostAfterUse);
                    currBestList = possibleList;
                }
            }
        }
        return currBestList;
    }

    private List<List<int>> GetAllPossibleIndexLists(int totalCount, int pickCount)
    {
        List<List<int>> result = new List<List<int>>();

        List<int> currList = new List<int>();
        GenerateCombination(0, currList, totalCount, pickCount, result);

        return result;
    }

    private void GenerateCombination(int start, List<int> currList, int totalCount, int pickCount, List<List<int>> result)
    {
        if (currList.Count == pickCount)
        {
            result.Add(new List<int>(currList));
        }
        else
        {
            for (int i = start; i < totalCount; ++i)
            {
                currList.Add(i);
                GenerateCombination(i + 1, currList, totalCount, pickCount, result);
                currList.RemoveAt(currList.Count - 1);
            }
        }
    }

    private void HandleOnActionAfterTurn(bool isClient, int roundNum, BattleCardData origCardData)
    {
        if (isClient) return;

        BattleDelayedCardData result = null;
        // first to check whether we have alreayd a delayed card with turnNum, 
        // -> if so, merge them
        // -> if no, push it back
        foreach (var item in battleDelayedCards)
        {
            if (item.roundNum == roundNum) { result = item; break; }
        }

        if (result == null) { result = new BattleDelayedCardData(roundNum); battleDelayedCards.Add(result); }

        // once we've found or create a DelayedCardData, we put the origCardData into it.
        result.AddBattleCardData(origCardData);
    }

    private void TryUseDelayedCard()
    {
        if (battleDelayedCards.Count <= 0)
        {
            isAutoUseDelayedCard = false;
            return;
        }

        int curRoundNum = BattleManager.Instance.GetCurrentBattle().GetCurRoundNum();
        int foundIndex = -1;
        for (int i = 0; i < battleDelayedCards.Count; i++)
        {
            if (battleDelayedCards[i].roundNum == curRoundNum)
            {
                foundIndex = i;
                break;
            }
        }

        // it is not the round.
        if (foundIndex == -1)
        {
            isAutoUseDelayedCard = false;
            return;
        }

        isAutoUseDelayedCard = true;

        var delayedCardData = battleDelayedCards[foundIndex];
        battleDelayedCards.RemoveAt(foundIndex);

        EventManager.Instance.SendEventSync(Core.Events.EventType.UI_OnPreviewCard, delayedCardData.battleCardData);

        GameCore.DelayCall(0.5f, () =>
        {
            EventManager.Instance.SendEventSync(Core.Events.EventType.UI_OnUseCard);
        });

        // also need to update DelayedActions Status's layer(value) if there is any delayedCard
        int leastRoundNumIndex = -1;
        int leastRoundNum = int.MaxValue;
        for (int i = 0; i < battleDelayedCards.Count; i++)
        {
            if (leastRoundNum < battleDelayedCards[i].roundNum)
            {
                leastRoundNumIndex = i;
                leastRoundNum = battleDelayedCards[i].roundNum;
            }
        }
        if (leastRoundNumIndex == -1) return;

        int leastTurnNum = (leastRoundNum - curRoundNum) / 2;
        BattleManager.Instance.GetCurrentBattle().ReplaceActionAfterTurnStatus(true, leastTurnNum); // update the status
    }
}
