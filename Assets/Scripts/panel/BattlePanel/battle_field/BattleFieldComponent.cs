using Assets.Scripts.gm;
using Assets.Scripts.managers.resourcemgr;
using Assets.Scripts.managers.uimgr;
using Assets.Scripts.utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.BattleField;
using System.Linq;
using System.Reflection;
using Assets.Scripts.logics;
using Assets.Scripts.managers.battlemgr;
using Assets.Scripts.panel.BattlePanel;
using Core.Events;
using Assets.Scripts.common;
using Assets.Scripts.managers.timermgr;

public class BattleFieldComponent : UIComponent
{
    private GameObject root; // should be the BattleField UI node.
    private Transform groupTopRoot, groupBottomRoot;

    public static string armyGroupPrefabPath = "ui_prefab/panel/battlepanel/battlefield/army_group";

    private const float UIScale = 9.353075f;
    private Vector2 groupSize = new Vector2(30, 20) / UIScale; // the icon size.

    private BattleCardData curCardData;
    private List<float> groupHighlightList; // each value in this list will be in range [0, 1]

    // when previewing a card, it will cache the params and used later when calling TryUseCard.
    //HashSet<Tuple<GroupSide, GroupSide, AttackedDir>> totalAtkArrowParams;
    private Dictionary<int, HashSet<Tuple<GroupSide, GroupSide, AttackedDir>>> totalAtkArrowParamDict;

    // cache the change of defense when previewing a card.
    private Dictionary<Tuple<GroupSide, BattleActionEffectType>, float> deltaDefenseCache;

    // cache the change of status when previewing a card.
    private Dictionary<Tuple<GroupSide, BattleActionEffectType>, float> deltaStatusCache;

    // cache the distant project VFX hit point(end pos) for later use
    private Dictionary<int, List<Vector3>> siegeAttackHitPosDict;

    private List<GameObject> effectPlayerKeyBuffer;
    private List<GameObject> inactiveEffectPlayerBuffer;

    public BattleFieldComponent(GameObject _root)
    {
        root = _root;
    }

    public override void Init()
    {
        groupTopRoot = root.transform.Find("top_root/top_field");
        groupBottomRoot = root.transform.Find("bottom_root/bottom_field");

        simulRoot = root.transform.Find("simulation_root");
        simulPlayerList = new List<SimulationPlayer>();

        atkRoot = root.transform.Find("top_root/selected");
        defRoot = root.transform.Find("bottom_root/selected");

        arrowsRoot = root.transform.Find("arrows_root");
        arrowsRoot.position = Vector3.zero; // put it to origin in world space.
        arrowsRoot.transform.localScale = Vector3.one * UIScale; // just make it equivalent to a point at origin without parent.
        curveArrowsDict = new Dictionary<Tuple<GroupSide, int, GroupSide, int, AttackedDir>, GameObject>();

        vfxsRoot = root.transform.Find("vfxs_root");
        vfxsRoot.position = Vector3.zero; // put it to origin in world space.
        vfxsRoot.transform.localScale = Vector3.one * UIScale; // just make it equivalent to a point at origin without parent.

        effectPlayerObjDict = new Dictionary<GameObject, float>();
        effectPlayerKeyBuffer = new List<GameObject>();
        inactiveEffectPlayerBuffer = new List<GameObject>();
        fadeGroupTransList = new List<Transform>();

        totalAtkArrowParamDict = new Dictionary<int, HashSet<Tuple<GroupSide, GroupSide, AttackedDir>>>();

        siegeAttackHitPosDict = new Dictionary<int, List<Vector3>>();

        InitGroupsInBattleField();
        InitGroupRectCenterDict();
        InitHPBarCompDict();
        InitStatusBarDict();
        InitHighlightGroupList();
        InitDeltaDefenseCache();
        InitDeltaStatusCache();
        InitSimulationPlayerList();
        InitEffectPlayerObjDict();
        InitEvents();
    }

    private void InitEvents()
    {
        EventManager.Instance.RegisterEventHandler<Action<GroupSide>>(Core.Events.EventType.Battle_OnDefenseUpdate, OnUpdateArmor);
        EventManager.Instance.RegisterEventHandler<Action<BattleCardData>>(Core.Events.EventType.UI_OnPreviewCard, TryPreviewCard);
        EventManager.Instance.RegisterEventHandler<Action>(Core.Events.EventType.UI_OnEndPreviewCard, EndPreviewCard);
        EventManager.Instance.RegisterEventHandler<Action>(Core.Events.EventType.UI_OnUseCard, TryUseCard);
    }

    private void InitDeltaDefenseCache()
    {
        deltaDefenseCache = new Dictionary<Tuple<GroupSide, BattleActionEffectType>, float>();
        BattleLogics.IterateGroupSide((GroupSide side) =>
        {
            deltaDefenseCache.Add(new(side, BattleActionEffectType.TERRAIN_DEFENSE), 0);
            deltaDefenseCache.Add(new(side, BattleActionEffectType.FORTIFICATION_DEFENSE), 0);
            deltaDefenseCache.Add(new(side, BattleActionEffectType.ELASTIC_DEFENSE), 0);
            deltaDefenseCache.Add(new(side, BattleActionEffectType.FORMATION_DEFENSE), 0);
            deltaDefenseCache.Add(new(side, BattleActionEffectType.ARMOUR_DEFENSE), 0);
        });
    }

    private void ResetDeltaDefenseCache()
    {
        BattleLogics.IterateGroupSide((GroupSide side) =>
        {
            deltaDefenseCache[new(side, BattleActionEffectType.TERRAIN_DEFENSE)] = 0;
            deltaDefenseCache[new(side, BattleActionEffectType.FORTIFICATION_DEFENSE)] = 0;
            deltaDefenseCache[new(side, BattleActionEffectType.ELASTIC_DEFENSE)] = 0;
            deltaDefenseCache[new(side, BattleActionEffectType.FORMATION_DEFENSE)] = 0;
            deltaDefenseCache[new(side, BattleActionEffectType.ARMOUR_DEFENSE)] = 0;
        });
    }

    private void OnUseDeltaDefenseCache()
    {
        var curBattle = BattleManager.Instance.GetCurrentBattle();
        var sides = new HashSet<GroupSide>();
        foreach (var item in deltaDefenseCache) 
        { 
            if(item.Value != 0)
            {
                curBattle.UpdateBattleDefense(item.Key, item.Value);
                sides.Add(item.Key.Item1);
            }
        };

        foreach (var side in sides) EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnDefenseUpdate, side);
    }

    private void OnApplyUnitLoss(GroupSide side)
    {
        BattleManager.Instance.GetCurrentBattle().OnApplyUnitLoss(side);
    }

    public override void Update()
    {
        UpdateEffectPlayers();
        if (fadeAllCubes) OnFadeAllCubes();
        if (fadeGroupIcons) OnFadeAllGroupIcons();
    }

    private void ReleaseEvents()
    {
        EventManager.Instance.UnRegisterEventHandler<Action<GroupSide>>(Core.Events.EventType.Battle_OnDefenseUpdate, OnUpdateArmor);
        EventManager.Instance.UnRegisterEventHandler<Action<BattleCardData>>(Core.Events.EventType.UI_OnPreviewCard, TryPreviewCard);
        EventManager.Instance.UnRegisterEventHandler<Action>(Core.Events.EventType.UI_OnEndPreviewCard, EndPreviewCard);
        EventManager.Instance.UnRegisterEventHandler<Action>(Core.Events.EventType.UI_OnUseCard, TryUseCard);
    }

    public override void Destroy()
    {
        Debug.Log("Calling BattleFieldComponent Destroy...");
        ReleaseGroupsInBattleField();
        ReleaseSimulationPlayerList();
        ReleaseEffectPlayerObjDict();
        ReleaseAllCurveArrows();
        ReleaseEvents();
    }

    private void InitHighlightGroupList()
    {
        // '1' is fully highlight, 'false' is no highlight.
        // For cubes at top parts, only those can be effected by strategy will be highlight.
        // For cubes at bottom parts, only those are the same group types specified by the card can be highlight.
        // Even it is highligh, at bottom parts, cubes may have a little bit gray according to the damaged soliders number.
        groupHighlightList = new List<float>();
        for (int n = 0; n < 72; n++) groupHighlightList.Add(0);
    }

    private void InitGroupsInBattleField()
    {
        for (int row = 0; row < 6; row++)
        {
            for(int col = 0; col < 12; col++)
            {
                var groupTrans = GetGroup(row, col);

                var battleGroupData = BattleLogics.GetBattleGroupDataFromCurrentBattle(row, col);
                
                // TODO: problem, groupIconPath is to show the major group type. 
                var groupIconPath = AssetsMapper.GetGroupIcon(battleGroupData, false, false);

                if (string.IsNullOrEmpty(groupIconPath))
                    groupTrans.gameObject.SetActive(false);
                else
                {
                    groupTrans.gameObject.SetActive(true);

                    GameObject armyGroupObj;
                    if (groupTrans.childCount <= 0)
                    {
                        armyGroupObj = ResourceManager.Instance.FetchResourceFromPool(armyGroupPrefabPath);
                        armyGroupObj.transform.SetParent(groupTrans);
                        armyGroupObj.transform.localPosition = Vector3.zero;
                    }
                    else armyGroupObj = groupTrans.GetChild(0).gameObject;

                    var spRenderer = armyGroupObj.GetComponent<SpriteRenderer>();
                    Sprite newSprite = ResourceManager.Instance.LoadResource(groupIconPath, typeof(Sprite), true) as Sprite;
                    spRenderer.sprite = newSprite;
                    spRenderer.transform.localScale = new Vector3(25, 25, 1);

                    armyGroupObj.GetComponent<BattleGroupIcon>().Init(row, col);
                }

                // rotate icon if it is at TOP side
                groupTrans.localRotation = row <= 2 ? Quaternion.AngleAxis(180f, Vector3.forward) : Quaternion.identity;
            }
        }
    }

    private void ReleaseGroupsInBattleField()
    {
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 12; col++)
            {
                var groupTrans = GetGroup(row, col);
                if (groupTrans.childCount <= 0) continue;
                Debug.Assert(groupTrans.childCount == 1);
                GameObject armyGroupObj = groupTrans.GetChild(0).gameObject;
                ResourceManager.Instance.CacheResourceIntoPool(armyGroupPrefabPath, armyGroupObj);
            }
        }
    }

    private void OnUpdateGroupsInBattleField()
    {
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 12; col++)
            {
                var groupTrans = GetGroup(row, col);

                var battleGroupData = BattleLogics.GetBattleGroupDataFromCurrentBattle(row, col);
                var amount = battleGroupData.GetTotalAmountAfterLoss();
                groupTrans.gameObject.SetActive(amount > 0);
            }
        }
    }

    // row: [0, 5], col: [0, 11]; From top to bottom, left to right.
    private Transform GetGroup(int row, int col)
    {
        if (row <= 2)
        {
            int index = (11 - col) + (2 - row) * 12;
            return groupTopRoot.Find(index.ToString());
        }
        else
        {
            int index = col + (row - 3) * 12;
            return groupBottomRoot.Find(index.ToString());
        }
    }

    private void SetGroupAlpha(Transform groupTrans, float alpha)
    {
        var spriteRenderer = groupTrans.GetChild(0).GetComponent<SpriteRenderer>();
        var color = spriteRenderer.material.color;
        color.a = alpha;
        spriteRenderer.material.color = color;
    }

    #region cubes related
    private Transform simulRoot;
    private List<SimulationPlayer> simulPlayerList;

    private void InitSimulationPlayerList()
    {
        for (int i = 0; i < 72; i++) 
        {
            GameObject spGameObj = new GameObject("SimulationPlayer");
            spGameObj.transform.SetParent(simulRoot);
            spGameObj.transform.ResetTransform();
            var newSimulPlayer = spGameObj.AddComponent<SimulationPlayer>();
            newSimulPlayer.PreloadCubes();
            simulPlayerList.Add(newSimulPlayer);
        }
    }

    private void ReleaseSimulationPlayerList()
    {
        foreach (var simulPlayer in simulPlayerList) simulPlayer.ReleaseCubes();
    }

    private void InitSimulationPlayer(ref SimulationPlayer simulPlayer, Vector3 pos, bool isTop, int cubeNum, float grayPercentage, bool enableSimulation)
    {
        simulPlayer.gameObject.SetActive(true);
        simulPlayer.transform.ResetTransform();
        simulPlayer.transform.position = pos; // using world position
        simulPlayer.transform.localScale = new Vector3(5, 5, 5); // it could be changed later.
        var config = new SimulationPlayerConfig();
        config.Init(cubeNum, grayPercentage); // init the cubes without Explosion Configs
        simulPlayer.Init(config, isTop, enableSimulation);
    }

    // similar logic to "GetClosestSimulPlayers()"
    private Transform GetClosestCenterGroupTrans(GroupSide side, AttackedDir dir)
    {
        int rowStart, rowEnd, colStart, colEnd;
        BattleLogics.GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
        int rowCenter = (rowStart + rowEnd - 1) / 2;
        int colCenter = (colStart + colEnd - 1) / 2;

        if (dir == AttackedDir.LEFT)
        {
            int colLeftMost = BattleLogics.GetFirstNonEmptyCol(side, true, false);
            if (colLeftMost != -1) return GetGroup(rowCenter, colLeftMost);
        }
        else if (dir == AttackedDir.RIGHT)
        {
            int colRightMost = BattleLogics.GetFirstNonEmptyCol(side, false, false);
            if (colRightMost != -1) return GetGroup(rowCenter, colRightMost);
        }
        else if (dir == AttackedDir.TOP)
        {
            int rowTopMost = BattleLogics.GetFirstNonEmptyRow(side, true, false);
            if (rowTopMost != -1) return GetGroup(rowTopMost, colCenter);
        }
        else if (dir == AttackedDir.BOTTOM)
        {
            int rowBottomMost = BattleLogics.GetFirstNonEmptyRow(side, false, false);
            if (rowBottomMost != -1) return GetGroup(rowBottomMost, colCenter);
        }
        else
        {
            throw new Exception("AttackedDir can not be NONE...");
        }
        return null;
    }

    private List<SimulationPlayer> GetClosestSimulPlayers(
        ref Dictionary<Tuple<int, int>, SimulationPlayer> simulPlayersDict,
        AttackedDir dir, int rowStart, int rowEnd, int colStart, int colEnd)
    {
        List<SimulationPlayer> result = new List<SimulationPlayer>();

        if (dir == AttackedDir.LEFT)
        {
            for (int col = colStart; col < colEnd; col++)
            {
                bool hasFound = false;
                for (int row = rowStart; row < rowEnd; row++)
                {
                    var key = new Tuple<int, int>(row, col);
                    if (simulPlayersDict.ContainsKey(key))
                    {
                        hasFound = true; // cause it will have the case the whole col has 3 groups.
                        result.Add(simulPlayersDict[key]);
                    }
                }
                if (hasFound) break;
            }
        }
        else if (dir == AttackedDir.RIGHT)
        {
            for (int col = colEnd - 1; col >= colStart; col--)
            {
                bool hasFound = false;
                for (int row = rowStart; row < rowEnd; row++)
                {
                    var key = new Tuple<int, int>(row, col);
                    if (simulPlayersDict.ContainsKey(key))
                    {
                        hasFound = true;
                        result.Add(simulPlayersDict[key]);
                    }
                }
                if (hasFound) break;
            }
        }
        else if (dir == AttackedDir.TOP)
        {
            for (int row = rowStart; row < rowEnd; row++)
            {
                bool hasFound = false;
                for (int col = colStart; col < colEnd; col++)
                {
                    var key = new Tuple<int, int>(row, col);
                    if (simulPlayersDict.ContainsKey(key))
                    {
                        hasFound = true;
                        result.Add(simulPlayersDict[key]);
                    }
                }
                if (hasFound) break;
            }
        }
        else if (dir == AttackedDir.BOTTOM)
        {
            for (int row = rowEnd - 1; row >= rowStart; row--)
            {
                bool hasFound = false;
                for (int col = colStart; col < colEnd; col++)
                {
                    var key = new Tuple<int, int>(row, col);
                    if (simulPlayersDict.ContainsKey(key))
                    {
                        hasFound = true;
                        result.Add(simulPlayersDict[key]);
                    }
                }
                if (hasFound) break;
            }
        }
        else
        {
            throw new Exception("AttackedDir can not be NONE...");
        }
        return result;
    }

    private List<Transform> fadeGroupTransList;
    private void OnFadeGroupsAlpha(float alpha)
    {
        foreach (var groupTrans in fadeGroupTransList)
            SetGroupAlpha(groupTrans, alpha);
    }

    private bool fadeAllCubes = false, fadeGroupIcons = false;
    private const float fadeCubesDuration = 1.0f, fadeGroupIconsDuration = 0.5f;
    private float fadePassTime1 = 0.0f, fadePassTime2 = 0.0f;
    private void OnTriggerFadeAllCubes()
    {
        fadeAllCubes = true;
        fadePassTime1 = 0.0f;
    }

    private void OnFadeAllCubes()
    {
        fadePassTime1 += Time.deltaTime;
        //Debug.Log("fadePassTime: " + fadePassTime);
        //float alpha = Mathf.Lerp(1, 0, fadePassTime / (fadeDuration - fadeWaitTime));
        float alpha = Mathf.Lerp(1, 0, fadePassTime1 / fadeCubesDuration);
        //alpha = Mathf.Pow(alpha, 1 / 2.2f); // gamma correction
        foreach(var simulPlayer in simulPlayerList)
            simulPlayer.SetAlpha(alpha);

        if (fadePassTime1 > fadeCubesDuration)
        {
            fadeAllCubes = false;
            OnHideAllCubes();
        }
    }

    private void OnHideAllCubes()
    {
        foreach (var simulPlayer in simulPlayerList)
            simulPlayer.gameObject.SetActive(false);
    }

    private void OnTriggerFadeAllGroupIcons()
    {
        fadeGroupIcons = true;
        fadePassTime2 = 0.0f;
    }
    private void OnFadeAllGroupIcons()
    {
        fadePassTime2 += Time.deltaTime;
        OnFadeGroupsAlpha(Mathf.Lerp(0, 1, fadePassTime2 / fadeGroupIconsDuration));
        if (fadePassTime2 > fadeGroupIconsDuration) fadeGroupIcons = false;
    }

    private void OnTriggerSimulation(int functionIndex)
    {
        foreach (var simulPlayer in simulPlayerList)
        {
            if (simulPlayer.gameObject.activeSelf)
                simulPlayer.OnTriggerExplosion(functionIndex);
        }
    }
    #endregion

    #region preview attack/defense
    private Transform atkRoot, defRoot;

    private void OnHideAllAtkHighlight(GroupSide side)
    {
        GetSelectUI(side).HideAllAttackedUI();
    }

    private void OnDisplayBeAttackedHighlight(GroupSide side, AttackedDir atkDir)
    {
        // attacked preview
        GetSelectUI(side).DisplayAttackedUI(BattleLogics.GetActiveRows(side, false), atkDir);
    }

    // highlight those group sides containing the battle group indicated by the card.
    private void OnDisplayBeSelectedHighlight(GroupSide side, bool gainArmor)
    {
        // defense preview
        GetSelectUI(side).DisplayDefenseUI(BattleLogics.GetActiveRows(side, false), gainArmor);
    }

    private SelectUI GetSelectUI(GroupSide side)
    {
        Transform parent = side.IsTop() ? atkRoot : defRoot;
        string transName = side.ToString().Split('_')[1].ToLower();
        return parent.Find(transName).GetComponent<SelectUI>();
    }

    private Dictionary<Tuple<GroupSide, int>, Vector3> groupRectCenterDict;
    private Vector3 GetGroupRectCenter(GroupSide side, int activeRows)
    {
        // if 'activeRow' is int, it means that it could be "ActiveRow.ONE | ActiveRow.TWO" something like that.
        return groupRectCenterDict[new(side, activeRows)];
    }

    private void InitGroupRectCenterDict()
    {
        groupRectCenterDict = new Dictionary<Tuple<GroupSide, int>, Vector3>();
        // one active row
        for (int i = 0; i < 3; i++)
        {
            int activeRows_TOP = 1 << i;
            int activeRows_BOTTOM = 1 << (i + 3);

            groupRectCenterDict[new(GroupSide.TOP_LEFT, activeRows_TOP)] = GetGroup(i, 1).position;
            groupRectCenterDict[new(GroupSide.BOTTOM_LEFT, activeRows_BOTTOM)] = GetGroup(i + 3, 1).position;

            groupRectCenterDict[new(GroupSide.TOP_MIDDLE, activeRows_TOP)] = (GetGroup(i, 5).position + GetGroup(i, 6).position) / 2;
            groupRectCenterDict[new(GroupSide.BOTTOM_MIDDLE, activeRows_BOTTOM)] = (GetGroup(i + 3, 5).position + GetGroup(i + 3, 6).position) / 2;

            groupRectCenterDict[new(GroupSide.TOP_RIGHT, activeRows_TOP)] = GetGroup(i, 10).position;
            groupRectCenterDict[new(GroupSide.BOTTOM_RIGHT, activeRows_BOTTOM)] = GetGroup(i + 3, 10).position;
        }

        // two active rows
        for (int i = 0; i < 2; i++)
        {
            bool isTop = i == 0;
            int rowStart = isTop ? 0 : 3;

            GroupSide left = isTop ? GroupSide.TOP_LEFT : GroupSide.BOTTOM_LEFT;
            GroupSide middle = isTop ? GroupSide.TOP_MIDDLE : GroupSide.BOTTOM_MIDDLE;
            GroupSide right = isTop ? GroupSide.TOP_RIGHT : GroupSide.BOTTOM_RIGHT;

            ActiveRow one = isTop ? ActiveRow.TOP_ONE : ActiveRow.BOTTOM_ONE;
            ActiveRow two = isTop ? ActiveRow.TOP_TWO : ActiveRow.BOTTOM_TWO;
            ActiveRow three = isTop ? ActiveRow.TOP_THREE : ActiveRow.BOTTOM_THREE;

            groupRectCenterDict[new(left, (int)one | (int)two)] = (GetGroup(rowStart, 1).position + GetGroup(rowStart + 1, 1).position) / 2;
            groupRectCenterDict[new(left, (int)two | (int)three)] = (GetGroup(rowStart + 1, 1).position + GetGroup(rowStart + 2, 1).position) / 2;
            groupRectCenterDict[new(left, (int)one | (int)three)] = GetGroup(rowStart + 1, 1).position;

            groupRectCenterDict[new(middle, (int)one | (int)two)] =
                ((GetGroup(rowStart, 5).position + GetGroup(rowStart + 1, 5).position) / 2 +
                (GetGroup(rowStart, 6).position + GetGroup(rowStart + 1, 6).position) / 2) / 2;
            groupRectCenterDict[new(middle, (int)two | (int)three)] =
                ((GetGroup(rowStart + 1, 5).position + GetGroup(rowStart + 2, 5).position) / 2 +
                (GetGroup(rowStart + 1, 6).position + GetGroup(rowStart + 2, 6).position) / 2) / 2;
            groupRectCenterDict[new(middle, (int)one | (int)three)] = (GetGroup(rowStart + 1, 5).position + GetGroup(rowStart + 1, 6).position) / 2;

            groupRectCenterDict[new(right, (int)one | (int)two)] = (GetGroup(rowStart, 10).position + GetGroup(rowStart + 1, 10).position) / 2;
            groupRectCenterDict[new(right, (int)two | (int)three)] = (GetGroup(rowStart + 1, 10).position + GetGroup(rowStart + 2, 10).position) / 2;
            groupRectCenterDict[new(right, (int)one | (int)three)] = GetGroup(rowStart + 1, 10).position;
        }

        // three active rows
        for (int i = 0; i < 2; i++)
        {
            bool isTop = i == 0;
            int rowCenter = isTop ? 1 : 4;

            GroupSide left = isTop ? GroupSide.TOP_LEFT : GroupSide.BOTTOM_LEFT;
            GroupSide middle = isTop ? GroupSide.TOP_MIDDLE : GroupSide.BOTTOM_MIDDLE;
            GroupSide right = isTop ? GroupSide.TOP_RIGHT : GroupSide.BOTTOM_RIGHT;

            ActiveRow one = isTop ? ActiveRow.TOP_ONE : ActiveRow.BOTTOM_ONE;
            ActiveRow two = isTop ? ActiveRow.TOP_TWO : ActiveRow.BOTTOM_TWO;
            ActiveRow three = isTop ? ActiveRow.TOP_THREE : ActiveRow.BOTTOM_THREE;

            groupRectCenterDict[new(left, (int)one | (int)two | (int)three)] = GetGroup(rowCenter, 1).position;
            groupRectCenterDict[new(middle, (int)one | (int)two | (int)three)] = (GetGroup(rowCenter, 5).position + GetGroup(rowCenter, 6).position) / 2;
            groupRectCenterDict[new(right, (int)one | (int)two | (int)three)] = GetGroup(rowCenter, 10).position;
        }
    }

    private void OnHideHighlight()
    {
        for (int i = 0; i < atkRoot.childCount; i++)
            atkRoot.GetChild(i).GetComponent<SelectUI>().HideUI();
        for (int i = 0; i < defRoot.childCount; i++)
            defRoot.GetChild(i).GetComponent<SelectUI>().HideUI();
    }

    private Transform arrowsRoot;
    private string arrowPrefabPath = "3d_prefab/battlefield/CurveMeshCreator";
    private Dictionary<Tuple<GroupSide, int, GroupSide, int, AttackedDir>, GameObject> curveArrowsDict;


    private Vector3 GetCurveArrowEndPos(GroupSide startSide, GroupSide endSide, AttackedDir atkDir)
    {
        int startSideActiveRow = (int)BattleLogics.GetActiveRows(startSide, false);
        int endSideActiveRow = (int)BattleLogics.GetActiveRows(endSide, false);
        Tuple<GroupSide, int, GroupSide, int, AttackedDir> key = new(startSide, startSideActiveRow, endSide, endSideActiveRow, atkDir);
        GameObject arrowGameObj = curveArrowsDict[key];
        return arrowGameObj.GetComponent<CurveMeshCreator>().GetEndPos();
    }

    private List<Vector3> GetCtrlPointsOfCurveArrow(GroupSide startSide, GroupSide endSide, AttackedDir atkDir)
    {
        int startSideActiveRow = (int)BattleLogics.GetActiveRows(startSide, false);
        int endSideActiveRow = (int)BattleLogics.GetActiveRows(endSide, false);
        Tuple<GroupSide, int, GroupSide, int, AttackedDir> key = new(startSide, startSideActiveRow, endSide, endSideActiveRow, atkDir);
        GameObject arrowGameObj = curveArrowsDict[key];
        return arrowGameObj.GetComponent<CurveMeshCreator>().GetCtronlPoints(); // return a copy of it.
    }

    private Vector3 GetHitpoint(GroupSide endSide, int activeRows, AttackedDir atkDir)
    {
        return GetSelectUI(endSide).GetHitPoint(activeRows, atkDir).position;
    }

    private GameObject CreateCurveArrow(
        GroupSide startSide, int startSideActiveRow, 
        GroupSide endSide, int endSideActiveRow, 
        AttackedDir atkDir)
    {
        //GroupSide startSide = ToGroupSide(startGroupIndex);
        //var startSideActiveRow = GetActiveRow(false, startSide);
        var rectSize1 = GetApproximateRectSize(startSideActiveRow, startSide);
        Vector3 startPos = GetGroupRectCenter(startSide, startSideActiveRow);

        //GroupSide endSide = ToGroupSide(endGroupIndex);
        //var activeRow2 = GetActiveRow(true, endSide);
        //var rectSize2 = GetApproximateRectSize(activeRow2, endSide);
        //Vector3 endPos = GetGroupRectCenter(true, endSide, (int)activeRow2);
        Vector3 endPos = GetHitpoint(endSide, endSideActiveRow, atkDir);

        //Debug.LogFormat("endPos: {0}, side: {1}, atkDir: {2}", endPos, endSide.ToString(), atkDir.ToString());

        //float arrowLen = 0.06f;
        Vector2? Ys = null;

        List<Vector2> bendDistList = new List<Vector2>();
        bool isStartSideInBottom = startSide.IsBottom();
        bool condition = (isStartSideInBottom && atkDir == AttackedDir.BOTTOM) || (!isStartSideInBottom && atkDir == AttackedDir.TOP);
        if (!BattleLogics.IsSameVerticalDir(startSide, endSide))
        {
            if (endSide.IsMiddle())
            {
                // LEFT/RIGHT to MIDDLE
                float signX = startSide.IsLeft() ? 1 : -1;
                float signY = startSide.IsBottom() ? 1 : -1;
                if (condition)
                {
                    startPos.x += rectSize1.x * 0.8f * signX;
                    startPos.y += (rectSize1.y + 1.5f * groupSize.y) * signY;
                    //endPos.x -= rectSize2.x * 0.5f * sign;
                    //endPos.y -= (rectSize2.y + 1.5f * groupSize.y);
                    bendDistList.Add(new Vector2(0.5f, -signX * signY * 0.3f)); // intermediate points local coord in its coordinate.
                }
                else
                {
                    // it means left/right to attack the middle's left/right/top side
                    if (isStartSideInBottom)
                        Debug.Assert(atkDir == AttackedDir.LEFT || atkDir == AttackedDir.RIGHT || atkDir == AttackedDir.TOP);
                    else
                        Debug.Assert(atkDir == AttackedDir.LEFT || atkDir == AttackedDir.RIGHT || atkDir == AttackedDir.BOTTOM);

                    if (atkDir == AttackedDir.LEFT || atkDir == AttackedDir.RIGHT)
                    {
                        startPos.x -= rectSize1.x * 0.5f * signX;
                        startPos.y += rectSize1.y * signY;
                        //endPos.x -= (rectSize2.x * 0.6f * sign);
                        bendDistList.Add(new Vector2(0.4f, signX * signY * 0.8f));
                        //arrowLen = 0.015f;
                    }
                    else
                    {
                        startPos.x -= rectSize1.x * signX;
                        startPos.y += rectSize1.y * signY;
                        //endPos.x -= rectSize2.x * 0.3f * sign;
                        //endPos.y += (rectSize2.y + groupSize.y);
                        bendDistList.Add(new Vector2(0.3f, signX * signY * 1.0f));
                        bendDistList.Add(new Vector2(1.3f, signX * signY * 0.5f));
                        //arrowLen = 0.01f;
                    }
                }
            }
            else
            {
                // MIDDLE to (LEFT or RIGHT)
                float signX = endSide.IsLeft() ? 1 : -1;
                float signY = startSide.IsBottom() ? 1 : -1;
                if (condition)
                {
                    startPos.x -= rectSize1.x * 0.4f * signX;
                    startPos.y += rectSize1.y * 1.5f * signY;
                    //endPos.x += rectSize2.x * 0.5f * sign;
                    //endPos.y -= (rectSize2.y + 1.5f * groupSize.y);

                    bendDistList.Add(new Vector2(0.5f, signX * signY * 0.2f)); // intermediate points local coord in its coordinate.
                }
                else
                {
                    Debug.Assert(atkDir == AttackedDir.LEFT || atkDir == AttackedDir.RIGHT);
                    startPos.x -= rectSize1.x * 0.6f * signX;

                    //endPos.x -= rectSize2.x * sign;
                    bendDistList.Add(new Vector2(0.4736122f, signX * signY * 0.5932584f)); // intermediate points local coord in its coordinate.
                    bendDistList.Add(new Vector2(0.7950704f, signX * signY * 0.7599084f)); // intermediate points local coord in its coordinate.
                    bendDistList.Add(new Vector2(1.1766f, signX * signY * 0.7205065f)); // intermediate points local coord in its coordinate.
                    bendDistList.Add(new Vector2(1.264231f, signX * signY * 0.3415604f)); // intermediate points local coord in its coordinate.
                    //arrowLen = 0.01f;
                    Ys = new Vector2(1f, 1f);
                }
            }
        }
        else
        {
            // same sides vertically attack: left to left, middle to middle, right to right.
            float signY = startSide.IsBottom() ? 1 : -1;
            if (condition)
            {
                // straight attack, linear arrow, no intermediate points
                startPos.y += (rectSize1.y + 1.5f * groupSize.y) * signY;
                //endPos.y -= (rectSize2.y + 1.5f * groupSize.y);
            }
            else
            {
                // middle can only do BOTTOM or TOP attack for another middle.
                Debug.LogFormat("startSide:{0}, endSide:{1}, atkDir:{2}", startSide, endSide, atkDir);

                // because from previous two conditions checking: we know startSide is the same direction vertically as endSide,
                // also atkDir is not BOTTOM. However, middle can not attack top.
                if (startSide == GroupSide.BOTTOM_MIDDLE) Debug.Assert(atkDir != AttackedDir.TOP);
                else if (startSide == GroupSide.TOP_MIDDLE) Debug.Assert(atkDir != AttackedDir.BOTTOM);

                float signX = startSide.IsLeft() ? 1 : -1;
                startPos.x -= rectSize1.x * signX;
                startPos.y += signY * rectSize1.y;
                //endPos.x -= rectSize2.x * sign;
                bendDistList.Add(new Vector2(0.5f, signX * signY * 1.0f));
                //arrowLen = 0.03f;
            }
        }

        int sampleNum = 50 + 100 * (bendDistList.Count);
        CurveArrowConfig config = new CurveArrowConfig(startPos, endPos, bendDistList, true, 0.5f, sampleNum);
        GameObject arrowGameObj = ResourceManager.Instance.FetchResourceFromPool(arrowPrefabPath);
        arrowGameObj.transform.SetParent(arrowsRoot);
        arrowGameObj.transform.ResetTransform();

        var meshRenderer = arrowGameObj.GetComponent<MeshRenderer>();
        // TODO: perhaps we can use MaterialBlock for better performance?
        meshRenderer.material = GameObject.Instantiate(meshRenderer.material); // copy the material
        //meshRenderer.material.SetFloat("_ArrowLen", arrowLen); // set the arrow length accordingly

        if (Ys != null)
        {
            meshRenderer.material.SetFloat("_Y1", Ys.Value.x); // set the interpolation threshold accordingly
            meshRenderer.material.SetFloat("_Y2", Ys.Value.y); // set the interpolation threshold accordingly
        }

        var curveArrow = arrowGameObj.GetComponent<CurveMeshCreator>();
        curveArrow.UpdateCtrlPoints(config);
        curveArrow.GenerateMesh();
        return arrowGameObj;
    }

    private void OnDisplayCurveArrow(GroupSide startSide, GroupSide endSide, AttackedDir atkDir)
    {
        int startSideActiveRow = BattleLogics.GetActiveRows(startSide, false);
        int endSideActiveRow = BattleLogics.GetActiveRows(endSide, false);

        Tuple<GroupSide, int, GroupSide, int, AttackedDir> key = new(startSide, startSideActiveRow, endSide, endSideActiveRow, atkDir);

        if (!curveArrowsDict.ContainsKey(key))
            curveArrowsDict[key] = CreateCurveArrow(startSide, startSideActiveRow, endSide, endSideActiveRow, atkDir);

        GameObject arrowGameObj = curveArrowsDict[key];
        arrowGameObj.SetActive(true);
    }

    private void OnHideAllCurveArrows()
    {
        foreach (var arrowGameObj in curveArrowsDict.Values) 
            arrowGameObj.SetActive(false);
    }

    private void ReleaseAllCurveArrows()
    {
        foreach (var arrowGameObj in curveArrowsDict.Values)
            ResourceManager.Instance.CacheResourceIntoPool(arrowPrefabPath, arrowGameObj);
    }
    #endregion

    #region VFX related
    private Transform vfxsRoot;
    private Dictionary<GameObject, float> effectPlayerObjDict;

    private EffectConfig GenerateEffectConfig_Linear(
        Vector3 startPos,
        Vector3 endPos,
        string projectile, string muzzle, string hit,
        float duration, Vector3 scale)
    {
        // we just need two position to determine the direction.
        EffectConfig config = new EffectConfig();
        config.points = new List<Vector2>() { startPos , endPos };
        config.baseHeight = startPos.z;
        config.maxHeight = endPos.z; // because we are going to use linear height.
        config.heightScale = 0;
        config.duration = duration;
        config.sampleFunc = null;
        config.projectilePrefabPath = projectile;
        config.muzzlePrefabPath = muzzle;
        config.hitPrefabPaths = new List<string>() { hit };
        config.vfxScales = scale;
        return config;
    }

    private EffectConfig GenerateEffectConfig_Bezier(
        GroupSide startSide, GroupSide endSide, AttackedDir atkDir,
        string projectile, string muzzle, string hit, float duration, Vector3 scale)
    {
        List<Vector3> ctrlPoints = GetCtrlPointsOfCurveArrow(startSide, endSide, atkDir);
        
        // we need a new end point on the center group at the closest row or col
        // which has at least one group(non-empty row/col)
        Vector3 newEndPos = ctrlPoints[ctrlPoints.Count - 1];
        Transform groupTrans = GetClosestCenterGroupTrans(endSide, atkDir);
        if (atkDir == AttackedDir.LEFT || atkDir == AttackedDir.RIGHT)
            newEndPos.x = groupTrans.position.x; // only change the x
        else
            newEndPos.y = groupTrans.position.y;
        //ctrlPoints.Add(extraEndPos);
        ctrlPoints[ctrlPoints.Count - 1] = newEndPos; // update it

        EffectConfig config = new EffectConfig();
        config.points = new List<Vector2>();
        foreach (var p in ctrlPoints) config.points.Add(new Vector2(p.x, p.y));
        config.baseHeight = ctrlPoints[0].z;
        config.maxHeight = config.baseHeight; // because the Bezier used here is on XY plane.
        config.heightScale = 0;
        config.duration = duration;

        // [Note] use this website for visualizeing the function: [link](https://www.desmos.com/calculator)
        float exponent = 2f;
        float threshold = 0.5f;
        config.sampleFunc = (float x) =>
        {
            if (x <= threshold) return Mathf.Pow(x, exponent);
            else return (1 - Mathf.Pow(threshold, exponent)) / (1 - threshold) * (x - 1) + 1;
        };

        if(startSide == GroupSide.BOTTOM_MIDDLE && (endSide == GroupSide.TOP_LEFT || endSide == GroupSide.TOP_RIGHT))
        {
            // center to left/right
            config.alphaFunc = (float x) =>
            {
                if (x < 0.4f) return 0;
                else return Mathf.Lerp(0, 1, (x - 0.4f) / 0.6f);
            };
        }

        config.projectilePrefabPath = projectile;
        config.muzzlePrefabPath = muzzle;
        config.hitPrefabPaths = new List<string>() { hit };
        config.vfxScales = scale;
        return config;
    }

    private EffectPlayer PlayEffect(EffectConfig config)
    {
        // TODO: actually no need to cache the epGameObj, we should preload the VFXs itself: Resources.Load, only preload.
        GameObject epGameObj = GetReusedEffectPlayer();
        var effectPlayer = epGameObj.GetComponent<EffectPlayer>();
        effectPlayer.Init(config);
        effectPlayerObjDict[epGameObj] = config.duration + 1f; // life time is (config.duration + 1f)
        return effectPlayer;
    }

    private void UpdateEffectPlayers()
    {
        effectPlayerKeyBuffer.Clear();
        inactiveEffectPlayerBuffer.Clear();

        foreach (var epGameObj in effectPlayerObjDict.Keys)
            effectPlayerKeyBuffer.Add(epGameObj);

        for (int i = 0; i < effectPlayerKeyBuffer.Count; i++)
        {
            var epGameObj = effectPlayerKeyBuffer[i];
            var oldRemainTime = effectPlayerObjDict[epGameObj];
            var newRemainTime = oldRemainTime - Time.deltaTime;
            effectPlayerObjDict[epGameObj] = newRemainTime;

            if (oldRemainTime > 0 && newRemainTime <= 0)
                inactiveEffectPlayerBuffer.Add(epGameObj);
        }

        for (int i = 0; i < inactiveEffectPlayerBuffer.Count; i++)
            inactiveEffectPlayerBuffer[i].SetActive(false);
    }

    public static int GetEffectPlayerMaxNum()
    {
        // TODO: to reduce the possible totalNum below,
        float shootNumRatioMAX = GetShootNumRatioMAX();
        int maxVFXNumInOneGroup = Mathf.RoundToInt(Mathf.Sqrt(BattleGroupData.maxAmount) * shootNumRatioMAX);
        // from side to side, within one group side, each group can hit 18 at maximum
        int maxVFXNumFromOneGroupSide = maxVFXNumInOneGroup * 18;
        int totalNum = maxVFXNumFromOneGroupSide * 3;
        return totalNum;
    }

    private void InitEffectPlayerObjDict()
    {
        var totalNum = GetEffectPlayerMaxNum();
        Debug.LogFormat("InitEffectPlayerObjDict, totalNum: {0}", totalNum);
        // preload a lot of effectPlayers
        for (int i = 0; i < totalNum; i++)
        {
            var effectPlayer = CreateEffectPlayer();
            effectPlayer.SetActive(false);
            effectPlayerObjDict.Add(effectPlayer, 0f);
        }
    }

    private void ReleaseEffectPlayerObjDict()
    {
        foreach (var effectPlayer in effectPlayerObjDict.Keys) effectPlayer.GetComponent<EffectPlayer>().ReleaseResources();
    }

    private GameObject GetReusedEffectPlayer()
    {
        foreach (var epGameObj in effectPlayerObjDict.Keys)
        {
            if (!epGameObj.activeSelf)
            {
                epGameObj.transform.ResetTransform();
                epGameObj.SetActive(true);
                return epGameObj;
            }
        }

        Debug.Log("Can not find a reused EffectPlayer!!!");

        var effectPlayer = CreateEffectPlayer();
        effectPlayerObjDict.Add(effectPlayer, 0f);
        return effectPlayer;
    }
    private GameObject CreateEffectPlayer()
    {
        GameObject epGameObj = new GameObject("EffectPlayer");
        epGameObj.transform.SetParent(vfxsRoot);
        epGameObj.transform.ResetTransform();
        epGameObj.AddComponent<EffectPlayer>();
        return epGameObj;
    }

    private void OnHideAllVFXs()
    {
        foreach (var epGameObj in effectPlayerObjDict.Keys)
            epGameObj.SetActive(false);
    }

    private float PlaySingleHitEffect(string hitVFX, GroupSide side, float delayTime, float hitVFXPlayTime)
    {
        // play efefct at the center point of the whole group side
        var activeRows = BattleLogics.GetActiveRows(side, false);
        Vector3 startPos = GetGroupRectCenter(side, activeRows);
        Vector3 endPos = startPos + Vector3.down;
        var scale = Table_constant.data["constant_0013"].param_float_list[side.IsMiddle() ? 0 : 1];
        Vector3 vfxScale = new Vector3(scale, scale, scale);
        var effectConfig = GenerateEffectConfig_Linear(startPos, endPos, null, null, hitVFX, delayTime, vfxScale);
        PlayEffect(effectConfig); // it will play the hit effect after 'waitTime'(delayTime) seconds
        return delayTime + hitVFXPlayTime; // hitVFXPlayTime in seconds for playing the hit vfx
    }

    private float OnPlayUpdateStatusEffect(int functionIndex, BattleActionEffectType actionEffectType, float delayTime)
    {
        if(Utils.HasAttribute<MultiplierFlag, BattleActionEffectType>(actionEffectType))
        {
            // for "MORAL_MULTIPLIER", "DISCIPLINE_MULTIPLIER"
            // TODO: for later new types, we can set its corresponding BattleActionEffectType to the a new custom attribute.
            string inputStr = actionEffectType.ToString().Replace("_MULTIPLIER", "");
            actionEffectType = Utils.ConvertStrToEnum<BattleActionEffectType>(inputStr);
        }

        string hitVFX;
        float totalTime = delayTime;
        var function = curCardData.functionList[functionIndex];
        bool isClientRound = BattleManager.Instance.IsClientRound();
        const bool needApplyLossForUI = false; // we set it false because we have not actually execute the attack/defense action
        var targetSides = BattleLogics.GetGroupSides(function.target, isClientRound, needApplyLossForUI);
        foreach(var side in targetSides)
        {
            float statusDelta = deltaStatusCache[new(side, actionEffectType)];
            AssetsMapper.GetUpdateStatusVFXPaths(actionEffectType, statusDelta, out hitVFX);
            Debug.Assert(hitVFX != null);
            var time = PlaySingleHitEffect(hitVFX, side, delayTime, 0.5f);
            totalTime = Mathf.Max(totalTime, time);
        }

        return totalTime + 0.5f; // wait a little bit
    }

    private float OnPlayEffectOnPlayer(float delayTime, string hitVFX, float hitVFXPlayTime)
    {
        float totalTime = delayTime;
        bool isClientRound = BattleManager.Instance.IsClientRound();
        BattleLogics.IterateGroupSide((GroupSide side) =>
        {
            if (BattleLogics.HasAtLeastOneGroupInGroupSide(side, false))
            {
                if ((isClientRound && side.IsBottom()) || (!isClientRound && side.IsTop()))
                {
                    var time = PlaySingleHitEffect(hitVFX, side, delayTime, hitVFXPlayTime);
                    totalTime = Mathf.Max(totalTime, time);
                }
            }
        });
        return totalTime + 0.5f; // wait a little bit
    }
    #endregion

    private Vector2 GetApproximateRectSize(int activeRows, GroupSide side)
    {
        bool hasRow1 = BattleLogics.HasActiveRow(activeRows, side.IsTop() ? ActiveRow.TOP_ONE : ActiveRow.BOTTOM_ONE);
        bool hasRow2 = BattleLogics.HasActiveRow(activeRows, side.IsTop() ? ActiveRow.TOP_TWO : ActiveRow.BOTTOM_TWO);
        bool hasRow3 = BattleLogics.HasActiveRow(activeRows, side.IsTop() ? ActiveRow.TOP_THREE : ActiveRow.BOTTOM_THREE);

        float y;
        if (hasRow1 && hasRow3) y = 3f;
        else if (hasRow1 && hasRow2 || hasRow2 && hasRow3) y = 2f;
        else y = 1f;

        float x = side.IsMiddle() ? 8 : 3;

        return groupSize * new Vector2(x, y);
    }

    #region HP/Status bar related
    private Dictionary<GroupSide, HPBarComponent> hpbarCompDict;

    private void InitHPBarCompDict()
    {
        hpbarCompDict = new Dictionary<GroupSide, HPBarComponent>();
        var curBattle = BattleManager.Instance.GetCurrentBattle();
        var parameters = new List<Tuple<GroupSide, int, string>>()
        {
            new (GroupSide.TOP_LEFT, curBattle.GetTotalAmountByGroupSide(GroupSide.TOP_LEFT, false), "top_root/top_gui/left/hp_bar"),
            new (GroupSide.TOP_MIDDLE, curBattle.GetTotalAmountByGroupSide(GroupSide.TOP_MIDDLE, false), "top_root/top_gui/center/hp_bar"),
            new (GroupSide.TOP_RIGHT, curBattle.GetTotalAmountByGroupSide(GroupSide.TOP_RIGHT, false), "top_root/top_gui/right/hp_bar"),
            new (GroupSide.BOTTOM_LEFT, curBattle.GetTotalAmountByGroupSide(GroupSide.BOTTOM_LEFT, false), "bottom_root/bottom_gui/left/hp_bar"),
            new (GroupSide.BOTTOM_MIDDLE, curBattle.GetTotalAmountByGroupSide(GroupSide.BOTTOM_MIDDLE, false), "bottom_root/bottom_gui/center/hp_bar"),
            new (GroupSide.BOTTOM_RIGHT, curBattle.GetTotalAmountByGroupSide(GroupSide.BOTTOM_RIGHT, false), "bottom_root/bottom_gui/right/hp_bar")
        };

        foreach(var param in parameters)
        {
            var trans = root.transform.Find(param.Item3);
            var hpbarComp = trans.GetComponent<HPBarComponent>();
            hpbarCompDict[param.Item1] = hpbarComp;
            var totalAmount = param.Item2;
            var totalArmor = BattleManager.Instance.GetCurrentBattle().GetGroupsideBattleDefense(param.Item1);
            hpbarComp.SetHPArmor(totalAmount, totalAmount, totalArmor);
            hpbarComp.gameObject.SetActive(true);
            hpbarComp.SetGroupSide(param.Item1);
        }
    }

    private Dictionary<GroupSide, StatusBar> statusBarDict;
    private void InitStatusBarDict()
    {
        statusBarDict = new Dictionary<GroupSide, StatusBar>();
        var parameters = new List<Tuple<GroupSide, bool, string>>()
        {
            new (GroupSide.TOP_LEFT, BattleLogics.HasAtLeastOneGroupInGroupSide(GroupSide.TOP_LEFT, false), "top_root/top_gui/left/status_bar"),
            new (GroupSide.TOP_MIDDLE, BattleLogics.HasAtLeastOneGroupInGroupSide(GroupSide.TOP_MIDDLE, false), "top_root/top_gui/center/status_bar"),
            new (GroupSide.TOP_RIGHT, BattleLogics.HasAtLeastOneGroupInGroupSide(GroupSide.TOP_RIGHT, false), "top_root/top_gui/right/status_bar"),
            new (GroupSide.BOTTOM_LEFT, BattleLogics.HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_LEFT, false), "bottom_root/bottom_gui/left/status_bar"),
            new (GroupSide.BOTTOM_MIDDLE, BattleLogics.HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_MIDDLE, false), "bottom_root/bottom_gui/center/status_bar"),
            new (GroupSide.BOTTOM_RIGHT, BattleLogics.HasAtLeastOneGroupInGroupSide(GroupSide.BOTTOM_RIGHT, false), "bottom_root/bottom_gui/right/status_bar")
        };

        foreach (var param in parameters)
        {
            var trans = root.transform.Find(param.Item3);
            var statusBar = trans.GetComponent<StatusBar>();
            statusBar.SetVisible(param.Item2);
            statusBarDict.Add(param.Item1, statusBar);
        }
    }

    private void OnUpdateArmor(GroupSide side)
    {
        Debug.LogFormat("OnUpdateArmor, side:{0}", side.ToString());
        var hpBarComp = GetHPBarComp(side);
        float totalArmor = BattleManager.Instance.GetCurrentBattle().GetGroupsideBattleDefense(side);
        hpBarComp.OnUpdateArmor(totalArmor);
    }

    private HPBarComponent GetHPBarComp(GroupSide side)
    {
        return hpbarCompDict[side];
    }

    private void OnPreviewDamage(GroupSide side)
    {
        float damage = BattleManager.Instance.GetCurrentBattle().GetTotalIncomingLossByGroupSide(side);
        if (damage == 0) return;
        var hpbarComp = GetHPBarComp(side);
        hpbarComp.OnPreviewDamage(damage);
        Debug.LogFormat("OnPreviewDamage: side:{0}, damage(loss):{1}", side.ToString(), damage);
        hpbarComp.TestPrint();
    }

    private void OnEndPreviewDamage()
    {
        foreach (var hpBarComp in hpbarCompDict.Values) hpBarComp.EndPreviewDamage();
    }

    private void OnReceiveDamage(GroupSide side)
    {
        // apply the preview damage...
        OnApplyUnitLoss(side);
        EventManager.Instance.SendEventSync(Core.Events.EventType.UI_OnUpdateHPBar, side);
        // we also need to update the armor
        OnUseDeltaDefenseCache();
    }

    private void OnPreviewArmor(GroupSide side)
    {
        float armor = BattleLogics.GetGroupsideBattleDefense(ref deltaDefenseCache, side);
        if (armor == 0) return;
        var hpbarComp = GetHPBarComp(side);
        hpbarComp.OnPreviewIncomingArmor(armor);
    }

    private void OnEndPreviewArmor()
    {
        foreach (var hpBarComp in hpbarCompDict.Values) hpBarComp.EndPreviewIncomingArmor();
    }

    private void OnReceiveArmor()
    {
        // TODO: actually the amount dict update logics should be similar to this one, we just cache the changement of data
        // once done playing VFXs, we apply them back to the real data stored in Battle. 
        // then send an event to let UI to update.
        OnUseDeltaDefenseCache(); // change the real data in Battle.

        //var curBattle = BattleManager.Instance.GetCurrentBattle();
        //var hpbarComp = GetHPBarComp(side);
        //hpbarComp.OnUpdateArmor(curBattle.GetGroupsideBattleDefense(side));
    }

    private bool HasArmor(GroupSide side)
    {
        var hpbarComp = GetHPBarComp(side);
        return hpbarComp.GetArmor() > 0;
    }
    #endregion

    private void TryPreviewCard(BattleCardData cardData)
    {
        BeforePreviewCard();

        Debug.Log("TryPreviewCard, action id = " + cardData.actionData.id);

        curCardData = cardData;

        //var cardid = string.Format("tactic_{0:0000}", 7);
        //curCardData = new BattleCardData(cardid); // test
        //curCardData = new BattleCardData("tactic_0037"); // test 
        //curCardData = new BattleCardData("tactic_0002"); // test 
        //curCardData = new BattleCardData("tactic_0048"); // test 
        //curCardData = new BattleCardData("tactic_0041"); // test 
        //curCardData = new BattleCardData("tactic_0028"); // test 
        
#if UNITY_EDITOR
        if(!string.IsNullOrEmpty(GMFunc.GMTestCardID))
        {
            Debug.LogFormat("Using GMFunc.GMTestCardID = {0} as cardID", GMFunc.GMTestCardID);
            curCardData = new BattleCardData(GMFunc.GMTestCardID);
            GMFunc.GMTestCardID = null;
        }
#endif

        Debug.Log("Description: " + curCardData.actionData.description);

        OnPreviewCard();
    }

    public void BeforePreviewCard()
    {
        curCardData = null;
        fadeGroupTransList.Clear();
        totalAtkArrowParamDict.Clear();
        siegeAttackHitPosDict.Clear();

        ResetGroupHighlightList();

        // hide all Attack background hightlight first (because we active the attack background one by one in below)
        BattleLogics.IterateGroupSide((GroupSide side) => 
        {
            if (BattleLogics.HasAtLeastOneGroupInGroupSide(side, false)) OnHideAllAtkHighlight(side);
        });

        ResetDeltaDefenseCache();
        ResetDeltaStatusCache();
    }

    // when players cancel preview.
    private void EndPreviewCard()
    {
        Debug.Log("EndPreviewCard");

        // recover displaying all group icons
        OnFadeGroupsAlpha(1);

        // hide highlights
        OnHideHighlight();

        // hide curve arrows
        OnHideAllCurveArrows();

        // hide cubes
        OnHideAllCubes();

        // remove the preview damage/armor
        OnEndPreviewDamage();
        OnEndPreviewArmor();

        BattleManager.Instance.GetCurrentBattle().ResetBattleFieldDataIncomingLossDict();
    }

    private Dictionary<Tuple<int, int>, SimulationPlayer> OnDisplayCubes(bool isClientRound)
    {
        var simulPlayerDict = new Dictionary<Tuple<int, int>, SimulationPlayer>();
        // initialize cubes on the BattleField
        for (int row = 0; row < 6; row++)
        {
            for (int col = 0; col < 12; col++)
            {
                int dataIndex = row * 12 + col;
                SimulationPlayer simulPlayer = simulPlayerList[dataIndex];

                var battleGroupData = BattleLogics.GetBattleGroupDataFromCurrentBattle(row, col);
                var amount = battleGroupData.GetTotalAmountBeforeLoss();
                if (amount <= 0)
                {
                    simulPlayer.gameObject.SetActive(false);
                    continue;
                }

                bool isTop = row < 3;
                float grayPercentage = 1.0f - groupHighlightList[dataIndex];

                var groupTrans = GetGroup(row, col);
                //SimulationPlayer simulPlayer = GetReusedSimulationPlayer();
                float cubeNumF = (float)(SimulationPlayer.maxCubeNum) * amount / battleGroupData.GetTotalMaxAmount();
                var cubeNum = Mathf.CeilToInt(cubeNumF);
                //if(cubeNum != SimulationPlayer.maxCubeNum)
                //{
                //    Debug.LogFormat("row:{0}, col:{1}, cubeNum:{2}", row, col, cubeNum);
                //}
                bool enableSimulation = (isClientRound && isTop) || (!isClientRound && !isTop);
                InitSimulationPlayer(ref simulPlayer, groupTrans.position, isTop, cubeNum, grayPercentage, enableSimulation);
                SetGroupAlpha(groupTrans, 0f); // equivalent to hide the icons but not actually set its activeSelf.
                fadeGroupTransList.Add(groupTrans);
                simulPlayerDict.Add(new(row, col), simulPlayer);
            }
        }
        return simulPlayerDict;
    }

    private void ResetGroupHighlightList()
    {
        for (int i = 0; i < groupHighlightList.Count; i++) groupHighlightList[i] = 0;
    }

    // highlight all groups in a groupside
    //private void UpdateGroupHighlightList(GroupSide side)
    //{
    //    int rowStart, rowEnd, colStart, colEnd;
    //    BattleLogics.GetRowColStartEnd(side, out rowStart, out rowEnd, out colStart, out colEnd);
    //    for (int row = rowStart; row < rowEnd; row++)
    //    {
    //        for (int col = colStart; col < colEnd; col++)
    //        {
    //            int dataIndex = row * 12 + col;
    //            groupHighlightList[dataIndex] = true;
    //        }
    //    }
    //}

    private void UpdateGroupHighlightList(
        ref HashSet<Tuple<int, int, BattleGroupType>> totalHighlightFriendUnits,
        ref HashSet<Tuple<int, int, BattleGroupType>> totalHighlightEnemyUnits)
    {
        var attackerActiveAmountDict = new Dictionary<Tuple<int, int>, float>();

        foreach (var tuple in totalHighlightFriendUnits)
        {
            var key = new Tuple<int, int>(tuple.Item1, tuple.Item2);
            var battleGroupData = BattleLogics.GetBattleGroupDataFromCurrentBattle(tuple.Item1, tuple.Item2);
            var activeAmount = battleGroupData.GetAmountByBattleGroupType(tuple.Item3, false);
            if (!attackerActiveAmountDict.ContainsKey(key)) attackerActiveAmountDict.Add(key, 0);
            attackerActiveAmountDict[key] += activeAmount;
        }

        // for defenders(enemy), highlight them all
        foreach (var tuple in totalHighlightEnemyUnits) 
        {
            int dataIndex = tuple.Item1 * 12 + tuple.Item2;
            groupHighlightList[dataIndex] = 1;
        }

        // for attackers, only the ratio = (amount of active soliders) / (total amount)
        foreach(var kv in attackerActiveAmountDict)
        {
            var battleGroupData = BattleLogics.GetBattleGroupDataFromCurrentBattle(kv.Key.Item1, kv.Key.Item2);
            var totalAmount = battleGroupData.GetTotalAmountBeforeLoss();
            float highlightRatio = kv.Value / totalAmount;
            int dataIndex = kv.Key.Item1 * 12 + kv.Key.Item2;
            groupHighlightList[dataIndex] = highlightRatio;
        }
    }

    private float GetExplosionForce(bool hasArmor, BattleActionEffectType actionEffectType)
    {
        float basicForce = hasArmor ? Table_constant.data["constant_0002"].param_float_list[0] :
            Table_constant.data["constant_0002"].param_float_list[1];
        float forceRatio;
        switch (actionEffectType)
        {
            case BattleActionEffectType.SIEGE_ATTACK:
                forceRatio = Table_constant.data["constant_0004"].param_float; break;
            case BattleActionEffectType.PROJECTILE_ATTACK:
                forceRatio = Table_constant.data["constant_0005"].param_float; break;
            case BattleActionEffectType.ARMOUR_PIERCING_ATTACK:
                forceRatio = Table_constant.data["constant_0006"].param_float; break;
            case BattleActionEffectType.PRIOTIZED_ATTACK:
                forceRatio = Table_constant.data["constant_0007"].param_float; break;
            case BattleActionEffectType.MELEE_ATTACK:
                forceRatio = Table_constant.data["constant_0008"].param_float; break;
            default:
                forceRatio = 1f; break;
        }
        Debug.LogFormat("actionEffectType:{0}, forceRatio:{1}, basicForce:{2}, finalForce:{3}",
            actionEffectType.ToString(), forceRatio, basicForce, forceRatio * basicForce);
        return forceRatio * basicForce;
    }

    private void AddExplosionConfigByStrategy(
        ref Dictionary<Tuple<int, int>, SimulationPlayer> simulPlayersDict,
        int functionIndex,
        GroupSide startSide,
        GroupSide endSide,
        AttackedDir atkDir)
    {
        // [Note] only those cubes closed to the bounding box will have explosion config.
        int rowStart, rowEnd, colStart, colEnd;
        BattleLogics.GetRowColStartEnd(endSide, out rowStart, out rowEnd, out colStart, out colEnd);
        var function = curCardData.functionList[functionIndex];
        //var explosionConfig = GenerateExplosionConfig(startSide, endSide, atkDir, function.effect);

        bool hasArmor = HasArmor(endSide);
        float radius = Table_constant.data["constant_0001"].param_float;
        float force = GetExplosionForce(hasArmor, function.effect);
        float zOffset = hasArmor ? Table_constant.data["constant_0003"].param_float_list[0] :
            Table_constant.data["constant_0003"].param_float_list[1];
        switch (function.effect)
        {
            case BattleActionEffectType.MELEE_ATTACK:
            case BattleActionEffectType.PRIOTIZED_ATTACK:
                {
                    // explosion center should be the hit point(end point of curve arrow), and only the closest row/col will be affected.
                    Vector3 center = GetCurveArrowEndPos(startSide, endSide, atkDir);
                    center.z += zOffset; // in order let the cubes fly up
                    var explosionConfig = new ExplosionConfig(center, radius, force);
                    var simulPlayers = GetClosestSimulPlayers(ref simulPlayersDict, atkDir, rowStart, rowEnd, colStart, colEnd);
                    foreach (var simulPlayer in simulPlayers) simulPlayer.AddExplosionConfig(functionIndex, explosionConfig);
                }
                break;

            case BattleActionEffectType.PROJECTILE_ATTACK:
                {
                    foreach(var simulPlayer in simulPlayersDict.Values)
                    {
                        Vector3 center = simulPlayer.transform.position;
                        center.z += zOffset; // in order let the cubes fly up
                        var explosionConfig = new ExplosionConfig(center, radius, force);
                        simulPlayer.AddExplosionConfig(functionIndex, explosionConfig);
                    }
                }
                break;

            case BattleActionEffectType.ARMOUR_PIERCING_ATTACK:
                {
                    var simulPlayers = GetClosestSimulPlayers(ref simulPlayersDict, atkDir, rowStart, rowEnd, colStart, colEnd);
                    foreach (var simulPlayer in simulPlayers)
                    {
                        Vector3 center = simulPlayer.transform.position;
                        center.z += zOffset; // in order let the cubes fly up

                        float signX = atkDir == AttackedDir.LEFT ? -1f : (atkDir == AttackedDir.RIGHT ? +1f : 0);
                        center.x += (signX * groupSize.x * 0.5f);

                        float signY = atkDir == AttackedDir.BOTTOM ? -1f : (atkDir == AttackedDir.TOP ? +1f : 0);
                        center.y += (signY * groupSize.y * 0.5f);
                        
                        var explosionConfig = new ExplosionConfig(center, radius, force);
                        simulPlayer.AddExplosionConfig(functionIndex, explosionConfig);
                    }
                }
                break;

            case BattleActionEffectType.SIEGE_ATTACK:
                {
                    // special feature required: each siege attack hit point is the center of explosion
                    var centerList = siegeAttackHitPosDict[functionIndex];
                    foreach (var simulPlayer in simulPlayersDict.Values)
                    {
                        foreach(var center in centerList)
                        {
                            Vector3 newCenter = center;
                            newCenter.z += zOffset; // in order let the cubes fly up
                            var explosionConfig = new ExplosionConfig(newCenter, radius, force);
                            simulPlayer.AddExplosionConfig(functionIndex, explosionConfig);
                        }
                    }
                }
                break;
            default:
                Debug.LogErrorFormat("Not implemented {0}", function.effect.ToString()); break;
        }


    }

    private void OnPreviewCard()
    {
        // (1) preview damage/armor on HP bar (our/enemy)
        // (2) Defense background hightlight
        // (3) CurveArrow display (if it is an ATK card), Attack background hightlight
        // (4) Cubes display
        // (5) Add explosion points according to curve arrows. (if it is an ATK card)

        var curBattle = BattleManager.Instance.GetCurrentBattle();
        var isClientRound = BattleManager.Instance.IsClientRound();
        var modifiedBattleDefenseData = curBattle.GetBattleDefenseDataCopy();

        //var modifiedMoralDict = curBattle.GetStatusCopy(BattleActionEffectType.MORAL.ToString());
        //var modifiedDisciplineDict = curBattle.GetStatusCopy(BattleActionEffectType.DISCIPLINE.ToString());

        // compute attack/def values
        //var totalAtkDict = new Dictionary<GroupSide, float>();
        //BattleLogics.IterateGroupSide((GroupSide side) => { totalAtkDict.Add(side, 0); totalDefDict.Add(side, 0); });
        //var totalLoseDict = new Dictionary<GroupSide, float>();
        //var totalDefDict = new Dictionary<GroupSide, float>();
        //BattleLogics.IterateGroupSide((GroupSide side) => { totalLoseDict.Add(side, 0); totalDefDict.Add(side, 0); });
        //BattleLogics.IterateGroupSide((GroupSide side) => { totalLoseDict.Add(side, 0); });

        //var totalAttackerUnits = new HashSet<Tuple<int, int, BattleGroupType>>();
        //var totalDefenderUnits = new HashSet<Tuple<int, int, BattleGroupType>>();
        var totalHighlightFriendSides = new HashSet<GroupSide>(); 
        var totalHighlightFriendUnits = new HashSet<Tuple<int, int, BattleGroupType>>();
        var totalHighlightEnemyUnits = new HashSet<Tuple<int, int, BattleGroupType>>();

        // needApplyLossForUI Ϊfalse��Ŀ����Ϊ�˼�ͷ������յ�
        const bool needApplyLossForUI = false; // we set it false because we have not actually execute the attack/defense action
        // needApplyLossForValues Ϊtrue��Ϊ��Ԥ�������ܴ򵽵ľ���. ����ֻ����ǰ��, �������������, ���ܻ����������(���������������)
        const bool needApplyLossForValues = true; // we actually need it

        // precompute moral_damage_modifier
        var moralDamageModifiers = BattleLogics.GetMoralDamageModifiers();
        var disciplineDamageModifiers = BattleLogics.GetDisciplineDamageModifiers();
        var defenseDamageModifiers = BattleLogics.GetDefenseDamageModifiers();

        for (int i = 0; i < curCardData.functionList.Count; i++)
        {
            var function = curCardData.functionList[i];
            float basicValue = function.EvaluateBasicValue();

            var possibleStartSides = BattleLogics.GetGroupSides(function.from, isClientRound, needApplyLossForUI);
            var possibleEndSides = BattleLogics.GetGroupSides(function.target, isClientRound, needApplyLossForUI);

            #region atk related
            if (BattleLogics.IsAtkAction(curCardData.actionTypeList[i]))
            {
                HashSet<Tuple<GroupSide, GroupSide, AttackedDir>> atkArrowParams;
                BattleLogics.GetCurveArrowParams(function.target, function.effect, ref possibleStartSides, ref possibleEndSides, out atkArrowParams);

                // put it into the total arrow parms
                totalAtkArrowParamDict.Add(i, atkArrowParams);

                foreach (var atkParam in atkArrowParams)
                {
                    float attackerAtk, defenderAtk, attackerCancelingAtk;
                    var startSide = atkParam.Item1;
                    var endSide = atkParam.Item2;

                    // get attackers
                    var attackerUnits = BattleLogics.GetAttackerUnitsByTargetType_AtkAction(startSide, function.from, isClientRound, needApplyLossForValues);
                    
                    totalHighlightFriendUnits.UnionWith(attackerUnits);
                    
                    totalHighlightFriendSides.Add(startSide);

                    // get defenders
                    var defenderUnits = BattleLogics.GetDefenderUnitsByTargetType_AtkAction(endSide, function.target, function.effect, isClientRound, needApplyLossForValues);
                    
                    totalHighlightEnemyUnits.UnionWith(defenderUnits);

                    BattleLogics.CalculateAttackFromOneSideToOther(
                        function.effect,
                        basicValue,
                        startSide,
                        endSide,
                        atkParam.Item3,
                        ref attackerUnits,
                        ref defenderUnits,
                        ref modifiedBattleDefenseData,
                        ref defenseDamageModifiers,
                        out attackerAtk,
                        out defenderAtk,
                        out attackerCancelingAtk);
                    //out attackerLose,
                    //out defenderLose);

                    Debug.LogFormat("attackerAtk:{0}, defenderAtk:{1}", attackerAtk, defenderAtk);

                    // update moral delta
                    // from attackers to defenders
                    deltaStatusCache[new(startSide, BattleActionEffectType.MORAL)] += BattleLogics.CalculateMoralDelta(attackerAtk, defenderAtk, moralDamageModifiers[endSide]);
                    // from defenders to attackers
                    deltaStatusCache[new(endSide, BattleActionEffectType.MORAL)] += BattleLogics.CalculateMoralDelta(defenderAtk, attackerAtk, moralDamageModifiers[startSide]);

                    Debug.Assert(attackerCancelingAtk >= 0);
                    // update discipline delta(only defenders)
                    deltaStatusCache[new(endSide, BattleActionEffectType.DISCIPLINE)] += BattleLogics.CalculateDisciplineDelta(defenderAtk, attackerCancelingAtk, disciplineDamageModifiers[startSide]);
                }
            }

            #endregion

            #region defense related
            if (BattleLogics.IsDefAction(curCardData.actionTypeList[i]))
            {
                var targetSides = possibleEndSides; // who needs to apply defense.
                foreach (var side in targetSides)
                {
                    // accelerate codes and avoid divided by 0
                    if (BattleLogics.HasAtLeastOneGroupInGroupSide(side, needApplyLossForValues))
                    {
                        var units = BattleLogics.GetUnitsByTargetType(side, function.target, isClientRound, needApplyLossForValues);
                        totalHighlightFriendUnits.UnionWith(units);
                        totalHighlightFriendSides.Add(side);

                        float gainDef = BattleLogics.CalculateDefense(function.effect, basicValue, side, needApplyLossForValues, ref units);
                        deltaDefenseCache[new(side, function.effect)] += gainDef; // cache it for later usage

                        // update discipline of defenders
                        var amount = curBattle.GetTotalAmountByGroupSide(side, needApplyLossForValues);
                        Debug.Assert(amount > 0);
                        deltaStatusCache[new(side, BattleActionEffectType.DISCIPLINE)] += BattleLogics.CalculateDisciplineDelta(gainDef, 10f, amount);
                    }
                }
            }
            #endregion

            #region update status related (only for handling the "normal" status) (some 'SpecialFunctionFlag' can add status as well)
            if (Utils.HasAttribute<StatusFlag, BattleActionEffectType>(function.effect) &&
                !Utils.HasAttribute<SpecialFunctionFlag, BattleActionEffectType>(function.effect))
            {
                var targetSides = possibleEndSides; // who needs to update status
                foreach (var side in targetSides)
                {
                    var units = BattleLogics.GetUnitsByTargetType(side, function.target, isClientRound, needApplyLossForValues);
                    totalHighlightFriendUnits.UnionWith(units);
                    totalHighlightFriendSides.Add(side);

                    deltaStatusCache[new(side, function.effect)] += basicValue;
                }
            }
            #endregion

            #region special
            // update moral by multiplying it with basicValue
            if (Utils.HasAttribute<MultiplierFlag, BattleActionEffectType>(function.effect))
            {
                // for "MORAL_MULTIPLIER", "DISCIPLINE_MULTIPLIER"
                string inputStr = function.effect.ToString().Replace("_MULTIPLIER", "");
                BattleActionEffectType targetStatusType = Utils.ConvertStrToEnum<BattleActionEffectType>(inputStr);
                var targetSides = possibleEndSides; // who needs to update status
                foreach (var side in targetSides)
                {
                    var units = BattleLogics.GetUnitsByTargetType(side, function.target, isClientRound, needApplyLossForValues);
                    totalHighlightFriendUnits.UnionWith(units);
                    totalHighlightFriendSides.Add(side);

                    var battleStatusData = BattleManager.Instance.GetCurrentBattle().GetBattleStatusData(side, targetStatusType.ToString());
                    float oldValue = battleStatusData.GetValue(); // because newValue = oldValue * basicValue
                    // so deltaValue = newValue - oldValue = oldValue * (basicValue - 1)
                    // TODO: not take into account the negative oldValue.
                    deltaStatusCache[new(side, targetStatusType)] += (oldValue * (basicValue - 1));
                }
            }
            else if(function.effect == BattleActionEffectType.ACTION_AFTER_TURN)
            {
                var targetSides = possibleEndSides; // who needs to update status
                foreach (var side in targetSides)
                {
                    var units = BattleLogics.GetUnitsByTargetType(side, function.target, isClientRound, needApplyLossForValues);
                    totalHighlightFriendUnits.UnionWith(units);
                    totalHighlightFriendSides.Add(side);

                    var battleStatusData = BattleManager.Instance.GetCurrentBattle().GetBattleStatusData(side, function.effect.ToString());
                    if(battleStatusData == null)
                    {
                        deltaStatusCache[new(side, function.effect)] = basicValue;
                    }
                    else
                    {
                        float oldValue = battleStatusData.GetValue(); // it means that delayed functions will be called after how many turns (oldValue)
                        float newValue = Mathf.Min(basicValue, oldValue); // because we only show one status for the nearest turns for delayed functions.
                        deltaStatusCache[new(side, function.effect)] = newValue - oldValue;
                    }
                }
            }
            else if(Utils.HasAttribute<SpecialFunctionFlag, BattleActionEffectType>(function.effect))
            {
                // [Note] if can not enter any if above, then we try to proceed with handling normal special function.
                // Just always highlight all available groupsides
                var targetSides = possibleEndSides; // who needs to update status
                foreach (var side in targetSides)
                {
                    var units = BattleLogics.GetUnitsByTargetType(side, function.target, isClientRound, needApplyLossForValues);
                    totalHighlightFriendUnits.UnionWith(units);
                    totalHighlightFriendSides.Add(side);
                }
            }
            #endregion

            if(Utils.HasAttribute<DelayedFunctionFlag, BattleActionEffectType>(function.effect))
            {
                break; // not continue to preview the following functinos because it will be delayed to use.
            }
        }

        // we have update "battleDefenseData" above, now compute the delta defense values
        var battleDefenseData = curBattle.GetBattleDefenseDataCopy(); // get the actual defense
        foreach(var item in battleDefenseData)
        {
            float lossDef = item.Value - modifiedBattleDefenseData[item.Key];
            deltaDefenseCache[item.Key] += lossDef;
        }
        
        // Highlight Groups in the battlefield. Update the 'groupHighlightList', must be called before "OnDisplayCubes"
        UpdateGroupHighlightList(ref totalHighlightFriendUnits, ref totalHighlightEnemyUnits);

        BattleLogics.IterateGroupSide((GroupSide side) =>
        {
            if (BattleLogics.HasAtLeastOneGroupInGroupSide(side, false))
            {
                OnPreviewDamage(side);
                OnPreviewArmor(side);

                if (totalHighlightFriendSides.Contains(side) &&
                 ((isClientRound && side.IsBottom()) || (!isClientRound && side.IsTop())))
                {
                    // display Attacker highlight, if gain armor, show defense, otherwise show select
                    float armor = curBattle.GetGroupsideBattleDefense(side) + BattleLogics.GetGroupsideBattleDefense(ref deltaDefenseCache, side);
                    OnDisplayBeSelectedHighlight(side, armor > 0);
                }
            }
        });

        for (int functionIndex = 0; functionIndex < curCardData.functionList.Count; functionIndex++)
        {
            if (!totalAtkArrowParamDict.ContainsKey(functionIndex)) continue;
            var totalAtkArrowParams = totalAtkArrowParamDict[functionIndex];
            foreach (var atkArrowParam in totalAtkArrowParams)
            {
                var startSide = atkArrowParam.Item1;
                var endSide = atkArrowParam.Item2;

#if UNITY_EDITOR
                Debug.Assert(BattleLogics.HasAtLeastOneGroupInGroupSide(startSide, false));
                Debug.Assert(BattleLogics.HasAtLeastOneGroupInGroupSide(endSide, false));
#endif

                var atkDir = atkArrowParam.Item3;
                // display curve arrows
                OnDisplayCurveArrow(startSide, endSide, atkDir);

                // display Defender background highlight
                OnDisplayBeAttackedHighlight(endSide, atkDir);
            }

            if (Utils.HasAttribute<DelayedFunctionFlag, BattleActionEffectType>(curCardData.functionList[functionIndex].effect))
            {
                break; // not continue to preview the following functinos because it will be delayed to use.
            }
        }
    }

    private void BeforeUseCard()
    {
        EventManager.Instance.SendEventSync(Core.Events.EventType.UI_OnBeforeUseCard);

        // hide highlights
        OnHideHighlight();

        // hide curve arrows
        OnHideAllCurveArrows();

        OnEndPreviewDamage();
        OnEndPreviewArmor();
    }

    private void AfterUseCard()
    {
        // hide vfxs
        OnHideAllVFXs();

        // hide highlights
        OnHideHighlight();

        // hide curve arrows
        OnHideAllCurveArrows();

        // hide cubes
        OnHideAllCubes();

        fadeGroupTransList.Clear();
        //totalAtkArrowParams.Clear();
        totalAtkArrowParamDict.Clear();

        curCardData = null;

        // send the event in next frame
        EventManager.Instance.SendEventAsync(Core.Events.EventType.UI_OnAfterUseCard);
    }

    private void TryUseCard()
    {
        Debug.Log("TryUseCard");

        // since we always need to preview card then use it. We do not need the index anymore.
        if (curCardData != null)
        {
            BeforeUseCard();
            float totalTime = OnUseCard();
            GameCore.DelayCall(totalTime, AfterUseCard);
        }
        else
            Debug.LogFormat("curData is null.");
    }

    private float OnUseCard()
    {
        float totalTime = 0f;

        // execute the VFXs play logics according to the curCardData

        for (int i = 0; i < curCardData.functionList.Count; i++)
        {
            totalTime += OnHandleBattleFunction(i, totalTime);

            if (Utils.HasAttribute<DelayedFunctionFlag, BattleActionEffectType>(curCardData.functionList[i].effect))
            {
                break; // not continue to preview the following functinos because it will be delayed to use.
            }
        }

        //foreach (var function in curCardData.functionList)
        //    totalTime += OnHandleBattleFunction(function, totalTime);

        totalTime += 0.3f; // for displaying hit effects.

        GameCore.DelayCall(totalTime, () =>
        {
            // Once finish, start to fade the cubes
            OnTriggerFadeAllCubes();

            OnUpdateGroupsInBattleField();
            OnUseDeltaStatusCache();
            OnUpdateHPStatusBarVisibility();
        });

        totalTime += fadeCubesDuration;
        GameCore.DelayCall(totalTime, () =>
        {
            // Once done, start to fade the group icons
            OnTriggerFadeAllGroupIcons();
        });

        totalTime += fadeGroupIconsDuration;

        return totalTime;
    }

    private float OnHandleSpecialFunction(float delayTime, BattleFunction function)
    {
        // first just play a VFX on a player.
        string vfxPath;
        float vfxPlayTime;
        AssetsMapper.GetSpecialFunctionVFXPathPlayTime(function.effect, out vfxPath, out vfxPlayTime);
        float totalVFXTime = OnPlayEffectOnPlayer(delayTime, vfxPath, vfxPlayTime); // could be either enemy or friend to draw cards

        // below is the its corresponding logics
        if (Utils.HasAttribute<DrawCardFlag, BattleActionEffectType>(function.effect))
        {
            var isClientRound = BattleManager.Instance.IsClientRound();
            int cardNumber = (int)function.EvaluateBasicValue();
            EventManager.Instance.SendEventAsync(Core.Events.EventType.Battle_OnDrawCard, isClientRound, cardNumber, function.effect);
            float drawCardAniTime = delayTime + 0.1f * cardNumber + 0.5f; // 0.1 seconds for drawing a card, then wait for 0.5s
            return Mathf.Max(totalVFXTime, drawCardAniTime);
        }
        else if (function.effect == BattleActionEffectType.BURN_CARD)
        {
            var isClientRound = BattleManager.Instance.IsClientRound();
            int cardNumber = (int)function.EvaluateBasicValue();
            EventManager.Instance.SendEventAsync(Core.Events.EventType.Battle_OnBurnCard, isClientRound, cardNumber);
            float burnCardAniTime = delayTime + 2f + 0.5f; // 2 seconds for burning a card, then wait for 0.5s
            return Mathf.Max(totalVFXTime, burnCardAniTime);
        }
        else if (function.effect == BattleActionEffectType.DISCARD_CARD)
        {
            var isClientRound = BattleManager.Instance.IsClientRound();
            int cardNumber = (int)function.EvaluateBasicValue();
            EventManager.Instance.SendEventAsync(Core.Events.EventType.Battle_OnDiscardCard, isClientRound, cardNumber);
            float discardCardAniTime = delayTime + 1f + 0.5f; // 1 seconds for discarding a card, then wait for 0.5s
            return Mathf.Max(totalVFXTime, discardCardAniTime);
        }
        else if (Utils.HasAttribute<ReduceCardCostFlag, BattleActionEffectType>(function.effect))
        {
            var isClientRound = BattleManager.Instance.IsClientRound();
            int reduceCostNum = (int)function.EvaluateBasicValue();
            EventManager.Instance.SendEventAsync(Core.Events.EventType.Battle_OnReduceCardCost, isClientRound, reduceCostNum, function.effect);
            return totalVFXTime;
        }
        else if (function.effect == BattleActionEffectType.ACTION_POINT) 
        {
            var isClientRound = BattleManager.Instance.IsClientRound();
            int extraActionPoint = (int)function.EvaluateBasicValue();
            EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnUpdateActionPoint, isClientRound, extraActionPoint);
            return totalVFXTime;
        }
        else if(function.effect == BattleActionEffectType.ACTION_AFTER_TURN)
        {
            var isClientRound = BattleManager.Instance.IsClientRound();
            int turnNum = (int)function.EvaluateBasicValue();
            // if my round is at roundNum, and I have to wait for N turns. Then means after (roundNum + N * 2), it will be the roundNum when I can use this card.
            int roundNum = BattleManager.Instance.GetCurrentBattle().GetCurRoundNum() + 2 * turnNum;

            EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnActionAfterTurn, isClientRound, roundNum, curCardData);
            return totalVFXTime;
        }
        else
        {
            return totalVFXTime;
        }
    }

    private float OnHandleBattleFunction(int functionIndex, float delayTime)
    {
        BattleFunction function = curCardData.functionList[functionIndex];

        if(function.effect == BattleActionEffectType.NONE || function.effect == BattleActionEffectType.MAX)
        {
            Debug.LogWarningFormat("function need to be implemented with effectType: {0}", function.effect.ToString());
            return 0;
        }

        if (Utils.HasAttribute<PlayCombatEffectFlag, BattleActionEffectType>(function.effect))
        {
            return OnPlayCombatEffect(functionIndex, function.effect, delayTime);
        }
        else if (Utils.HasAttribute<PlayDistantEffectFlag, BattleActionEffectType>(function.effect))
        {
            return OnPlayDistantEffect(functionIndex, function.effect, delayTime);
        }
        else if (Utils.HasAttribute<PlayDefenseEffectFlag, BattleActionEffectType>(function.effect))
        {
            return OnPlayDefenseEffect(delayTime);
        }
        else if(Utils.HasAttribute<PlayUpdateStatusEffectFlag, BattleActionEffectType>(function.effect))
        {
            return OnPlayUpdateStatusEffect(functionIndex, function.effect, delayTime);
        }
        else if(Utils.HasAttribute<SpecialFunctionFlag, BattleActionEffectType>(function.effect))
        {
            return OnHandleSpecialFunction(delayTime, function);
        }
        else
        {
            Debug.LogWarningFormat("function need to be implemented with effectType: {0}", function.effect.ToString());
            return 0;
        }

        //switch (function.effect)
        //{
        //    case BattleActionEffectType.MELEE_ATTACK:
        //    case BattleActionEffectType.PRIOTIZED_ATTACK:
        //        return OnPlayCombatEffect(functionIndex, function.effect, delayTime);

        //    case BattleActionEffectType.ARMOUR_PIERCING_ATTACK:
        //    case BattleActionEffectType.PROJECTILE_ATTACK:
        //    case BattleActionEffectType.SIEGE_ATTACK:
        //        return OnPlayDistantEffect(functionIndex, function.effect, delayTime);

        //    case BattleActionEffectType.ELASTIC_DEFENSE:
        //    case BattleActionEffectType.ARMOUR_DEFENSE:
        //        return OnPlayDefenseEffect(delayTime);

        //    case BattleActionEffectType.MORAL:
        //    case BattleActionEffectType.DISCIPLINE:
        //    case BattleActionEffectType.COUNTERATTACK_MODIFIER:
        //        return OnPlayUpdateStatusEffect(function.effect, delayTime);

        //    case BattleActionEffectType.NONE:
        //    default:
        //        Debug.LogWarningFormat("function need to be implemented with effectType: {0}", function.effect.ToString());
        //        return 0;
        //        //throw new Exception(string.Format("function need to be implemented with effectType: {0}", function.effect.ToString()));
        //}
    }

    private EffectConfig GenerateEffectConfig_Parabola(
        Vector3 startPos, Vector3 endPos, float heightScale,
        string projectile, string muzzle, string hit,
        float duration, Vector3 scale)
    {
        EffectConfig config = new EffectConfig();
        config.points = new List<Vector2> { new Vector2(startPos.x, startPos.y), new Vector2(endPos.x, endPos.y) };
        config.baseHeight = startPos.z;
        config.maxHeight = config.baseHeight; // not used in this situation.
        config.heightScale = heightScale; // TODO: for now just use it
        config.duration = duration;
        config.projectilePrefabPath = projectile;
        config.muzzlePrefabPath = muzzle;
        config.hitPrefabPaths = new List<string>() { hit };
        config.vfxScales = scale;
        return config;
    }

    // effects for combat attacks(like those attacks from one direction to hit a row or a col)
    private float OnPlayCombatEffect(int functionIndex, BattleActionEffectType actionEffectType, float delayTime)
    {
        const float atkEffectDuration = 1.0f;
        string projectile, muzzle, hit;
        AssetsMapper.GetVFXPaths(actionEffectType, out projectile, out muzzle, out hit);

        // [Note] simulPlayer is depending on GroupHighlightList, so it needs to be called after initializing higlightlist.
        // display all cubes and retruen a dict refers to them
        var isClientRound = BattleManager.Instance.IsClientRound();
        var simulPlayersDict = OnDisplayCubes(isClientRound);

        HashSet<GroupSide> endSideSet = new HashSet<GroupSide>();
        var defenseEffectRecord = new Dictionary<Tuple<GroupSide, AttackedDir>, bool>();
        var totalAtkArrowParams = totalAtkArrowParamDict[functionIndex];
        foreach (var curveArrowParam in totalAtkArrowParams)
        {
            var startSide = curveArrowParam.Item1;
            var endSide = curveArrowParam.Item2;
            var atkDir = curveArrowParam.Item3;

            // each curve arrow will create a explosion point
            AddExplosionConfigByStrategy(ref simulPlayersDict, functionIndex, startSide, endSide, atkDir);

            float scale = Table_constant.data["constant_0015"].param_float;
            Vector3 vfxScale = new Vector3(scale, scale, scale);

            // play atk effect
            var atkEffectConfig = GenerateEffectConfig_Bezier(
                startSide, endSide, atkDir, projectile, muzzle, hit, atkEffectDuration, vfxScale);
            if (HasArmor(endSide))
            {
                atkEffectConfig.hitPrefabPaths.Add("vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_Defense_hit");
                var key = new Tuple<GroupSide, AttackedDir>(endSide, atkDir);
                if (!defenseEffectRecord.ContainsKey(key))
                {
                    PlayDefenseEffectOnGroupSide(endSide, atkDir, delayTime + atkEffectDuration);
                    defenseEffectRecord[key] = true;
                }
            }
            GameCore.DelayCall(delayTime, () => { PlayEffect(atkEffectConfig); });
            endSideSet.Add(endSide);
        }

        float totalTime = atkEffectDuration + delayTime;

        GameCore.DelayCall(totalTime, () =>
        {
            OnTriggerSimulation(functionIndex);
            
            BattleLogics.IterateGroupSide((GroupSide side) => 
            {
                if (BattleLogics.HasAtLeastOneGroupInGroupSide(side, false)) OnReceiveDamage(side); 
            });
        });

        return totalTime;
    }

    private Vector3 GetRandomPosWithinGroupTrans(int row, int col)
    {
        var trans = GetGroup(row, col);
        float xMin = trans.position.x - groupSize.x / 2;
        float xMax = xMin + groupSize.x;
        float yMin = trans.position.y - groupSize.y / 2;
        float yMax = yMin + groupSize.y;
        float x = UnityEngine.Random.Range(xMin, xMax);
        float y = UnityEngine.Random.Range(yMin, yMax);
        float z = trans.position.z;
        return new Vector3(x, y, z);
    }

    // TODO: once we have updated "GetShootNumRatio", we need to update below function as well.
    private static float GetShootNumRatioMAX()
    {
        float max = Mathf.Max(Table_constant.data["constant_0009"].param_float, Table_constant.data["constant_0010"].param_float);
        max = Mathf.Max(max, Table_constant.data["constant_0011"].param_float);
        return max;
    }

    private float GetShootNumRatio(BattleActionEffectType actionEffectType)
    {
        switch(actionEffectType)
        {
            case BattleActionEffectType.SIEGE_ATTACK: return Table_constant.data["constant_0009"].param_float;
            case BattleActionEffectType.PROJECTILE_ATTACK: return Table_constant.data["constant_0010"].param_float;
            case BattleActionEffectType.ARMOUR_PIERCING_ATTACK: return Table_constant.data["constant_0011"].param_float;
        }
        return 1f;
    }

    private Vector3 GetDistantVFXScale(BattleActionEffectType actionEffectType)
    {
        float scale = 1f;
        switch (actionEffectType)
        {
            case BattleActionEffectType.SIEGE_ATTACK:
                {
                    scale = Table_constant.data["constant_0016"].param_float; break;
                }
            default:
                {
                    scale = Table_constant.data["constant_0012"].param_float; break;
                }
        }
        return new Vector3(scale, scale, scale);
    }

    private float OnPlayDistantEffect(int functionIndex, BattleActionEffectType actionEffectType, float delayTime)
    {
        float totalTime = delayTime;

        const bool needApplyLossForUI = false; // we don't need apply loss, cause it is just visual effect.
        const float atkEffectDuration = 1.0f;
        float TINY_DELAY_TIME_MAX = Table_constant.data["constant_0017"].param_float;
        float tinyDelayTimeMin = TINY_DELAY_TIME_MAX;
        string projectile, muzzle, hit;
        AssetsMapper.GetVFXPaths(actionEffectType, out projectile, out muzzle, out hit);

        var isClientRound = BattleManager.Instance.IsClientRound();
        var simulPlayersDict = OnDisplayCubes(isClientRound);

        bool isParabolAttack = BattleLogics.IsParabolaAttack(actionEffectType);
        float heightScale = isParabolAttack ? (-1f * Table_constant.data["constant_0018"].param_float) : 0f;

        Vector3 distantVFXScale = GetDistantVFXScale(actionEffectType);

        var function = curCardData.functionList[functionIndex];

        #region new logic for playing distant attack VFXs
        var defenseEffectRecord = new Dictionary<Tuple<GroupSide, AttackedDir>, bool>();
        var totalAtkArrowParams = totalAtkArrowParamDict[functionIndex];
        foreach (var curveArrowParam in totalAtkArrowParams)
        {
            var startSide = curveArrowParam.Item1;
            var endSide = curveArrowParam.Item2;
            var atkDir = curveArrowParam.Item3;
            bool endSideHasArmor = HasArmor(endSide);

            if (endSideHasArmor)
            {
                var key = new Tuple<GroupSide, AttackedDir>(endSide, atkDir);
                if (!defenseEffectRecord.ContainsKey(key))
                {
                    var atkDir2 = !isParabolAttack ? atkDir : AttackedDir.NONE; // none means the attack is from head.
                    PlayDefenseEffectOnGroupSide(endSide, atkDir2, delayTime + atkEffectDuration);
                    defenseEffectRecord[key] = true;
                }
            }

            // TODO: to improve below codes, avoid repeating iteration to get attackers/defenders
            // get attackers
            var attackerUnits = BattleLogics.GetAttackerUnitsByTargetType_AtkAction(startSide, function.from, isClientRound, needApplyLossForUI);
            var attackerList = BattleLogics.ConvertIndexesGroupTypeToIndexList(ref attackerUnits);

            // get defenders
            var defenderUnits = BattleLogics.GetDefenderUnitsByTargetType_AtkAction(endSide, function.target, function.effect, isClientRound, needApplyLossForUI);
            var defenderList = BattleLogics.ConvertIndexesGroupTypeToIndexList(ref defenderUnits);

            foreach (var tuple in attackerList)
            {
                var atkGroupTrans = GetGroup(tuple.Item1, tuple.Item2);
                var startPos = atkGroupTrans.position;
                var atkBattleGroupData = BattleLogics.GetBattleGroupDataFromCurrentBattle(tuple.Item1, tuple.Item2);
                int amount = atkBattleGroupData.GetTotalAmountBeforeLoss();
                float shootNumRatio = GetShootNumRatio(actionEffectType);
                int shootNum = Mathf.RoundToInt(Mathf.Sqrt(amount) * shootNumRatio);
                // pick one defender randomly each time, repeat it 'shootNum' times.
                for (int i = 0; i < shootNum; i++) 
                {
                    var randomIndex = UnityEngine.Random.Range(0, defenderList.Count);
                    var defender = defenderList[randomIndex];
                    var endPos = GetRandomPosWithinGroupTrans(defender.Item1, defender.Item2);

                    if (!siegeAttackHitPosDict.ContainsKey(functionIndex)) siegeAttackHitPosDict.Add(functionIndex, new List<Vector3>());
                    siegeAttackHitPosDict[functionIndex].Add(endPos); // for special feature: siege attack hit point is the explosion point.

                    float tinyDelayTime = UnityEngine.Random.Range(0.0f, TINY_DELAY_TIME_MAX);
                    tinyDelayTimeMin = Mathf.Min(tinyDelayTimeMin, tinyDelayTime);

                    var atkEffectConfig = GenerateEffectConfig_Parabola(
                        startPos, endPos, heightScale, projectile, muzzle, hit, atkEffectDuration, distantVFXScale);

                    if (endSideHasArmor)
                        atkEffectConfig.hitPrefabPaths.Add("vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_Defense_hit");

                    GameCore.DelayCall(tinyDelayTime + delayTime, () =>
                    {
                        // play atk effect
                        PlayEffect(atkEffectConfig);
                    });
                }
            }

            // each curve arrow will create a explosion point
            AddExplosionConfigByStrategy(ref simulPlayersDict, functionIndex, startSide, endSide, atkDir);
        }
        #endregion

        totalTime = tinyDelayTimeMin + delayTime + atkEffectDuration;
        GameCore.DelayCall(totalTime, () =>
        {
            // when hit
            OnTriggerSimulation(functionIndex);

            BattleLogics.IterateGroupSide((GroupSide side) => 
            {
                if (BattleLogics.HasAtLeastOneGroupInGroupSide(side, false)) OnReceiveDamage(side); 
            });
        });

        // wait for a little bit to show physic simulation and finish playing hit effect
        totalTime = TINY_DELAY_TIME_MAX + delayTime + atkEffectDuration + 0.3f;
        return totalTime;
    }

    private void PlayDefenseEffectOnGroupSide(GroupSide side, AttackedDir atkDir, float waitTime)
    {
        // play defense efefct after hit.
        // at the center point of the whole group side to cover its head
        var activeRows = BattleLogics.GetActiveRows(side, false);
        Vector3 startPos = GetGroupRectCenter(side, activeRows);
        //bool isTop = side.IsTop();
        //bool isClientRound = BattleManager.Instance.IsClientRound();
        //float signY = isClientRound ? 1 : -1;
        if (atkDir != AttackedDir.NONE)
        {
            var rectSize = GetApproximateRectSize(activeRows, side);
            if (atkDir == AttackedDir.LEFT) startPos.x -= rectSize.x;
            else if (atkDir == AttackedDir.RIGHT) startPos.x += rectSize.x;
            else if (atkDir == AttackedDir.TOP) startPos.y += rectSize.y;
            else if (atkDir == AttackedDir.BOTTOM) startPos.y -= rectSize.y;
        }

        Vector3 endPos;
        if (atkDir == AttackedDir.LEFT) endPos = startPos + Vector3.right;
        else if (atkDir == AttackedDir.RIGHT) endPos = startPos + Vector3.left;
        else if (atkDir == AttackedDir.TOP) endPos = startPos + Vector3.down;
        else if (atkDir == AttackedDir.BOTTOM) endPos = startPos + Vector3.up;
        else endPos = startPos + Vector3.back;

        string defenseVFX = "vfx/VFX_Klaus/Prefabs/Stylized Shoot & Hit/FX_Shoot_Defense";
        var scale = Table_constant.data["constant_0014"].param_float_list[side.IsMiddle() ? 0 : 1];
        Vector3 vfxScale = new Vector3(scale, scale, scale);
        var defEffectConfig = GenerateEffectConfig_Linear(startPos, endPos, null, null, defenseVFX, waitTime, vfxScale);
        var effectPlayer = PlayEffect(defEffectConfig); // it will play the hit effect after 'waitTime' seconds

        Debug.LogFormat("PlayDefenseEffectOnGroupSide, side: {0}, atkDir: {1}", side.ToString(), atkDir.ToString());
        if (atkDir != AttackedDir.NONE)
        {
            // rotate the hit(defense) VFX close to the camera view direction which is Z direction.
            Action<GameObject> callBack = (GameObject hitVFX) =>
            {
                // hit VFX -forward is the hit direction
                float condition = Vector3.Dot(hitVFX.transform.up, Vector3.back);
                //Debug.LogFormat("hitVFX.transform.up: {0}", hitVFX.transform.up);
                float threshold = Mathf.Cos(80f / 180f * Mathf.PI);
                if (Mathf.Abs(condition) <= threshold)
                {
                    // >= 80 degrees then rotate the forward a little bit
                    Vector3 rotAxis = Vector3.Cross(hitVFX.transform.up, Vector3.back);
                    const float rotAngle = 30f;
                    Quaternion quat = Quaternion.AngleAxis(rotAngle, rotAxis);
                    hitVFX.transform.up = quat * hitVFX.transform.up;
                    // hitVFX default forward is Vector3.up
                    //Debug.LogFormat("Rotate the defense hit VFX by {0} degrees. isTop: {1}, side: {2}, atkDir: {3}",
                    //    rotAngle, isTop, side.ToString(), atkDir.ToString());
                }
            };
            effectPlayer.SetUpdateHitPrefabCallBack(callBack);
        }
    }

    private float OnPlayDefenseEffect(float delayTime)
    {
        bool isClientRound = BattleManager.Instance.IsClientRound();
        BattleLogics.IterateGroupSide((GroupSide side) =>
        {
            if (BattleLogics.HasAtLeastOneGroupInGroupSide(side, false))
            {
                if ((isClientRound && side.IsBottom()) || (!isClientRound && side.IsTop()))
                    GameCore.DelayCall(delayTime, () => { PlayDefenseEffectOnGroupSide(side, AttackedDir.NONE, 0f); });
            }
        });


        //if (BattleManager.Instance.IsClientRound())
        //{
        //    BattleLogics.IterateBottomGroupSide((GroupSide side) =>
        //    {
        //        GameCore.DelayCall(delayTime, () => { PlayDefenseEffectOnGroupSide(side, AttackedDir.NONE, 0f); });
        //    });
        //}
        //else
        //{
        //    BattleLogics.IterateTopGroupSide((GroupSide side) =>
        //    {
        //        GameCore.DelayCall(delayTime, () => { PlayDefenseEffectOnGroupSide(side, AttackedDir.NONE, 0f); });
        //    });
        //}

        GameCore.DelayCall(delayTime + 0.2f, OnReceiveArmor);

        return delayTime + 0.2f;
    }

    private void OnUpdateHPStatusBarVisibility()
    {
        BattleLogics.IterateGroupSide((GroupSide side) =>
        {
            bool visible = BattleLogics.HasAtLeastOneGroupInGroupSide(side, true);
            hpbarCompDict[side].SetVisible(visible);
            statusBarDict[side].SetVisible(visible);
        });
    }

    private void InitDeltaStatusCache()
    {
        // TODO: to add new status below
        deltaStatusCache = new Dictionary<Tuple<GroupSide, BattleActionEffectType>, float>();
        BattleLogics.IterateGroupSide((GroupSide side) =>
        {
            // ignoring the NONE
            for (int i = 1; i < (int)(BattleActionEffectType.MAX); i++) 
            {
                BattleActionEffectType actionEffectType = (BattleActionEffectType)i;
                if(Utils.HasAttribute<StatusFlag, BattleActionEffectType>(actionEffectType))
                {
                    deltaStatusCache.Add(new(side, actionEffectType), 0);
                }
            }

            //deltaStatusCache.Add(new(side, BattleActionEffectType.MORAL), 0);
            //deltaStatusCache.Add(new(side, BattleActionEffectType.DISCIPLINE), 0);
            //deltaStatusCache.Add(new(side, BattleActionEffectType.COUNTERATTACK_MODIFIER), 0);
        });
    }

    private void ResetDeltaStatusCache()
    {
        BattleLogics.IterateGroupSide((GroupSide side) =>
        {
            for (int i = 1; i < (int)(BattleActionEffectType.MAX); i++)
            {
                BattleActionEffectType actionEffectType = (BattleActionEffectType)i;
                if (Utils.HasAttribute<StatusFlag, BattleActionEffectType>(actionEffectType))
                {
                    deltaStatusCache[new(side, actionEffectType)] = 0;
                }
            }

            //deltaStatusCache[new(side, BattleActionEffectType.MORAL)] = 0;
            //deltaStatusCache[new(side, BattleActionEffectType.DISCIPLINE)] = 0;
            //deltaStatusCache[new(side, BattleActionEffectType.COUNTERATTACK_MODIFIER)] = 0;
        });
    }

    private void OnUseDeltaStatusCache()
    {
        var curBattle = BattleManager.Instance.GetCurrentBattle();
        var sides = new HashSet<GroupSide>();
        foreach (var item in deltaStatusCache)
        {
            if (item.Value != 0)
            {
                curBattle.UpdateBattleStatus(item.Key.Item1, item.Key.Item2.ToString(), item.Value);
                sides.Add(item.Key.Item1);
            }
        };
        foreach (var side in sides) EventManager.Instance.SendEventSync(Core.Events.EventType.Battle_OnStatusUpdate, side);
    }

    public BattleCardData GetBattleCardData()
    {
        return curCardData;
    }

    public void Test1(GroupSide side)
    {
        OnReceiveDamage(side);
    }

    public void Test2()
    {
        OnUpdateGroupsInBattleField();
        OnUpdateHPStatusBarVisibility();
    }
}
