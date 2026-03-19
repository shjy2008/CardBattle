using System;
using System.Collections.Generic;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.BattlePanel;
using DG.Tweening;
using UnityEngine;

public class BattleSortCard
{
    static float maxGap = 320.0f * BattleCard.cardScale;
    const float minGap = 110.0f;

    public BattleSortCard()
    {
    }

    public static float GetGapWithCardCount(int cardCount)
    {
        if (cardCount >= 12)
        {
            return minGap;
        }
        float maxWidth = minGap * 11 + 300 * BattleCard.cardScale;
        float gap = (maxWidth - 300 * BattleCard.cardScale) / (cardCount - 1);
        if (gap > maxGap)
            gap = maxGap;

        return gap;
    }

    public static Vector3 GetCardLocalPos(int cardCount, float cardGap, int index)
    {
        float x = -cardGap * ((cardCount - 1) / 2.0f) + cardGap * index;
        return new Vector3(x, 0, 0);
    }

    public void Sort()
    {
        BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
        var cardList = battlePanel.GetUIComponent<BattleCardComponent>().CardList;

        int cardCount = cardList.Count;
        float cardGap = GetGapWithCardCount(cardCount);
        for (int i = 0; i < cardList.Count; ++i)
        {
            GameObject cardObj = cardList[i].Item1;

            Vector3 localPos = GetCardLocalPos(cardCount, cardGap, i);
            cardObj.transform.DOLocalMove(localPos, 0.2f);
            cardObj.GetComponent<BattleCard>().SetOriginalLocalPos(localPos);
        }


    }
}
