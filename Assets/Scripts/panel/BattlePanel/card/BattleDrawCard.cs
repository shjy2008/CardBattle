using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.common;
using Assets.Scripts.logics;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.managers.prefabmgr;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.BattlePanel;
using Assets.Scripts.utility;
using DG.Tweening;
using UnityEngine;

public class BattleDrawCard
{
    private bool isClient;
    private int totalDrawCardCount;

    public BattleDrawCard(bool _isClient)
    {
        isClient = _isClient;
        totalDrawCardCount = 0;
    }

    public void DrawCard(int drawCount, BattleElementType chooseElementType = BattleElementType.NONE)
    {
        List<BattleCardData> pickedCardDatas = BattlePanel.GetBattleCardComponent().GetBattleCardPileData().SelectCardsByElements(drawCount, chooseElementType);

        BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
        int existingCardCount = battlePanel.GetUIComponent<BattleCardComponent>().CardList.Count;

        int newCardCount = pickedCardDatas.Count;
        float cardGap = BattleSortCard.GetGapWithCardCount(existingCardCount + newCardCount);

        Transform cardNodeTransform = battlePanel.transform.Find("CardParent");

        Camera uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();

        //string[] cardIdList = new string[] { "tactic_0001", "tactic_0002", "tactic_0003", "tactic_0004", "tactic_0005",
        //                                     "tactic_0006", "tactic_0007", "tactic_0008", "tactic_0009", "tactic_0010" };

        for (int i = 0; i < newCardCount; ++i)
        {
            int indexInHand = existingCardCount + i;

            //string path = string.Format("ui_prefab/card/Card{0}", i > 9 ? 9 : i);
            string path = "ui_prefab/card/Card";
            GameObject prefabObj = ResourceManager.Instance.LoadResource(path, typeof(GameObject)) as GameObject;
            GameObject cardObj = GameObject.Instantiate(prefabObj, cardNodeTransform);
            cardObj.GetComponent<BattleCard>().SetCardIndex(indexInHand);

            //BattleCardData cardData = new BattleCardData(string.Format("tactic_{0:0000}", i + 1));
            BattleCardData cardData = pickedCardDatas[i];
            cardObj.GetComponent<BattleCard>().SetCardData(cardData);

            cardObj.transform.localPosition = BattleSortCard.GetCardLocalPos(existingCardCount + newCardCount, cardGap, indexInHand);
            cardObj.GetComponent<BattleCard>().SetOriginalLocalPos(cardObj.transform.localPosition);
            Vector3 toPos = cardObj.transform.position;

            Vector3 fromPos = battlePanel.transform.Find("bottom_right/cards").position;
            cardObj.transform.position = fromPos;

            // Set sortingOrder
            int addSortingOrder = 10 * totalDrawCardCount;
            ++totalDrawCardCount;
            Utils.AddChildrenSortingOrder(cardObj.transform, addSortingOrder);

            battlePanel.GetUIComponent<BattleCardComponent>().DrawCard(cardObj, cardData);

            // Fly
            cardObj.SetActive(false);
            cardObj.GetComponent<BattleCard>().SetIsFlying(true);
            battlePanel.StartCoroutine(UIUtils.DelayedAction(i * 0.1f, () =>
            {
                cardObj.SetActive(true);
                FlyFromPile(cardObj, toPos);
            }));

        }
    }

    private void FlyFromPile(GameObject cardObj, Vector3 toPos)
    {
        float flyTime = 0.5f;

        cardObj.transform.DOMove(toPos, flyTime);

        cardObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        float cardScale = cardObj.GetComponent<BattleCard>().GetCardScale();
        cardObj.transform.DOScale(new Vector3(cardScale, cardScale, cardScale), flyTime);

        FadeChildren fade = cardObj.AddComponent<FadeChildren>();
        fade.fadeDuration = flyTime;
        fade.fadeFromOpacity = 0.0f;
        fade.fadeToOpacity = 1.0f;

        BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
        battlePanel.StartCoroutine(UIUtils.DelayedAction(flyTime, () =>
        {
            cardObj.GetComponent<BattleCard>().SetIsFlying(false);
        }));
    }
}
