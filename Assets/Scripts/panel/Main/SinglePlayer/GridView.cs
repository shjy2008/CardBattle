/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2019 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using FancyScrollView;
using UnityEngine;

namespace Assets.Scripts.panel.Main.SinglePlayer
{
    public class GridView : FancyGridView<ItemData, Context>
    {
        [SerializeField] int columnCount = 1;
        [SerializeField] MainSinglePlayerCell cellPrefab = default;
        [SerializeField] Row rowPrefab = default;

        protected override int ColumnCount => columnCount;

        public override FancyScrollViewCell<ItemData, Context> CellTemplate => cellPrefab;

        protected override FancyGridViewRow<ItemData, Context> RowTemplate => rowPrefab;

        public float PaddingTop
        {
            get => paddingHead;
            set
            {
                paddingHead = value;
                Refresh();
            }
        }

        public float PaddingBottom
        {
            get => paddingTail;
            set
            {
                paddingTail = value;
                Refresh();
            }
        }

        public float SpacingY
        {
            get => spacing;
            set
            {
                spacing = value;
                Refresh();
            }
        }

        public float SpacingX
        {
            get => columnSpacing;
            set
            {
                columnSpacing = value;
                Refresh();
            }
        }

        public void UpdateSelection(int index)
        {
            if (Context.SelectedItemIndex == index)
            {
                return;
            }

            Context.SelectedItemIndex = index;
            Refresh();
        }
    }
}
