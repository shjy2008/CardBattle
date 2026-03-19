using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Core.Events;
using System;
using Assets.Scripts.data;
using UnityEngine.UI;
using Assets.Scripts.managers.uimgr;
using FancyScrollView;

namespace Assets.Scripts.panel.Main.SinglePlayer
{
    public class MainSinglePlayerPanel : MonoBehaviour
    {
        [SerializeField] GridView gridView = default;
        private Scroller scroller;

        private float perCellPosition;

        // Use this for initialization
        void Start()
        {
            scroller = transform.Find("ScrollView").GetComponent<Scroller>();

            float maxPosition = gridView.DataCount - 1; // Don't know why but debug it and get this result

            float spacingX = gridView.SpacingY; // Gridview is originally for vertical mode. Since it's horizontal should use SpacingY
            float cellWidth = gridView.CellTemplate.GetComponent<RectTransform>().rect.width;
            float viewportWidth = transform.Find("ScrollView/Viewport").GetComponent<RectTransform>().rect.width;
            float totalWidth = gridView.DataCount * (cellWidth + spacingX) - spacingX;
            float moveWidth = totalWidth - viewportWidth;
            perCellPosition = maxPosition / moveWidth * (cellWidth + spacingX);
        }

        void Update()
        {
            Transform pageBtn = transform.Find("page_button");
            float maxPosition = gridView.DataCount - 1;
            pageBtn.Find("right").gameObject.SetActive((maxPosition - scroller.Position) > 0.1f);
            pageBtn.Find("left").gameObject.SetActive(scroller.Position > 0.1f);
        }

        private void OnDestroy()
        {

        }

        private void OnEnable()
        {
            UpdateScrollViewData();
        }

        public void UpdateScrollViewData()
        {
            List<ItemData> itemDataList = new List<ItemData>();

            for (int i = 0; i < 11; ++i)
            {
                itemDataList.Add(new ItemData(i));
            }

            //int i = 0;
            //foreach (int key in Table_tower.data.Keys)
            //{
            //    if (key <= TechniqueData.Instance.GetRebornLevel())
            //    {
            //        continue;
            //    }

            //    ItemData data = new ItemData(key);
            //    itemDataList.Add(data);
            //    ++i;

            //    if (key >= TowerData.Instance.GetUnlockedTowerId() + 1)
            //    {
            //        break;
            //    }
            //}

            gridView.UpdateContents(itemDataList.ToArray());
        }


        private void ScrollToIndex(int index)
        {
            scroller.ScrollTo(perCellPosition * index, 0.2f);
        }

        private int GetCurrentIndex()
        {
            Scroller scroller = transform.Find("ScrollView").GetComponent<Scroller>();
            int index = (int)(scroller.Position / perCellPosition + 0.5f);
            return index;
        }
        
        public void OnLeftPageBtnClk()
        {
            int index = GetCurrentIndex();
            Scroller scroller = transform.Find("ScrollView").GetComponent<Scroller>();
            ScrollToIndex(index - 1);
        }

        public void OnRightPageBtnClk()
        {
            ScrollToIndex(GetCurrentIndex() + 1);
        }
    }
}
