using UnityEngine;
using System.Collections;
using System;
using Assets.Scripts.managers.uimgr;
using UnityEngine;
using Assets.Scripts.common;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Assets.Scripts.panel.ArmyManage
{
    public class ArmySoldierListPop : BaseUI
    {
        private string[] unitKeyList;// = new string[] { "unit_0001", "unit_0002" };

        public void SetUnitsDict(Dictionary<string, int> unitsDict)
        {
            // Sort by largest number to smallest number
            List<Tuple<string, int>> unitsPairList = new List<Tuple<string, int>>();
            foreach (KeyValuePair<string, int> kv in unitsDict)
            {
                unitsPairList.Add(new Tuple<string, int>(kv.Key, kv.Value));
            }
            unitsPairList.Sort((x, y) => y.Item2.CompareTo(x.Item2));

            unitKeyList = new string[unitsPairList.Count];
            for (int i = 0; i < unitsPairList.Count; ++i)
            {
                unitKeyList[i] = unitsPairList[i].Item1;
            }

            GameObject obj0 = transform.Find("bg/0").gameObject;
            for (int i = 0; i < unitKeyList.Length; ++i)
            {
                string key = unitKeyList[i];
                Table_unit.Data data = Table_unit.data[key];
                GameObject obj;
                if (i == 0)
                {
                    obj = obj0;
                }
                else
                {
                    obj = GameObject.Instantiate<GameObject>(obj0, obj0.transform.parent);
                    obj.name = i.ToString();
                    obj.transform.localPosition = new Vector3(obj0.transform.localPosition.x + i * 50, obj0.transform.localPosition.y, obj0.transform.localPosition.z);
                    obj.transform.localScale = obj0.transform.localScale;
                }
                obj.transform.Find("icon").GetComponent<Image>().sprite = Resources.Load<Sprite>("images/unit/unit_type/" + data.group);

                Transform imgTransform = obj.transform.Find("mask/img");
                imgTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>("images/unit/unit_bg/" + data.faceimage);
                float imgWidth = imgTransform.GetComponent<RectTransform>().rect.width;
                float imgHeight = imgTransform.GetComponent<RectTransform>().rect.height;
                float itemWidth = obj0.transform.GetComponent<RectTransform>().rect.width;
                float itemHeight = obj0.transform.GetComponent<RectTransform>().rect.height;
                float x = -(imgWidth * data.faceCenter[0] - itemWidth / 2);
                float y = -(imgHeight * data.faceCenter[1] - itemHeight * 0.8f);
                if (y < -(imgHeight - itemHeight))
                {
                    y = -(imgHeight - itemHeight);
                }
                imgTransform.localPosition = new Vector3(x - itemWidth / 2, y - itemHeight / 2, 0);
            }
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnHoverSoldierItem(HoverToShowUI hoverToShowUI)
        {
            int index = int.Parse(hoverToShowUI.gameObject.name);
            string unitId = unitKeyList[index];

            UIManager.Instance.CloseAndDestroyUI("UnitCardPanel");
            GameObject uiObj = UIManager.Instance.OpenUI("UnitCardPanel");
            uiObj.GetComponent<UnitCardPanel>().SetUnitId(unitId);

            Vector2 topRightPos = hoverToShowUI.GetTopRightPos();

            RectTransform rectTransform = uiObj.transform.GetComponent<RectTransform>();
            float x = rectTransform.rect.width * rectTransform.pivot.x - 10; // Because we set z to -30, x should be closer to the cursor
            float y = rectTransform.rect.height * (1 - rectTransform.pivot.y);
            uiObj.transform.localPosition = new Vector3(topRightPos.x + x, topRightPos.y - y, uiObj.transform.localPosition.z);

            HoverOutCloseUI.AddChildToParent(uiObj, gameObject);
            hoverToShowUI.AddChildrenUI(uiObj.GetComponent<UnitCardPanel>());
        }

        public void OnHoverOutSoldierItem(HoverToShowUI hoverToShowUI)
        {
            GameObject uiObj = UIManager.Instance.GetUI("UnitCardPanel");
            HoverOutCloseUI.RemoveChildFromParent(uiObj, gameObject);
            if (hoverToShowUI && uiObj)
                hoverToShowUI.RemoveChildrenUI(uiObj.GetComponent<UnitCardPanel>());

            UIManager.Instance.CloseAndDestroyUI("UnitCardPanel");

        }
    }
}
