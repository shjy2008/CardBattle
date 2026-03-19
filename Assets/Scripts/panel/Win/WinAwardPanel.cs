using System;
using Assets.Scripts.common;
using Assets.Scripts.managers.uimgr;
using UnityEngine;


namespace Assets.Scripts.panel.Win
{
    public class WinAwardPanel : BaseUI
    {
        private bool isSelected = false;
        private int selectedIndex = -1;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

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

        public void OnBtnBackClk()
        {
            CloseAndDestroy();
            UIManager.Instance.OpenUI("WinPanel");
        }

        public void OnBtnConfirmClk()
        {
            UIManager.Instance.GetUI("WinPanel").GetComponent<BaseUI>().CloseAndDestroy();
            CloseAndDestroy();

            var uiObj = UIManager.Instance.GetOpenUI("BattlePanel");
            if (uiObj != null)
            {
                BaseUI baseUI = uiObj.GetComponent<BaseUI>();
                baseUI?.CloseAndDestroy();
            }

            var battleMapUIObj = UIManager.Instance.OpenUI("BattleMap");
            BattleMap battleMap = battleMapUIObj.GetComponent<BattleMap>();
            battleMap.SetTrigger();
        }

        public void ClickAwardCard(int index)
        {
            selectedIndex = index;
            isSelected = true;
        }

        public int GetSelectedIndex()
        {
            return selectedIndex;
        }

        public bool GetIsSelected()
        {
            return isSelected;
        }
    }
}
