using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.managers.timermgr;
using Core.Events;
using System;
using UnityEngine;
using Assets.Scripts.utility;
using Assets.Scripts.BattleField;
using Assets.Scripts.panel.BattlePanel;
using Assets.Scripts.managers.resourcemgr;
using System.Reflection;
using Assets.Scripts.logics;
using System.Collections.Generic;
using Newtonsoft.Json;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.data;
using Assets.Scripts.common;
using Assets.Scripts.managers.archivemgr;

namespace Assets.Scripts.gm
{
    public static class GMFunc
    {
        //所有gm函数都加到这里来,以GM开头.

        public static void GMTest0()
        {
            Debug.Log("GMTest");
        }

        public static void GMTestWithParam(string s)
        {
            Debug.Log("GMTestWithParam1: " + s);
        }

        public static void GMT1(int _startSide, int _endSide, int _atkDirs)
        {
            Debug.Log("calling GMT1");
            //GameObject battleFiled = GameObject.Find("Canvas/BattleField");
            //BattleFieldComponent comp = new BattleFieldComponent(battleFiled);
            //comp.Init();

            //GroupSide startSide = (GroupSide)_startSide;
            //GroupSide endSide = (GroupSide)_endSide;
            //for (int i = 0; i < 4; i++)
            //{
            //    int dir = (1 << i);
            //    if ((dir & _atkDirs) == dir)
            //        comp.OnDisplayCurveArrow(startSide, endSide, (AttackedDir)dir);
            //}
            //comp.OnDisplayCubes(startSide, endSide, _atkDirs, 15f); // TODO: make it as a function call for a card.
        }

        public static void GMT2(bool isDisplay)
        {
            // test preview defense and attack
            //Debug.Log("calling GMT2");
            //GameObject battleFiled = GameObject.Find("Canvas/BattleField");
            //BattleFieldComponent comp = new BattleFieldComponent(battleFiled);
            //comp.Init();

            //if (isDisplay)
            //{
            //    comp.OnDisplayHighlight((int)AttackedDir.BOTTOM);
            //}
            //else
            //{
            //    comp.OnHideHighlight();
            //}

            // TODO: to display curve arrow as well.
        }

        public static void GMT3()
        {
            Debug.Log("calling GMT3");

            BattleManager.Instance.CreateNewBattle(1);

            //UIManager.Instance.CloseAndDestroyUI("BattlePanel"); // not working --> only destroy the gameobject. Not calling the BaseUI.Destroy

            var uiObj = UIManager.Instance.GetOpenUI("BattlePanel");
            if(uiObj != null)
            {
                BaseUI baseUI = uiObj.GetComponent<BaseUI>();
                baseUI?.CloseAndDestroy();
            }
            UIManager.Instance.OpenUI("BattlePanel");
        }

        //public static void GMT4(string transName)
        //{
        //    Debug.Log("calling GMT4");
        //    GameObject battleFiled = GameObject.Find("Canvas/BattleField");
        //    BattleFieldComponent comp = new BattleFieldComponent(battleFiled);
        //    comp.Init();
        //    comp.TryUseCard();


        //    GameObject curveArrow = GameObject.Find(transName);
        //    curveArrow.GetComponent<CurveMeshCreator>().GenerateMesh();
        //}

        //public static void GMT5()
        //{
        //    Debug.Log("calling GMT5");
        //    GameObject battleFiled = GameObject.Find("Canvas/BattleField");
        //    BattleFieldComponent comp = new BattleFieldComponent(battleFiled);
        //    comp.Init();
        //    comp.TryUseCard();
        //}

        public static void GMT6(int _startSide, int _endSide, int _atkDir)
        {
            Debug.Log("calling GMT6");
            //GameObject battleFiled = GameObject.Find("Canvas/BattleField");
            //BattleFieldComponent comp = new BattleFieldComponent(battleFiled);
            //comp.Init();

            //GroupSide startSide = (GroupSide)_startSide;
            //GroupSide endSide = (GroupSide)_endSide;
            //AttackedDir dir = (AttackedDir)_atkDir;
            //comp.OnDisplayCurveArrow(startSide, endSide, dir);
            ////comp.OnDisplayCubes(startSide, endSide, (int)AttackedDir.BOTTOM, 15f); // TODO: make it as a function call for a card.
            //comp.PlayChargeEffect(GroupSide.LEFT, GroupSide.LEFT, dir);
        }

        public static void GMT7(float alpha)
        {
            Debug.Log("calling GMT7");
            //GameObject battleFiled = GameObject.Find("Canvas/BattleField");
            //BattleFieldComponent comp = new BattleFieldComponent(battleFiled);
            //comp.Init();
            //comp.OnDisplayCurveArrow(GroupSide.LEFT, GroupSide.LEFT, AttackedDir.BOTTOM);
            //comp.OnDisplayCubes(GroupSide.LEFT, GroupSide.LEFT, (int)AttackedDir.BOTTOM, 15f); // TODO: make it as a function call for a card.
            //comp.TestAlpah(alpha);
        }

        public static void GMT8()
        {
            //Debug.Log("calling GMT8");
            //var comp = BattlePanel.GetBattleFieldComponent();
            //BattleCardData cardData = new BattleCardData("tactic_0008");
            //comp.TryPreviewCard(cardData);

        }

        public static void GMT9()
        {
            //Debug.Log("calling GMT9");
            //var comp = BattlePanel.GetBattleFieldComponent();
            //comp.EndPreviewCard();
        }

        //public static void GMT10(float x, float y, float z, float radius, float force)
        //{
        //    Debug.Log("calling GMT10");
        //    GameObject simul = GameObject.Find("Simul");
        //    var simulPlayer = simul.GetComponent<SimulationPlayer>();
        //    var config = new SimulationPlayerConfig();
        //    config.Init(40, 0); // init the cubes without Explosion Configs
        //    simulPlayer.Init(config, true);
        //    Vector3 center = new Vector3(x, y, z);
        //    simulPlayer.AddExplosionConfig(new ExplosionConfig(center, radius, force));
        //}

        public static void GMT11()
        {
            Debug.Log("calling GMT11");
            GameObject battleFiled = GameObject.Find("Canvas/BattleField");
            BattleFieldComponent comp = new BattleFieldComponent(battleFiled);
            comp.Init();

            Type type = typeof(BattleFieldComponent);
            MethodInfo methodInfo = type.GetMethod("OnDisplayCurveArrow", BindingFlags.NonPublic | BindingFlags.Instance);
            //object[] parameters = { GroupSide.MIDDLE, GroupSide.LEFT, AttackedDir.BOTTOM };
            //methodInfo.Invoke(comp, parameters);
            //comp.OnDisplayCurveArrow(GroupSide.MIDDLE, GroupSide.LEFT, AttackedDir.BOTTOM);
            //comp.OnDisplayCurveArrow(GroupSide.MIDDLE, GroupSide.LEFT, AttackedDir.LEFT);
            //comp.OnDisplayCurveArrow(GroupSide.MIDDLE, GroupSide.RIGHT, AttackedDir.BOTTOM);
            //comp.OnDisplayCurveArrow(GroupSide.MIDDLE, GroupSide.RIGHT, AttackedDir.RIGHT);
        }

        public static void GMT12(float timeScale)
        {
            Debug.Log("calling GMT12");
            Time.timeScale = timeScale;
        }

        public static void GMTSetTimeScale(float timeScale)
        {
            Debug.Log("calling GMTSetTimeScale");
            Time.timeScale = timeScale;
        }

        public static void GMT13()
        {
            Debug.Log("calling GMT13");
            string projectile = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit Vol.2/FX_Shoot_Ice_projectile";
            var mainPrefab = ResourceManager.Instance.LoadResourceAsGameObject(projectile);
            mainPrefab.transform.ResetTransform();
        }

        [Serializable]
        public struct A
        {
            public string target;
            public string effect;
            public string basicValue;
        }

        [Serializable]
        public struct B
        {
            public string target;
            public string effect;
            public float basicValue;
        }

        public static void GMT14()
        {
            Debug.Log("calling GMT14");

            //var battleCardData1 = new BattleCardData("tactic_001");
            var battleCardData2 = new BattleCardData("tactic_002");
            var battleCardData3 = new BattleCardData("tactic_003");
        }

        public static void GMTestAtkDef()
        {
            Debug.Log("GMTestAtkDef");

            //var amountDict = BattleManager.Instance.GetCurrentBattle().GetAmountDictCopy();

            //var attackerGroupSide = GroupSide.BOTTOM_LEFT;
            //var defenderGroupSide = GroupSide.TOP_LEFT;

            //var attackerUnits = BattleManager.Instance.GetCurrentBattle().GetUnitIDs(false, attackerGroupSide);
            //var defenderUnits = BattleManager.Instance.GetCurrentBattle().GetUnitIDs(true, defenderGroupSide);

            //float atk1, atk2;
            //BattleLogics.CalculateAttackFromOneSideToOther(
            //    BattleActionEffectType.MELEE_ATTACK,
            //    1.0f,
            //    false,
            //    attackerGroupSide,
            //    defenderGroupSide,
            //    ref attackerUnits,
            //    ref defenderUnits,
            //    ref amountDict,
            //    out atk1,
            //    out atk2);

            //Debug.LogFormat("atk1: {0}, atk2: {1}", atk1, atk2);
        }

        public static void GMUpdateStatus(string statusID, float delta)
        {
            Debug.Log("GMUpdateStatus");

            BattleLogics.IterateGroupSide((GroupSide side) =>
            {
                //BattleManager.Instance.GetCurrentBattle().UpdateBattleStatus(
                //    side,
                //    BattleActionEffectType.DISCIPLINE.ToString(),
                //    2.0f);

                BattleManager.Instance.GetCurrentBattle().UpdateBattleStatus(
                    side,
                    statusID,
                    delta);

                //BattleManager.Instance.GetCurrentBattle().UpdateBattleStatus(
                //    side,
                //    BattleActionEffectType.CHAOS.ToString(),
                //    4);

                //BattleManager.Instance.GetCurrentBattle().UpdateBattleStatus(
                //    side,
                //    BattleActionEffectType.FLEE.ToString(),
                //    5);

                //BattleManager.Instance.GetCurrentBattle().UpdateBattleStatus(
                //    side,
                //    BattleActionEffectType.COUNTERATTACK_MODIFIER.ToString(),
                //    5);

                //BattleManager.Instance.GetCurrentBattle().UpdateBattleStatus(
                //    side,
                //    BattleActionEffectType.INHERIT_ALL_DEFENSE.ToString(),
                //    (add > 0 ? 1 : -1) * 5);
                EventManager.Instance.SendEventAsync(Core.Events.EventType.Battle_OnStatusUpdate, side);
            });

        }

        //public static void GMPrintUnitIDs(int row, int col)
        //{
        //    Debug.Log("GMPrintUnitIDs");
        //    var curBattle = BattleManager.Instance.GetCurrentBattle();
        //    var groupData = curBattle.GetBattleGroupData(row, col);
        //    var allUnits = groupData.GetUnitIDs(false);
        //    foreach (var kv in allUnits)
        //    {
        //        Debug.LogFormat("unitID: {0}, amount: {1}", kv.Key, kv.Value);
        //    }
        //}

        public static void GMSetLanguage(int language)
        {
            ArchiveManager.Instance.GetCurrentArchiveData().playerData.SetLanguage((UIUtils.LanguageType)language);
            Debug.LogFormat("Current language: {0}", ((UIUtils.LanguageType)language).ToString());
        }

        public static void GMAddDefense(GroupSide side, BattleActionEffectType defenseType, float value)
        {
            // use this parameter for testing: "TOP_LEFT,TERRAIN_DEFENSE,10"
            Debug.LogFormat("GMAddDefense: {0}, {1}, {2}", side.ToString(), defenseType.ToString(), value);
            BattleManager.Instance.GetCurrentBattle().UpdateBattleDefense(new(side, defenseType), value);
            EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnDefenseUpdate, side);
        }

        public static void GMPreloadResources()
        {
            Debug.Log("GMPreloadResources");
            ResourceManager.Instance.PreloadResources();
        }

        public static void GMOnNextRound()
        {
            EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnNextRound);
        }

        public static void GMBattleStatus()
        {
            Debug.Log("GMBattleStatus");
            BattleLogics.IterateGroupSide((GroupSide side) =>
            {
                var totalAmount = BattleManager.Instance.GetCurrentBattle().GetTotalAmountByGroupSide(side, false);
                var totalArmor = BattleManager.Instance.GetCurrentBattle().GetGroupsideBattleDefense(side);
                Debug.LogFormat("Side:{0}, hp:{1}, armor:{2}", side.ToString(), totalAmount, totalArmor);
            });

        }

        public static void GMCreateSaveFile(string name)
        {
            ArchiveManager.Instance.CreateDefaultArchiveData(name);
        }

        public static void GMSetAIEnable(bool enable)
        {
            BattleAIController.enable = enable;
        }

        public static void GMKillGroupSide(GroupSide side)
        {
            var curBattle = BattleManager.Instance.GetCurrentBattle();
            int rowStart, rowEnd, colStart, colEnd;
            BattleLogics.GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
            for (int row = rowStart; row < rowEnd; row++)
            {
                for (int col = colStart; col < colEnd; col++)
                {
                    var battleGroupData = curBattle.GetBattleGroupData(row, col);
                    var amount = battleGroupData.GetTotalAmountBeforeLoss();
                    battleGroupData.CacheLoss(amount);
                }
            }

            BattlePanel battlePanel = UIManager.Instance.GetOpenUI("BattlePanel").GetComponent<BattlePanel>();
            BattlePanel.GetBattleFieldComponent().Test1(side);
            BattlePanel.GetBattleFieldComponent().Test2();
        }

        public static void GMDisplayFPS(bool enable)
        {
            if (enable) UIManager.Instance.OpenUI("DebugPanel");
            else UIManager.Instance.CloseUI("DebugPanel");
        }

        public static void GMShjyTest(BattleBasicValueType valueType)
        {
            //float ret = BattleLogics.GetBasicValueRatio(valueType);
            var ret = ArchiveManager.Instance.GetCurrentArchiveData().playerData.MapRowIndexList;
            var k = string.Join(", ", ret);
            Debug.Log(k);
        }

        public static string GMTestCardID;
        public static void GMSetCurrentCard(string id)
        {
            Debug.LogFormat("GMSetCurrentCard, id = {0}", id);
            GMTestCardID = id;
            // tactic_0043
        }
    }
}
