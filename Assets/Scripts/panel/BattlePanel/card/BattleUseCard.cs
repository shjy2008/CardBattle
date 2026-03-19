using System;
using System.Collections;
using Assets.Scripts.common;
using Assets.Scripts.logics;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.managers.prefabmgr;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.BattlePanel;
using Core.Events;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BattleUseCard
{
    private bool isClient;

    public BattleUseCard(bool _isClient)
    {
        isClient = _isClient;
    }

    public void UseCard(BattleCardData cardData, GameObject gameObject)
    {
        if (!isClient)
        {
            bool isSpecial = cardData.actionData.ifSpecial;
            if (isSpecial)
                BurnAction(gameObject);
            else
            {
                RotateFlyToPileAction(gameObject, isClient);

                cardData.minusCost = 0;
                BattleManager.Instance.GetCurrentBattle().GetBattleAIController().GetBattleCardPileData().AddCardData(cardData);
            }
        }
        else
        {
            bool isSpecial = cardData.actionData.ifSpecial;
            if (isSpecial)
                BurnAction(gameObject);
            else
            {
                RotateFlyToPileAction(gameObject, isClient);

                // Data back to pile
                cardData.minusCost = 0;
                BattlePanel.GetBattleCardComponent().GetBattleCardPileData().AddCardData(cardData);
            }

            // Element
            bool isNextElement = false;
            for (int i = 0; i < cardData.actionData.element.Count; ++i)
            {
                int elementId = BattleElementComponent.GetIdByElementName(cardData.actionData.element[i]);
                if (elementId == BattlePanel.GetBattleElementComponent().GetNextElementId())
                {
                    isNextElement = true;
                    break;
                }
            }
            bool isCollectAll5 = false;
            if (isNextElement)
            {
                isCollectAll5 = BattlePanel.GetBattleElementComponent().AddNextElement();
            }
            BattlePanel.GetBattleCardComponent().UpdateAllCardElements();

            BattlePanel.GetBattleCardComponent().UpdateAllCardDesc();

            //BattleFieldComponent battlefieldComponent = battlePanel.GetUIComponent<BattleFieldComponent>();
            //battlefieldComponent.SetBeforeAfterUseCardCallBack(BeforeUseCardCallback, AfterUseCardCallback);
            //battlefieldComponent.TryUseCard();
            //EventManager.Instance.SendEventSync<Action, Action>(
            //    Core.Events.EventType.UI_OnSetBeforeAfterUseCardCallBack,
            //    BeforeUseCardCallback, AfterUseCardCallback);
            EventManager.Instance.SendEventSync(Core.Events.EventType.UI_OnUseCard);
        }
    }

    //private void BeforeUseCardCallback(BattleCardData cardData)
    //{
    //    BattlePanel.GetBattleCardComponent().IsUsingCard = true;
    //}

    //private void AfterUseCardCallback(BattleCardData cardData)
    //{
    //    BattlePanel.GetBattleCardComponent().IsUsingCard = false;
    //}

    public static void BurnAction(GameObject gameObject)
    {
        Transform transform = gameObject.transform;
        // Frame renderer fade
        SpriteRenderer frameRenderer = transform.Find("frame").gameObject.GetComponent<SpriteRenderer>();
        frameRenderer.material.SetFloat("_Metallic", 0.0f);
        frameRenderer.material.SetFloat("_Smoothness", 0.0f);

        // Fade out
        transform.Find("cube").gameObject.SetActive(false);
        FadeChildren fade = transform.gameObject.AddComponent<FadeChildren>();
        fade.fadeDuration = 3.0f;
        fade.fadeFromOpacity = 1.0f;
        fade.fadeToOpacity = 0.0f;

        BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
        GameObject bgObj = transform.Find("mask/bg").gameObject;
        battlePanel.StartCoroutine(UIUtils.DelayedAction(0.0f, () =>
        {
            fade.RemoveRendererFromList(bgObj.GetComponent<SpriteRenderer>());
        }));

        battlePanel.StartCoroutine(UIUtils.DelayedAction(fade.fadeDuration, () =>
        {
            GameObject.Destroy(gameObject);
        }));

        Material dissolveMat = ResourceManager.Instance.LoadResource("ParticlePack/EffectExamples/Misc Effects/Materials/Dissolve", typeof(Material)) as Material;
        bgObj.GetComponent<SpriteRenderer>().material = dissolveMat;


        GameObject cardDissolve = PrefabManager.Instance.GetNewGameObject("CardDissolve", transform.Find("mask/bg").transform);
        cardDissolve.transform.localPosition = new Vector3(0, 0, -30);
        cardDissolve.transform.localScale = new Vector3(4, 4, 4);
        battlePanel.StartCoroutine(UIUtils.DelayedAction(5.0f, () =>
        {
            // 销毁预制体
            GameObject.Destroy(cardDissolve);
        }));

        SpawnEffect effect = bgObj.AddComponent<SpawnEffect>();
        effect.spawnEffectTime = 4;
        effect.pause = 3;
        effect.fadeIn = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    }

    public static void RotateFlyToPileAction(GameObject gameObject, bool _isClient)
    {
        Transform transform = gameObject.transform;
        BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
        Vector3 toPos;
        if (!_isClient)
        {
            toPos = battlePanel.transform.Find("left").position;
        }
        else
        {
            toPos = battlePanel.transform.Find("bottom_right/cards").position;
        }

        float rotateTime = 0.5f;
        float flyTime = 0.5f;
        // Rotate
        float angle = Vector3.Angle(transform.position, toPos);
        transform.DORotate(new Vector3(0, 0, angle + 360), rotateTime, RotateMode.FastBeyond360).SetRelative();

        // Scale
        transform.DOScale(new Vector3(0.1f, 0.1f, 0.1f), rotateTime + flyTime);

        battlePanel.StartCoroutine(UIUtils.DelayedAction(rotateTime, () =>
        {
            // Fly
            transform.DOMove(toPos, flyTime);

            // Fade out
            transform.Find("cube").gameObject.SetActive(false);
            FadeChildren fade = transform.gameObject.AddComponent<FadeChildren>();
            fade.fadeDuration = 0.5f;
            fade.fadeFromOpacity = 1.0f;
            fade.fadeToOpacity = 0.0f;

            battlePanel.StartCoroutine(UIUtils.DelayedAction(fade.fadeDuration, () =>
            {
                GameObject.Destroy(gameObject);

                GameObject cardFlyEffect = PrefabManager.Instance.GetNewGameObject("CardFlyToPile", battlePanel.transform);
                cardFlyEffect.transform.position = toPos;

                battlePanel.StartCoroutine(UIUtils.DelayedAction(1.0f, () =>
                {
                    GameObject.Destroy(cardFlyEffect);
                }));
            }));
        }));
    }
}
