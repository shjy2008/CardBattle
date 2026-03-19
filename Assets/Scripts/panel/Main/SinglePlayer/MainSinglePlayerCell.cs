/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2019 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using System.Numerics;
using Assets.Scripts.common;
using Assets.Scripts.data;
using Assets.Scripts.managers.uimgr;
using FancyScrollView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace Assets.Scripts.panel.Main.SinglePlayer
{
    public class MainSinglePlayerCell : FancyScrollViewCell<ItemData, Context>
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI desc;

        private ItemData m_itemData;

        public override void UpdateContent(ItemData itemData)
        {
            m_itemData = itemData;

            title.text = m_itemData.key.ToString();

            //Table_tower.Data towerData = Table_tower.data[itemData.towerKey];

            //nameText.text = Table_language.data[towerData.name].text_cn;

            //levelText.text = TowerData.Instance.GetTowerLevel(itemData.towerKey).ToString();

            //atkText.text = string.Format("ATK: {0}", BigNumber.BigInteger2FormatString(BattleFormular.GetTowerAtk(itemData.towerKey)));

            //bool hasUnlocked = TowerData.Instance.GetUnlockedTowerId() >= m_itemData.towerKey;
            //lockedParent.SetActive(!hasUnlocked);
            //upgradeBtnParent.SetActive(hasUnlocked);

            //BigInteger needCoinNum = BattleFormular.GetTowerUpgradeCoin(m_itemData.towerKey);
            //bool isEnough = ArchiveManager.Instance.GetCurrentArchiveData().playerData.coin >= needCoinNum;

            //upgradeNeedCoin.text = BigNumber.BigInteger2FormatString(needCoinNum);
            //upgradeNeedCoin.color = isEnough ? colorNormal : colorNotEnough;

            //upgradeBtn.interactable = isEnough;

            //BigInteger upgradeAddAtkNum = BattleFormular.GetTowerLevelAddAtk(itemData.towerKey, TowerData.Instance.GetTowerLevel(itemData.towerKey) + 1);
            //upgradeAddAtk.text = string.Format("ATK + {0}", BigNumber.BigInteger2FormatString(upgradeAddAtkNum));
        }

        // 点升级
        //public void OnUpradeBtnClk()
        //{
        //    bool success = TowerData.Instance.UpgradeTower(m_itemData.towerKey);
        //    if (success)
        //    {
        //        TowerUpgradePanel towerUpgradePanel = UIManager.Instance.GetOpenUI("MainPanel").GetComponent<MainPanel>()
        //            .towerUpgradePanel.GetComponent<TowerUpgradePanel>();
        //        towerUpgradePanel.UpdateScrollViewData();
        //    }
        //}



        // todo 下面这个都一样的，看能否抽出来
        public override void UpdatePosition(float position, ScrollDirection scrollDirection)
        {
            var spacing = Context.GetColumnSpacing();
            var columnCount = Context.GetColumnCount();
            var count = Index % columnCount;

            var cellSize = (transform as RectTransform).rect.width;
            var x = (cellSize + spacing) * (count - (columnCount - 1) * 0.5f);
            var y = transform.localPosition.y;

            transform.localPosition = new Vector2(x, y);
        }

    }
}
