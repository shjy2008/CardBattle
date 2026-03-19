using System;
using System.Collections.Generic;
using Assets.Scripts.logics;
using Assets.Scripts.managers.uimgr;
using UnityEngine;

namespace Assets.Scripts.panel.BattlePanel
{
    public class BattleElementComponent : UIComponent
    {
        public static string GetElementNameById(int elementId)
        {
            switch (elementId)
            {
                case 1:
                    return BattleElementType.WATER.ToString();
                case 2:
                    return BattleElementType.WOOD.ToString();
                case 3:
                    return BattleElementType.FIRE.ToString();
                case 4:
                    return BattleElementType.EARTH.ToString();
                case 5:
                    return BattleElementType.METAL.ToString();
                default:
                    return "";
            }
        }

        public static int GetIdByElementName(string elementName)
        {
            if (Enum.TryParse<BattleElementType>(elementName, out var element))
            {
                return (int)element;
            }
            else
            {
                return 0;
            }
        }

        private List<int> elementIdList;
        private List<GameObject> elementObjList;

        public BattleElementComponent()
        {

        }

        public override void Init()
        {
            elementIdList = new List<int>();
            elementIdList.Add(1); // TODO: 要根据天气、地形来决定

            elementObjList = new List<GameObject>();
            for (int i = 0; i < 5; ++i)
            {
                BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
                Transform elementParent = battlePanel.transform.Find("bottom_right/element");
                elementObjList.Add(elementParent.Find(i.ToString()).gameObject);
            }

            UpdateUI();
        }

        public override void Update()
        {

        }

        public override void Destroy()
        {

        }

        private void UpdateUI()
        {
            for (int i = 0; i < 5; ++i)
            {
                if (i < elementIdList.Count)
                {
                    elementObjList[i].SetActive(true);
                    elementObjList[i].GetComponent<BattleElementMono>().SetElementId(elementIdList[i]);
                    elementObjList[i].GetComponent<BattleElementMono>().SetHighlighted(i == elementIdList.Count - 1);
                }
                else
                {
                    elementObjList[i].SetActive(false);
                }
            }
        }

        public int GetNextElementId()
        {
            int curElementId = elementIdList[elementIdList.Count - 1];
            int nextElementId = curElementId + 1;
            if (nextElementId > 5)
                nextElementId = 1;
            return nextElementId;
        }

        // if collect all 5 elements, return true
        public bool AddNextElement()
        {
            int nextElementId = GetNextElementId();
            elementIdList.Add(nextElementId);

            // collect all 5 elements
            bool isCollectAll5 = false;
            if (elementIdList.Count == 5)
            {
                int firstId = elementIdList[0];
                elementIdList = new List<int>() { firstId };
                isCollectAll5 = true;
            }

            UpdateUI();

            return isCollectAll5;
        }
    }
}
