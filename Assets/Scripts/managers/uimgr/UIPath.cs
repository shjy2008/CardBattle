using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.managers.uimgr
{
    public class UIPath
    {
        public enum UILayer
        {
            Normal,
            HintBox,
            Tips,
        }

        public class UIParam
        {
            public string path;
            public UILayer layer;
        }

        public static Dictionary<string, UIParam> PathData = new Dictionary<string, UIParam>
        {
            {"GMPanel", new UIParam { path = "ui_prefab/panel/GMPanel", layer = UILayer.HintBox} },
            {"HintBox", new UIParam { path = "ui_prefab/common/HintBox", layer = UILayer.HintBox} },
            {"BattlePanel", new UIParam { path = "ui_prefab/panel/battlepanel/BattlePanel", layer = UILayer.Normal} },
            // Common
            {"UnitCardPanel", new UIParam { path = "ui_prefab/panel/common/UnitCardPanel", layer = UILayer.Normal} },
            {"LandscapeCard", new UIParam { path = "ui_prefab/panel/common/LandscapeCard", layer = UILayer.Normal} },
            {"DescPanel", new UIParam { path = "ui_prefab/panel/common/DescPanel", layer = UILayer.Normal} },
            {"DescPanelSecond", new UIParam { path = "ui_prefab/panel/common/DescPanel", layer = UILayer.Normal} },
            // Army Manage Panel
            {"ArmyManagePanel", new UIParam { path = "ui_prefab/panel/ArmyManage/ArmyManagePanel", layer = UILayer.Normal} },
            {"ArmyCoinPop", new UIParam { path = "ui_prefab/panel/ArmyManage/ArmyCoinPop", layer = UILayer.Normal} },
            {"ArmyEffectPop", new UIParam { path = "ui_prefab/panel/ArmyManage/ArmyEffectPop", layer = UILayer.Normal} },
            {"ArmySoldierListPop", new UIParam { path = "ui_prefab/panel/ArmyManage/ArmySoldierListPop", layer = UILayer.Normal} },
            {"ArmyHeroInfoPop", new UIParam { path = "ui_prefab/panel/ArmyManage/ArmyHeroInfoPop", layer = UILayer.Normal} },
            // Win
            {"WinPanel", new UIParam { path = "ui_prefab/panel/Win/WinPanel", layer = UILayer.Normal} },
            {"WinAwardPanel", new UIParam { path = "ui_prefab/panel/Win/WinAwardPanel", layer = UILayer.Normal} },
            // Main
            {"MainPanel", new UIParam { path = "ui_prefab/panel/Main/MainPanel", layer = UILayer.Normal} },
            // Map
            {"BattleMap", new UIParam { path = "ui_prefab/panel/map/BattleMap", layer = UILayer.Normal} },
            {"BattleMapDotPopUI", new UIParam { path = "ui_prefab/panel/map/BattleMapDotPopUI", layer = UILayer.Normal} },
            // Event
            {"EventPanel", new UIParam { path = "ui_prefab/panel/Event/EventPanel", layer = UILayer.Normal} },
            // Debug
            {"DebugPanel", new UIParam{ path = "ui_prefab/panel/common/DebugPanel", layer = UILayer.HintBox} },
            // Card
            {"CardPanel", new UIParam{ path = "ui_prefab/panel/Card/CardPanel", layer = UILayer.Normal} },


        };

    }
}
