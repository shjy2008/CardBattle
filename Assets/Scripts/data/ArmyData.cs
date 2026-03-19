using System;
using System.Collections.Generic;
using Assets.Scripts.logics;
using Newtonsoft.Json;

namespace Assets.Scripts.data
{
    [Serializable]
    public class ArmyData
    {
        public static int ArmyCount = 36;
        public static int HeroCount = 9;

        private static Dictionary<int, List<int>> heroIndexToArmyIndexList;

        public Dictionary<int, BattleGroupData> IndexToBattleGroupData;

        [JsonProperty("indexToHeroId")]
        private Dictionary<int, int> indexToHeroId; // HeroId: 1-5. If HeroId == 0, then no hero
        public Dictionary<int, int> IndexToHeroId
        {
            get { return indexToHeroId; }
            set { indexToHeroId = value; }
        }

        public ArmyData()
        {
            heroIndexToArmyIndexList = new Dictionary<int, List<int>>();
            heroIndexToArmyIndexList[0] = new List<int> { 0, 1, 2 };
            heroIndexToArmyIndexList[1] = new List<int> { 3, 4, 5, 6, 7, 8 };
            heroIndexToArmyIndexList[2] = new List<int> { 9, 10, 11 };
            heroIndexToArmyIndexList[3] = new List<int> { 12, 13, 14 };
            heroIndexToArmyIndexList[4] = new List<int> { 15, 16, 17, 18, 19, 20 };
            heroIndexToArmyIndexList[5] = new List<int> { 21, 22, 23 };
            heroIndexToArmyIndexList[6] = new List<int> { 24, 25, 26 };
            heroIndexToArmyIndexList[7] = new List<int> { 27, 28, 29, 30, 31, 32 };
            heroIndexToArmyIndexList[8] = new List<int> { 33, 34, 35 };

            IndexToBattleGroupData = new Dictionary<int, BattleGroupData>();
            // TODO: BattleGroupData for test
            for (int i = 0; i < ArmyCount; i++)
            {
                BattleGroupData battleGroupData = new BattleGroupData();
                battleGroupData.InitByRandom();
                IndexToBattleGroupData[i] = battleGroupData;
            }

            // Hero
            indexToHeroId = new Dictionary<int, int>();
            indexToHeroId[0] = 1;
            indexToHeroId[1] = 2;
            indexToHeroId[2] = 3;
            indexToHeroId[3] = 0;
            indexToHeroId[4] = 4;
            indexToHeroId[5] = 0;
            indexToHeroId[6] = 5;
            indexToHeroId[7] = 2;
            indexToHeroId[8] = 3;
        }

        public static List<int> GetArmyIndexListByHeroIndex(int heroIndex)
        {
            return heroIndexToArmyIndexList[heroIndex];
        }

        //public static string GetImagePathWithArmyId(int armyId)
        //{
        //    string name = "";
        //    switch (armyId)
        //    {
        //        case 1:
        //            name = "nato_unit_icon_anti_armor_unit";
        //            break;
        //        case 2:
        //            name = "nato_unit_icon_cavalry_unit";
        //            break;
        //        case 3:
        //            name = "nato_unit_icon_engineer_unit";
        //            break;
        //        case 4:
        //            name = "nato_unit_icon_heavy_unit";
        //            break;
        //        case 5:
        //            name = "nato_unit_icon_light_unit";
        //            break;
        //        default:
        //            return "";
        //    }
        //    return "images/group_icons/" + name;
        //}

        public static string GetImagePathWithHeroId(int heroId)
        {
            string name = "";
            switch (heroId)
            {
                case 1:
                    name = "hero_1";
                    break;
                case 2:
                    name = "hero_2";
                    break;
                case 3:
                    name = "hero_3";
                    break;
                case 4:
                    name = "hero_4";
                    break;
                case 5:
                    name = "hero_5";
                    break;
                default:
                    return "";
            }
            return "images/group_icons/" + name;
        }

    }
}
