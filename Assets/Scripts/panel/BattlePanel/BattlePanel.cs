using System;
using System.Collections.Generic;
using Assets.Scripts.common;
using Assets.Scripts.data;
using Assets.Scripts.logics;
using Assets.Scripts.managers.archivemgr;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.panel.Card;
using Core.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.panel.BattlePanel
{
    public class BattlePanel : BaseUI
    {
        private BattleCardComponent cardComponent;
        private BattleFieldComponent battleFieldComponent;
        private BattleElementComponent elementComponent;

        private string landscapeId;
        private string weatherId;

        public int LevelIndexInRow;

        // Use this for initialization
        protected override void Start()
        {
            cardComponent = new BattleCardComponent();
            AddUIComponent(cardComponent);

            battleFieldComponent = new BattleFieldComponent(transform.Find("BattleField").gameObject);
            AddUIComponent(battleFieldComponent);

            elementComponent = new BattleElementComponent();
            AddUIComponent(elementComponent);

            UpdateWeatherId();

            EventManager.Instance.RegisterEventHandler<Action<BattleResult>>(Core.Events.EventType.Battle_OnFinish, HandleOnFinishBattle);
        }

        public override void CloseAndDestroy()
        {
            base.CloseAndDestroy();
            EventManager.Instance.UnRegisterEventHandler<Action<BattleResult>>(Core.Events.EventType.Battle_OnFinish, HandleOnFinishBattle);
        }

        private void HandleOnFinishBattle(BattleResult result)
        {
            Debug.LogFormat("BattleResult: {0}", result.ToString());

            var clientRoundType = BattleManager.Instance.GetCurrentBattle().GetClientRoundType();
            if ((result == BattleResult.PLAYER1_WIN && clientRoundType == RoundType.PLAYER1_ROUND) || 
                (result == BattleResult.PLAYER2_WIN && clientRoundType == RoundType.PLAYER2_ROUND))
            {
                UIManager.Instance.OpenUI("WinPanel");

                ArchiveManager.Instance.GetCurrentArchiveData().playerData.EnterNextMapLevel(LevelIndexInRow);

                // 剩余的兵继承到下一场战斗，复活部分兵 HEAL_AFTER_BATTLE
                Battle curBattle = BattleManager.Instance.GetCurrentBattle();
                List<BattleGroupData> afterBattleGroupDataList = curBattle.GetClientBattleGroupDataList();
                float ratio = curBattle.GetHealAfterBattleRatio();
                for (int i = 0; i < 36; ++i)
                {
                    BattleGroupData originData = ArchiveManager.Instance.GetCurrentArchiveData().playerData.ArmyData.IndexToBattleGroupData[i];
                    BattleGroupData afterData = afterBattleGroupDataList[i];
                    BattleGroupData newGroupData = BattleGroupData.GetBattleGroupDataAfterPartlyHeal(originData, afterData, ratio);
                    ArchiveManager.Instance.GetCurrentArchiveData().playerData.ArmyData.IndexToBattleGroupData[i] = newGroupData;
                }

                ArchiveManager.Instance.SaveCurrentArchiveDataToFile();
            }
            else
            {
                // TODO: lose
            }
        }


        public void SetLandscapeId(string _landscapeId)
        {
            landscapeId = _landscapeId;
            string path = AssetsMapper.GetLandscapeIconPath(_landscapeId);
            transform.Find("bottom_right/landscape/landscape").GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
        }

        private void UpdateWeatherId()
        {
            weatherId = string.Format("landscape_{0:1000}", UnityEngine.Random.Range(1, 5)); // 1-4
            string path = AssetsMapper.GetLandscapeIconPath(weatherId);
            transform.Find("bottom_right/landscape/weather").GetComponent<Image>().sprite = Resources.Load<Sprite>(path);
        }

        public void OnHoverLandscapeItem(HoverToShowUI hoverToShowUI)
        {
            OpenLandscapePanel(hoverToShowUI, landscapeId);
        }

        public void OnHoverWeatherItem(HoverToShowUI hoverToShowUI)
        {
            OpenLandscapePanel(hoverToShowUI, weatherId);
        }

        private void OpenLandscapePanel(HoverToShowUI hoverToShowUI, string landscapeId)
        {
            UIManager.Instance.CloseAndDestroyUI("LandscapeCard");
            GameObject uiObj = UIManager.Instance.OpenUI("LandscapeCard");
            uiObj.GetComponent<LandscapeCard>().SetLandscapeId(landscapeId);

            Vector2 topRightPos = hoverToShowUI.GetTopRightPos();

            RectTransform rectTransform = uiObj.transform.GetComponent<RectTransform>();
            float x = rectTransform.rect.width * rectTransform.pivot.x - 10; // Because we set z to -30, x should be closer to the cursor
            x -= uiObj.transform.GetComponent<RectTransform>().rect.width;
            x -= hoverToShowUI.gameObject.transform.GetComponent<RectTransform>().rect.width;
            float y = rectTransform.rect.height * (1 - rectTransform.pivot.y);
            uiObj.transform.localPosition = new Vector3(topRightPos.x + x, topRightPos.y - y, uiObj.transform.localPosition.z);

            //HoverOutCloseUI.AddChildToParent(uiObj, gameObject);
            hoverToShowUI.AddChildrenUI(uiObj.GetComponent<LandscapeCard>());
        }

        public void OnHoverOutLandscapeItem(HoverToShowUI hoverToShowUI)
        {
            GameObject uiObj = UIManager.Instance.GetUI("LandscapeCard");
            //HoverOutCloseUI.RemoveChildFromParent(uiObj, gameObject);
            if (hoverToShowUI && uiObj)
                hoverToShowUI.RemoveChildrenUI(uiObj.GetComponent<LandscapeCard>());

            UIManager.Instance.CloseAndDestroyUI("LandscapeCard");
        }

        public void OnNextLevelBtnClk()
        {
            if (cardComponent.IsUsingCard)
                return;

            ArchiveManager.Instance.GetCurrentArchiveData().playerData.Level += 1;
            transform.Find("left/top_left/level").GetComponent<BattleLevelBar>().refreshLevel();

            cardComponent.AllCardFlyBack();

            UpdateWeatherId();

            EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnNextRound);
        }

        public void OnBtnArmyManageClk()
        {
            UIManager.Instance.OpenUI("ArmyManagePanel");
        }

        public void OnCardPileBtnClk()
        {
            GameObject obj = UIManager.Instance.OpenUI("CardPanel");
            obj.GetComponent<CardPanel>().Init(true);
        }

        public void OnMenuBtnClk()
        {
            UIManager.Instance.OpenUI("MainPanel");
        }

        public void OnPauseBtnClk()
        {
            gameObject.SetActive(false);
            var battleMapUIObj = UIManager.Instance.OpenUI("BattleMap");
            BattleMap battleMap = battleMapUIObj.GetComponent<BattleMap>();
            battleMap.SetTrigger();
        }

        public void OnSettingBtnClk()
        {
            GameObject obj = UIManager.Instance.OpenUI("EventPanel");
            obj.GetComponent<EventPanel>().SetEventId("event_00001");
            //GameObject uiObj = UIManager.Instance.OpenUI("ArmySoldierListPop");
            //GameObject uiObj = UIManager.Instance.OpenUI("UnitCardPanel");
            //uiObj.GetComponent<UnitCardPanel>().SetUnitId("unit_0001"); // TODO
        }

        public static BattleCardComponent GetBattleCardComponent()
        {
            BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
            BattleCardComponent battleCardComponent = battlePanel.GetUIComponent<BattleCardComponent>();
            return battleCardComponent;
        }

        public static BattleElementComponent GetBattleElementComponent()
        {
            BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
            BattleElementComponent battleElementComponent = battlePanel.GetUIComponent<BattleElementComponent>();
            return battleElementComponent;
        }

        public static BattleFieldComponent GetBattleFieldComponent()
        {
            BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
            return battlePanel.GetUIComponent<BattleFieldComponent>();
        }
    }
}
