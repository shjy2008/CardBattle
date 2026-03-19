using UnityEngine;
using System.Collections;
using Assets.Scripts.managers.uimgr;
using System.Collections.Generic;
using Assets.Scripts.logics;
using Assets.Scripts.data;
using Assets.Scripts.managers.battlemgr;
using TMPro;
using Assets.Scripts.utility;
using System;
using Assets.Scripts.managers.archivemgr;

namespace Assets.Scripts.panel.Card
{
    public class CardPanel : BaseUI
    {
        [SerializeField] GridView gridView = default;

        private Dictionary<BattleElementType, float> elementWeightDict;

        static List<string> strSequenceList = new List<string>() { "square", "triangle", "circle" };

        // Use this for initialization
        protected override void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnConfirmBtnClk()
        {
            CloseAndDestroy();
        }

        public void Init(bool isInBattle)
        {
            InitElementWeightDict(isInBattle);
            InitElementPercentage();
            InitScrollView();
        }

        private void InitElementWeightDict(bool isInBattle)
        {
            List<BattleGroupData> battleGroupDataList = new List<BattleGroupData>();
            if (!isInBattle)
            {
                for (int i = 0; i < 36; ++i)
                {
                    BattleGroupData battleGroupData = ArchiveManager.Instance.GetCurrentArchiveData().playerData.ArmyData.IndexToBattleGroupData[i];
                    battleGroupDataList.Add(battleGroupData);
                }
            }
            else
            {
                Battle curBattle = BattleManager.Instance.GetCurrentBattle();
                battleGroupDataList = curBattle.GetClientBattleGroupDataList();
            }

            elementWeightDict = BattleCardPileData.GetElementWeightDict(battleGroupDataList);
        }

        private void InitElementPercentage()
        {

            float totalWeight = 0.0f;
            foreach (KeyValuePair<BattleElementType, float> kv in elementWeightDict)
            {
                totalWeight += kv.Value;
            }

            int percentCount = 0;
            int percentSum = 0;
            foreach (KeyValuePair<BattleElementType, float> kv in elementWeightDict)
            {
                int percent;
                if (percentCount < elementWeightDict.Count - 1)
                {
                    percent = Mathf.RoundToInt(kv.Value / totalWeight * 100);
                    percentSum += percent;
                }
                else
                {
                    percent = 100 - percentSum;
                }
                string strPercent = percent.ToString() + "%";
                Transform ratioTrans = transform.Find(string.Format("bg/top/elements/{0}/ratio", kv.Key.ToString()));
                ratioTrans.GetComponent<TextMeshProUGUI>().text = strPercent;
                ++percentCount;
            }
        }

        private void InitScrollView()
        {
            UpdateScrollViewData();
        }

        public void UpdateScrollViewData()
        {
            List<bool> selectedBoolList = GetSequanceSelectedStatus();

            List<ItemData> itemDataList = new List<ItemData>();

            var cardDataList = new List<BattleCardData>();
            var cardIdList = ArchiveManager.Instance.GetCurrentArchiveData().playerData.cardIdList;
            foreach (string cardId in cardIdList)
            {
                BattleCardData cardData = new BattleCardData(cardId);
                cardDataList.Add(cardData);
            }
            foreach (BattleCardData cardData in cardDataList)
            {
                List<string> sequenceList = BattleCard.GetSequenceList(cardData);
                if ((!selectedBoolList[0] || sequenceList.Contains(strSequenceList[0])) &&
                    (!selectedBoolList[1] || sequenceList.Contains(strSequenceList[1])) &&
                    (!selectedBoolList[2] || sequenceList.Contains(strSequenceList[2])))
                {
                    ItemData itemData = new ItemData(cardData);
                    itemDataList.Add(itemData);
                }
            }

            itemDataList.Sort((x, y) =>
            {
                return (int)(GetCardDataWeight(y.cardData) - GetCardDataWeight(x.cardData));
            });

            gridView.UpdateContents(itemDataList.ToArray());
        }

        private float GetCardDataWeight(BattleCardData cardData)
        {
            List<string> cardElements = cardData.actionData.element;
            float cardWeight = 0;
            foreach (string element in cardElements)
            {
                BattleElementType elementType = Utils.ConvertStrToEnum<BattleElementType>(element);
                cardWeight += elementWeightDict[elementType];
            }
            return cardWeight;
        }

        private List<bool> GetSequanceSelectedStatus()
        {
            List<bool> selectedBoolList = new List<bool>();
            foreach (string sequence in strSequenceList)
            {
                bool isSelected = transform.Find("bg/filter/" + sequence).GetComponent<CardPanelSequenceFilter>().GetIsSelected();
                selectedBoolList.Add(isSelected);
            }
            return selectedBoolList;
        }
    }
}
