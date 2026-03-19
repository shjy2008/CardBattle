using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.common;
using Assets.Scripts.logics;
using Assets.Scripts.managers.prefabmgr;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.BattlePanel;
using Assets.Scripts.utility;
using DG.Tweening;
using UnityEngine;

public class BattleCardFlyBack
{
    public BattleCardFlyBack()
    {
    }

    public void FlyBack(List<Tuple<GameObject, BattleCardData>> cardList)
    {
        BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();

        for (int i = 0; i < cardList.Count; ++i)
        {
            GameObject cardObj = cardList[i].Item1;
            Vector3 toPos = battlePanel.transform.Find("bottom_right/cards").position;

            // Fly
            cardObj.GetComponent<BattleCard>().SetIsFlying(true);
            battlePanel.StartCoroutine(UIUtils.DelayedAction(i * 0.1f, () =>
            {
                FlyToPileAction(cardObj, toPos);
            }));

            // Data back to pile
            BattlePanel.GetBattleCardComponent().GetBattleCardPileData().AddCardData(cardList[i].Item2);
        }
    }

    private void FlyToPileAction(GameObject cardObj, Vector3 toPos)
    {
        float flyTime = 0.5f;

        cardObj.transform.DOMove(toPos, flyTime);
        cardObj.transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), flyTime);

        FadeChildren fade = cardObj.AddComponent<FadeChildren>();
        fade.fadeDuration = flyTime;
        fade.fadeFromOpacity = 1.0f;
        fade.fadeToOpacity = 0.0f;

        BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
        battlePanel.StartCoroutine(UIUtils.DelayedAction(flyTime, () =>
        {
            GameObject.Destroy(cardObj);
        }));
    }
}
