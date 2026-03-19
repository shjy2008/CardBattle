using System;
using System.Collections.Generic;
using BigInteger = System.Numerics.BigInteger;
using Assets.Scripts.utility.Singleton;
using Newtonsoft.Json;
using UnityEngine;
using Core.Events;
using Assets.Scripts.common;
using Assets.Scripts.logics;

namespace Assets.Scripts.data
{
    [Serializable]
    public class PlayerData
    {
        // 关卡
        public int Level;

        // 部队
        public ArmyData ArmyData;

        // 地图
        public long curMapRandomSeed; // 当前地图随机数种子
        public int curMapId; // 地图id，从1开始，一直到5
        public int curMapDifficulty; // 当前地图id的地图难度，1-5
        public List<int> MapRowIndexList; // 地图已通关的index，初始化是0，通关下一关之后，list加上这一关的rowIndex

        // 手牌
        public List<string> cardIdList;

        public PlayerData()
        {
            Level = 1;
            ArmyData = new ArmyData();

            // TODO: read local data on disk
            curMapRandomSeed = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            curMapId = 1;
            curMapDifficulty = 1;
            MapRowIndexList = new List<int>() { 0 };

            cardIdList = new List<string>();

            // TODO: read local data on disk
            for (int i = 0; i < 50; ++i)
            {
                string cardId = string.Format("tactic_{0:0000}", i + 1);
                //cardId = "stratagem_0018";
                //BattleCardData _cardData = new BattleCardData(cardId);
                cardIdList.Add(cardId);
            }
            //for (int i = 0; i < 2; ++i)
            //{
            //    BattleCardData cardData_6 = new BattleCardData("tactic_0006");
            //    cardDataList.Add(cardData_6);
            //}
        }

        public void SetLanguage(UIUtils.LanguageType _language)
        {
            // PlayerPrefs is designed for saving small data like game settings, preferences.
            PlayerPrefs.SetInt("language", (int)_language);
        }

        public UIUtils.LanguageType GetLanguage()
        {
            return (UIUtils.LanguageType)PlayerPrefs.GetInt("language", (int)UIUtils.LanguageType.EN);
        }

        public void EnterNextMapLevel(int levelIndexInRow)
        {
            if (MapRowIndexList.Count >= 6)
            {
                // next map
                curMapDifficulty += 1;
                if (curMapDifficulty > 5)
                {
                    curMapDifficulty = 1;
                    curMapId += 1;
                }
            }
            else
            {
                MapRowIndexList.Add(levelIndexInRow);
            }

        }

    }
}
