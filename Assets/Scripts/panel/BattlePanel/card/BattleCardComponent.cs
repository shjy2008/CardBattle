using System;
using System.Collections.Generic;
using Assets.Scripts.common;
using Assets.Scripts.logics;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.managers.prefabmgr;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.utility;
using Core.Events;
using UnityEngine;

namespace Assets.Scripts.panel.BattlePanel
{
    public class BattleCardComponent : UIComponent
    {
        private List<Tuple<GameObject, BattleCardData>> cardList = new List<Tuple<GameObject, BattleCardData>>();
        public List<Tuple<GameObject, BattleCardData>> CardList
        {
            get { return cardList; }
        }

        private GameObject currMovingCard = null;
        public GameObject CurrentMovingCard
        {
            get { return currMovingCard; }
            set { currMovingCard = value; }
        }

        private bool isUsingCard = false;
        public bool IsUsingCard
        {
            get { return isUsingCard; }
            set { isUsingCard = value; }
        }

        private BattleDrawCard battleDrawCard;
        private BattleCardFlyBack battleCardFlyBack;
        private BattleSortCard battleSortCard;
        private BattleUseCard battleUseCard;
        private BattleCardPileData battleCardPileData;
        private BattleCardCost battleCardCost;
        private BattleCardHistory battleCardHistory;

        public BattleCardComponent()
        {

        }

        public override void Init()
        {
            battleDrawCard = new BattleDrawCard(true);

            GameCore.DelayCall(0.5f, () =>
            {
                battleDrawCard.DrawCard(8);
            });

            battleCardFlyBack = new BattleCardFlyBack();

            battleSortCard = new BattleSortCard();

            battleUseCard = new BattleUseCard(true);

            battleCardPileData = new BattleCardPileData(true);

            battleCardCost = new BattleCardCost();

            battleCardHistory = new BattleCardHistory();

            EventManager.Instance.RegisterEventHandler<Action>(Core.Events.EventType.UI_OnBeforeUseCard, HandleOnBeforeUseCard);
            EventManager.Instance.RegisterEventHandler<Action>(Core.Events.EventType.UI_OnAfterUseCard, HandleOnAfterUseCard);

            EventManager.Instance.RegisterEventHandler<Action<bool, int>>(Core.Events.EventType.Battle_OnUpdateActionPoint, HandleOnUpdateActionPoint);
            EventManager.Instance.RegisterEventHandler<Action<bool, int, BattleActionEffectType>>(Core.Events.EventType.Battle_OnDrawCard, HandleOnDrawCard);
            EventManager.Instance.RegisterEventHandler<Action<bool, int>>(Core.Events.EventType.Battle_OnBurnCard, HandleOnBurnCard);
            EventManager.Instance.RegisterEventHandler<Action<bool, int>>(Core.Events.EventType.Battle_OnDiscardCard, HandleOnDiscardCard);
            EventManager.Instance.RegisterEventHandler<Action<bool, int, BattleActionEffectType>>(Core.Events.EventType.Battle_OnReduceCardCost, HandleOnReduceCardCost);
            EventManager.Instance.RegisterEventHandler<Action<bool>>(Core.Events.EventType.Battle_OnInheritAllActionPoint, HandleOnInheritAllActionPoint);



        }

        public override void Update()
        {

        }

        public override void Destroy()
        {
            EventManager.Instance.UnRegisterEventHandler<Action>(Core.Events.EventType.UI_OnBeforeUseCard, HandleOnBeforeUseCard);
            EventManager.Instance.UnRegisterEventHandler<Action>(Core.Events.EventType.UI_OnAfterUseCard, HandleOnAfterUseCard);

            EventManager.Instance.UnRegisterEventHandler<Action<bool, int>>(Core.Events.EventType.Battle_OnUpdateActionPoint, HandleOnUpdateActionPoint);
            EventManager.Instance.UnRegisterEventHandler<Action<bool, int, BattleActionEffectType>>(Core.Events.EventType.Battle_OnDrawCard, HandleOnDrawCard);
            EventManager.Instance.UnRegisterEventHandler<Action<bool, int>>(Core.Events.EventType.Battle_OnBurnCard, HandleOnBurnCard);
            EventManager.Instance.UnRegisterEventHandler<Action<bool, int>>(Core.Events.EventType.Battle_OnDiscardCard, HandleOnDiscardCard);
            EventManager.Instance.UnRegisterEventHandler<Action<bool, int, BattleActionEffectType>>(Core.Events.EventType.Battle_OnReduceCardCost, HandleOnReduceCardCost);
            EventManager.Instance.UnRegisterEventHandler<Action<bool>>(Core.Events.EventType.Battle_OnInheritAllActionPoint, HandleOnInheritAllActionPoint);
        }

        public int GetHandCardNumberByElement(BattleElementType selectElementType = BattleElementType.NONE)
        {
            int count = 0;
            foreach (var card in cardList)
            {
                List<string> cardElements = card.Item2.actionData.element;
                foreach (string elementStr in cardElements)
                {
                    BattleElementType elementType = Utils.ConvertStrToEnum<BattleElementType>(elementStr);
                    if (elementType == selectElementType)
                    {
                        ++count;
                    }
                }
            }
            return count;
        }

        public int GetUsedCardNumberByElement(BattleElementType selectElementType = BattleElementType.NONE)
        {
            int count = 0;
            var usedCardDatas = battleCardHistory.GetCurRoundUsedCardList();
            foreach (var card in usedCardDatas)
            {
                List<string> cardElements = card.actionData.element;
                foreach (string elementStr in cardElements)
                {
                    BattleElementType elementType = Utils.ConvertStrToEnum<BattleElementType>(elementStr);
                    if (elementType == selectElementType)
                    {
                        ++count;
                    }
                }
            }
            return count;
        }

        public void DrawCard(GameObject cardObj, BattleCardData cardData)
        {
            cardList.Add(new(cardObj, cardData));
            battleCardHistory.AddDrawnCard(cardData);
        }

        public void RemoveCard(GameObject cardObj)
        {
            foreach (var card in cardList)
            {
                if (card.Item1 == cardObj)
                {
                    cardList.Remove(card);
                    break;
                }
            }
        }

        public void AllCardFlyBack()
        {
            battleCardFlyBack.FlyBack(CardList);
            cardList.Clear();
        }

        public void SortCards()
        {
            battleSortCard.Sort();
        }

        public void UseCard(BattleCardData cardData, GameObject gameObject)
        {
            battleCardCost.UseCardConsumeCost(cardData);

            RemoveCard(gameObject);

            SortCards();

            battleUseCard.UseCard(cardData, gameObject);
            battleCardHistory.AddUsedCard(cardData);
        }

        public void UpdateAllCardElements()
        {
            foreach (var card in cardList)
            {
                card.Item1.GetComponent<BattleCard>().UpdateElement();
            }
        }

        public void UpdateAllCardDesc()
        {
            foreach (var card in cardList)
            {
                card.Item1.GetComponent<BattleCard>().UpdateDesc();
            }
        }

        private void HandleOnBeforeUseCard()
        {
            if (BattleManager.Instance.IsClientRound())
                isUsingCard = true;
        }

        private void HandleOnAfterUseCard()
        {
            if (BattleManager.Instance.IsClientRound())
                isUsingCard = false;
        }

        private void HandleOnUpdateActionPoint(bool isClient, int pointDiff)
        {
            if (isClient)
            {
                battleCardCost.UpdateCost(pointDiff);
            }
        }

        private void HandleOnDrawCard(bool isClient, int count, BattleActionEffectType effectType)
        {
            if (isClient)
            {
                GameCore.DelayCall(1.0f, () =>
                {
                    BattleElementType elementType = GetElementTypeWithEffectType(effectType);
                    battleDrawCard.DrawCard(count, elementType);
                    SortCards();
                });
            }
        }

        public static BattleElementType GetElementTypeWithEffectType(BattleActionEffectType effectType)
        {
            List<BattleActionEffectType> drawNowList = new List<BattleActionEffectType>() {
                    BattleActionEffectType.DRAW_METAL_CARD, BattleActionEffectType.DRAW_WOOD_CARD,BattleActionEffectType.DRAW_WATER_CARD,
                    BattleActionEffectType.DRAW_FIRE_CARD,BattleActionEffectType.DRAW_EARTH_CARD,BattleActionEffectType.DRAW_CARD};

            List<BattleActionEffectType> drawNextRoundList = new List<BattleActionEffectType>() {
                    BattleActionEffectType.DRAW_METAL_CARD_NEXT_TURN, BattleActionEffectType.DRAW_WOOD_CARD_NEXT_TURN,BattleActionEffectType.DRAW_WATER_CARD_NEXT_TURN,
                    BattleActionEffectType.DRAW_FIRE_CARD_NEXT_TURN,BattleActionEffectType.DRAW_EARTH_CARD_NEXT_TURN,BattleActionEffectType.DRAW_CARD_NEXT_TURN};

            int index = -1;
            if (drawNowList.Contains(effectType))
            {
                index = drawNowList.IndexOf(effectType);
            }
            else if (drawNextRoundList.Contains(effectType))
            {
                index = drawNowList.IndexOf(effectType);
            }

            BattleElementType elementType = BattleElementType.NONE;
            if (index >= 0 && index <= 4)
            {
                switch (index)
                {
                    case 0: elementType = BattleElementType.METAL; break;
                    case 1: elementType = BattleElementType.WOOD; break;
                    case 2: elementType = BattleElementType.WATER; break;
                    case 3: elementType = BattleElementType.FIRE; break;
                    case 4: elementType = BattleElementType.EARTH; break;
                    default: break;
                }
            }
            return elementType;
        }

        private void HandleOnBurnCard(bool isClient, int count)
        {
            var chosenCardList = RandomUtils.ListChoice(cardList, count);
            foreach (var card in chosenCardList)
            {
                BattleUseCard.BurnAction(card.Item1);
            }

            var newCardList = new List<Tuple<GameObject, BattleCardData>>();
            foreach (var card in cardList)
            {
                if (!chosenCardList.Contains(card))
                {
                    newCardList.Add(card);
                }
            }
            cardList = newCardList;

            GameCore.DelayCall(2.0f, () =>
            {
                battleSortCard.Sort();
            });
        }

        private void HandleOnDiscardCard(bool isClient, int count)
        {
            var chosenCardList = RandomUtils.ListChoice(cardList, count);
            foreach (var card in chosenCardList)
            {
                BattleUseCard.RotateFlyToPileAction(card.Item1, true);
            }

            var newCardList = new List<Tuple<GameObject, BattleCardData>>();
            foreach (var card in cardList)
            {
                if (!chosenCardList.Contains(card))
                {
                    newCardList.Add(card);
                }
            }
            cardList = newCardList;

            battleSortCard.Sort();
        }

        private void HandleOnReduceCardCost(bool isClient, int count, BattleActionEffectType effectType)
        {
            int costToMinus = 0;
            if (effectType == BattleActionEffectType.REDUCE_COST_BY_1)
                costToMinus = 1;
            else if (effectType == BattleActionEffectType.REDUCE_COST_BY_2)
                costToMinus = 2;
            else if (effectType == BattleActionEffectType.REDUCE_COST_BY_3)
                costToMinus = 3;

            List<BattleCardData> drawnCardList = battleCardHistory.GetCurRoundDrawnCardList();
            List<Tuple<GameObject, BattleCardData>> affectedCardList = new List<Tuple<GameObject, BattleCardData>>();
            for (int i = 0; i < drawnCardList.Count; ++i)
            {
                var drawnCardData = drawnCardList[drawnCardList.Count - 1 - i];
                bool isInHand = false;
                Tuple<GameObject, BattleCardData> handCardTuple = null;
                foreach (var handCard in cardList)
                {
                    if (drawnCardData == handCard.Item2)
                    {
                        isInHand = true;
                        handCardTuple = handCard;
                        break;
                    }
                }

                if (isInHand)
                    affectedCardList.Add(handCardTuple);

                if (affectedCardList.Count >= count)
                    break;
            }

            foreach (var affectedCard in affectedCardList)
            {
                affectedCard.Item1.GetComponent<BattleCard>().GetCardData().minusCost += costToMinus;
                affectedCard.Item1.GetComponent<BattleCard>().UpdateCost();
            }
        }

        private void HandleOnInheritAllActionPoint(bool isClient)
        {
            if (isClient)
            {
                battleCardCost.UpdateCost(battleCardCost.GetLastRoundRemainingCost());

            }
        }

        public BattleCardPileData GetBattleCardPileData()
        {
            return battleCardPileData;
        }

        public BattleCardCost GetBattleCardCost()
        {
            return battleCardCost;
        }

        public BattleDrawCard GetBattleDrawCard()
        {
            return battleDrawCard;
        }

        public BattleCardHistory GetBattleCardHistory()
        {
            return battleCardHistory;
        }
    }
}
