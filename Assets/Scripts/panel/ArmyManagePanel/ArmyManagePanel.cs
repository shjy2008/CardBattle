using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.common;
using Assets.Scripts.data;
using Assets.Scripts.logics;
using Assets.Scripts.managers.archivemgr;
using Assets.Scripts.managers.inputmgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.common;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.panel.ArmyManage
{
    public class ArmyManagePanel : BaseUI
    {
        private bool pressingShift = false;

        private GameObject currMovingArmy = null;
        public GameObject CurrMovingArmy
        {
            get { return currMovingArmy; }
            set { currMovingArmy = value; }
        }

        private GameObject currMovingHero = null;
        public GameObject CurrMovingHero
        {
            get { return currMovingHero; }
            set { currMovingHero = value; }
        }

        private Dictionary<int, BattleGroupData> indexToBattleGroupData; // If BattleGroupData == null, then no army
        private Dictionary<int, int> indexToHeroId; // HeroId: 1-5. If HeroId == 0, then no hero

        public ArmyManagePanel()
        {

        }

        // Use this for initialization
        protected override void Start()
        {
            transform.Find("bg/army/army").gameObject.SetActive(true);
            transform.Find("bg/army/hero").gameObject.SetActive(false);
            indexToBattleGroupData = new Dictionary<int, BattleGroupData>(ArchiveManager.Instance.GetCurrentArchiveData().playerData.ArmyData.IndexToBattleGroupData);
            indexToHeroId = new Dictionary<int, int>(ArchiveManager.Instance.GetCurrentArchiveData().playerData.ArmyData.IndexToHeroId);
            UpdateArmy();
            UpdateHero();
        }


        // Update is called once per frame
        void Update()
        {
            GameObject unitCardPanelObj = UIManager.Instance.GetOpenUI("UnitCardPanel");
            if (!unitCardPanelObj)
            {
                bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                // Check if the Shift key is pressed
                if (shiftPressed && !pressingShift)
                {
                    transform.Find("bg/army/army").gameObject.SetActive(false);
                    transform.Find("bg/army/hero").gameObject.SetActive(true);
                    UIManager.Instance.CloseAndDestroyUI("ArmySoldierListPop");
                    UIManager.Instance.CloseAndDestroyUI("UnitCardPanel");

                    if (currMovingArmy)
                        currMovingArmy.GetComponent<ArmyManagePanelArmy>().OnPointerUp(null);
                    if (currMovingHero)
                        currMovingHero.GetComponent<ArmyManagePanelHero>().OnPointerUp(null);
                }
                if (!shiftPressed && pressingShift)
                {
                    transform.Find("bg/army/army").gameObject.SetActive(true);
                    transform.Find("bg/army/hero").gameObject.SetActive(false);
                    UIManager.Instance.CloseAndDestroyUI("ArmyHeroInfoPop");
                }

                pressingShift = shiftPressed;
            }

            UpdateHighlight();
        }

        public void OnBtnConfirmClk()
        {
            if (!CheckEveryArmyIsLedByHero())
            {
                HintBox.ShowHintBox("Tips", "每个军团都需要由英雄率领", 1);
                return;
            }

            // Save
            ArchiveManager.Instance.GetCurrentArchiveData().playerData.ArmyData.IndexToBattleGroupData = indexToBattleGroupData;
            ArchiveManager.Instance.GetCurrentArchiveData().playerData.ArmyData.IndexToHeroId = indexToHeroId;

            CloseAndDestroy();
        }

        private bool CheckEveryArmyIsLedByHero()
        {
            for (int i = 0; i < ArmyData.HeroCount; ++i)
            {
                int heroId = indexToHeroId[i];
                if (heroId == 0)
                {
                    List<int> armyIndexList = ArmyData.GetArmyIndexListByHeroIndex(i);
                    foreach (int armyIndex in armyIndexList)
                    {
                        BattleGroupData data = indexToBattleGroupData[armyIndex];
                        if (data != null)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void UpdateArmy()
        {
            Transform armyParent = transform.Find("bg/army/army");
            for (int i = 0; i < ArmyData.ArmyCount; ++i)
            {
                Transform child = armyParent.Find(i.ToString());

                GameObject army_group = child.Find("army_group").gameObject;
                if (indexToBattleGroupData[i] == null)
                {
                    army_group.SetActive(false);
                }
                else
                {
                    string path = AssetsMapper.GetGroupIcon(indexToBattleGroupData[i], true, false);
                    army_group.SetActive(true);
                    army_group.GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
                }
            }
        }

        private void UpdateHero()
        {
            Transform heroParent = transform.Find("bg/army/hero");
            for (int i = 0; i < ArmyData.HeroCount; ++i)
            {
                Transform child = heroParent.Find(i.ToString());

                string path = ArmyData.GetImagePathWithHeroId(indexToHeroId[i]);
                GameObject imageObj = child.Find("image").gameObject;
                if (path == "")
                {
                    imageObj.SetActive(false);
                }
                else
                {
                    imageObj.SetActive(true);
                    imageObj.GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
                }
            }
        }

        private void UpdateHighlight()
        {
            if (currMovingArmy != null)
            {
                int currMovingArmyIndex = currMovingArmy.GetComponent<ArmyManagePanelArmy>().GetArmyIndex();
                for (int i = 0; i < ArmyData.ArmyCount; ++i)
                {
                    if (i != currMovingArmyIndex)
                    {
                        Transform armyParent = transform.Find("bg/army/army");
                        Transform child = armyParent.Find(i.ToString());
                        Transform highlight = child.transform.Find("highlight");
                        if (UIUtils.PositionInTransform(InputHelper.GetTouchPos() / Const.GetResolutionRatio(), child))
                        {
                            if (highlight == null)
                            {
                                GameObject highlightObj = new GameObject("highlight");
                                RectTransform rectTransform = highlightObj.AddComponent<RectTransform>();

                                Image imageComp = highlightObj.AddComponent<Image>();
                                Sprite sprite = Resources.Load<Sprite>("graphics/UI/ui_window_border/UI_window_border_3");
                                imageComp.sprite = sprite;
                                imageComp.type = Image.Type.Sliced;

                                highlightObj.transform.SetParent(child);
                                highlightObj.transform.SetAsFirstSibling();

                                rectTransform.anchorMin = Vector2.zero;
                                rectTransform.anchorMax = Vector2.one;
                                rectTransform.offsetMin = Vector2.zero;
                                rectTransform.offsetMax = Vector2.zero;
                                rectTransform.localScale = Vector3.one;
                                rectTransform.localPosition = Vector3.zero;
                            }
                        }
                        else
                        {
                            if (highlight != null)
                            {
                                GameObject.Destroy(highlight.gameObject);
                            }
                        }
                    }
                }
            }

            if (currMovingHero != null)
            {
                int currMovingHeroIndex = currMovingHero.GetComponent<ArmyManagePanelHero>().GetHeroIndex();
                for (int i = 0; i < ArmyData.HeroCount; ++i)
                {
                    if (i != currMovingHeroIndex)
                    {
                        Transform armyParent = transform.Find("bg/army/hero");
                        Transform child = armyParent.Find(i.ToString());
                        Transform highlight = child.transform.Find("highlight");
                        Transform highlight_half = child.transform.Find("highlight_half");

                        if (UIUtils.PositionInTransform(InputHelper.GetTouchPos() / Const.GetResolutionRatio(), child))
                        {
                            List<int> fromArmyIndexList = ArmyData.GetArmyIndexListByHeroIndex(currMovingHeroIndex);
                            int toHeroIndex = int.Parse(child.name);
                            List<int> toArmyIndexList = ArmyData.GetArmyIndexListByHeroIndex(toHeroIndex);
                            if (fromArmyIndexList.Count == toArmyIndexList.Count || toArmyIndexList.Count == 3)
                            {
                                highlight.gameObject.SetActive(true);
                            }
                            else
                            {
                                highlight_half.gameObject.SetActive(true);
                            }
                        }
                        else
                        {
                            highlight.gameObject.SetActive(false);
                            if (highlight_half)
                            {
                                highlight_half.gameObject.SetActive(false);
                            }
                        }
                    }

                }
            }
        }

        public void finishMovingArmy(int fromArmyIndex, Vector2 pos)
        {
            bool found = false;
            int toIndex = 0;
            for (int i = 0; i < ArmyData.ArmyCount; ++i)
            {
                if (i != fromArmyIndex)
                {
                    Transform armyParent = transform.Find("bg/army/army");
                    Transform child = armyParent.Find(i.ToString());
                    if (UIUtils.PositionInTransform(pos, child))
                    {
                        // Remove highlight
                        Transform highlight = child.transform.Find("highlight");
                        if (highlight != null)
                        {
                            GameObject.Destroy(highlight.gameObject);
                        }

                        toIndex = i;
                        found = true;
                        break;
                    }
                }

            }
            if (found)
            {
                BattleGroupData tempData = indexToBattleGroupData[fromArmyIndex];
                indexToBattleGroupData[fromArmyIndex] = indexToBattleGroupData[toIndex];
                indexToBattleGroupData[toIndex] = tempData;
                UpdateArmy();
            }
        }

        public void finishMovingHero(int fromHeroIndex, Vector2 pos)
        {
            bool found = false;
            int toHeroIndex = 0;
            for (int i = 0; i < ArmyData.HeroCount; ++i)
            {
                if (i != fromHeroIndex)
                {
                    Transform heroParent = transform.Find("bg/army/hero");
                    Transform child = heroParent.Find(i.ToString());
                    if (UIUtils.PositionInTransform(pos, child))
                    {
                        // Remove highlight
                        Transform highlight = child.transform.Find("highlight");
                        Transform highlight_half = child.transform.Find("highlight_half");
                        highlight.gameObject.SetActive(false);
                        if (highlight_half)
                        {
                            highlight_half.gameObject.SetActive(false);
                        }

                        toHeroIndex = i;
                        found = true;
                        break;
                    }
                }
            }
            if (found)
            {
                int tempHeroId = indexToHeroId[fromHeroIndex];
                indexToHeroId[fromHeroIndex] = indexToHeroId[toHeroIndex];
                indexToHeroId[toHeroIndex] = tempHeroId;
                UpdateHero();

                // Exchange armys belong to heros
                List<int> fromArmyIndexList = ArmyData.GetArmyIndexListByHeroIndex(fromHeroIndex);
                List<int> toArmyIndexList = ArmyData.GetArmyIndexListByHeroIndex(toHeroIndex);
                int exchangeCount = 3;
                if (fromArmyIndexList.Count == 6 && toArmyIndexList.Count == 6)
                {
                    exchangeCount = 6;
                }
                for (int i = 0; i < exchangeCount; ++i)
                {
                    BattleGroupData tempData = indexToBattleGroupData[fromArmyIndexList[i]];
                    indexToBattleGroupData[fromArmyIndexList[i]] = indexToBattleGroupData[toArmyIndexList[i]];
                    indexToBattleGroupData[toArmyIndexList[i]] = tempData;
                }
                UpdateArmy();
            }
        }



        // Hover show UI
        public void OnHoverCoin(HoverToShowUI hoverToShowUI)
        {
            GameObject uiObj = UIManager.Instance.OpenUI("ArmyCoinPop");
            uiObj.transform.localPosition = hoverToShowUI.GetTopRightPos();
        }

        public void OnHoverOutCoin(HoverToShowUI hoverToShowUI)
        {
            UIManager.Instance.CloseAndDestroyUI("ArmyCoinPop");
        }

        public void OnHoverArmyEffect(HoverToShowUI hoverToShowUI)
        {
            GameObject uiObj = UIManager.Instance.OpenUI("ArmyEffectPop");
            uiObj.transform.localPosition = hoverToShowUI.GetTopRightPos();
        }

        public void OnHoverOutArmyEffect(HoverToShowUI hoverToShowUI)
        {
            UIManager.Instance.CloseAndDestroyUI("ArmyEffectPop");
        }

        public void OnHoverArmyItem(HoverToShowUI hoverToShowUI)
        {
            if (currMovingArmy != null)
            {
                return;
            }
            int index = int.Parse(hoverToShowUI.gameObject.name);
            BattleGroupData battleGroupData = indexToBattleGroupData[index];
            Dictionary<string, int> unitsDict = battleGroupData.GetUnitsDictCopyBeforeLoss();
            //string[] unitList = new string[unitsDict.Count];
            //int i = 0;
            //foreach (KeyValuePair<string, int> kv in unitsDict)
            //{
            //    unitList[i] = kv.Key;
            //    ++i;
            //}

            GameObject uiObj = UIManager.Instance.OpenUI("ArmySoldierListPop");
            uiObj.GetComponent<ArmySoldierListPop>().SetUnitsDict(unitsDict);
            uiObj.transform.localPosition = hoverToShowUI.GetTopRightPos();
        }

        public void OnHoverOutArmyItem(HoverToShowUI hoverToShowUI)
        {
            UIManager.Instance.CloseAndDestroyUI("ArmySoldierListPop");
        }

        public void OnHoverHeroItem(HoverToShowUI hoverToShowUI)
        {
            if (currMovingHero != null)
            {
                return;
            }
            GameObject uiObj = UIManager.Instance.OpenUI("ArmyHeroInfoPop");
            uiObj.transform.localPosition = hoverToShowUI.GetTopRightPos();
        }

        public void OnHoverOutHeroItem(HoverToShowUI hoverToShowUI)
        {
            UIManager.Instance.CloseAndDestroyUI("ArmyHeroInfoPop");
        }
    }
}
