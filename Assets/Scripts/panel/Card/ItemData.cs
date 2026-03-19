/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2019 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using Assets.Scripts.logics;

namespace Assets.Scripts.panel.Card
{
    public class ItemData
    {
        public BattleCardData cardData { get; }

        public ItemData(BattleCardData _cardData) => cardData = _cardData;
    }
}
