using Assets.Scripts.logics;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.managers.timermgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.BattlePanel;
using Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleClientController : BattleBasicController
{
    private bool isAutoUseDelayedCard;
    private List<BattleDelayedCardData> battleDelayedCards;

    public BattleClientController()
    {
        isAutoUseDelayedCard = false;
        battleDelayedCards = new List<BattleDelayedCardData>();
    }

    public override void Init()
    {
        EventManager.Instance.RegisterEventHandler<Action<BattleCardData>>(Core.Events.EventType.UI_OnPreviewCard, HandleOnPreviewCard);
        EventManager.Instance.RegisterEventHandler<Action>(Core.Events.EventType.UI_OnEndPreviewCard, HandleOnEndPreviewCard);
        EventManager.Instance.RegisterEventHandler<Action>(Core.Events.EventType.UI_OnUseCard, HandleOnUseCard);
        EventManager.Instance.RegisterEventHandler<Action<bool, int, BattleCardData>>(Core.Events.EventType.Battle_OnActionAfterTurn, HandleOnActionAfterTurn);
        EventManager.Instance.RegisterEventHandler<Action>(Core.Events.EventType.UI_OnAfterUseCard, HandleOnAfterUseCard);
    }

    public override void Destroy()
    {
        EventManager.Instance.UnRegisterEventHandler<Action<BattleCardData>>(Core.Events.EventType.UI_OnPreviewCard, HandleOnPreviewCard);
        EventManager.Instance.UnRegisterEventHandler<Action>(Core.Events.EventType.UI_OnEndPreviewCard, HandleOnEndPreviewCard);
        EventManager.Instance.UnRegisterEventHandler<Action>(Core.Events.EventType.UI_OnUseCard, HandleOnUseCard);
        EventManager.Instance.UnRegisterEventHandler<Action<bool, int, BattleCardData>>(Core.Events.EventType.Battle_OnActionAfterTurn, HandleOnActionAfterTurn);
        EventManager.Instance.UnRegisterEventHandler<Action>(Core.Events.EventType.UI_OnAfterUseCard, HandleOnAfterUseCard);
    }

    public override void Update()
    {

    }

    private void HandleOnPreviewCard(BattleCardData cardData)
    {
        // TODO: for PVP, to send a packet msg
    }

    private void HandleOnEndPreviewCard()
    {
        // TODO: for PVP, to send a packet msg
    }

    private void HandleOnUseCard()
    {
        // TODO: for PVP, to send a packet msg
        var curBattle = BattleManager.Instance.GetCurrentBattle();
        if (curBattle.IsClientRound() && !curBattle.IsBattleFinish())
        {
            var cardData = BattlePanel.GetBattleFieldComponent().GetBattleCardData();
            // TODO:
        }
    }

    public override void OnRoundStart()
    {
        BattlePanel.GetBattleCardComponent().GetBattleCardCost().SetLastRoundRemainingCost();
        BattlePanel.GetBattleCardComponent().GetBattleCardCost().ResetCost();

        BattlePanel.GetBattleCardComponent().GetBattleCardHistory().StartNewRound();

        BattlePanel.GetBattleCardComponent().GetBattleDrawCard().DrawCard(8);

        // minus 1 for all 'ACTION_AFTER_TURN' status
        BattleManager.Instance.GetCurrentBattle().CountDownActionAfterTurnStatus();

        // wait until all cards have been drawn
        isAutoUseDelayedCard = true; // assume it can use delayed cards.
        float waitTime = 0.1f * 8 + 0.5f;
        GameCore.DelayCall(waitTime, TryUseDelayedCard);
    }

    public override void OnRoundEnd()
    {
        // TODO:
    }

    private void HandleOnActionAfterTurn(bool isClient, int roundNum, BattleCardData origCardData)
    {
        if (!isClient) return;

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
            if (battleDelayedCards[i].roundNum < leastRoundNum)
            {
                leastRoundNumIndex = i;
                leastRoundNum = battleDelayedCards[i].roundNum;
            }
        }
        if (leastRoundNumIndex == -1) return;

        int leastTurnNum = (leastRoundNum - curRoundNum) / 2;
        BattleManager.Instance.GetCurrentBattle().ReplaceActionAfterTurnStatus(true, leastTurnNum); // update the status
    }

    public bool GetIsAutoUseDelayedCard() { return isAutoUseDelayedCard; }

    private void HandleOnAfterUseCard()
    {
        if (!BattleManager.Instance.IsClientRound()) return;

        if (isAutoUseDelayedCard)
        {
            TryUseDelayedCard(); // try to CONTINUE to auto use delayed cards
        }
    }
}
